using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public abstract class HPacket : IDisposable
{
    public const int MAX_ALLOC_SIZE = 256;

    private bool _disposed;
    private IMemoryOwner<byte> _owner;

    public short Id { get; }
    public IHFormat Format { get; }

    public int Length { get; internal set; }
    internal Memory<byte> Buffer { get; private set; }

    public static implicit operator HPacketReader(HPacket packet) => packet.AsReader();

    public HPacket(IHFormat format, short id)
    {

    }
    public HPacket(IHFormat format, short id, out HPacketWriter packetOut)
    {
        Id = id;
        Format = format;
        Length = format.MinPacketLength;
        Buffer = new byte[format.MinBufferSize];

        packetOut = new HPacketWriter(format, Span<byte>.Empty, this);
    }

    /// <summary>
    /// Initializes an instance of <see cref="HPacket"/> with a non-rented heap allocated buffer.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="id"></param>
    /// <param name="buffer"></param>
    protected HPacket(IHFormat format, short id, Memory<byte> buffer)
    {
        Id = id;
        Buffer = buffer;
        Format = format;
        Length = buffer.Length - format.MinPacketLength;
    }
    /// <summary>
    /// Initializes an instance of <see cref="HPacket"/> with a buffer that is owned by a memory pool implementation.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="id"></param>
    /// <param name="length"></param>
    /// <param name="owner"></param>
    protected HPacket(IHFormat format, short id, int length, IMemoryOwner<byte> owner)
        : this(format, id, owner.Memory[..length])
    {
        _owner = owner;
    }

    public HPacketReader AsReader() => !_disposed
        ? (new(Format, Buffer.Span.Slice(Format.MinBufferSize, Length - Format.MinPacketLength)))
        : throw new ObjectDisposedException("The underlying buffer has already been disposed.");

    internal void EnsureMinimumCapacity(ref Span<byte> packetSpan, int minimumCapacity, int position)
    {
        int capacity = packetSpan.Length - position;
        if (capacity > minimumCapacity) return;

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

        Span<byte> expandedBodySpan = expandedBuffer.Span[Format.MinBufferSize..];

        packetSpan.CopyTo(expandedBodySpan);
        packetSpan = expandedBodySpan;

        Buffer = expandedBuffer;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
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