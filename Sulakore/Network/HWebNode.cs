using System;
using System.Net;
using System.Text;
using System.Buffers;
using System.Threading;
using System.Net.Sockets;
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
    // TODO: This class will replace HNode, implement HMessage reading/writing.
    public class HWebNode : NetworkStream
    {
        private readonly SemaphoreSlim _sendSemaphore, _receiveSemaphore;
        private readonly Queue<IMemoryOwner<byte>> _rentedMaskOwners, _rentedPayloadOwners;

        private bool _disposed;
        private SslStream _securePayloadLayer;
        private X509Certificate _clientCertificate;

        public HFormat SendFormat { get; private set; }
        public HFormat ReceiveFormat { get; private set; }

        public HotelEndPoint RemoteEndPoint { get; }

        public bool IsUpgraded { get; private set; }
        public bool IsWebSocket { get; private set; }
        public bool IsConnected => !_disposed && Socket.Connected;
        public bool IsAuthenticated => _securePayloadLayer?.IsAuthenticated ?? false;

        public HWebNode(Socket socket)
            : base(socket, false)
        {
            _sendSemaphore = new SemaphoreSlim(1, 1);
            _receiveSemaphore = new SemaphoreSlim(2, 2);
            _rentedMaskOwners = new Queue<IMemoryOwner<byte>>();
            _rentedPayloadOwners = new Queue<IMemoryOwner<byte>>();

            socket.NoDelay = true;
            socket.LingerState = new LingerOption(false, 0);
            if (socket.RemoteEndPoint is IPEndPoint ipEndPoint)
            {
                RemoteEndPoint = new HotelEndPoint(ipEndPoint);
            }
        }

        public async Task AuthenticateAsClientAsync(X509Certificate clientCertificate)
        {
            _clientCertificate = clientCertificate;
            _securePayloadLayer = new SslStream(this, true, ValidateRemoteCertificate);
            await _securePayloadLayer.AuthenticateAsClientAsync(new SslClientAuthenticationOptions()
            {
                LocalCertificateSelectionCallback = SelectLocalCertificate
            }).ConfigureAwait(false);
        }
        public async Task AuthenticateAsServerAsync(X509Certificate serverCertificate)
        {
            _securePayloadLayer = new SslStream(this, true, ValidateRemoteCertificate);
            await _securePayloadLayer.AuthenticateAsServerAsync(serverCertificate).ConfigureAwait(false);
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
        private X509Certificate SelectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers) => _clientCertificate;

        public bool ReflectFormats(HWebNode local)
        {
            SendFormat = local.ReceiveFormat;
            ReceiveFormat = local.SendFormat;
            return IsWebSocket = local.IsWebSocket;
        }
        public async Task<bool> UpgradeWebSocketAsync()
        {
            static string HashWebSocketKey(Span<byte> requestHeader)
            {
                const int KEY_SIZE = 24;
                const int MERGED_KEY_LENGTH = 60;
                const string RFC6455_GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

                int keyIndex = requestHeader.IndexOf(Encoding.ASCII.GetBytes("Sec-WebSocket-Key")) + 19;
                requestHeader.Slice(keyIndex, KEY_SIZE).CopyTo(requestHeader);
                Encoding.ASCII.GetBytes(RFC6455_GUID, requestHeader.Slice(KEY_SIZE));

                int hashSize = SHA1.HashData(requestHeader.Slice(0, MERGED_KEY_LENGTH), requestHeader.Slice(MERGED_KEY_LENGTH));
                return Convert.ToBase64String(requestHeader.Slice(MERGED_KEY_LENGTH, hashSize));
            }

            if (IsUpgraded || !await DetermineFormatsAsync()) return IsUpgraded;
            using IMemoryOwner<byte> requestHeaderOwner = RentTrimmedRegion(1024, out Memory<byte> requestHeaderRegion);

            await ReceiveAsync(requestHeaderRegion).ConfigureAwait(false);
            string hashedMergedKey = HashWebSocketKey(requestHeaderRegion.Span);

            var responseHeaderBuilder = new StringBuilder();
            responseHeaderBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
            responseHeaderBuilder.AppendLine("Connection: Upgrade");
            responseHeaderBuilder.AppendLine("Upgrade: websocket");
            responseHeaderBuilder.AppendLine($"Sec-WebSocket-Accept: {hashedMergedKey}");
            responseHeaderBuilder.AppendLine();

            byte[] responseData = Encoding.UTF8.GetBytes(responseHeaderBuilder.ToString());
            await SendAsync(responseData).ConfigureAwait(false);

            return IsUpgraded = true;
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

        public ValueTask<int> SendAsync(string message)
        {
            return SendAsync(Encoding.UTF8.GetBytes(message));
        }
        public ValueTask<int> SendAsync(HPacket packet)
        {
            return SendAsync(packet.ToBytes());
        }

        public Task<HPacket> ReceiveAsync()
        {
            if (ReceiveFormat == null)
            {
                throw new NullReferenceException($"Cannot receive structued data while {nameof(ReceiveFormat)} is null.");
            }
            return ReceiveFormat.ReceivePacketAsync(this);
        }

        public virtual async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int received = 0;
            if (!IsConnected) return -1;
            if (buffer.Length == 0) return 0;
            await _receiveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!IsUpgraded || _rentedMaskOwners.Count > 0)
                {
                    received = await SocketReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (_rentedMaskOwners.TryDequeue(out IMemoryOwner<byte> maskOwner))
                    {
                        // TODO: Check if the received amount of data is equal to the frame's payload size, otherwise keep reading if we have enough room.
                        int frameHeaderExtra = DecodeFrameHeader(maskOwner.Memory.Span, out _, out ulong payloadSize);
                        MemoryMarshal.Write(maskOwner.Memory.Span.Slice(2 + frameHeaderExtra), ref received); // Notify the owner how much of the encrypted/masked payload was read.
                        UnmaskPayload(buffer.Span.Slice(0, received), maskOwner.Memory.Span.Slice(2 + frameHeaderExtra - 4, 4));
                    }
                }
                else if (_rentedMaskOwners.Count == 0) received = await UnwrapWebSocketFramesAsync(buffer, cancellationToken).ConfigureAwait(false);
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
                if (IsUpgraded)
                {
                    sent = await WrapWebSocketFramesAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
                else sent = await SocketSendAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            catch { return -1; }
            finally { _sendSemaphore.Release(); }
            return sent;
        }

        protected virtual ValueTask<int> ReceivePayloadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (IsAuthenticated)
            {
                return _securePayloadLayer.ReadAsync(buffer, cancellationToken);
            }
            else return SocketReceiveAsync(buffer, cancellationToken);
        }
        protected virtual async ValueTask<int> SendPayloadAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (IsAuthenticated)
            {
                await _securePayloadLayer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                return buffer.Length;
            }
            else return await SocketSendAsync(buffer, cancellationToken).ConfigureAwait(false);
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

            bool isMasked = false;
            ulong payloadSize = 0;
            int frameHeaderSize = 0;
            Memory<byte> headerRegion = null;
            IMemoryOwner<byte> headerOwner = null;
            if (_rentedPayloadOwners.Count == 0)
            {
                // Rent a portion of memory that can accomdate the maximum size of a frame header: (header:2, payloadSize:2/8, mask:4)
                headerOwner = RentTrimmedRegion(2 + sizeof(ulong) + 4, out headerRegion);
                frameHeaderSize = await SocketReceiveAsync(headerRegion.Slice(0, 2), cancellationToken).ConfigureAwait(false);

                int frameHeaderExtra = DecodeFrameHeader(headerRegion.Span, out isMasked, out payloadSize);
                if (frameHeaderExtra > 0)
                {
                    frameHeaderSize += await SocketReceiveAsync(headerRegion.Slice(2, frameHeaderExtra), cancellationToken).ConfigureAwait(false);
                    DecodeFrameHeader(headerRegion.Span, out isMasked, out payloadSize); // Decode the header again, since we might now be able to retrieve the actual payload size of the frame.
                }

                // The payload will first need to be unmasked, and then be passed through our SslStream instance to get decrypted.
                if (isMasked && IsAuthenticated)
                {
                    _rentedMaskOwners.Enqueue(headerOwner);
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

            int dataSent = 0;
            using IMemoryOwner<byte> frameHeaderOwner = RentTrimmedRegion(2, out Memory<byte> frameHeaderRegion);
            for (int i = 0, payloadLeft = buffer.Length; payloadLeft > 0; payloadLeft -= MAX_FRAME_PAYLOAD_SIZE, i++)
            {
                bool isFinalFrame = payloadLeft <= MAX_FRAME_PAYLOAD_SIZE;
                int payloadStart = MAX_FRAME_PAYLOAD_SIZE * i;
                int payloadSize = isFinalFrame ? payloadLeft : MAX_FRAME_PAYLOAD_SIZE;

                int header = isFinalFrame ? 1 : 0;
                header = (header << 1) + 0; // RSV1 - IsDataCompressed
                header = (header << 1) + 0; // RSV2
                header = (header << 1) + 0; // RSV3
                header = (header << 4) + (i == 0 ? 2 : 0); // If this is the first frame, mark it as a BINARY, otherwise CONTINUATION.
                header = (header << 1) + 0;
                header = (header << 7) + payloadSize;

                BinaryPrimitives.WriteUInt16BigEndian(frameHeaderRegion.Span, (ushort)header);
                dataSent += await SocketSendAsync(frameHeaderRegion, cancellationToken).ConfigureAwait(false);
                dataSent += await SendPayloadAsync(buffer.Slice(payloadStart, payloadSize), cancellationToken).ConfigureAwait(false);
            }
            return dataSent;
        }

        // These methods shorten some lines/expressions of code where the Socket.Receive/Send calls require a SocketFlags parameter.
        private ValueTask<int> SocketPeekAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return Socket.ReceiveAsync(buffer, SocketFlags.Peek, cancellationToken);
        }
        private ValueTask<int> SocketReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return Socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
        private ValueTask<int> SocketSendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return Socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            if (!_disposed)
            {
                _disposed = true;
                try
                {
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
                    _securePayloadLayer?.Dispose();
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Disconnect(false);
                    Socket.Close(0);
                }
                catch { }
            }
        }

        private static void UnmaskPayload(Span<byte> buffer, Span<byte> mask)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] ^= mask[i % 4];
            }
        }
        private static IMemoryOwner<T> RentTrimmedRegion<T>(int size, out Memory<T> trimmedRegion)
        {
            var trimmedOwner = MemoryPool<T>.Shared.Rent(size);
            trimmedRegion = trimmedOwner.Memory.Slice(0, size);
            return trimmedOwner;
        }
        private static int DecodeFrameHeader(Span<byte> header, out bool isMasked, out ulong payloadSize)
        {
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

        public static async Task<HWebNode> AcceptAsync(int port)
        {
            using var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            listenSocket.LingerState = new LingerOption(false, 0);
            listenSocket.Listen(1);

            Socket socket = await listenSocket.AcceptAsync().ConfigureAwait(false);
            listenSocket.Close();

            return new HWebNode(socket);
        }
        public static async Task<HWebNode> ConnectAsync(IPEndPoint endpoint)
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
            else return new HWebNode(socket);
        }
        public static Task<HWebNode> ConnectAsync(IPAddress[] addresses, int port) => ConnectAsync(new HotelEndPoint(addresses[0], port));
        public static Task<HWebNode> ConnectAsync(string host, int port) => ConnectAsync(HotelEndPoint.Parse(host, port));
        public static Task<HWebNode> ConnectAsync(IPAddress address, int port) => ConnectAsync(new HotelEndPoint(address, port));
    }
}