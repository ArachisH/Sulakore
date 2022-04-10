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

using Sulakore.Network.Formats;
using Sulakore.Network.Buffers;
using Sulakore.Cryptography.Ciphers;

namespace Sulakore.Network;

public sealed class HNode : IDisposable
{
    private static readonly Random _rng;
    private static readonly byte[] _emptyMask;
    private static readonly byte[] _okBytes, _startTLSBytes;
    private static readonly byte[] _upgradeWebSocketResponseBytes;
    private static readonly byte[] _rfc6455GuidBytes, _secWebSocketKeyBytes;

    private readonly Socket _socket;
    private readonly SemaphoreSlim _sendSemaphore, _receiveSemaphore, _packetReceiveSemaphore;

    private byte[] _mask;
    private bool _disposed;
    private Stream _socketStream, _webSocketStream;

    public IStreamCipher Encrypter { get; set; }
    public IStreamCipher Decrypter { get; set; }
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

        RemoteEndPoint = remoteEndPoint;
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
        using IMemoryOwner<byte> initialBytesOwner = BufferHelper.Rent(6, out Memory<byte> initialBytesRegion);

        int initialBytesRead = await _socket.ReceiveAsync(initialBytesRegion, SocketFlags.Peek).ConfigureAwait(false);
        ParseInitialBytes(initialBytesRegion.Span.Slice(0, initialBytesRead), out bool isWebSocket, out ushort possibleId);

        bool wasDetermined = true;
        IsWebSocket = isWebSocket;
        if (IsWebSocket || possibleId == 4000)
        {
            //SendFormat = HFormat.EvaWire;
            //ReceiveFormat = HFormat.EvaWire;
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
        await SendAsync(Encoding.UTF8.GetBytes(webRequest)).ConfigureAwait(false);

        using IMemoryOwner<byte> receiveOwner = BufferHelper.Rent(256, out Memory<byte> receiveRegion);
        int received = await ReceiveAsync(receiveRegion).ConfigureAwait(false);

        // Create the mask that will be used for the WebSocket payloads.
        _mask = _emptyMask;
        //_rng.NextBytes(_mask);

        IsUpgraded = true;
        _socketStream = _webSocketStream = new WebSocketStream(_socketStream, _mask, false); // Anything now being sent or received through the stream will be parsed using the WebSocket protocol.

        await SendAsync(_startTLSBytes).ConfigureAwait(false);
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
        using IMemoryOwner<byte> receivedOwner = BufferHelper.Rent(1024, out Memory<byte> receivedRegion);
        int received = await ReceiveAsync(receivedRegion).ConfigureAwait(false);

        using IMemoryOwner<byte> responseOwner = BufferHelper.Rent(256, out Memory<byte> responseRegion);
        FillWebResponse(receivedRegion.Span.Slice(0, received), responseRegion.Span, out int responseWritten);
        await SendAsync(responseRegion.Slice(0, responseWritten)).ConfigureAwait(false);

        // Begin receiving/sending data as WebSocket frames.
        IsUpgraded = true;
        _socketStream = _webSocketStream = new WebSocketStream(_socketStream);

        received = await ReceiveAsync(receivedRegion).ConfigureAwait(false);
        if (IsTLSRequested(receivedRegion.Span.Slice(0, received)))
        {
            await SendAsync(_okBytes).ConfigureAwait(false);

            var secureSocketStream = new SslStream(_socketStream, false, ValidateRemoteCertificate);
            _socketStream = secureSocketStream;

            await secureSocketStream.AuthenticateAsServerAsync(certificate).ConfigureAwait(false);
        }
        else throw new Exception("The client did not send 'StartTLS'.");
        return IsUpgraded;
    }

    public async Task SendPacketAsync(HPacket packet, CancellationToken cancellationToken = default)
    {
        Memory<byte> buffer = packet.Buffer.Slice(0, packet.Length + packet.Format.MinBufferSize - packet.Format.MinPacketLength);
        Encipher(Encrypter, buffer.Span, IsWebSocket);

        await SendAsync(buffer, cancellationToken).ConfigureAwait(false);
    }
    public async Task<HPacket> ReceivePacketAsync(IHFormat format, CancellationToken cancellationToken = default)
    {
        if (format == null)
        {
            throw new ArgumentNullException(nameof(format));
        }

        HPacket packet = null;
        await _packetReceiveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            IMemoryOwner<byte> owner = null;
            Memory<byte> buffer = new byte[format.MinBufferSize];

            int received;
            do received = await ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            while (received == 0);

            // Did the first buffer receive meet the minimum requirements?
            if (received != buffer.Length || !format.TryReadHeader(buffer.Span, out int length, out short id, out _)) return null;

            int fullPacketLength = format.MinBufferSize - format.MinPacketLength + length;
            if (fullPacketLength > buffer.Length) // Should the buffer be enlarged to ensure it fits the full packet?
            {
                Memory<byte> enlargedBuffer;
                if (fullPacketLength > HPacket.MAX_ALLOC_SIZE)
                {
                    owner = BufferHelper.Rent(fullPacketLength, out enlargedBuffer);
                }
                else enlargedBuffer = new byte[fullPacketLength];

                buffer.CopyTo(enlargedBuffer);
                buffer = enlargedBuffer;
            }
            else if (length == -1) { /* TODO: Assume the packet ends with null/0 ?? */ }

            while (received < fullPacketLength)
            {
                received += await ReceiveAsync(buffer[received..], cancellationToken).ConfigureAwait(false);
            }

            if (received != fullPacketLength)
            {
                /* TODO: What's going on? */
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            if (Decrypter != null)
            {
                Encipher(Decrypter, buffer.Span, IsWebSocket);
            }

            packet = new HPacket(format, id, length, buffer, owner);
        }
        finally { _packetReceiveSemaphore.Release(); }
        return packet;
    }

    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
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
    public async ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
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
        if (!_disposed)
        {
            try { _socketStream.Dispose(); }
            catch { /* The socket doesn't like being shutdown/closed and will throw a fit everytime. */ }
            Encrypter?.Dispose();
            Decrypter?.Dispose();
            _disposed = true;
        }
        _socketStream = null;
        Encrypter = Decrypter = null;
        GC.SuppressFinalize(this);
    }

    private static void Encipher(IStreamCipher cipher, Span<byte> buffer, bool isWebSocket)
    {
        if (isWebSocket)
        {
            // Reverse the packet id and encrypt/decrypt it.
            Span<byte> idBuffer = stackalloc byte[2] { buffer[5], buffer[4] };
            cipher.Process(idBuffer);

            // After encryption/decryption, then reverse it back and place it on the original buffer.
            buffer[4] = idBuffer[1];
            buffer[5] = idBuffer[0];
        }
        else cipher.Process(buffer);
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
}