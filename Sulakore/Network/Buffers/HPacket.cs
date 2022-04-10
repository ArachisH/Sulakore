using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public sealed class HPacket : IDisposable
{
    public const int MAX_ALLOC_SIZE = 256;

    private bool _disposed;
    private IMemoryOwner<byte> _owner;

    public short Id { get; }
    public IHFormat Format { get; }
    public int Length { get; internal set; }

    internal bool IsReadOnly { get; }
    internal Memory<byte> Buffer { get; private set; }

    public static implicit operator HPacketReader(HPacket packet) => packet.AsReader();

    public HPacket(IHFormat format, short id, out HPacketWriter packetOut)
    {
        Id = id;
        Format = format;
        Length = format.MinPacketLength;
        Buffer = new byte[format.MinBufferSize];

        packetOut = new HPacketWriter(this, Span<byte>.Empty);
    }
    internal HPacket(IHFormat format, short id, int length, Memory<byte> buffer, IMemoryOwner<byte> owner = null)
    {
        _owner = owner;

        Id = id;
        Format = format;
        Length = length;
        Buffer = buffer;
        IsReadOnly = true;
    }

    public HPacketReader AsReader() => !_disposed
        ? (new(Format, Buffer.Span.Slice(Format.MinBufferSize, Length - Format.MinPacketLength)))
        : throw new ObjectDisposedException("The underlying buffer has already been disposed.");

    internal bool EnsureMinimumCapacity(ref Span<byte> packetSpan, int minimumCapacity, int position)
    {
        int capacity = packetSpan.Length - position;
        if (capacity > minimumCapacity) return true;

        int capacityRequired = packetSpan.Length + (minimumCapacity * (minimumCapacity <= 32 ? 2 : 1)) - capacity;
        capacityRequired = Format.MinBufferSize + Math.Clamp(capacityRequired, Format.MinBufferSize, int.MaxValue);

        Memory<byte> expandedBuffer;
        if (capacityRequired >= MAX_ALLOC_SIZE)
        {
            IMemoryOwner<byte> expandedOwner = MemoryPool<byte>.Shared.Rent(capacityRequired);
            expandedBuffer = expandedOwner.Memory;

            _owner?.Dispose();
            _owner = expandedOwner;
        }
        else expandedBuffer = new byte[capacityRequired];

        Span<byte> expandedBodySpan = expandedBuffer.Span.Slice(Format.MinBufferSize);
        packetSpan.CopyTo(expandedBodySpan);
        packetSpan = expandedBodySpan;

        Buffer = expandedBuffer;
        return false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _owner?.Dispose();
            }
            _disposed = true;
        }
    }
}