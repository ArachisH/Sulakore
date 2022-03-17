using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public ref struct HPacketReader
{
    private readonly ReadOnlySpan<byte> _packetSpan;

    public IHFormat Format { get; }
    public int Position { get; set; }
    public int Available => _packetSpan.Length - Position;

    public byte this[int index] => _packetSpan[index];

    public HPacketReader(IHFormat format, ReadOnlySpan<byte> packetSpan)
    {
        _packetSpan = packetSpan;

        Position = 0;
        Format = format;
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