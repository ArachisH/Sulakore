using System;
using System.Text;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading.Tasks;

namespace Sulakore.Network.Protocol
{
    public sealed class EvaWireFormat : IFormat
    {
        public Span<byte> SliceBody(Span<byte> source) => source.Slice(6);
        public int ReadLength(ReadOnlySpan<byte> source) => ReadInt32(source, out _);
        public short ReadId(ReadOnlySpan<byte> source) => ReadInt16(source.Slice(4), out _);
        public void WriteId(Span<byte> destination, short value) => WriteInt16(destination.Slice(4), value);

        public void WriteUTF8(Span<byte> destination, ReadOnlySpan<char> value)
        {
            WriteInt16(destination, (short)value.Length);
            Encoding.UTF8.GetBytes(value, destination.Slice(sizeof(short)));
        }
        public void WriteBoolean(Span<byte> destination, bool value) => destination[0] = value ? 1 : 0;
        public void WriteInt32(Span<byte> destination, int value) => BinaryPrimitives.WriteInt32BigEndian(destination, value);
        public void WriteInt16(Span<byte> destination, short value) => BinaryPrimitives.WriteInt16BigEndian(destination, value);
        public void WriteFloat(Span<byte> destination, float value) => BinaryPrimitives.WriteSingleBigEndian(destination, value);
        public void WriteUInt16(Span<byte> destination, ushort value) => BinaryPrimitives.WriteUInt16BigEndian(destination, value);
        public void WriteDouble(Span<byte> destination, double value) => BinaryPrimitives.WriteDoubleBigEndian(destination, value);

        public int ReadInt32(ReadOnlySpan<byte> source, out int size)
        {
            size = sizeof(int);
            return BinaryPrimitives.ReadInt32BigEndian(source);
        }
        public short ReadInt16(ReadOnlySpan<byte> source, out int size)
        {
            size = sizeof(short);
            return BinaryPrimitives.ReadInt16BigEndian(source);
        }
        public float ReadFloat(ReadOnlySpan<byte> source, out int size)
        {
            size = sizeof(float);
            return BinaryPrimitives.ReadSingleBigEndian(source);
        }
        public bool ReadBoolean(ReadOnlySpan<byte> source, out int size)
        {
            size = sizeof(bool);
            return source[0] != 0;
        }
        public ushort ReadUInt16(ReadOnlySpan<byte> source, out int size)
        {
            size = sizeof(ushort);
            return BinaryPrimitives.ReadUInt16BigEndian(source);
        }
        public double ReadDouble(ReadOnlySpan<byte> source, out int size)
        {
            size = sizeof(double);
            return BinaryPrimitives.ReadDoubleBigEndian(source);
        }
        public ReadOnlySpan<byte> ReadUTF8(ReadOnlySpan<byte> source, out int size)
        {
            short length = ReadInt16(source, out size);

            size += length;
            return source.Slice(sizeof(short), length);
        }

        public void Encrypt(HNode node, Span<byte> data)
        {}
        public void Decrypt(HNode node, Span<byte> data)
        {
            if (node.Decrypter == null) return;
            if (node.IsWebSocket)
            {
                Span<byte> idBytes = data.Slice(4, 2);
                idBytes.Reverse();

                node.Decrypter.Process(idBytes);
                idBytes.Reverse();
            }
            else node.Decrypter.Process(data);
        }

        public ValueTask<int> SendPacketAsync(HNode node, HReadOnlyPacket packet)
        {
            throw new NotImplementedException();
        }
        public async ValueTask<IMemoryOwner<byte>> ReceivePacketAsync(HNode node)
        {
            using IMemoryOwner<byte> lengthOwner = MemoryPool<byte>.Shared.Rent(4);

            int lengthReceived = await node.ReceiveAsync(lengthOwner.Memory.Slice(0, 4)).ConfigureAwait(false);
            if (lengthReceived != 4) node.Dispose();

            int length = BinaryPrimitives.ReadInt32BigEndian(lengthOwner.Memory.Span);
            IMemoryOwner<byte> packetOwner = MemoryPool<byte>.Shared.Rent(4 + length);

            int received = 0;
            do received += await node.ReceiveAsync(packetOwner.Memory.Slice(4 + received, length - received)).ConfigureAwait(false);
            while (received != length);

            Decrypt(node, packetOwner.Memory.Slice(0, 4 + length).Span);
            return packetOwner;
        }
    }
}