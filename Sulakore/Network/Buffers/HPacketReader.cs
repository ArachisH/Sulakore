using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public ref struct HPacketReader
{
    private readonly IHFormat _format;
    private readonly ReadOnlySpan<byte> _source;

    public int _position;

    public short Id { get; }
    public int Length { get; }

    public int Available => _source.Length - _position;

    public static implicit operator ReadOnlySpan<byte>(HPacketReader reader) => reader._source;

    public HPacketReader(IHFormat format, ReadOnlySpan<byte> source)
    {
        _format = format;
        _source = source;

        if (!format.TryReadHeader(source, out int length, out short id, out int bytesRead))
        {
            // TODO: Throw exception?
        }
        _position = bytesRead;

        Id = id;
        Length = length;
    }

    public T Read<T>() where T : struct
    {
        T value = Read<T>(_position, out int bytesRead);
        _position += bytesRead;
        return value;
    }
    public T Read<T>(int position, out int bytesRead) where T : struct
        => _format.Read<T>(_source.Slice(position), out bytesRead);

    public string ReadUTF8()
    {
        string value = ReadUTF8(_position, out int bytesRead);
        _position += bytesRead;
        return value;
    }
    public string ReadUTF8(int position, out int bytesRead)
        => _format.ReadUTF8(_source.Slice(position), out bytesRead);
}