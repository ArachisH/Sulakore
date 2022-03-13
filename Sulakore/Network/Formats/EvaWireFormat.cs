using System.Text;
using System.Buffers;
using System.Text.Unicode;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sulakore.Network.Formats;

/// <summary>
/// Provides a high-performance low-level APIs to write to and read from buffer in EvaWire-protocol format.
/// </summary>
/// <remarks>
/// Due to heavy optimizations, do not trust <c>out</c> parameters in <c>Try</c>-prefixed methods when the operation is unsuccessful (returns <c>false</c>). 
/// The <c>out</c> parameters are only initialized when the operation is successful. This expected behaviour from runtime libraries.
/// </remarks>
public sealed class EvaWireFormat : IHFormat
{
    public bool HasLengthIndicator => true;
    public int MinPacketLength => sizeof(short);
    public int MinBufferSize => sizeof(int) + MinPacketLength;

    public bool IsUnity { get; }

    public EvaWireFormat(bool isUnity) 
        => IsUnity = isUnity;

    public short ReadId(ReadOnlySpan<byte> source) => Read<short>(source.Slice(4), out _);
    public bool TryReadId(ReadOnlySpan<byte> source, out short id, out int bytesRead)
        => TryRead(source.Slice(4), out id, out bytesRead);

    public int ReadLength(ReadOnlySpan<byte> source) => Read<int>(source, out _);
    public bool TryReadLength(ReadOnlySpan<byte> source, out int length, out int bytesRead)
        => TryRead(source, out length, out bytesRead);

    public void WriteId(Span<byte> destination, short id) => Write(destination.Slice(4), id);
    public bool TryWriteId(Span<byte> destination, short id, out int bytesWritten)
        => TryWrite(destination.Slice(4), id, out bytesWritten);

    public void WriteLength(Span<byte> destination, int length) => Write(destination, length);
    public bool TryWriteLength(Span<byte> destination, int length, out int bytesWritten)
        => TryWrite(destination, length, out bytesWritten);

    public int GetSize<T>(T value) where T : struct => Unsafe.SizeOf<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetSize(ReadOnlySpan<char> value) => sizeof(short) + Encoding.UTF8.GetByteCount(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T>(Span<byte> destination, T value) where T : struct
    {
        if (!TryWrite(destination, value, out _))
            ThrowHelper.ThrowIndexOutOfRangeException();
    }

    public void WriteUTF8(Span<byte> destination, ReadOnlySpan<char> value)
    {
        Utf8.FromUtf16(value, destination.Slice(sizeof(short)), out _, out int bytesWritten);
        Write(destination, (short)bytesWritten);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWriteUTF8(Span<byte> destination, ReadOnlySpan<char> value, out int bytesWritten)
    {
        Unsafe.SkipInit(out bytesWritten);
        if (destination.Length < sizeof(short))
            return false;

        var status = Utf8.FromUtf16(value, destination.Slice(sizeof(short)), out _, out bytesWritten);
        if (status != OperationStatus.Done)
            return false;

        if (!TryWrite(destination, (short)bytesWritten, out int written))
            return false;

        bytesWritten += written;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRead<T>(ReadOnlySpan<byte> source, out T value, out int bytesRead) where T : struct
    {
        Unsafe.SkipInit(out bytesRead);
        Unsafe.SkipInit(out value);
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
        else if (typeof(T) == typeof(bool) || typeof(T) == typeof(byte))
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>(ReadOnlySpan<byte> source, out int bytesRead) where T : struct
    {
        if (!TryRead(source, out T value, out bytesRead))
            ThrowHelper.ThrowIndexOutOfRangeException();
        return value;
    }

    public string ReadUTF8(ReadOnlySpan<byte> source, out int bytesRead)
    {
        if (!TryReadUTF8(source, out string value, out bytesRead))
            ThrowHelper.ThrowIndexOutOfRangeException();
        return value;
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
    public bool TryReadUTF8(ReadOnlySpan<byte> source, Span<char> destination, out int bytesRead, out int charsWritten)
    {
        Unsafe.SkipInit(out charsWritten);
        if (!TryRead(source, out short length, out bytesRead) || 
            source.Length < sizeof(short) + length) return false;

        var status = Utf8.ToUtf16(source, destination, out int actualLength, out charsWritten);
        bytesRead += actualLength;
        return status == OperationStatus.Done;
    }
}