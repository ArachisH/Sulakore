using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public ref struct HPacketReader
{
    private readonly IHFormat _format;
    private readonly ReadOnlySpan<byte> _source;

    public int Position { get; private set; }
    public int Available => _source.Length - Position;

    public static implicit operator ReadOnlySpan<byte>(HPacketReader packetIn) => packetIn._source;

    public HPacketReader(IHFormat format, ReadOnlySpan<byte> source)
    {
        _format = format;
        _source = source;

        Position = 0;
    }

    public T Read<T>() where T : struct
    {
        T value = Read<T>(Position, out int bytesRead);
        Position += bytesRead;
        return value;
    }
    public T Read<T>(int position, out int bytesRead) where T : struct
        => _format.Read<T>(_source.Slice(position), out bytesRead);

    public string ReadUTF8()
    {
        string value = ReadUTF8(Position, out int bytesRead);
        Position += bytesRead;
        return value;
    }
    public string ReadUTF8(int position, out int bytesRead)
        => _format.ReadUTF8(_source.Slice(position), out bytesRead);
}