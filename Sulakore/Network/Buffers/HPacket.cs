using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public abstract class HPacket : IDisposable
{
    public const int MAX_ALLOC_SIZE = 256;

    private bool _disposed;
    private IMemoryOwner<byte> _owner;

    public short Id { get; init; }
    public HFormat Format { get; init; }

    public ReadOnlyMemory<byte> Buffer { get; private set; }

    public static implicit operator HPacketReader(HPacket packet) => packet.AsReader();

    /// <summary>
    /// Initializes an instance of <see cref="HPacket"/> with a non-rented heap allocated buffer.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="id"></param>
    /// <param name="buffer"></param>
    protected HPacket(HFormat format, short id, Memory<byte> buffer)
    {
        Id = id;
        Format = format;
        Buffer = buffer;
    }
    /// <summary>
    /// Initializes an expandable instance of <see cref="HPacket"/> that can be written to by accessing the <paramref name="packetOut"/> parameter.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="id"></param>
    /// <param name="packetOut"></param>
    protected HPacket(HFormat format, short id, out HPacketWriter packetOut)
    {
        Id = id;
        Format = format;

        Memory<byte> buffer = new byte[format.MinBufferSize];
        Buffer = buffer;

        Span<byte> packetSpan = buffer.Span;
        format.WriteId(packetSpan, Id);
        format.WriteLength(packetSpan, format.MinPacketLength);

        packetOut = new HPacketWriter(format, packetSpan, this);
    }
    /// <summary>
    /// Initializes an instance of <see cref="HPacket"/> with a buffer that is owned by a memory pool implementation.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="id"></param>
    /// <param name="length"></param>
    /// <param name="owner"></param>
    protected HPacket(HFormat format, short id, int length, IMemoryOwner<byte> owner)
        : this(format, id, owner.Memory[..length])
    {
        _owner = owner;
    }

    public HPacketReader AsReader() => !_disposed
        ? (new(this))
        : throw new ObjectDisposedException("The underlying buffer has already been disposed.");

    internal void EnsureMinimumCapacity(ref Span<byte> packetSpan, int minimumCapacity, int position)
    {
        int capacity = packetSpan.Length - position;
        int capacityRequired = packetSpan.Length + minimumCapacity - capacity;
        if (minimumCapacity > capacity)
        {
            Memory<byte> expandedBuffer;
            if (capacityRequired >= MAX_ALLOC_SIZE)
            {
                IMemoryOwner<byte> expandedOwner = MemoryPool<byte>.Shared.Rent(capacityRequired);
                expandedBuffer = expandedOwner.Memory;

                packetSpan.CopyTo(expandedBuffer.Span);

                _owner?.Dispose();
                _owner = expandedOwner;
            }
            else
            {
                expandedBuffer = new byte[capacityRequired * 2];
                packetSpan.CopyTo(expandedBuffer.Span);
            }
            Buffer = expandedBuffer;
        }
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