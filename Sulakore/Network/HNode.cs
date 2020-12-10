using System;
using System.Net;
using System.Text;
using System.Buffers;
using System.Threading;
using System.Net.Sockets;
using System.Buffers.Text;
using System.Net.Security;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using Sulakore.Network.Protocol;

namespace Sulakore.Network
{
    // TODO: Cleanup
    public class HNode : NetworkStream
    {
        private static readonly Random _rng;
        private static readonly byte[] _okBytes, _startTLSBytes;
        private static readonly byte[] _upgradeWebSocketResponseBytes;
        private static readonly byte[] _rfc6455GuidBytes, _secWebSocketKeyBytes;

        private readonly Queue<Memory<byte>> _frameHeaders;
        private readonly SemaphoreSlim _sendSemaphore, _receiveSemaphore;
        private readonly Queue<IMemoryOwner<byte>> _rentedHeaderOwners, _rentedPayloadOwners;

        private bool _disposed;
        private byte[] _mask = null;
        private SslStream _secureSocketLayer;
        private SslStream _securePayloadLayer;

        public HFormat SendFormat { get; set; }
        public HFormat ReceiveFormat { get; set; }
        public HotelEndPoint RemoteEndPoint { get; private set; }

        public bool IsClient { get; private set; }
        public bool IsUpgraded { get; private set; }
        public bool IsWebSocket { get; private set; }
        public bool IsConnected => !_disposed && Socket.Connected;
        public bool IsAuthenticated => _secureSocketLayer?.IsAuthenticated ?? _securePayloadLayer?.IsAuthenticated ?? false;
        public bool IsDoubleLayeredSSL => (_secureSocketLayer?.IsAuthenticated ?? false) && (_securePayloadLayer?.IsAuthenticated ?? false);

        static HNode()
        {
            _rng = new Random();
            _okBytes = Encoding.UTF8.GetBytes("OK");
            _startTLSBytes = Encoding.UTF8.GetBytes("StartTLS");
            _secWebSocketKeyBytes = Encoding.UTF8.GetBytes("Sec-WebSocket-Key: ");
            _rfc6455GuidBytes = Encoding.UTF8.GetBytes("258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
            _upgradeWebSocketResponseBytes = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: ");
        }
        public HNode(Socket socket)
            : base(socket, false)
        {
            _sendSemaphore = new SemaphoreSlim(2, 2);
            _frameHeaders = new Queue<Memory<byte>>();
            _receiveSemaphore = new SemaphoreSlim(2, 2);
            _rentedHeaderOwners = new Queue<IMemoryOwner<byte>>();
            _rentedPayloadOwners = new Queue<IMemoryOwner<byte>>();

            socket.NoDelay = true;
            socket.LingerState = new LingerOption(false, 0);
            if (socket.RemoteEndPoint is IPEndPoint ipEndPoint)
            {
                RemoteEndPoint = new HotelEndPoint(ipEndPoint);
            }
        }

        public bool ReflectFormats(HNode other)
        {
            SendFormat = other.ReceiveFormat;
            ReceiveFormat = other.SendFormat;
            return IsWebSocket = other.IsWebSocket;
        }
        public async Task<bool> DetermineFormatsAsync()
        {
            static void ParseInitialBytes(Span<byte> initialBytesSpan, out bool isWebSocket, out ushort possibleId)
            {
                const int GET_MAGIC = 542393671;
                isWebSocket = BinaryPrimitives.ReadInt32LittleEndian(initialBytesSpan) == GET_MAGIC;
                possibleId = BinaryPrimitives.ReadUInt16BigEndian(initialBytesSpan.Slice(4, 2));
            }

            // Connection type can only be determined in the beginning. ("GET"/WebSocket/EvaWire, '4000'/Raw/EvaWire, '206'/Raw/Wedgie)
            using IMemoryOwner<byte> initialBytesOwner = RentTrimmedRegion(6, out Memory<byte> initialBytesRegion);

            int initialBytesRead = await SocketPeekAsync(initialBytesRegion).ConfigureAwait(false);
            ParseInitialBytes(initialBytesRegion.Span.Slice(0, initialBytesRead), out bool isWebSocket, out ushort possibleId);

            IsWebSocket = isWebSocket;
            if (IsWebSocket || possibleId == 4000)
            {
                SendFormat = HFormat.EvaWire;
                ReceiveFormat = HFormat.EvaWire;
            }
            else if (possibleId == 206)
            {
                SendFormat = HFormat.WedgieIn;
                ReceiveFormat = HFormat.WedgieOut;
            }
            return IsWebSocket;
        }
        public async Task<bool> UpgradeWebSocketAsClientAsync()
        {
            static string GenerateWebSocketKey()
            {
                Span<byte> keyGenerationBuffer = stackalloc byte[24];
                _rng.NextBytes(keyGenerationBuffer.Slice(0, 16));

                Base64.EncodeToUtf8InPlace(keyGenerationBuffer, 16, out int encodedSize);
                return Encoding.UTF8.GetString(keyGenerationBuffer.Slice(0, encodedSize));
            }
            static bool IsTLSAccepted(ReadOnlySpan<byte> message) => message.SequenceEqual(_okBytes);
            const string requestHeaders = "Connection: Upgrade\r\n"
                                          + "Pragma: no-cache\r\n"
                                          + "Cache-Control: no-cache\r\n"
                                          + "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.67 Safari/537.36 Edg/87.0.664.52\r\n"
                                          + "Upgrade: websocket\r\n"
                                          + "Origin: https://images.habbogroup.com\r\n"
                                          + "Sec-WebSocket-Version: 13\r\n"
                                          + "Accept-Encoding: gzip, deflate, br\r\n"
                                          + "Accept-Language: en-US,en;q=0.9";

            IsClient = true;
            // Initialize the top-most secure tunnel where ALL data will be read/written from/to.
            _secureSocketLayer = new SslStream(new NetworkStream(Socket, false), false, ValidateRemoteCertificate);
            await _secureSocketLayer.AuthenticateAsClientAsync(RemoteEndPoint.Host, null, false).ConfigureAwait(false);
            if (!_secureSocketLayer.IsAuthenticated) return false;

            string webRequest = $"GET /websocket HTTP/1.1\r\nHost: {RemoteEndPoint}\r\n{requestHeaders}\r\nSec-WebSocket-Key: {GenerateWebSocketKey()}\r\n\r\n";
            await SendAsync(webRequest).ConfigureAwait(false);

            using IMemoryOwner<byte> receiveOwner = RentTrimmedRegion(256, out Memory<byte> receiveRegion);
            int received = await ReceiveAsync(receiveRegion).ConfigureAwait(false);

            // Create the mask that will be used for the WebSocket payloads.
            _mask = new byte[4];
            //_rng.NextBytes(_mask);
            IsUpgraded = true; // Once upgraded, all data will be converted to/from WebSocket frames.

            await SendAsync("StartTLS").ConfigureAwait(false);
            received = await ReceiveAsync(receiveRegion).ConfigureAwait(false);
            if (!IsTLSAccepted(receiveRegion.Span.Slice(0, received))) return false;

            // Initialize the second secure tunnel layer where ONLY the WebSocket payload data will be read/written from/to.
            _securePayloadLayer = new SslStream(this, true, ValidateRemoteCertificate);
            await _securePayloadLayer.AuthenticateAsClientAsync(RemoteEndPoint.Host, null, false).ConfigureAwait(false);

            return IsUpgraded;
        }
        public async Task<bool> UpgradeWebSocketAsServerAsync(X509Certificate certificate)
        {
            static bool IsTLSRequested(ReadOnlySpan<byte> message) => message.SequenceEqual(_startTLSBytes);
            static void FillWebResponse(ReadOnlySpan<byte> webRequest, Span<byte> webResponse, out int responseWritten)
            {
                int keyStart = webRequest.LastIndexOf(_secWebSocketKeyBytes) + _secWebSocketKeyBytes.Length;
                int keySize = webRequest.Slice(keyStart).IndexOf((byte)13); // Carriage Return

                Span<byte> mergedKey = stackalloc byte[keySize + _rfc6455GuidBytes.Length];
                webRequest.Slice(keyStart, keySize).CopyTo(mergedKey);
                _rfc6455GuidBytes.CopyTo(mergedKey.Slice(keySize));

                _upgradeWebSocketResponseBytes.CopyTo(webResponse);
                responseWritten = _upgradeWebSocketResponseBytes.Length;

                int keyHashedSize = SHA1.HashData(mergedKey, webResponse.Slice(_upgradeWebSocketResponseBytes.Length));
                Base64.EncodeToUtf8InPlace(webResponse.Slice(responseWritten), keyHashedSize, out int keyEncodedSize);
                responseWritten += keyEncodedSize;

                Span<byte> eof = webResponse.Slice(responseWritten);
                eof[0] = eof[2] = 13; // Carriage Return
                eof[1] = eof[3] = 10; // New Line
                responseWritten += 4; // \r\n\r\n
            }

            if (IsUpgraded || !await DetermineFormatsAsync()) return IsUpgraded;
            using IMemoryOwner<byte> receivedOwner = RentTrimmedRegion(1024, out Memory<byte> receivedRegion);
            int received = await ReceiveAsync(receivedRegion).ConfigureAwait(false);

            using IMemoryOwner<byte> responseOwner = RentTrimmedRegion(256, out Memory<byte> responseRegion);
            FillWebResponse(receivedRegion.Span.Slice(0, received), responseRegion.Span, out int responseWritten);
            await SendAsync(responseRegion.Slice(0, responseWritten)).ConfigureAwait(false);

            // Begin receiving/sending data as WebSocket frames.
            IsUpgraded = true;

            received = await ReceiveAsync(receivedRegion).ConfigureAwait(false);
            if (IsTLSRequested(receivedRegion.Span.Slice(0, received)))
            {
                await SendAsync(_okBytes).ConfigureAwait(false);

                _securePayloadLayer = new SslStream(this, true, ValidateRemoteCertificate);
                await _securePayloadLayer.AuthenticateAsServerAsync(certificate).ConfigureAwait(false);
            }
            else throw new Exception("The client did not send 'StartTLS'.");
            return IsUpgraded;
        }

        public Task<HPacket> ReceiveAsync()
        {
            if (ReceiveFormat == null)
            {
                throw new NullReferenceException($"Cannot receive structued data while {nameof(ReceiveFormat)} is null.");
            }
            return ReceiveFormat.ReceivePacketAsync(this);
        }
        public ValueTask<int> SendAsync(ushort id, params object[] values)
        {
            if (SendFormat == null)
            {
                throw new NullReferenceException($"Cannot send structued data while {nameof(SendFormat)} is null.");
            }
            return SendAsync(SendFormat.Construct(id, values));
        }

        public ValueTask<int> SendAsync(string message)
        {
            return SendAsync(Encoding.UTF8.GetBytes(message));
        }
        public ValueTask<int> SendAsync(HPacket packet)
        {
            return SendAsync(packet.ToBytes());
        }

        public virtual async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int received = 0;
            if (!IsConnected) return -1;
            if (buffer.Length == 0) return 0;
            await _receiveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!IsUpgraded || _rentedHeaderOwners.Count > 0)
                {
                    int limit = buffer.Length;
                    if (_rentedHeaderOwners.TryPeek(out IMemoryOwner<byte> headerOwner))
                    {
                        int frameHeaderExtra = DecodeFrameHeader(headerOwner.Memory.Span, out bool isMasked, out ulong payloadSize, out int opcode);
                        limit = (int)payloadSize;
                    }

                    received = await SocketReceiveAsync(buffer.Slice(0, limit), cancellationToken).ConfigureAwait(false);
                    if (_rentedHeaderOwners.TryDequeue(out headerOwner))
                    {
                        int frameHeaderExtra = DecodeFrameHeader(headerOwner.Memory.Span, out bool isMasked, out ulong payloadSize, out int opcode);
                        MemoryMarshal.Write(headerOwner.Memory.Span.Slice(14), ref received); // Notify the owner how much of the encrypted/masked payload was read.
                        if (isMasked)
                        {
                            UnmaskPayload(buffer.Span.Slice(0, received), headerOwner.Memory.Span.Slice(2 + (frameHeaderExtra - 4), 4));
                        }
                    }
                }
                else if (_rentedHeaderOwners.Count == 0)
                {
                    received = await UnwrapWebSocketFramesAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
                // TODO: Decrypt
            }
            catch { return -1; }
            finally { _receiveSemaphore.Release(); }
            return received;
        }
        public virtual async ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int sent = 0;
            if (!IsConnected) return -1;
            if (buffer.Length <= 0) return 0;
            await _sendSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // TODO: Encrypt
                if (IsUpgraded && _frameHeaders.Count == 0)
                {
                    sent = await WrapWebSocketFramesAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    ReadOnlyMemory<byte> payload = buffer;
                    if (_frameHeaders.TryDequeue(out Memory<byte> extendedFrameHeaderRegion))//&& IsClient) // Only deque and ENTER when acting as client, otherwise just deque it.
                    {
                        if (IsClient)
                        {
                            int encodedFrameHeaderSize = EncodeFrameHeader(extendedFrameHeaderRegion.Span.Slice(4), buffer.Length, IsClient);
                            sent += await SocketSendAsync(extendedFrameHeaderRegion.Slice(4, encodedFrameHeaderSize), cancellationToken).ConfigureAwait(false);
                            sent += await SocketSendAsync(_mask, cancellationToken).ConfigureAwait(false);
                            MaskPayload(buffer.Span, _mask, out payload);
                        }
                        else
                        {
                            const int MAX_FRAME_PAYLOAD_SIZE = 125;

                            int dataSent = 0;
                            Memory<byte> frameHeaderRegion = extendedFrameHeaderRegion.Slice(4);
                            for (int i = 0, payloadLeft = buffer.Length; payloadLeft > 0; payloadLeft -= MAX_FRAME_PAYLOAD_SIZE, i++)
                            {
                                bool isFinalFrame = payloadLeft <= MAX_FRAME_PAYLOAD_SIZE;
                                int payloadStart = MAX_FRAME_PAYLOAD_SIZE * i;
                                int payloadSize = isFinalFrame ? payloadLeft : MAX_FRAME_PAYLOAD_SIZE;

                                int header = isFinalFrame || IsClient ? 1 : 0;
                                header = (header << 1) + 0; // RSV1 - IsDataCompressed
                                header = (header << 1) + 0; // RSV2
                                header = (header << 1) + 0; // RSV3
                                header = (header << 4) + (i == 0 || IsClient ? 2 : 0); // If this is the first frame, mark it as a BINARY, otherwise CONTINUATION.
                                header = (header << 1) + (IsClient ? 1 : 0); // Mask should always be present when sending data to the server.

                                int frameHeaderSize = 2;
                                header = (header << 7) + payloadSize;
                                payloadSize = isFinalFrame ? payloadLeft : MAX_FRAME_PAYLOAD_SIZE;

                                ReadOnlyMemory<byte> encryptedPayloadFragment = buffer.Slice(payloadStart, payloadSize);
                                BinaryPrimitives.WriteUInt16BigEndian(frameHeaderRegion.Span, (ushort)header);
                                dataSent += await SocketSendAsync(frameHeaderRegion.Slice(0, frameHeaderSize), cancellationToken).ConfigureAwait(false);
                                dataSent += await SocketSendAsync(encryptedPayloadFragment, cancellationToken).ConfigureAwait(false);
                            }
                            return dataSent;
                        }
                    }
                    sent += await SocketSendAsync(payload, cancellationToken).ConfigureAwait(false);
                }
            }
            catch { return -1; }
            finally { _sendSemaphore.Release(); }
            return sent;
        }

        protected virtual ValueTask<int> ReceivePayloadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_securePayloadLayer?.IsAuthenticated ?? false)
            {
                return _securePayloadLayer.ReadAsync(buffer, cancellationToken);
            }
            else return SocketReceiveAsync(buffer, cancellationToken);
        }
        protected virtual async ValueTask<int> SendPayloadAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_securePayloadLayer?.IsAuthenticated ?? false)
            {
                await _securePayloadLayer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                return buffer.Length;
            }
            return await SocketSendAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        protected virtual async ValueTask<int> UnwrapWebSocketFramesAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            static void WriteExtendedPayloadHeader(Span<byte> extendedPayload, int consumed, int payloadSize)
            {
                MemoryMarshal.Write(extendedPayload, ref consumed);
                MemoryMarshal.Write(extendedPayload.Slice(4), ref payloadSize);
            }
            static bool ConsumePayload(Span<byte> extendedPayload, Span<byte> buffer, out int consumed)
            {
                int totalConsumed = MemoryMarshal.Read<int>(extendedPayload);
                int payloadSize = MemoryMarshal.Read<int>(extendedPayload.Slice(4));

                // How much of the payload in bytes is left for consumption.
                int payloadLeft = (payloadSize - totalConsumed);

                // Copy as much of the remaining payload to the buffer as possible.
                consumed = Math.Min(payloadLeft, buffer.Length);
                bool isEntirePayloadConsumed = consumed == payloadLeft;

                extendedPayload.Slice(8 + totalConsumed, consumed).CopyTo(buffer);
                totalConsumed += consumed;

                MemoryMarshal.Write(extendedPayload, ref totalConsumed);
                return isEntirePayloadConsumed;
            }

            int opcode = 0;
            bool isMasked = false;
            ulong payloadSize = 0;
            int frameHeaderSize = 0;
            Memory<byte> headerRegion = null;
            IMemoryOwner<byte> headerOwner = null;
            if (_rentedPayloadOwners.Count == 0)
            {
                headerOwner = RentTrimmedRegion(18, out headerRegion);
                frameHeaderSize = await SocketReceiveAsync(headerRegion.Slice(0, 2), cancellationToken).ConfigureAwait(false);
                frameHeaderSize += DecodeFrameHeader(headerRegion.Span.Slice(0, 2), out isMasked, out payloadSize, out opcode);
                if (frameHeaderSize > 2)
                {
                    await SocketReceiveAsync(headerRegion.Slice(2, frameHeaderSize - 2), cancellationToken).ConfigureAwait(false);
                    DecodeFrameHeader(headerRegion.Span, out isMasked, out payloadSize, out opcode); // Decode the header again, since we might now be able to retrieve the actual payload size of the frame.
                }
                if (payloadSize == 0) return 0;

                // The payload will first need to be unmasked, and then be passed through our SslStream instance to get decrypted.
                if (IsDoubleLayeredSSL || isMasked && IsAuthenticated)
                {
                    _rentedHeaderOwners.Enqueue(headerOwner);
                }
            }

            int consumed = 0;
            bool wasPayloadOwnerPeeked = false;
            bool isEntirePayloadConsumed = true;
            IMemoryOwner<byte> payloadOwner = null;
            try
            {
                Memory<byte> payloadRegion = null;
                Memory<byte> extendedPayloadRegion = null;
                if (!(wasPayloadOwnerPeeked = _rentedPayloadOwners.TryPeek(out payloadOwner)))
                {
                    // If we enter this block, then that means 'headerOwner' is NOT null.
                    // We use the first 8 bytes to hold two Int32 values that represents the size in bytes of the payload already consumed, and the size in bytes left to be consumed.
                    payloadOwner = RentTrimmedRegion(8 + (int)payloadSize, out extendedPayloadRegion);
                    payloadRegion = extendedPayloadRegion.Slice(8);

                    int payloadRead = await ReceivePayloadAsync(payloadRegion, cancellationToken).ConfigureAwait(false); // At this point, we trust that payloadRead IS EQUAL to payloadSize.
                    if (headerOwner != null && (IsDoubleLayeredSSL || isMasked && IsAuthenticated))
                    {
                        int encrytedPayloadRead = MemoryMarshal.Read<int>(headerOwner.Memory.Span.Slice(14));
                    }

                    WriteExtendedPayloadHeader(extendedPayloadRegion.Span, 0, payloadRead);
                    if (isMasked && !IsAuthenticated)
                    {
                        UnmaskPayload(payloadRegion.Span, headerRegion.Span.Slice(frameHeaderSize - 4, 4));
                    }
                }
                else extendedPayloadRegion = payloadOwner.Memory;

                isEntirePayloadConsumed = ConsumePayload(extendedPayloadRegion.Span, buffer.Span, out consumed);
                if (!isEntirePayloadConsumed && !wasPayloadOwnerPeeked)
                {
                    _rentedPayloadOwners.Enqueue(payloadOwner);
                }
            }
            finally
            {
                headerOwner?.Dispose();
                if (isEntirePayloadConsumed)
                {
                    if (wasPayloadOwnerPeeked)
                    {
                        _rentedPayloadOwners.Dequeue();
                    }
                    payloadOwner.Dispose();
                }
            }
            return consumed;
        }
        protected virtual async ValueTask<int> WrapWebSocketFramesAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            const int MAX_FRAME_PAYLOAD_SIZE = 125;

            using IMemoryOwner<byte> frameHeaderOwner = RentTrimmedRegion(18, out Memory<byte> extendedFrameHeaderRegion);
            Memory<byte> frameHeaderRegion = extendedFrameHeaderRegion.Slice(4); // Reserved for returning the amount of encrypted bytes sent.

            if (IsAuthenticated && !IsClient)
            {
                _frameHeaders.Enqueue(extendedFrameHeaderRegion);
                return await SendPayloadAsync(buffer, cancellationToken).ConfigureAwait(false); // Just send it all.
            }

            if (IsClient && IsDoubleLayeredSSL)
            {
                // This WebSocket frame header should be built using the encrypted payload size.
                _frameHeaders.Enqueue(extendedFrameHeaderRegion);
                //MaskPayload(buffer.Span, _mask, out ReadOnlyMemory<byte> masked);
                return await SendPayloadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }

            int dataSent = 0;
            for (int i = 0, payloadLeft = buffer.Length; payloadLeft > 0; payloadLeft -= MAX_FRAME_PAYLOAD_SIZE, i++)
            {
                bool isFinalFrame = payloadLeft <= MAX_FRAME_PAYLOAD_SIZE;
                int payloadStart = MAX_FRAME_PAYLOAD_SIZE * i;
                int payloadSize = isFinalFrame ? payloadLeft : MAX_FRAME_PAYLOAD_SIZE;

                int header = isFinalFrame || IsClient ? 1 : 0;
                header = (header << 1) + 0; // RSV1 - IsDataCompressed
                header = (header << 1) + 0; // RSV2
                header = (header << 1) + 0; // RSV3
                header = (header << 4) + (i == 0 || IsClient ? 2 : 0); // If this is the first frame, mark it as a BINARY, otherwise CONTINUATION.
                header = (header << 1) + (IsClient ? 1 : 0); // Mask should always be present when sending data to the server.

                int frameHeaderSize = 2;
                if (IsClient && buffer.Length > 125)
                {
                    if (buffer.Length <= ushort.MaxValue)
                    {
                        payloadSize = 126;
                        frameHeaderSize += 2;
                        BinaryPrimitives.WriteUInt16BigEndian(frameHeaderRegion.Span.Slice(2), (ushort)buffer.Length);
                    }
                    else
                    {
                        payloadSize = 127;
                        frameHeaderSize += 8;
                        BinaryPrimitives.WriteUInt64BigEndian(frameHeaderRegion.Span.Slice(2), (ulong)buffer.Length);
                    }
                }
                header = (header << 7) + payloadSize;
                payloadSize = isFinalFrame ? payloadLeft : MAX_FRAME_PAYLOAD_SIZE;

                BinaryPrimitives.WriteUInt16BigEndian(frameHeaderRegion.Span, (ushort)header);
                if (!IsAuthenticated || IsClient)
                {
                    dataSent += await SocketSendAsync(frameHeaderRegion.Slice(0, frameHeaderSize), cancellationToken).ConfigureAwait(false);
                }
                ReadOnlyMemory<byte> payload = buffer.Slice(payloadStart, payloadSize);
                if (_mask != null)
                {
                    dataSent += await SocketSendAsync(_mask, cancellationToken).ConfigureAwait(false);
                    MaskPayload(buffer.Span, _mask, out payload);
                }

                dataSent += await SendPayloadAsync(payload, cancellationToken).ConfigureAwait(false);
                if (IsClient) break;
            }
            return dataSent;
        }

        // These methods shorten some lines/expressions of code where the Socket.Receive/Send calls require a SocketFlags parameter.
        private ValueTask<int> SocketPeekAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_secureSocketLayer?.IsAuthenticated ?? false)
            {
                throw new NotSupportedException();
            }
            return Socket.ReceiveAsync(buffer, SocketFlags.Peek, cancellationToken);
        }
        private ValueTask<int> SocketReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_secureSocketLayer?.IsAuthenticated ?? false)
            {
                return _secureSocketLayer.ReadAsync(buffer, cancellationToken);
            }
            return Socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
        private async ValueTask<int> SocketSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_secureSocketLayer?.IsAuthenticated ?? false)
            {
                await _secureSocketLayer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                return buffer.Length;
            }
            return await Socket.SendAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            if (!_disposed)
            {
                _disposed = true;
                try
                {
                    if (_secureSocketLayer != null)
                    {
                        await _secureSocketLayer.DisposeAsync().ConfigureAwait(false);
                    }
                    if (_securePayloadLayer != null)
                    {
                        await _securePayloadLayer.DisposeAsync().ConfigureAwait(false);
                    }
                    Socket.Shutdown(SocketShutdown.Both);
                    await Task.Factory.FromAsync(Socket.BeginDisconnect(false, null, null), Socket.EndDisconnect).ConfigureAwait(false);
                    Socket.Close(0);
                }
                catch { }
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposed && disposing)
            {
                _disposed = true;
                try
                {
                    _secureSocketLayer?.Dispose();
                    _securePayloadLayer?.Dispose();
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Disconnect(false);
                    Socket.Close(0);
                }
                catch { }
            }
        }

        private static int DecodeFrameHeader(Span<byte> header, out bool isMasked, out ulong payloadSize, out int opcode)
        {
            opcode = header[0] & 0b00001111;
            payloadSize = (ulong)(header[1] & 0x7F);
            isMasked = (header[1] & 0x80) == 0x80;

            int frameHeaderExtra = 0;
            switch (payloadSize)
            {
                case 126:
                {
                    frameHeaderExtra = sizeof(ushort);
                    if (header.Length >= 4)
                    {
                        payloadSize = BinaryPrimitives.ReadUInt16BigEndian(header.Slice(2));
                    }
                    break;
                }
                case 127:
                {
                    frameHeaderExtra = sizeof(ulong);
                    if (header.Length >= 10)
                    {
                        payloadSize = BinaryPrimitives.ReadUInt64BigEndian(header.Slice(2));
                    }
                    break;
                }
            }
            return frameHeaderExtra + (isMasked ? 4 : 0);
        }
        private static int EncodeFrameHeader(Span<byte> header, int payloadLength, bool isSendingAsClient, bool isFirstFragment = true, bool isFinalFragment = false)
        {
            int headerContainer = isFinalFragment || isSendingAsClient ? 1 : 0;
            headerContainer = (headerContainer << 1) + 0; // RSV1 - IsDataCompressed
            headerContainer = (headerContainer << 1) + 0; // RSV2 - Always 0
            headerContainer = (headerContainer << 1) + 0; // RSV3 - Always 0
            headerContainer = (headerContainer << 4) + (isFirstFragment || isSendingAsClient ? 2 : 0); // The first fragment should signify whether the frame is a binary, or end-user is currently processing a continuation fragment.
            headerContainer = (headerContainer << 1) + (isSendingAsClient ? 1 : 0); // Mask will always be present when sending data to the server.

            int frameHeaderBytesWritten = 2;
            if (isSendingAsClient && payloadLength > 125)
            {
                if (payloadLength <= ushort.MaxValue)
                {
                    frameHeaderBytesWritten += 2;
                    headerContainer = (headerContainer << 7) + 126;
                    BinaryPrimitives.WriteUInt16BigEndian(header.Slice(2), (ushort)payloadLength);
                }
                else
                {
                    frameHeaderBytesWritten += 8;
                    headerContainer = (headerContainer << 7) + 127;
                    BinaryPrimitives.WriteUInt64BigEndian(header.Slice(2), (ulong)payloadLength);
                }
            }
            else headerContainer = (headerContainer << 7) + payloadLength;

            BinaryPrimitives.WriteUInt16BigEndian(header, (ushort)headerContainer);
            return frameHeaderBytesWritten;
        }

        private static void UnmaskPayload(Span<byte> buffer, Span<byte> mask)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] ^= mask[i % 4];
            }
        }
        private static void MaskPayload(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> mask, out ReadOnlyMemory<byte> maskedRegion)
        {
            var masked = new byte[payload.Length];
            for (int i = 0; i < masked.Length; i++)
            {
                masked[i] = (byte)(payload[i] ^ mask[i % 4]);
            }
            maskedRegion = new ReadOnlyMemory<byte>(masked);
        }

        private static IMemoryOwner<T> RentTrimmedRegion<T>(int size, out Memory<T> trimmedRegion)
        {
            var trimmedOwner = MemoryPool<T>.Shared.Rent(size);
            trimmedRegion = trimmedOwner.Memory.Slice(0, size);
            return trimmedOwner;
        }
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        #region NetworkStream Overrides
        public override int Read(byte[] buffer, int offset, int size)
        {
            return ReadAsync(buffer, offset, size, CancellationToken.None).GetAwaiter().GetResult();
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ReceiveAsync(buffer, cancellationToken);
        }
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int size, CancellationToken cancellationToken)
        {
            return await ReceiveAsync(buffer.AsMemory(offset, size), cancellationToken).ConfigureAwait(false);
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            WriteAsync(buffer, offset, size, CancellationToken.None).GetAwaiter().GetResult();
        }
        public override async Task WriteAsync(byte[] buffer, int offset, int size, CancellationToken cancellationToken)
        {
            await SendAsync(buffer.AsMemory(offset, size), cancellationToken).ConfigureAwait(false);
        }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await SendAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        public override int EndRead(IAsyncResult asyncResult) => throw new NotSupportedException();
        public override void EndWrite(IAsyncResult asyncResult) => throw new NotSupportedException();
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state) => throw new NotSupportedException();
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state) => throw new NotSupportedException();
        #endregion

        public static async Task<HNode> AcceptAsync(int port)
        {
            using var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            listenSocket.LingerState = new LingerOption(false, 0);
            listenSocket.Listen(1);

            Socket socket = await listenSocket.AcceptAsync().ConfigureAwait(false);
            listenSocket.Close();

            return new HNode(socket);
        }
        public static async Task<HNode> ConnectAsync(IPEndPoint endpoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await Task.Factory.FromAsync(socket.BeginConnect(endpoint, null, null), socket.EndConnect).ConfigureAwait(false);
            }
            catch { /* Ignore all exceptions. */ }

            if (!socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return null;
            }
            else
            {
                var node = new HNode(socket);
                if (endpoint is HotelEndPoint hotelEndPoint)
                {
                    node.RemoteEndPoint = hotelEndPoint;
                }
                return node;
            }
        }
        public static Task<HNode> ConnectAsync(string host, int port) => ConnectAsync(HotelEndPoint.Parse(host, port));
        public static Task<HNode> ConnectAsync(IPAddress address, int port) => ConnectAsync(new HotelEndPoint(address, port));
        public static Task<HNode> ConnectAsync(IPAddress[] addresses, int port) => ConnectAsync(new HotelEndPoint(addresses[0], port));
    }
}