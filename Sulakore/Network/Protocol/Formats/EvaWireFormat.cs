using System.Text;
using System.Buffers;
using System.Text.Unicode;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

using Sulakore.Buffers;
using Sulakore.Cryptography.Ciphers;

namespace Sulakore.Network.Protocol.Formats;

public sealed class EvaWireFormat : IFormat
{
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

    public ValueTask<int> SendPacketAsync(HNode node, Memory<byte> buffer)
    {
        Encipher(node.Encrypter, buffer.Span, node.IsWebSocket);
        return node.SendAsync(buffer);
    }
    public async ValueTask<int> SendPacketAsync(HNode node, ReadOnlyMemory<byte> buffer)
    {
        using IMemoryOwner<byte> encryptedOwner = MemoryPool<byte>.Shared.Rent(buffer.Length);
        Memory<byte> encryptedRegion = encryptedOwner.Memory[..buffer.Length];
        buffer.CopyTo(encryptedRegion);

        Encipher(node.Encrypter, encryptedRegion.Span, node.IsWebSocket);
        return await node.SendAsync(encryptedRegion).ConfigureAwait(false);
    }
    public async Task<HPacket> ReceiveRentedPacketAsync(HNode node)
    {
        var packet = new HPacket(this, 6);

        int bytesReceived = await node.ReceiveAsync(packet.LengthRegion).ConfigureAwait(false);
        if (bytesReceived != 4) return null;

        int packetLength = BinaryPrimitives.ReadInt32BigEndian(packet.LengthRegion.Span);
        packet.EnsureMinimumCapacity(packetLength);

        // TODO
        //int received = 0;
        //do received += await node.ReceiveAsync(packetOwner.Memory.Slice(4 + received, packetLength - received)).ConfigureAwait(false);
        //while (received != packetLength);

        //if (node.Decrypter != null)
        //{
        //    Encipher(node.Decrypter, packetOwner.Memory[..(4 + packetLength)].Span, node.IsWebSocket);
        //}

        return packet;
    }

    private static void Encipher(IStreamCipher cipher, Span<byte> buffer, bool isWebSocket)
    {
        if (cipher == null) return;
        if (isWebSocket)
        {
            Span<byte> packetId = buffer.Slice(4, 2);
            packetId.Reverse();

            cipher.Process(packetId);
            packetId.Reverse();
        }
        else cipher.Process(buffer);
    }
}