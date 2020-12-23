using System;
using System.Text;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sulakore.Network.Protocol
{
    public class EvaWireFormat : HFormat
    {
        public override int IdPosition => 4;
        public override string Name => "EVAWIRE";

        public EvaWireFormat()
            : base(false)
        { }

        public override ushort GetId(IList<byte> data)
        {
            return ReadUInt16(data, 4);
        }
        public override byte[] GetBody(IList<byte> data)
        {
            var body = new byte[data.Count - 6];
            for (int i = 0; i < body.Length; i++)
            {
                body[i] = data[i + 6];
            }
            return body;
        }

        public override int GetSize(int value) => 4;
        public override int GetSize(bool value) => 1;
        public override int GetSize(string value) => 2 + Encoding.UTF8.GetByteCount(value);
        public override int GetSize(ushort value) => 2;
        public override int GetSize(double value) => 6;

        public override byte[] GetBytes(int value)
        {
            var data = new byte[4];
            data[0] = (byte)(value >> 24);
            data[1] = (byte)(value >> 16);
            data[2] = (byte)(value >> 8);
            data[3] = (byte)value;
            return data;
        }
        public override byte[] GetBytes(bool value)
        {
            return new byte[] { (byte)(value ? 1 : 0) };
        }
        public override byte[] GetBytes(string value)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(value);
            byte[] lengthData = GetBytes((ushort)stringData.Length);

            var data = new byte[lengthData.Length + stringData.Length];
            Buffer.BlockCopy(lengthData, 0, data, 0, lengthData.Length);
            Buffer.BlockCopy(stringData, 0, data, lengthData.Length, stringData.Length);
            return data;
        }
        public override byte[] GetBytes(ushort value)
        {
            var data = new byte[2];
            data[0] = (byte)(value >> 8);
            data[1] = (byte)value;
            return data;
        }
        public override byte[] GetBytes(double value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return data;
        }

        public override int ReadInt32(IList<byte> data, int index)
        {
            int result = data[index++] << 24;
            result += data[index++] << 16;
            result += data[index++] << 8;
            return result + data[index];
        }
        public override string ReadUTF8(IList<byte> data, int index)
        {
            ushort length = ReadUInt16(data, index);
            index += 2;

            var chunk = new byte[length];
            for (int i = index, j = 0; j < length; i++, j++)
            {
                chunk[j] = data[i];
            }
            return Encoding.UTF8.GetString(chunk, 0, length);
        }
        public override bool ReadBoolean(IList<byte> data, int index)
        {
            return data[index] == 1;
        }
        public override ushort ReadUInt16(IList<byte> data, int index)
        {
            int result = data[index++] << 8;
            return (ushort)(result + data[index]);
        }
        public override double ReadDouble(IList<byte> data, int index)
        {
            var chunk = new byte[data.Count - index];
            for (int i = chunk.Length - 1, j = index + chunk.Length; i >= 0; i--, j--)
            {
                chunk[i] = data[i];
            }
            return BitConverter.ToDouble(chunk, 0);
        }

        protected override byte[] ConstructTails(ushort id, IList<byte> body)
        {
            var data = new byte[6 + body.Count];
            PlaceBytes(body.Count + 2, data, 0);
            PlaceBytes(id, data, 4);

            body.CopyTo(data, 6);
            return data;
        }
        public override async ValueTask<HPacket> ReceivePacketAsync(HNode node)
        {
            using IMemoryOwner<byte> lengthOwner = MemoryPool<byte>.Shared.Rent(4);

            int lengthReceived = 0;
            do lengthReceived = await node.ReceiveAsync(lengthOwner.Memory.Slice(0, 4)).ConfigureAwait(false);
            while (lengthReceived == 0);

            if (lengthReceived != 4) node.Dispose();
            int length = BinaryPrimitives.ReadInt32BigEndian(lengthOwner.Memory.Span);

            using IMemoryOwner<byte> bodyOwner = MemoryPool<byte>.Shared.Rent(length);
            Memory<byte> body = bodyOwner.Memory.Slice(0, length);

            int received = 0;
            do received += await node.ReceiveAsync(bodyOwner.Memory.Slice(received, length - received));
            while (received != length);

            var packetData = new byte[sizeof(int) + length];
            BinaryPrimitives.WriteInt32BigEndian(packetData, length);
            Buffer.BlockCopy(bodyOwner.Memory.ToArray(), 0, packetData, 4, length);

            if (node.IsWebSocket && node.Decrypter != null)
            {
                node.Decrypter.Process(packetData.AsSpan().Slice(5, 1));
                node.Decrypter.Process(packetData.AsSpan().Slice(4, 1));
            }
            return CreatePacket(packetData);
        }

        public override HPacket CreatePacket()
        {
            return new EvaWirePacket();
        }
        public override HPacket CreatePacket(IList<byte> data)
        {
            return new EvaWirePacket(data);
        }
        public override HPacket CreatePacket(ushort id, params object[] values)
        {
            return new EvaWirePacket(id, values);
        }
    }
}