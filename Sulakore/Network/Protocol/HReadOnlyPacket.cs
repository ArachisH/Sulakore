using Sulakore.Network.Protocol.Formats;

namespace Sulakore.Network.Protocol;

public ref struct HReadOnlyPacket
{
    private readonly ReadOnlySpan<byte> _buffer;

    public int Position { get; set; }
    public IFormat Format { get; init; }

    public readonly short Id => Format.ReadId(_buffer);
    public readonly int Length => Format.ReadLength(_buffer);
    public readonly int Available => _buffer.Length - Position;

    public HReadOnlyPacket(IFormat format, ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;

        Position = 0;
        Format = format;
    }

    public T Read<T>() where T : struct
    {
        T value = Format.Read<T>(_buffer, out int bytesRead);
        Position += bytesRead;
        return value;
    }
    public string ReadUTF8()
    {
        string value = Format.ReadUTF8(_buffer, out int bytesRead);
        Position += bytesRead;
        return value;
    }
}