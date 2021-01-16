using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Sulakore.Network.Protocol
{
    public class WedgieFormat : IFormat
    {
        private readonly bool _isOutgoing;

        public WedgieFormat(bool isOutgoing)
        {
            _isOutgoing = isOutgoing;
        }

        public Span<byte> SliceBody(Span<byte> source)
        {
            throw new NotImplementedException();
        }
        public short ReadId(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }
        public void WriteId(Span<byte> destination, short value)
        {
            throw new NotImplementedException();
        }

        public void WriteInt32(Span<byte> destination, int value)
        {
            throw new NotImplementedException();
        }
        public void WriteInt16(Span<byte> destination, short value)
        {
            throw new NotImplementedException();
        }
        public void WriteBoolean(Span<byte> destination, bool value)
        {
            throw new NotImplementedException();
        }
        public void WriteUInt16(Span<byte> destination, ushort value)
        {
            throw new NotImplementedException();
        }
        public void WriteDouble(Span<byte> destination, double value)
        {
            throw new NotImplementedException();
        }
        public void WriteString(Span<byte> destination, ReadOnlySpan<char> value)
        {
            throw new NotImplementedException();
        }

        public int ReadInt32(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }
        public short ReadInt16(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }
        public bool ReadBoolean(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }
        public ushort ReadUInt16(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }
        public double ReadDouble(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }
        public ReadOnlySpan<char> ReadString(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> SendPacketAsync(HNode node, HPacket packet)
        {
            throw new NotImplementedException();
        }
        public ValueTask<IMemoryOwner<byte>> ReceivePacketAsync(HNode node)
        {
            throw new NotImplementedException();
        }

        public int ReadInt32(ReadOnlySpan<byte> source, out int size)
        {
            throw new NotImplementedException();
        }

        public short ReadInt16(ReadOnlySpan<byte> source, out int size)
        {
            throw new NotImplementedException();
        }

        public bool ReadBoolean(ReadOnlySpan<byte> source, out int size)
        {
            throw new NotImplementedException();
        }

        public ushort ReadUInt16(ReadOnlySpan<byte> source, out int size)
        {
            throw new NotImplementedException();
        }

        public double ReadDouble(ReadOnlySpan<byte> source, out int size)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<char> ReadUTF8(ReadOnlySpan<byte> source, out int size)
        {
            throw new NotImplementedException();
        }

        public void WriteFloat(Span<byte> destination, float value)
        {
            throw new NotImplementedException();
        }

        public float ReadFloat(ReadOnlySpan<byte> source, out int size)
        {
            throw new NotImplementedException();
        }
    }
}