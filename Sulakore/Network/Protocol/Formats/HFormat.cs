using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sulakore.Network.Protocol
{
    public abstract class HFormat
    {
        protected bool IsOutgoing { get; }

        public abstract string Name { get; }
        public abstract int IdPosition { get; }

        public static EvaWireFormat EvaWire { get; }
        public static WedgieFormat WedgieIn { get; }
        public static WedgieFormat WedgieOut { get; }

        static HFormat()
        {
            EvaWire = new EvaWireFormat();
            WedgieIn = new WedgieFormat(false);
            WedgieOut = new WedgieFormat(true);
        }
        protected HFormat()
        { }
        protected HFormat(bool isOutgoing)
        {
            IsOutgoing = isOutgoing;
        }

        public abstract ushort GetId(IList<byte> data);
        public abstract byte[] GetBody(IList<byte> data);

        public abstract byte[] GetBytes(int value);
        public void PlaceBytes(int value, IList<byte> destination)
        {
            PlaceBytes(value, destination, 0);
        }
        public void PlaceBytes(int value, IList<byte> destination, int offset)
        {
            PlaceBytes(GetBytes(value), destination, offset);
        }

        public abstract byte[] GetBytes(bool value);
        public void PlaceBytes(bool value, IList<byte> destination)
        {
            PlaceBytes(value, destination, 0);
        }
        public void PlaceBytes(bool value, IList<byte> destination, int offset)
        {
            PlaceBytes(GetBytes(value), destination, offset);
        }

        public abstract byte[] GetBytes(string value);
        public void PlaceBytes(string value, IList<byte> destination)
        {
            PlaceBytes(value, destination, 0);
        }
        public void PlaceBytes(string value, IList<byte> destination, int offset)
        {
            PlaceBytes(GetBytes(value), destination, offset);
        }

        public abstract byte[] GetBytes(ushort value);
        public void PlaceBytes(ushort value, IList<byte> destination)
        {
            PlaceBytes(value, destination, 0);
        }
        public void PlaceBytes(ushort value, IList<byte> destination, int offset)
        {
            PlaceBytes(GetBytes(value), destination, offset);
        }

        public abstract byte[] GetBytes(double value);
        public void PlaceBytes(double value, IList<byte> destination)
        {
            PlaceBytes(value, destination, 0);
        }
        public void PlaceBytes(double value, IList<byte> destination, int offset)
        {
            PlaceBytes(GetBytes(value), destination, offset);
        }

        public abstract int GetSize(int value);
        public abstract int GetSize(bool value);
        public abstract int GetSize(string value);
        public abstract int GetSize(ushort value);
        public abstract int GetSize(double value);

        public abstract int ReadInt32(IList<byte> data, int index);
        public abstract string ReadUTF8(IList<byte> data, int index);
        public abstract bool ReadBoolean(IList<byte> data, int index);
        public abstract ushort ReadUInt16(IList<byte> data, int index);
        public abstract double ReadDouble(IList<byte> data, int index);

        public byte[] Construct(ushort id, params object[] values)
        {
            var body = new List<byte>();
            foreach (object value in values)
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Byte: body.Add((byte)value); break;
                    case TypeCode.Int32: body.AddRange(GetBytes((int)value)); break;
                    case TypeCode.Boolean: body.AddRange(GetBytes((bool)value)); break;
                    case TypeCode.UInt32: body.AddRange(GetBytes((ushort)value)); break;
                    case TypeCode.Double: body.AddRange(GetBytes((double)value)); break;
                    case TypeCode.String: body.AddRange(GetBytes((string)value)); break;
                    case TypeCode.Char: body.AddRange(GetBytes(((char)value).ToString())); break;
                    default:
                    {
                        IList<byte> blob = null;
                        switch (value)
                        {
                            case IList<byte> data:
                            blob = data;
                            break;

                            case IHabboData data:
                            blob = data.ToBytes();
                            break;
                        }
                        if (blob == null)
                        {
                            throw new NullReferenceException($"Unable to convert '{value.GetType().Name}' to byte[].");
                        }
                        body.AddRange(blob);
                        break;
                    }
                }
            }
            return ConstructTails(id, body);
        }
        public void PlaceBytes(IList<byte> data, IList<byte> destination, int index)
        {
            for (int i = 0; i < data.Count; i++)
            {
                destination[index++] = data[i];
            }
        }

        public abstract Task<HPacket> ReceivePacketAsync(HNode node);
        protected abstract byte[] ConstructTails(ushort id, IList<byte> body);

        public abstract HPacket CreatePacket(IList<byte> data);
        public abstract HPacket CreatePacket(ushort id, params object[] values);

        public static HFormat GetFormat(string name)
        {
            switch (name)
            {
                case "EVAWIRE": return EvaWire;
                case "WEDGIE-IN": return WedgieIn;
                case "WEDGIE-OUT": return WedgieOut;

                default: return null;
            }
        }
    }
}