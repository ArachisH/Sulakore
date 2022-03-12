using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public ref struct HPacketReader
{
    private readonly ReadOnlySpan<byte> _packetSpan;

    public HFormat Format { get; }
    public int Position { get; set; }
    public int Available => _packetSpan.Length - Position;

    public byte this[int index] => _packetSpan[index];

    public HPacketReader(HPacket packet)
        : this(packet.Format, packet.Buffer.Span)
    { }
    public HPacketReader(HFormat format, ReadOnlySpan<byte> packetSpan)
    {
        _packetSpan = packetSpan;

        Format = format;
        Position = format.MinBufferSize;
    }

    public T Read<T>() where T : struct
    {
        T value = Format.Read<T>(_packetSpan[Position..], out int bytesRead);
        Position += bytesRead;
        return value;
    }
    public string ReadUTF8()
    {
        string value = Format.ReadUTF8(_packetSpan[Position..], out int bytesRead);
        Position += bytesRead;
        return value;
    }
}