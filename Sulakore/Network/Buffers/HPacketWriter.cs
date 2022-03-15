using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public ref struct HPacketWriter
{
    private readonly HPacket _packet;

    private Span<byte> _packetSpan;

    public int Position { get; set; }
    public IHFormat Format { get; init; }

    public HPacketWriter(IHFormat format, Span<byte> packetSpan)
        : this(format, packetSpan, null)
    { }
    internal HPacketWriter(IHFormat format, Span<byte> packetSpan, HPacket packet)
    {
        _packet = packet;
        _packetSpan = packetSpan;

        Format = format;
        Position = format.MinBufferSize;
    }

    public void Write<T>(T value) where T : struct
    {
        int size = Format.GetSize(value);
        Span<byte> valueSpan = GetSpan(size);

        Format.Write(valueSpan, value, out _);
        Position += size;
    }
    public void WriteUTF8(ReadOnlySpan<char> value)
    {
        int size = Format.GetSize(value);
        Span<byte> valueSpan = GetSpan(size);

        Format.WriteUTF8(valueSpan, value, out _);
        Position += size;
    }

    private Span<byte> GetSpan(int size)
    {
        if (size > _packetSpan.Length - Position)
        {
            if (_packet == null)
            {
                throw new InsufficientMemoryException($"There is not enough room in the buffer for the given type, and unable to grow as no {nameof(HPacket)} instance is present.");
            }
            _packet.EnsureMinimumCapacity(ref _packetSpan, size, Position);
        }
        return _packetSpan[Position..];
    }
}