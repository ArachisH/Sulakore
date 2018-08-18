using System;
using System.Text;
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

        public override int GetSize(int value)
        {
            return 4;
        }
        public override int GetSize(bool value)
        {
            return 1;
        }
        public override int GetSize(string value)
        {
            return (2 + Encoding.UTF8.GetByteCount(value));
        }
        public override int GetSize(ushort value)
        {
            return 2;
        }
        public override int GetSize(double value)
        {
            return 6;
        }

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
            int result = (data[index++] << 24);
            result += (data[index++] << 16);
            result += (data[index++] << 8);
            return (result + data[index]);
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
            return (data[index] == 1);
        }
        public override ushort ReadUInt16(IList<byte> data, int index)
        {
            int result = (data[index++] << 8);
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

        public override async Task<HPacket> ReceivePacketAsync(HNode node)
        {
            byte[] lengthBlock = await node.AttemptReceiveAsync(4, 3).ConfigureAwait(false);
            if (lengthBlock == null)
            {
                node.Disconnect();
                return null;
            }

            byte[] body = await node.AttemptReceiveAsync(ReadInt32(lengthBlock, 0), 3).ConfigureAwait(false);
            if (body == null)
            {
                node.Disconnect();
                return null;
            }

            var data = new byte[4 + body.Length];
            Buffer.BlockCopy(lengthBlock, 0, data, 0, 4);
            Buffer.BlockCopy(body, 0, data, 4, body.Length);

            return CreatePacket(data);
        }
        protected override byte[] ConstructTails(ushort id, IList<byte> body)
        {
            var data = new byte[6 + body.Count];
            PlaceBytes(body.Count + 2, data, 0);
            PlaceBytes(id, data, 4);

            body.CopyTo(data, 6);
            return data;
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