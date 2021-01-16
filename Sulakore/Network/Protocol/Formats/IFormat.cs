using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Sulakore.Network.Protocol
{
    public interface IFormat
    {
        public static EvaWireFormat EvaWire { get; } = new EvaWireFormat();
        public static WedgieFormat WedgieIn { get; } = new WedgieFormat(false);
        public static WedgieFormat WedgieOut { get; } = new WedgieFormat(true);

        public Span<byte> SliceBody(Span<byte> source);
        public short ReadId(ReadOnlySpan<byte> source);
        public void WriteId(Span<byte> destination, short value);

        public void WriteInt32(Span<byte> destination, int value);
        public void WriteInt16(Span<byte> destination, short value);
        public void WriteFloat(Span<byte> destination, float value);
        public void WriteBoolean(Span<byte> destination, bool value);
        public void WriteUInt16(Span<byte> destination, ushort value);
        public void WriteDouble(Span<byte> destination, double value);
        public void WriteUTF8(Span<byte> destination, ReadOnlySpan<char> value);

        public int ReadInt32(ReadOnlySpan<byte> source, out int size);
        public short ReadInt16(ReadOnlySpan<byte> source, out int size);
        public float ReadFloat(ReadOnlySpan<byte> source, out int size);
        public bool ReadBoolean(ReadOnlySpan<byte> source, out int size);
        public ushort ReadUInt16(ReadOnlySpan<byte> source, out int size);
        public double ReadDouble(ReadOnlySpan<byte> source, out int size);
        public ReadOnlySpan<byte> ReadUTF8(ReadOnlySpan<byte> source, out int size);

        public abstract ValueTask<int> SendPacketAsync(HNode node, HReadOnlyPacket packet);
        public abstract ValueTask<IMemoryOwner<byte>> ReceivePacketAsync(HNode node);
    }
}