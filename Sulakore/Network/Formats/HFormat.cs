namespace Sulakore.Network.Formats;

public abstract class HFormat
{
    public static EvaWireFormat EvaWire { get; } = new EvaWireFormat();

    public abstract int MinBufferSize { get; }
    public abstract int MinPacketLength { get; }
    public abstract bool HasLengthIndicator { get; }

    public abstract short ReadId(ReadOnlySpan<byte> soruce);
    public abstract int ReadLength(ReadOnlySpan<byte> source);

    public abstract void WriteId(Span<byte> destination, short id);
    public abstract void WriteLength(Span<byte> destination, int length);

    public abstract int GetSize<T>(T value) where T : struct;
    public abstract int GetSize(ReadOnlySpan<char> value);

    public void Write<T>(ref Span<byte> destination, T value) where T : struct
    {
        Write(destination, value);
        destination = destination[GetSize(value)..];
    }
    public abstract void Write<T>(Span<byte> destination, T value) where T : struct;

    public void Write(ref Span<byte> destination, ReadOnlySpan<char> value)
    {
        Write(destination, value);
        destination = destination[GetSize(value)..];
    }
    public abstract void Write(Span<byte> destination, ReadOnlySpan<char> value);

    public T Read<T>(ref ReadOnlySpan<byte> source) where T : struct
    {
        T value = Read<T>(source, out int bytesRead);
        source = source[bytesRead..];
        return value;
    }
    public abstract T Read<T>(ReadOnlySpan<byte> source, out int bytesRead) where T : struct;

    public string ReadUTF8(ref ReadOnlySpan<byte> source)
    {
        string value = ReadUTF8(source, out int bytesRead);
        source = source[bytesRead..];
        return value;
    }
    public abstract string ReadUTF8(ReadOnlySpan<byte> source, out int bytesRead);
}