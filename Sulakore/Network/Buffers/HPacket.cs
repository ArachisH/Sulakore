using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public sealed class HPacket : IMemoryOwner<byte>, IBufferWriter<byte>
{
    private IMemoryOwner<byte> _owner;

    private bool _disposed;

    public short Id { get; set; }
    public int Length => Format.MinPacketLength + Memory.Length;

    public HFormat Format { get; }
    public Memory<byte> Memory { get; }

    public HPacket(HFormat format, int minBufferSize = -1)
    {
        if (minBufferSize == -1)
        {
            minBufferSize = format.MinBufferSize;
        }
        if (minBufferSize < format.MinBufferSize)
        {
            throw new ArgumentException($"The minimum buffer size must be greater than {format.MinBufferSize}", nameof(minBufferSize));
        }
        _owner = MemoryPool<byte>.Shared.Rent(minBufferSize);

        Memory = _owner.Memory[Format.MinBufferSize..]; // Expose only the body of the packet for reading/writing.
        Format = format;
    }

    public HPacketWriter AsWriter()
    {
        return new HPacketWriter(this);
    }
    public HPacketReader AsPacketReader()
    {
        return new HPacketReader(this);
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
                _owner.Dispose();
            }
            _disposed = true;
        }
    }

    public void Advance(int count)
    {

    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        // TODO: If there is not enough space available in the current buffer, rent a larger one and dispose the current one.
        return Memory[..(sizeHint != 0 ? sizeHint : Memory.Length)];
    }
    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;
}