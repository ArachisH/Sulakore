using System.Net;
using System.Text;
using System.Buffers;
using System.Net.Sockets;
using System.Diagnostics;
using System.Buffers.Text;
using System.Net.Security;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Sulakore.Buffers;
using Sulakore.Cryptography.Ciphers;
using Sulakore.Network.Protocol.Formats;

namespace Sulakore.Network
{
    public class HNode : IDisposable
    {
        private static readonly Random _rng;
        private static readonly byte[] _emptyMask;
        private static readonly byte[] _okBytes, _startTLSBytes;
        private static readonly byte[] _upgradeWebSocketResponseBytes;
        private static readonly byte[] _rfc6455GuidBytes, _secWebSocketKeyBytes;

        private readonly Socket _socket;
        private readonly SemaphoreSlim _sendSemaphore, _receiveSemaphore, _packetReceiveSemaphore;

        private bool _disposed;
        private byte[] _mask = null;
        /// <summary>
        /// Represents a possibly mutli-layered secured stream capable of reading/writing in the WebSocket protocol.
        /// </summary>
        private Stream _socketStream, _webSocketStream;

        public IStreamCipher Encrypter { get; set; }
        public IStreamCipher Decrypter { get; set; }

        public IFormat SendFormat { get; set; }
        public IFormat ReceiveFormat { get; set; }
        public HotelEndPoint RemoteEndPoint { get; private set; }

        public bool IsClient { get; private set; }
        public bool IsUpgraded { get; private set; }
        public bool IsWebSocket { get; private set; }
        public int BypassReceiveSecureTunnel { get; set; }
        public bool IsConnected => !_disposed && _socket.Connected;

        static HNode()
        {
            _rng = new Random();
            _emptyMask = new byte[4];
            _okBytes = Encoding.UTF8.GetBytes("OK");
            _startTLSBytes = Encoding.UTF8.GetBytes("StartTLS");
            _secWebSocketKeyBytes = Encoding.UTF8.GetBytes("Sec-WebSocket-Key: ");
            _rfc6455GuidBytes = Encoding.UTF8.GetBytes("258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
            _upgradeWebSocketResponseBytes = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: ");
        }
        public HNode()
            : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        { }
        public HNode(Socket socket)
            : this(socket, new HotelEndPoint((IPEndPoint)socket.RemoteEndPoint))
        { }
        private HNode(Socket socket, HotelEndPoint remoteEndPoint)
        {
            _socketStream = new BufferedNetworkStream(socket);

            _socket = socket;
            _sendSemaphore = new SemaphoreSlim(1, 1);
            _receiveSemaphore = new SemaphoreSlim(1, 1);
            _packetReceiveSemaphore = new SemaphoreSlim(1, 1);

            socket.NoDelay = true;
            socket.LingerState = new LingerOption(false, 0);

            SendFormat = IFormat.EvaWire;
            ReceiveFormat = IFormat.EvaWire;
            RemoteEndPoint = remoteEndPoint;
        }

        /// <summary>
        /// Sends a mutable packet where if encryption is necessary then the buffer will be overwritten.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public ValueTask<int> SendPacketAsync(Memory<byte> buffer)
        {
            if (SendFormat == null)
            {
                throw new NullReferenceException($"Cannot send structured data while {nameof(SendFormat)} is null.");
            }
            return SendFormat.SendPacketAsync(this, buffer);
        }
        /// <summary>
        /// Sends an immuttable packet where if encryption is necessary then the buffer will be copied.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public ValueTask<int> SendPacketAsync(ReadOnlyMemory<byte> buffer)
        {
            if (SendFormat == null)
            {
                throw new NullReferenceException($"Cannot send structured data while {nameof(SendFormat)} is null.");
            }
            return SendFormat.SendPacketAsync(this, buffer);
        }
        /// <summary>
        /// Receives a packet directly into <paramref name="packetRegion"/>.
        /// </summary>
        public async Task<HPacket> ReceiveRentedPacketAsync()
        {
            if (ReceiveFormat == null)
            {
                throw new NullReferenceException($"Cannot receive structured data while {nameof(ReceiveFormat)} is null.");
            }

            HPacket packetOwner = null;
            await _packetReceiveSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                packetOwner = await ReceiveFormat.ReceiveRentedPacketAsync(this).ConfigureAwait(false);
            }
            finally { _packetReceiveSemaphore.Release(); }

            if (packetOwner == null)
            {
                Dispose();
            }

            return packetOwner;
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
            using IMemoryOwner<byte> initialBytesOwner = Rent(6, out Memory<byte> initialBytesRegion);

            int initialBytesRead = await _socket.ReceiveAsync(initialBytesRegion, SocketFlags.Peek).ConfigureAwait(false);
            ParseInitialBytes(initialBytesRegion.Span.Slice(0, initialBytesRead), out bool isWebSocket, out ushort possibleId);

            bool wasDetermined = true;
            IsWebSocket = isWebSocket;
            if (IsWebSocket || possibleId == 4000)
            {
                SendFormat = IFormat.EvaWire;
                ReceiveFormat = IFormat.EvaWire;
            }
            else if (possibleId == 206)
            {
                throw new NotSupportedException("Wedgie format is currently not supported.");
            }
            else wasDetermined = false;

            return IsWebSocket || wasDetermined;
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

            var secureSocketStream = new SslStream(_socketStream, false, ValidateRemoteCertificate);
            _socketStream = secureSocketStream; // Take ownership of the main input/output stream, and override the field with an SslStream instance that wraps around it.

            await secureSocketStream.AuthenticateAsClientAsync(RemoteEndPoint.Host, null, false).ConfigureAwait(false);
            if (!secureSocketStream.IsAuthenticated) return false;

            string webRequest = $"GET /websocket HTTP/1.1\r\nHost: {RemoteEndPoint}\r\n{requestHeaders}\r\nSec-WebSocket-Key: {GenerateWebSocketKey()}\r\n\r\n";
            await SendPacketAsync(Encoding.UTF8.GetBytes(webRequest)).ConfigureAwait(false);

            using IMemoryOwner<byte> receiveOwner = Rent(256, out Memory<byte> receiveRegion);
            int received = await ReceiveAsync(receiveRegion).ConfigureAwait(false);

            // Create the mask that will be used for the WebSocket payloads.
            _mask = _emptyMask;
            //_rng.NextBytes(_mask);

            IsUpgraded = true;
            _socketStream = _webSocketStream = new WebSocketStream(_socketStream, _mask, false); // Anything now being sent or received through the stream will be parsed using the WebSocket protocol.

            await SendPacketAsync(_startTLSBytes).ConfigureAwait(false);
            received = await ReceiveAsync(receiveRegion).ConfigureAwait(false);
            if (!IsTLSAccepted(receiveRegion.Span.Slice(0, received))) return false;

            // Initialize the second secure tunnel layer where ONLY the WebSocket payload data will be read/written from/to.
            secureSocketStream = new SslStream(_socketStream, false, ValidateRemoteCertificate);
            _socketStream = secureSocketStream; // This stream layer will decrypt/encrypt the payload using the WebSocket protocol.
            await secureSocketStream.AuthenticateAsClientAsync(RemoteEndPoint.Host, null, false).ConfigureAwait(false);
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

            if (IsUpgraded || !await DetermineFormatsAsync().ConfigureAwait(false)) return IsUpgraded;
            using IMemoryOwner<byte> receivedOwner = Rent(1024, out Memory<byte> receivedRegion);
            int received = await ReceiveAsync(receivedRegion).ConfigureAwait(false);

            using IMemoryOwner<byte> responseOwner = Rent(256, out Memory<byte> responseRegion);
            FillWebResponse(receivedRegion.Span.Slice(0, received), responseRegion.Span, out int responseWritten);
            await SendPacketAsync(responseRegion.Slice(0, responseWritten)).ConfigureAwait(false);

            // Begin receiving/sending data as WebSocket frames.
            IsUpgraded = true;
            _socketStream = _webSocketStream = new WebSocketStream(_socketStream);

            received = await ReceiveAsync(receivedRegion).ConfigureAwait(false);
            if (IsTLSRequested(receivedRegion.Span.Slice(0, received)))
            {
                await SendPacketAsync(_okBytes).ConfigureAwait(false);

                var secureSocketStream = new SslStream(_socketStream, false, ValidateRemoteCertificate);
                _socketStream = secureSocketStream;
                await secureSocketStream.AuthenticateAsServerAsync(certificate).ConfigureAwait(false);
            }
            else throw new Exception("The client did not send 'StartTLS'.");
            return IsUpgraded;
        }

        public virtual async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) return -1;
            if (buffer.Length == 0) return 0;

            await _receiveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                Stream tunnel = _socketStream;
                if (BypassReceiveSecureTunnel > 0 && _webSocketStream != null)
                {
                    tunnel = _webSocketStream;
                    BypassReceiveSecureTunnel--;
                }
                return await tunnel.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            catch { return -1; }
            finally { _receiveSemaphore.Release(); }
        }
        public virtual async ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) return -1;
            if (buffer.Length == 0) return 0;

            await _sendSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _socketStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                await _socketStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                return buffer.Length;
            }
            catch { return -1; }
            finally { _sendSemaphore.Release(); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                try { _socketStream.Dispose(); }
                catch { /* The socket doesn't like being shutdown/closed and will throw a fit everytime. */ }
                Encrypter?.Dispose();
                Decrypter?.Dispose();
                _disposed = true;
            }
            _socketStream = null;
            Encrypter = Decrypter = null;
        }

        [DebuggerStepThrough]
        private static IMemoryOwner<byte> Rent(int size, out Memory<byte> trimmedRegion)
        {
            var trimmedOwner = MemoryPool<byte>.Shared.Rent(size);
            trimmedRegion = trimmedOwner.Memory.Slice(0, size);
            return trimmedOwner;
        }
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

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
        public static async Task<HNode> ConnectAsync(HotelEndPoint endPoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await Task.Factory.FromAsync(socket.BeginConnect(endPoint, null, null), socket.EndConnect).ConfigureAwait(false);
            }
            catch { /* Ignore all exceptions. */ }

            if (!socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return null;
            }
            else return new HNode(socket, endPoint);
        }
        public static Task<HNode> ConnectAsync(string host, int port) => ConnectAsync(HotelEndPoint.Parse(host, port));
        public static Task<HNode> ConnectAsync(IPAddress address, int port) => ConnectAsync(new HotelEndPoint(address, port));
        public static Task<HNode> ConnectAsync(IPAddress[] addresses, int port) => ConnectAsync(new HotelEndPoint(addresses[0], port));
        public static Task<HNode> ConnectAsync(IPEndPoint endPoint) => ConnectAsync(endPoint as HotelEndPoint ?? new HotelEndPoint(endPoint));

        private sealed class WebSocketStream : Stream
        {
            private const int MAX_WEBSOCKET_TOCLIENT_PAYLOAD_SIZE = 125;

            private static readonly byte[] _emptyMask = new byte[4];

            private readonly byte[] _mask;
            private readonly bool _isClient;
            private readonly bool _leaveOpen;
            private readonly Stream _innerStream;

            private bool _disposed;

            public WebSocketStream(Stream innerStream)
                : this(innerStream, null, false)
            { }
            public WebSocketStream(Stream innerStream, byte[] mask, bool leaveOpen)
            {
                _mask = mask;
                _leaveOpen = leaveOpen;
                _isClient = mask != null;
                _innerStream = innerStream;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                bool isMasked;
                int op, received, payloadLength;
                using IMemoryOwner<byte> headerOwner = Rent(14, out Memory<byte> headerRegion);
                do
                {
                    received = await _innerStream.ReadAsync(headerRegion.Slice(0, 2), cancellationToken).ConfigureAwait(false);
                    if (received != 2) return -1; // The size of the WebSocket frame header should at minimum be two bytes.
                    HeaderDecode(headerRegion.Span, out isMasked, out payloadLength, out op);
                }
                while (payloadLength == 0 || op != 2); // Continue to receive fragments until a binary frame with a payload size more than zero is found.

                switch (payloadLength)
                {
                    case 126:
                    {
                        received += await _innerStream.ReadAsync(headerRegion.Slice(received, sizeof(ushort)), cancellationToken).ConfigureAwait(false);
                        payloadLength = BinaryPrimitives.ReadUInt16BigEndian(headerRegion.Slice(2, sizeof(ushort)).Span);
                        break;
                    }
                    case 127:
                    {
                        received += await _innerStream.ReadAsync(headerRegion.Slice(received, sizeof(ulong)), cancellationToken).ConfigureAwait(false);
                        payloadLength = (int)BinaryPrimitives.ReadUInt64BigEndian(headerRegion.Slice(2, sizeof(ulong)).Span); // I hope payloads aren't actually this big.
                        break;
                    }
                }

                Memory<byte> maskRegion = null;
                if (isMasked)
                {
                    maskRegion = headerRegion.Slice(received, 4);
                    await _innerStream.ReadAsync(maskRegion, cancellationToken).ConfigureAwait(false);
                }

                received = 0;
                Memory<byte> payloadRegion = buffer.Slice(0, Math.Min(payloadLength, buffer.Length));
                do
                {
                    // Attempt to copy the entire WebSocket payload into the buffer, otherwise, if there is not enough room, specify how much data has been left with the '_leftoverPayloadBytes' field.
                    received += await _innerStream.ReadAsync(payloadRegion.Slice(received, payloadRegion.Length - received), cancellationToken).ConfigureAwait(false);
                }
                while (received != payloadRegion.Length);

                if (isMasked)
                {
                    PayloadUnmask(payloadRegion.Slice(0, received).Span, maskRegion.Span);
                }
                return received;
            }
            public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                using IMemoryOwner<byte> headerOwner = Rent(14, out Memory<byte> headerRegion);
                for (int i = 0, payloadLeft = buffer.Length; payloadLeft > 0; payloadLeft -= MAX_WEBSOCKET_TOCLIENT_PAYLOAD_SIZE, i++)
                {
                    int headerLength = HeaderEncode(headerRegion.Span, payloadLeft, i == 0, _isClient, out bool isFinalFragment);
                    int payloadLength = isFinalFragment ? payloadLeft : MAX_WEBSOCKET_TOCLIENT_PAYLOAD_SIZE;

                    IMemoryOwner<byte> maskedOwner = null;
                    ReadOnlyMemory<byte> payloadFragment = buffer.Slice(i * MAX_WEBSOCKET_TOCLIENT_PAYLOAD_SIZE, payloadLength);

                    await _innerStream.WriteAsync(headerRegion.Slice(0, headerLength), cancellationToken).ConfigureAwait(false);
                    if (_mask != null)
                    {
                        await _innerStream.WriteAsync(_mask, cancellationToken).ConfigureAwait(false);
                        maskedOwner = PayloadMask(payloadFragment.Span, _mask);
                    }

                    if (maskedOwner != null) // MaskPayload could return null, which means that we used the '_emptyMask' instance, WHICH also means no masking was done on the payload.
                    {
                        await _innerStream.WriteAsync(maskedOwner.Memory.Slice(0, payloadFragment.Length), cancellationToken).ConfigureAwait(false);
                        maskedOwner.Dispose();
                    }
                    else await _innerStream.WriteAsync(payloadFragment, cancellationToken).ConfigureAwait(false);

                    if (_isClient) break;
                }
            }

            #region Stream Implementation
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();

            public override void Flush() => _innerStream.Flush();
            public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing && !_disposed && !_leaveOpen)
                {
                    _innerStream.Dispose();
                    _disposed = true;
                }
            }
            #endregion

            private static void PayloadUnmask(Span<byte> payload, Span<byte> mask)
            {
                for (int i = 0; i < payload.Length; i++)
                {
                    payload[i] ^= mask[i % 4];
                }
            }
            private static IMemoryOwner<byte> PayloadMask(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> mask)
            {
                if (mask == _emptyMask) return null;
                IMemoryOwner<byte> maskedOwner = Rent(payload.Length, out Memory<byte> maskedRegion);

                Span<byte> masked = maskedRegion.Span;
                for (int i = 0; i < payload.Length; i++)
                {
                    masked[i] = (byte)(payload[i] ^ mask[i % 4]);
                }
                return maskedOwner;
            }

            [DebuggerStepThrough]
            private static void HeaderDecode(Span<byte> header, out bool isMasked, out int payloadLength, out int op)
            {
                op = header[0] & 0b00001111;
                payloadLength = header[1] & 0x7F;
                isMasked = (header[1] & 0x80) == 0x80;
            }
            private static int HeaderEncode(Span<byte> header, int payloadLeft, bool isFirstFragment, bool isClient, out bool isFinalFragment)
            {
                isFinalFragment = isClient || payloadLeft <= MAX_WEBSOCKET_TOCLIENT_PAYLOAD_SIZE;

                int headerBits = isFinalFragment ? 1 : 0;
                headerBits = (headerBits << 1) + 0; // RSV1 - IsDataCompressed
                headerBits = (headerBits << 1) + 0; // RSV2
                headerBits = (headerBits << 1) + 0; // RSV3
                headerBits = (headerBits << 4) + (isFirstFragment ? 2 : 0); // Mark as binary, otherwise continuation
                headerBits = (headerBits << 1) + (isClient ? 1 : 0); // Payload should be masked when acting as the client, otherwise specify that no mask is present(0).

                int headerLength = 2;
                int payloadLength = isFinalFragment ? payloadLeft : MAX_WEBSOCKET_TOCLIENT_PAYLOAD_SIZE;
                if (isClient && payloadLeft > 125) // Fragmenting the payload is not necessary when sending data to the server.
                {
                    if (payloadLeft <= ushort.MaxValue)
                    {
                        payloadLength = 126;
                        headerLength += sizeof(ushort);
                        BinaryPrimitives.WriteUInt16BigEndian(header.Slice(2), (ushort)payloadLeft);
                    }
                    else
                    {
                        payloadLength = 127;
                        headerLength += sizeof(ulong);
                        BinaryPrimitives.WriteUInt64BigEndian(header.Slice(2), (ulong)payloadLeft);
                    }
                }

                headerBits = (headerBits << 7) + payloadLength;
                BinaryPrimitives.WriteUInt16BigEndian(header, (ushort)headerBits);

                return headerLength;
            }
        }
        private sealed class BufferedNetworkStream : Stream
        {
            private const int DEFAULT_BUFFER_SIZE = 4096;

            private readonly NetworkStream _networkStream;
            private readonly BufferedStream _readBuffer, _writeBuffer;

            private bool _disposed;

            public int ReadBufferSize { get; init; }
            public int WriteBufferSize { get; init; }

            public BufferedNetworkStream(Socket socket)
                : this(socket, DEFAULT_BUFFER_SIZE, DEFAULT_BUFFER_SIZE)
            { }
            public BufferedNetworkStream(Socket socket, int readBufferSize, int writeBufferSize)
            {
                ReadBufferSize = readBufferSize;
                WriteBufferSize = writeBufferSize;

                _networkStream = new NetworkStream(socket, true);
                _readBuffer = new BufferedStream(_networkStream, readBufferSize);
                _writeBuffer = new BufferedStream(_networkStream, writeBufferSize);
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _readBuffer.ReadAsync(buffer, cancellationToken);
            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _writeBuffer.WriteAsync(buffer, cancellationToken);

            #region Stream Implementation
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();

            public override void Flush() => _writeBuffer.Flush();
            public override Task FlushAsync(CancellationToken cancellationToken) => _writeBuffer.FlushAsync(cancellationToken);

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing && _disposed)
                {
                    _networkStream.Dispose();
                    _readBuffer.Dispose();
                    _writeBuffer.Dispose();
                    _disposed = true;
                }
            }
            #endregion
        }
    }
}