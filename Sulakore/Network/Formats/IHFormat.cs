namespace Sulakore.Network.Formats;

public interface IHFormat
{
    public static EvaWireFormat EvaWire { get; } = new EvaWireFormat(isUnity: false);
    public static EvaWireFormat EvaWireUnity { get; } = new EvaWireFormat(isUnity: true);

    public int MinBufferSize { get; }
    public int MinPacketLength { get; }
    public bool HasLengthIndicator { get; }

    public int GetSize<T>(T value) where T : struct;
    public int GetSize(ReadOnlySpan<char> value);

    public short ReadId(ReadOnlySpan<byte> source);
    public bool TryReadId(ReadOnlySpan<byte> source, out short id, out int bytesRead);
    
    public int ReadLength(ReadOnlySpan<byte> source);
    public bool TryReadLength(ReadOnlySpan<byte> source, out int length, out int bytesRead);

    public void WriteId(Span<byte> destination, short id);
    public bool TryWriteId(Span<byte> source, short id, out int bytesWritten);
    
    public void WriteLength(Span<byte> destination, int length);
    public bool TryWriteLength(Span<byte> source, int length, out int bytesWritten);

    public void Write<T>(ref Span<byte> destination, T value) where T : struct
    {
        Write(destination, value);
        destination = destination.Slice(GetSize(value));
    }
    public void Write<T>(Span<byte> destination, T value) where T : struct;
    public bool TryWrite<T>(Span<byte> destination, T value, out int bytesWritten) where T : struct;

    public void WriteUTF8(ref Span<byte> destination, ReadOnlySpan<char> value)
    {
        WriteUTF8(destination, value);
        destination = destination.Slice(GetSize(value));
    }
    public void WriteUTF8(Span<byte> destination, ReadOnlySpan<char> value);
    public bool TryWriteUTF8(Span<byte> destination, ReadOnlySpan<char> value, out int bytesWritten);

    public T Read<T>(ref ReadOnlySpan<byte> source) where T : struct
    {
        T value = Read<T>(source, out int bytesRead);
        source = source.Slice(bytesRead);
        return value;
    }
    public T Read<T>(ReadOnlySpan<byte> source, out int bytesRead) where T : struct;
    public bool TryRead<T>(ReadOnlySpan<byte> source, out T value, out int bytesRead) where T : struct;
    
    public string ReadUTF8(ref ReadOnlySpan<byte> source)
    {
        string value = ReadUTF8(source, out int bytesRead);
        source = source.Slice(bytesRead);
        return value;
    }
    public string ReadUTF8(ReadOnlySpan<byte> source, out int bytesRead);
    public bool TryReadUTF8(ReadOnlySpan<byte> source, out string value, out int bytesRead);
    public bool TryReadUTF8(ReadOnlySpan<byte> source, Span<char> destination, out int bytesRead, out int charsWritten);
}