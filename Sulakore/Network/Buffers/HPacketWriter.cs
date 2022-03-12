using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public ref struct HPacketWriter
{
    private readonly HPacket _packet;

    private Span<byte> _bufferSpan;

    public int Position { get; set; }
    public HFormat Format { get; init; }

    public HPacketWriter(HPacket packet)
        : this(packet.Format, packet._buffer.Span)
    {
        _packet = packet;
    }
    public HPacketWriter(HFormat format, Span<byte> bufferSpan)
    {
        _packet = null;
        _bufferSpan = bufferSpan;

        Position = 0;
        Format = format;
    }

    public void Write<T>(T value) where T : struct
    {
        int size = Format.GetSize(value);
        Span<byte> valueSpan = GetSpan(size);

        Format.Write(valueSpan, value);
        Position += size;
    }
    public void WriteUTF8(ReadOnlySpan<char> value)
    {
        int size = Format.GetSize(value);
        Span<byte> valueSpan = GetSpan(size);

        Format.WriteUTF8(valueSpan, value);
        Position += size;
    }

    private Span<byte> GetSpan(int size)
    {
        if (size > _bufferSpan.Length - Position)
        {
            if (_packet != null)
            {
                _packet.EnsureCapacity(size, Position);
                _bufferSpan = _packet._buffer.Span;
            }
            else throw new InsufficientMemoryException($"There is not enough room in the buffer for the given type, and unable to grow as no {nameof(HPacket)} instance is present.");
        }
        return _bufferSpan[Position..];
    }
}