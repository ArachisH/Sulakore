using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public ref struct HPacketWriter
{
    private readonly HPacket _packet;

    private Span<byte> _destination;

    public int Position { get; set; }
    public IHFormat Format { get; init; }

    public HPacketWriter(IHFormat format, Span<byte> destination)
        : this(format, destination, null)
    { }
    internal HPacketWriter(IHFormat format, Span<byte> destination, HPacket packet)
    {
        _packet = packet;
        _destination = destination;

        Position = 0;
        Format = format;
    }

    public void Write<T>(T value) where T : struct
    {
        int size = Format.GetSize(value);
        Span<byte> advanced = Advance(size);

        Format.Write(advanced, value, out _);
    }
    public void WriteUTF8(ReadOnlySpan<char> value)
    {
        int size = Format.GetSize(value);
        Span<byte> advanced = Advance(size);

        Format.WriteUTF8(advanced, value, out _);
    }

    private Span<byte> Advance(int size)
    {
        if (_packet != null)
        {
            _packet.Length += size;
            _packet.EnsureMinimumCapacity(ref _destination, size, Position);
        }

        Span<byte> advanced = _destination[Position..];
        Position += size;

        return advanced;
    }
}