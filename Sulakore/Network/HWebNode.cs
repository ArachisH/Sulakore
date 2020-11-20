using System;
using System.Text;
using System.Buffers;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Sulakore.Network
{
    public class HWebNode : HNode
    {
        private readonly bool _isServer;

        private const int MAX_FRAME_SIZE = 125;
        private const int GET_MAGIC_INT32 = 542393671;
        private const string RFC6455_GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public HWebNode()
            : base()
        { }
        public HWebNode(Socket client)
            : base(client)
        { }

        public async Task HandshakeAsServer()
        {
            byte[] possibleGET = await PeekAsync(4).ConfigureAwait(false);
            if (BinaryPrimitives.ReadInt32LittleEndian(possibleGET) != GET_MAGIC_INT32) return;

            byte[] requestData = await ReceiveAsync(1024).ConfigureAwait(false);
            string[] requestHeaderLines = Encoding.UTF8.GetString(requestData, 0, requestData.Length - 4).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            string webSocketKey = null;
            foreach (string headerLine in requestHeaderLines)
            {
                if (!headerLine.StartsWith("Sec-WebSocket-Key", StringComparison.OrdinalIgnoreCase)) continue;
                webSocketKey = headerLine[19..];
            }

            byte[] dataToHash = Encoding.UTF8.GetBytes(webSocketKey + RFC6455_GUID);
            using SHA1 algo = SHA1.Create();
            string hashedKey = Convert.ToBase64String(algo.ComputeHash(dataToHash));

            var responseHeaderLinesBuilder = new StringBuilder();
            responseHeaderLinesBuilder.AppendLine("HTTP/1.1 101 Switching Protocols");
            responseHeaderLinesBuilder.AppendLine("Connection: Upgrade");
            responseHeaderLinesBuilder.AppendLine("Upgrade: websocket");
            responseHeaderLinesBuilder.AppendLine($"Sec-WebSocket-Accept: {hashedKey}");
            responseHeaderLinesBuilder.AppendLine();

            byte[] responseData = Encoding.UTF8.GetBytes(responseHeaderLinesBuilder.ToString());
            await base.SendAsync(responseData, 0, responseData.Length, SocketFlags.None).ConfigureAwait(false);
        }

        public Task<int> SendTextAsync(string message)
        {
            byte[] messageData = Encoding.UTF8.GetBytes(message);
            return SendWebMessageAsync(messageData, 0, messageData.Length, true, SocketFlags.None);
        }
        public Task<int> SendBinaryAsync(string message) => SendBinaryAsync(Encoding.UTF8.GetBytes(message));
        public Task<int> SendBinaryAsync(byte[] binary) => SendWebMessageAsync(binary, 0, binary.Length, false, SocketFlags.None);

        public async Task<string> ReceiveTextAsync()
        {
            byte[] binary = await ReceiveWebMessageAsync(SocketFlags.None).ConfigureAwait(false);
            return Encoding.UTF8.GetString(binary);
        }
        public Task<byte[]> ReceiveBinaryAsync() => ReceiveWebMessageAsync(SocketFlags.None);

        protected virtual async Task<byte[]> ReceiveWebMessageAsync(SocketFlags socketFlags)
        {
            byte[] headerData = ArrayPool<byte>.Shared.Rent(14);
            int headerBytesRead = await ReceiveAsync(headerData, 0, 2, socketFlags).ConfigureAwait(false);

            bool isFinalFrame = (headerData[0] & 0x80) == 0x80;
            byte op = (byte)(headerData[0] & 0x0F);
            bool isMasking = (headerData[1] & 0x80) == 0x80;
            var framePayloadLength = (ulong)headerData[1] & 0x7F;

            if (!isFinalFrame)
            {
                System.Diagnostics.Debugger.Break();
            }

            if (op != 0x02) // Not a binary message.
            {
                System.Diagnostics.Debugger.Break();
            }

            if (framePayloadLength == 126)
            {
                headerBytesRead += await ReceiveAsync(headerData, headerBytesRead, sizeof(ushort), socketFlags).ConfigureAwait(false);
                framePayloadLength = BinaryPrimitives.ReadUInt16BigEndian(headerData.AsSpan(2, sizeof(ushort)));
            }
            else if (framePayloadLength == 127)
            {
                headerBytesRead += await ReceiveAsync(headerData, headerBytesRead, sizeof(ulong), socketFlags).ConfigureAwait(false);
                framePayloadLength = BinaryPrimitives.ReadUInt64BigEndian(headerData.AsSpan(2, sizeof(ulong)));
            }

            if (isMasking)
            {
                headerBytesRead += await ReceiveAsync(headerData, headerBytesRead, 4).ConfigureAwait(false);
            }

            var framePayload = new byte[framePayloadLength];
            int bodyRead = await ReceiveAsync(framePayload, 0, framePayload.Length, socketFlags).ConfigureAwait(false);

            if (bodyRead != framePayload.Length)
            {
                System.Diagnostics.Debugger.Break();
            }

            if (isMasking)
            {
                for (int i = 0; i < framePayload.Length; i++)
                {
                    framePayload[i] ^= headerData[(i % 4) + headerBytesRead - 4];
                }
            }

            byte[] applicationData = null;
            if (!isFinalFrame)
            {
                byte[] nextFramePayload = await ReceiveWebMessageAsync(socketFlags).ConfigureAwait(false);
                applicationData = new byte[framePayload.Length + nextFramePayload.Length];

                Buffer.BlockCopy(framePayload, 0, applicationData, 0, framePayload.Length);
                Buffer.BlockCopy(nextFramePayload, 0, applicationData, framePayload.Length, nextFramePayload.Length);
            }
            return applicationData ?? framePayload;
        }
        protected virtual async Task<int> SendWebMessageAsync(byte[] buffer, int offset, int size, bool isText, SocketFlags socketFlags)
        {
            int dataSent = 0;
            byte[] headerBuffer = ArrayPool<byte>.Shared.Rent(2);
            double totalFrames = Math.Ceiling(size / (float)MAX_FRAME_SIZE);
            for (int i = 0; i < totalFrames; i++)
            {
                bool isFinalFrame = i + 1 == totalFrames;
                int frameStart = offset + (i * MAX_FRAME_SIZE);
                int frameSize = (isFinalFrame ? size - frameStart : MAX_FRAME_SIZE);

                int header = isFinalFrame ? 1 : 0;
                header = (header << 1) + 0; // RSV1 - IsDataCompressed
                header = (header << 1) + 0; // RSV2
                header = (header << 1) + 0; // RSV3
                header = (header << 4) + (i == 0 ? (isText ? 1 : 2) : 0); // If this is the first frame, mark it as a BINARY, otherwise CONTINUATION.
                header = (header << 1) + 0;
                header = (header << 7) + frameSize;

                BinaryPrimitives.WriteUInt16BigEndian(headerBuffer, (ushort)header);
                dataSent += await SendAsync(headerBuffer, 0, 2, socketFlags).ConfigureAwait(false);
                dataSent += await SendAsync(buffer, frameStart, frameSize, socketFlags).ConfigureAwait(false);
            }
            ArrayPool<byte>.Shared.Return(headerBuffer);
            return dataSent;
        }
    }
}