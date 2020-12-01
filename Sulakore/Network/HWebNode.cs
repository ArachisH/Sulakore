using System;
using System.Buffers;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Buffers.Binary;
using System.Threading.Tasks;

using Sulakore.Network.Protocol;
using System.Net;

namespace Sulakore.Network
{
    // TODO: This class will replace HNode, implement HMessage reading/writing.
    public class HWebNode : NetworkStream
    {
        private const int GET_MAGIC = 542393671;
        private const int MAX_FRAME_PAYLOAD_SIZE = 125;
        private const string RFC6455_GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private bool _disposed, _hasUpgraded;
        private SslStream _securePayloadLayer;
        private readonly int _payloadLeftovers;

        private readonly SemaphoreSlim _sendSemaphore;

        public int Sent { get; private set; }
        public HFormat SendFormat { get; private set; }

        public int Received { get; private set; }
        public HFormat ReceiveFormat { get; private set; }

        public HotelEndPoint EndPoint { get; }

        public bool IsWebSocket { get; private set; }
        public bool IsConnected => !_disposed && Socket.Connected;

        public HWebNode(Socket socket)
            : base(socket, false)
        {
            _sendSemaphore = new SemaphoreSlim(1, 1);

            socket.NoDelay = true;
            socket.LingerState = new LingerOption(false, 0);
            if (socket.RemoteEndPoint is IPEndPoint ipEndPoint)
            {
                EndPoint = new HotelEndPoint(ipEndPoint);
            }
        }

        /* TODO: Implement AuthenticateAs(Client/Server) with their respective overloads.
         * Create NetworkStream instance and pass our Socket object to it, so that we can pass data to the SslStream  without encrypting our WebSocket frame headers. */

        public bool ReflectFormats(HWebNode local)
        {
            SendFormat = local.ReceiveFormat;
            ReceiveFormat = local.SendFormat;
            return IsWebSocket = local.IsWebSocket;
        }
        public async Task UpgradeWebSocketAsync()
        {
            // TODO:
            //byte[] possibleGET = await PeekAsync(4).ConfigureAwait(false);
            //if (BinaryPrimitives.ReadInt32LittleEndian(possibleGET) != GET_MAGIC) return;

            //byte[] requestData = await ReceiveAsync(1024).ConfigureAwait(false);
            //string[] requestHeaderLines = Encoding.UTF8.GetString(requestData, 0, requestData.Length - 4).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            //string webSocketKey = null;
            //foreach (string headerLine in requestHeaderLines)
            //{
            //    if (!headerLine.StartsWith("Sec-WebSocket-Key", StringComparison.OrdinalIgnoreCase)) continue;
            //    webSocketKey = headerLine[19..];
            //}

            //byte[] dataToHash = Encoding.UTF8.GetBytes(webSocketKey + RFC6455_GUID);
            //using SHA1 algo = SHA1.Create();
            //string hashedKey = Convert.ToBase64String(algo.ComputeHash(dataToHash));

            //var responseHeaderLinesBuilder = new StringBuilder();
            //responseHeaderLinesBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
            //responseHeaderLinesBuilder.AppendLine("Connection: Upgrade");
            //responseHeaderLinesBuilder.AppendLine("Upgrade: websocket");
            //responseHeaderLinesBuilder.AppendLine($"Sec-WebSocket-Accept: {hashedKey}");
            //responseHeaderLinesBuilder.AppendLine();

            //byte[] responseData = Encoding.UTF8.GetBytes(responseHeaderLinesBuilder.ToString());
            //await base.SendAsync(responseData, 0, responseData.Length, SocketFlags.None).ConfigureAwait(false);
        }
        public async Task<bool> DetermineFormatsAsync()
        {
            // Connection type can only be determined in the beginning. ("GET"/WebSocket/EvaWire, '4000'/Raw/EvaWire, '206'/Raw/Wedgie)
            if (Received > 0) return IsWebSocket;

            using IMemoryOwner<byte> initialBytesRegion = MemoryPool<byte>.Shared.Rent(6);
            initialBytesRegion.Memory.Span.Fill(0);

            await Socket.ReceiveAsync(initialBytesRegion.Memory, SocketFlags.Peek).ConfigureAwait(false);
            IsWebSocket = BinaryPrimitives.ReadInt32LittleEndian(initialBytesRegion.Memory.Span) == GET_MAGIC;
            ushort possibleId = BinaryPrimitives.ReadUInt16BigEndian(initialBytesRegion.Memory.Span.Slice(4, 2));

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

        public virtual async ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int sent = 0;
            if (IsConnected && buffer.Length > 0)
            {
                await _sendSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    // TODO: Encrypt
                    if (IsWebSocket && _hasUpgraded)
                    {
                        sent = await WrapWebSocketFramesAsync(buffer, cancellationToken).ConfigureAwait(false);
                    }
                    else sent = await Socket.SendAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                }
                catch { sent = -1; }
                finally { _sendSemaphore.Release(); }
            }
            if (Sent > 0)
            {
                Sent += sent;
            }
            return sent;
        }
        public virtual async ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
            int received = 0;
            if (IsConnected && buffer.Length > 0)
            {
                try
                {
                    if (IsWebSocket && _hasUpgraded)
                    {
                        received = await UnwrapWebSocketFramesAsync(buffer, cancellationToken).ConfigureAwait(false);
                    }
                    else received = await Socket.ReceiveAsync(buffer, socketFlags, cancellationToken).ConfigureAwait(false);
                    // TODO: Decrypt
                }
                catch { received = -1; }
            }
            if (received > 0)
            {
                Received += received;
            }
            return received;
        }
        public virtual ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

        public async Task<byte[]> AttemptReceiveAsync(int size, int attempts)
        {
            var data = new byte[size];
            int totalBytesRead = 0, nullBytesReadCount = 0;
            var dataRegion = new Memory<byte>(data, 0, size);
            do
            {
                int bytesLeft = data.Length - totalBytesRead;
                int bytesRead = await ReceiveAsync(dataRegion.Slice(totalBytesRead, bytesLeft), SocketFlags.None).ConfigureAwait(false);

                if (IsConnected && bytesRead > 0)
                {
                    nullBytesReadCount = 0;
                    totalBytesRead += bytesRead;
                }
                else if (!IsConnected || ++nullBytesReadCount >= attempts)
                {
                    return null;
                }
            }
            while (totalBytesRead != data.Length);
            return data;
        }
        protected async Task<byte[]> ReceiveAndTrimAsync(int size, SocketFlags socketFlags)
        {
            using IMemoryOwner<byte> bufferOwner = MemoryPool<byte>.Shared.Rent(size);
            int read = await ReceiveAsync(bufferOwner.Memory, socketFlags).ConfigureAwait(false);
            if (read < 1)
            {
                return Array.Empty<byte>();
            }

            var data = new byte[read];
            bufferOwner.Memory.CopyTo(data);
            return data;
        }

        #region NetworkStream Overrides
        public override int Read(byte[] buffer, int offset, int size)
        {
            return ReadAsync(buffer, offset, size, CancellationToken.None).GetAwaiter().GetResult();
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int size, CancellationToken cancellationToken)
        {
            Memory<byte> bufferMemory = buffer.AsMemory(offset, size);
            return await ReceiveAsync(bufferMemory, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            WriteAsync(buffer, offset, size, CancellationToken.None).GetAwaiter().GetResult();
        }
        public override async Task WriteAsync(byte[] buffer, int offset, int size, CancellationToken cancellationToken)
        {
            Memory<byte> bufferMemory = buffer.AsMemory(offset, size);
            await SendAsync(bufferMemory, cancellationToken).ConfigureAwait(false);
        }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await SendAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return base.EndRead(asyncResult);
        }
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return base.BeginRead(buffer, offset, size, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.EndWrite(asyncResult);
        }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return base.BeginWrite(buffer, offset, size, callback, state);
        }
        #endregion

        protected virtual ValueTask<int> ReceivePayloadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_securePayloadLayer != null)
            {
                return _securePayloadLayer.ReadAsync(buffer, cancellationToken);
            }
            else return Socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
        protected virtual async ValueTask<int> SendPayloadAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_securePayloadLayer != null)
            {
                await _securePayloadLayer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                return buffer.Length;
            }
            else return await Socket.SendAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        }
        protected virtual async ValueTask<int> UnwrapWebSocketFramesAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            using IMemoryOwner<byte> headerRegionOwner = MemoryPool<byte>.Shared.Rent(14);
            int headerRead = await Socket.ReceiveAsync(headerRegionOwner.Memory, SocketFlags.None, cancellationToken).ConfigureAwait(false);

            bool isFinalFrame = (headerRegionOwner.Memory.Span[0] & 0x80) == 0x80;
            bool isMasking = (headerRegionOwner.Memory.Span[1] & 0x80) == 0x80;
            var payloadSize = (ulong)headerRegionOwner.Memory.Span[1] & 0x7F;

            if (payloadSize == 126)
            {
                headerRead += await Socket.ReceiveAsync(headerRegionOwner.Memory.Slice(2, sizeof(ushort)), SocketFlags.None, cancellationToken).ConfigureAwait(false);
                payloadSize = BinaryPrimitives.ReadUInt16BigEndian(headerRegionOwner.Memory.Span.Slice(2));
            }
            else if (payloadSize == 127)
            {
                headerRead += await Socket.ReceiveAsync(headerRegionOwner.Memory.Slice(2, sizeof(ulong)), SocketFlags.None, cancellationToken).ConfigureAwait(false);
                payloadSize = BinaryPrimitives.ReadUInt64BigEndian(headerRegionOwner.Memory.Span.Slice(2));
            }

            if (isMasking)
            {
                headerRead += await Socket.ReceiveAsync(headerRegionOwner.Memory.Slice(headerRead, 4), SocketFlags.None, cancellationToken).ConfigureAwait(false);
            }

            int payloadRead = await ReceivePayloadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (isMasking)
            {
                Memory<byte> maskRegion = headerRegionOwner.Memory.Slice(headerRead - 4, 4);
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer.Span[i] ^= maskRegion.Span[i % 4];
                }
            }

            if (isFinalFrame)
            {
                System.Diagnostics.Debugger.Break();
            }
            if (payloadRead != buffer.Length)
            {
                System.Diagnostics.Debugger.Break();
            }

            Received += headerRead;
            return payloadRead;
        }
        protected virtual async ValueTask<int> WrapWebSocketFramesAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int dataSent = 0;
            using IMemoryOwner<byte> headerRegionOwner = MemoryPool<byte>.Shared.Rent(2);
            for (int i = 0, payloadLeft = buffer.Length; payloadLeft > 0; payloadLeft -= MAX_FRAME_PAYLOAD_SIZE)
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

                BinaryPrimitives.WriteUInt16BigEndian(headerRegionOwner.Memory.Span, (ushort)header);
                dataSent += await Socket.SendAsync(headerRegionOwner.Memory.Slice(0, 2), SocketFlags.None, cancellationToken: cancellationToken).ConfigureAwait(false);
                dataSent += await SendPayloadAsync(buffer.Slice(payloadStart, payloadSize), cancellationToken).ConfigureAwait(false);
            }
            return dataSent;
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            if (!_disposed)
            {
                _disposed = true;
                try
                {
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
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Disconnect(false);
                    Socket.Close(0);
                }
                catch { }
            }
        }

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
            else return new HNode(socket);
        }
        public static Task<HNode> ConnectAsync(string host, int port) => ConnectAsync(HotelEndPoint.Parse(host, port));
        public static Task<HNode> ConnectAsync(IPAddress address, int port) => ConnectAsync(new HotelEndPoint(address, port));
        public static Task<HNode> ConnectAsync(IPAddress[] addresses, int port) => ConnectAsync(new HotelEndPoint(addresses[0], port));
    }
}