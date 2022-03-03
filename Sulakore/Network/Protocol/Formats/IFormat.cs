using Sulakore.Buffers;

namespace Sulakore.Network.Protocol.Formats;

public interface IFormat
{
    public static EvaWireFormat EvaWire { get; } = new EvaWireFormat();

    public int MinBufferSize { get; }
    public int MinPacketLength { get; }

    public short ReadId(ReadOnlySpan<byte> soruce);
    public int ReadLength(ReadOnlySpan<byte> source);

    public void WriteId(Span<byte> destination, short id);
    public void WriteLength(Span<byte> destination, int length);

    public int GetSize<T>(T value) where T : struct;
    public int GetSize(ReadOnlySpan<char> value);

    public void Write<T>(Span<byte> destination, T value) where T : struct;
    public void Write(Span<byte> destination, ReadOnlySpan<char> value);

    public T Read<T>(ReadOnlySpan<byte> source, out int bytesRead) where T : struct;
    public string ReadUTF8(ReadOnlySpan<byte> source, out int bytesRead);

    public abstract ValueTask<int> SendPacketAsync(HNode node, Memory<byte> packetRegion);
    public abstract ValueTask<int> SendPacketAsync(HNode node, ReadOnlyMemory<byte> packetRegion);
    public abstract Task<HPacket> ReceiveRentedPacketAsync(HNode node);
}