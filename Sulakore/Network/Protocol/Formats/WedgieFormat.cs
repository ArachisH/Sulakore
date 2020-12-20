using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sulakore.Network.Protocol
{
    public class WedgieFormat : HFormat
    {
        private readonly Dictionary<HNode, List<byte>> _dataCrumbs;

        public override int IdPosition { get; }
        public override string Name => IsOutgoing ? "WEDGIE-OUT" : "WEDGIE-IN";

        public WedgieFormat(bool isOutgoing)
            : base(isOutgoing)
        {
            _dataCrumbs = new Dictionary<HNode, List<byte>>();

            IdPosition = isOutgoing ? 3 : 0;
        }

        public override ushort GetId(IList<byte> data)
        {
            int idIndex = IsOutgoing ? 3 : 0;
            int result = ReadUnmaskedByte(data, ref idIndex);

            result = (result << 6) | ReadUnmaskedByte(data, ref idIndex);
            return (ushort)result;
        }
        public override byte[] GetBody(IList<byte> data)
        {
            int bodyStart = IsOutgoing ? 5 : 2;
            var body = new byte[(data.Count - (IsOutgoing ? 5 : 3))];
            for (int i = 0; i < body.Length; i++)
            {
                body[i] = data[i + bodyStart];
            }
            return body;
        }

        public override int GetSize(int value)
        {
            value = Math.Abs(value);
            value >>= 2;

            int length = 1;
            while (value != 0)
            {
                length++;
                value >>= 6;
            }
            return length;
        }
        public override int GetSize(bool value) => 1;

        public override int GetSize(string value)
        {
            int valueSize = Encoding.UTF8.GetByteCount(value);
            valueSize += IsOutgoing ? GetSize(valueSize) : 1;

            return valueSize;
        }
        public override int GetSize(ushort value) => GetSize((int)value);
        public override int GetSize(double value)
        {
            throw new NotSupportedException();
        }

        public override byte[] GetBytes(int value)
        {
            int mask = (value < 0) ? 4 : 0;
            value = Math.Abs(value);

            var buffer = new byte[6];
            buffer[0] = (byte)(64 | (value & 3));
            value >>= 2;

            int length = 1;
            for (int i = 1; value != 0; i++, length++, value >>= 6)
            {
                buffer[i] = (byte)(64 | (value & 63));
            }
            buffer[0] |= (byte)((length << 3) | mask);

            if (length != buffer.Length)
            {
                var data = new byte[length];
                Buffer.BlockCopy(buffer, 0, data, 0, length);

                buffer = data;
            }
            return buffer;
        }
        public override byte[] GetBytes(bool value)
        {
            return new byte[] { (byte)(64 | ((value ? 1 : 0) & 63)) };
        }
        public override byte[] GetBytes(string value)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(value);
            var data = new byte[(IsOutgoing ? 2 : 1) + stringData.Length];
            if (IsOutgoing)
            {
                PlaceBytes((ushort)stringData.Length, data);
                Buffer.BlockCopy(stringData, 0, data, 2, stringData.Length);
            }
            else
            {
                data[^1] = 2;
                Buffer.BlockCopy(stringData, 0, data, 0, stringData.Length);
            }
            return data;
        }
        public override byte[] GetBytes(ushort value)
        {
            var data = new byte[2];
            data[0] = (byte)(64 | (value >> 6));
            data[1] = (byte)(64 | (63 & value));
            return data;
        }
        public override byte[] GetBytes(double value)
        {
            throw new NotSupportedException();
        }

        public override int ReadInt32(IList<byte> data, int index)
        {
            var value = ReadUnmaskedByte(data, ref index);

            var result = value & 3;
            var isNegative = (value & 4) == 4;
            var byteCount = ((value & 56) >> 3) | 0;
            for (int i = 1, j = 2; i < byteCount; i++, j += 6)
            {
                value = ReadUnmaskedByte(data, ref index);
                result = (value << j) | result;
            }
            if (isNegative)
            {
                result *= -1;
            }
            return result;
        }
        public override string ReadUTF8(IList<byte> data, int index)
        {
            int length = 0;
            if (IsOutgoing)
            {
                length = ReadUInt16(data, index);
                index += 2;
            }
            else while (data[index + length] != 2) length++;

            var chunk = new byte[length];
            for (int i = 0; i < length; i++)
            {
                chunk[i] = data[index++];
            }
            return Encoding.UTF8.GetString(chunk);
        }
        public override bool ReadBoolean(IList<byte> data, int index)
        {
            return ReadInt32(data, index) == 1;
        }
        public override ushort ReadUInt16(IList<byte> data, int index)
        {
            return (ushort)(data[index + 1] - 64 + (data[index] - 64) * 64);
        }
        public override double ReadDouble(IList<byte> data, int index)
        {
            return double.NaN;
        }

        protected override byte[] ConstructTails(ushort id, IList<byte> body)
        {
            int bodyStart = IsOutgoing ? 5 : 2;
            var data = new byte[(IsOutgoing ? 5 : 3) + body.Count];
            if (IsOutgoing)
            {
                data[0] = 64;
                PlaceBytes((ushort)(body.Count + 2), data, 1);
                PlaceBytes(id, data, 3);
            }
            else
            {
                PlaceBytes(id, data, 0);
                data[^1] = 1;
            }
            body.CopyTo(data, bodyStart);
            return data;
        }
        public override ValueTask<HPacket> ReceivePacketAsync(HNode node) => throw new NotSupportedException();

        public override HPacket CreatePacket()
        {
            return new WedgiePacket(IsOutgoing);
        }
        public override HPacket CreatePacket(IList<byte> data)
        {
            return new WedgiePacket(IsOutgoing, data);
        }
        public override HPacket CreatePacket(ushort id, params object[] values)
        {
            return new WedgiePacket(IsOutgoing, id, values);
        }

        private byte[] AttemptStitchBuffer(List<byte> dataCrumb)
        {
            byte[] data = null;
            int blockEndIndex = dataCrumb.IndexOf(1);
            if (blockEndIndex != -1)
            {
                int length = blockEndIndex + 1;
                data = new byte[length];

                dataCrumb.CopyTo(0, data, 0, length);
                dataCrumb.RemoveRange(0, length);
            }
            return data;
        }
        private byte ReadUnmaskedByte(IList<byte> data, ref int index)
        {
            return (byte)(data[index++] & 63);
        }
    }
}