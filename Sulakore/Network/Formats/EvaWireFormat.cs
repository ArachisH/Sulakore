using System.Text;
using System.Buffers;
using System.Text.Unicode;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Sulakore.Network.Formats;

public sealed class EvaWireFormat : IHFormat
{
    public int MinBufferSize => sizeof(int) + MinPacketLength;
    public int MinPacketLength => sizeof(short);
    public bool HasLengthIndicator => true;

    public bool IsUnity { get; }

    public EvaWireFormat(bool isUnity) => IsUnity = isUnity;

    public bool TryReadLength(ReadOnlySpan<byte> source, out int length, out int bytesRead)
        => TryRead(source, out length, out bytesRead);
    public bool TryWriteLength(Span<byte> destination, int length, out int bytesWritten)
        => TryWrite(destination, length, out bytesWritten);
    
    public bool TryReadId(ReadOnlySpan<byte> source, out short id, out int bytesRead)
        => TryRead(source.Slice(4), out id, out bytesRead);
    public bool TryWriteId(Span<byte> destination, short id, out int bytesWritten) 
        => TryWrite(destination.Slice(4), id, out bytesWritten);

    public bool TryReadHeader(ReadOnlySpan<byte> source, out int length, out short id, out int bytesRead)
    {
        Unsafe.SkipInit(out id);
        if (TryReadLength(source, out length, out bytesRead) &&
            TryReadId(source, out id, out int idBytesRead))
        {
            bytesRead += idBytesRead;
            return true;
        }
        return false;
    }
    public bool TryWriteHeader(Span<byte> destination, int length, short id, out int bytesWritten)
    {
        if (TryWriteLength(destination, length, out bytesWritten) &&
            TryWriteId(destination, id, out int idBytesWritten))
        {
            bytesWritten += idBytesWritten;
            return true;
        }
        return false;
    }

    public int GetSize<T>(T value) where T : struct => Unsafe.SizeOf<T>();
    public int GetSize(ReadOnlySpan<char> value) => sizeof(short) + Encoding.UTF8.GetByteCount(value);

    public bool TryRead<T>(ReadOnlySpan<byte> source, out T value, out int bytesRead) where T : struct
    {
        Unsafe.SkipInit(out value);
        Unsafe.SkipInit(out bytesRead);

        ref byte sourcePtr = ref MemoryMarshal.GetReference(source);
        if (typeof(T) == typeof(int))
        {
            if (source.Length < sizeof(int)) return false;
            value = (T)(object)BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, int>(ref sourcePtr));
            bytesRead = sizeof(int);
            return true;
        }
        if (typeof(T) == typeof(uint))
        {
            if (source.Length < sizeof(uint)) return false;
            value = (T)(object)BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, uint>(ref sourcePtr));
            bytesRead = sizeof(uint);
            return true;
        }
        else if (typeof(T) == typeof(short))
        {
            if (source.Length < sizeof(short)) return false;
            value = (T)(object)BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, short>(ref sourcePtr));
            bytesRead = sizeof(short);
            return true;
        }
        else if (typeof(T) == typeof(ushort))
        {
            if (source.Length < sizeof(ushort)) return false;
            value = (T)(object)BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ushort>(ref sourcePtr));
            bytesRead = sizeof(ushort);
            return true;
        }
        else if (typeof(T) == typeof(bool) || typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
        {
            if (source.IsEmpty) return false;
            value = Unsafe.As<byte, T>(ref sourcePtr);
            bytesRead = sizeof(byte);
            return true;
        }
        else if (typeof(T) == typeof(long))
        {
            if (source.Length < sizeof(long)) return false;
            value = (T)(object)BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, long>(ref sourcePtr));
            bytesRead = sizeof(long);
            return true;
        }
        else if (typeof(T) == typeof(ulong))
        {
            if (source.Length < sizeof(ulong)) return false;
            value = (T)(object)BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, ulong>(ref sourcePtr));
            bytesRead = sizeof(ulong);
            return true;
        }
        else if (typeof(T) == typeof(float))
        {
            if (source.Length < sizeof(float)) return false;
            value = (T)(object)BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, int>(ref sourcePtr)));
            bytesRead = sizeof(float);
            return true;
        }
        else if (typeof(T) == typeof(double))
        {
            if (source.Length < sizeof(double)) return false;
            value = (T)(object)BitConverter.Int64BitsToDouble(
                BinaryPrimitives.ReverseEndianness(Unsafe.As<byte, long>(ref sourcePtr)));
            bytesRead = sizeof(double);
            return true;
        }
        return false;
    }
    public bool TryWrite<T>(Span<byte> destination, T value, out int bytesWritten) where T : struct
    {
        Unsafe.SkipInit(out bytesWritten);

        ref byte destPtr = ref MemoryMarshal.GetReference(destination);
        if (typeof(T) == typeof(int))
        {
            if (destination.Length < sizeof(int)) return false;
            int @int = BinaryPrimitives.ReverseEndianness((int)(object)value);
            Unsafe.WriteUnaligned(ref destPtr, @int);
            bytesWritten = sizeof(int);
            return true;
        }
        else if (typeof(T) == typeof(short))
        {
            if (destination.Length < sizeof(short)) return false;
            short @short = BinaryPrimitives.ReverseEndianness((short)(object)value);
            Unsafe.WriteUnaligned(ref destPtr, @short);
            bytesWritten = sizeof(short);
            return true;
        }
        else if (typeof(T) == typeof(bool))
        {
            if (destination.IsEmpty) return false;
            bool @bool = (bool)(object)value;
            Unsafe.WriteUnaligned(ref destPtr, Unsafe.As<bool, byte>(ref @bool));
            bytesWritten = sizeof(bool);
            return true;
        }
        else if (typeof(T) == typeof(byte))
        {
            if (destination.IsEmpty) return false;
            Unsafe.WriteUnaligned(ref destPtr, (byte)(object)value);
            bytesWritten = sizeof(byte);
            return true;
        }
        else if (typeof(T) == typeof(long))
        {
            if (destination.Length < sizeof(long)) return false;
            long @long = BinaryPrimitives.ReverseEndianness((long)(object)value);
            Unsafe.WriteUnaligned(ref destPtr, @long);
            bytesWritten = sizeof(long);
            return true;
        }
        else if (typeof(T) == typeof(float))
        {
            if (destination.Length < sizeof(float)) return false;
            int @float = BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits((float)(object)value));
            Unsafe.WriteUnaligned(ref destPtr, @float);
            bytesWritten = sizeof(float);
            return true;
        }
        else if (typeof(T) == typeof(double))
        {
            if (destination.Length < sizeof(double)) return false;
            long @double = BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits((double)(object)value));
            Unsafe.WriteUnaligned(ref destPtr, @double);
            bytesWritten = sizeof(double);
            return true;
        }
        return false;
    }

    public bool TryReadUTF8(ReadOnlySpan<byte> source, out string value, out int bytesRead)
    {
        Unsafe.SkipInit(out value);
        if (!TryRead(source, out short length, out bytesRead) ||
            source.Length < sizeof(short) + length) return false;

        bytesRead += length;
        value = Encoding.UTF8.GetString(source.Slice(sizeof(short), length));
        return true;
    }

    public bool TryWriteUTF8(Span<byte> destination, ReadOnlySpan<char> value, out int bytesWritten)
    {
        Unsafe.SkipInit(out bytesWritten);
        if (destination.Length < sizeof(short))
            return false;

        OperationStatus status = Utf8.FromUtf16(value, destination.Slice(sizeof(short)), out _, out bytesWritten);
        if (status != OperationStatus.Done)
            return false;

        if (!TryWrite(destination, (short)bytesWritten, out int written))
            return false;

        bytesWritten += written;
        return true;
    }
    public bool TryReadUTF8(ReadOnlySpan<byte> source, Span<char> destination, out int bytesRead, out int charsWritten)
    {
        Unsafe.SkipInit(out charsWritten);
        if (!TryRead(source, out short length, out bytesRead) || 
            source.Length < sizeof(short) + length || 
            destination.Length < length) return false;

        OperationStatus status = Utf8.ToUtf16(source, destination.Slice(sizeof(short), length), out int actualLength, out charsWritten);
        bytesRead += actualLength;
        return status == OperationStatus.Done;
    }
}