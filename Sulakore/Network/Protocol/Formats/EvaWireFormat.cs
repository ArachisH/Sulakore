using System.Text;
using System.Text.Unicode;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Sulakore.Network.Protocol.Formats;

public sealed class EvaWireFormat : IFormat
{
    public bool HasLengthIndicator => true;
    public int MinPacketLength => sizeof(short);
    public int MinBufferSize => sizeof(int) + MinPacketLength;

    public short ReadId(ReadOnlySpan<byte> source) => Read<short>(source[4..], out _);
    public int ReadLength(ReadOnlySpan<byte> source) => Read<int>(source, out _);

    public void WriteId(Span<byte> destination, short id) => Write(destination[4..], id);
    public void WriteLength(Span<byte> destination, int length) => Write(destination, length);

    public int GetSize<T>(T value) where T : struct => Unsafe.SizeOf<T>();
    public int GetSize(ReadOnlySpan<char> value) => sizeof(short) + Encoding.UTF8.GetByteCount(value);

    public void Write<T>(Span<byte> destination, T value) where T : struct
    {
        if (typeof(T) == typeof(int))
        {
            BinaryPrimitives.WriteInt32BigEndian(destination, (int)(object)value);
        }
        else if (typeof(T) == typeof(short))
        {
            BinaryPrimitives.WriteInt16BigEndian(destination, (short)(object)value);
        }
        else if (typeof(T) == typeof(bool))
        {
            bool @bool = (bool)(object)value;
            destination[0] = Unsafe.As<bool, byte>(ref @bool);
        }
        else if (typeof(T) == typeof(byte))
        {
            destination[0] = (byte)(object)value;
        }
        else if (typeof(T) == typeof(long))
        {
            BinaryPrimitives.WriteInt64BigEndian(destination, (long)(object)value);
        }
        else if (typeof(T) == typeof(float))
        {
            BinaryPrimitives.WriteSingleBigEndian(destination, (float)(object)value);
        }
        else if (typeof(T) == typeof(double))
        {
            BinaryPrimitives.WriteDoubleBigEndian(destination, (double)(object)value);
        }
    }
    public void Write(Span<byte> destination, ReadOnlySpan<char> value)
    {
        Utf8.FromUtf16(value, destination[sizeof(short)..], out _, out int bytesWritten);
        Write(destination, (short)bytesWritten);
    }

    public T Read<T>(ReadOnlySpan<byte> source, out int bytesRead) where T : struct
    {
        T value = default;
        bytesRead = GetSize(value);
        if (typeof(T) == typeof(int))
        {
            value = (T)(object)BinaryPrimitives.ReadInt32BigEndian(source);
        }
        else if (typeof(T) == typeof(short))
        {
            value = (T)(object)BinaryPrimitives.ReadInt16BigEndian(source);
        }
        else if (typeof(T) == typeof(bool))
        {
            value = (T)(object)(source[0] != 0);
        }
        else if (typeof(T) == typeof(byte))
        {
            value = (T)(object)source[0];
        }
        else if (typeof(T) == typeof(long))
        {
            value = (T)(object)BinaryPrimitives.ReadInt64BigEndian(source);
        }
        else if (typeof(T) == typeof(float))
        {
            value = (T)(object)BinaryPrimitives.ReadSingleBigEndian(source);
        }
        else if (typeof(T) == typeof(double))
        {
            value = (T)(object)BinaryPrimitives.ReadDoubleBigEndian(source);
        }
        return value;
    }
    public string ReadUTF8(ReadOnlySpan<byte> source, out int bytesRead)
    {
        var length = Read<short>(source, out bytesRead);
        bytesRead += length;

        return Encoding.UTF8.GetString(source.Slice(sizeof(short), length));
    }
}