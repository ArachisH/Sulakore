using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public class HPacket : IDisposable
{
    public const int MAX_ALLOC_SIZE = byte.MaxValue;

    private bool _disposed;
    internal Memory<byte> _buffer;
    internal IMemoryOwner<byte> _owner;

    public short Id { get; }
    public HFormat Format { get; }

    public ReadOnlyMemory<byte> Buffer => !_disposed
        ? _buffer
        : throw new ObjectDisposedException("The underlying buffer has already been disposed.");

    public static implicit operator HPacketReader(HPacket packet) => packet.AsReader();

    public HPacket(HFormat format, short id)
    {
        Id = id;
        Format = format;

        _buffer = new byte[format.MinBufferSize];
        Span<byte> bufferSpan = _buffer.Span;

        format.WriteId(bufferSpan, Id);
        format.WriteLength(bufferSpan, format.MinPacketLength);
    }
    /// <summary>
    /// Initializes an instance of <see cref="HPacket"/> with a non-rented heap allocated buffer.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="id"></param>
    /// <param name="buffer"></param>
    protected internal HPacket(HFormat format, short id, Memory<byte> buffer)
    {
        _buffer = buffer;

        Id = id;
        Format = format;
    }
    /// <summary>
    /// Initializes an instance of <see cref="HPacket"/> with a buffer that is owned by a memory pool implementation.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="id"></param>
    /// <param name="length"></param>
    /// <param name="owner"></param>
    protected internal HPacket(HFormat format, short id, int length, IMemoryOwner<byte> owner)
        : this(format, id, owner.Memory[..length])
    {
        _owner = owner;
    }

    public HPacketReader AsReader() => !_disposed
        ? (new(this))
        : throw new ObjectDisposedException("The underlying buffer has already been disposed.");

    internal Memory<byte> EnsureCapacity(int capacity, int position)
    {
        // TODO
        _owner = null;
        _buffer = null;

        return _buffer;
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

    public static HPacket Create(HFormat format, short id, out HPacketWriter packetOut)
    {
        var packet = new HPacket(format, id);
        packetOut = new HPacketWriter(packet);

        return packet;
    }
}