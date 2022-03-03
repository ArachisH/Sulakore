using System.Buffers;

using Sulakore.Network.Protocol;
using Sulakore.Network.Protocol.Formats;

namespace Sulakore.Buffers;

public sealed class HPacket : IMemoryOwner<byte>
{
    private bool _disposed;
    private IMemoryOwner<byte> _owner;

    public Memory<byte> BodyRegion => Memory[4..];
    public Memory<byte> LengthRegion => Memory[..4];

    public IFormat Format { get; }
    public Memory<byte> Memory { get; private set; }

    public HPacket(IFormat format, int minBufferSize = -1)
    {
        _disposed = false;
        _owner = MemoryPool<byte>.Shared.Rent(minBufferSize);

        Memory = _owner.Memory[..minBufferSize];
    }

    //public void Write(params object[] values)
    //{
    //    var packet = new HPacket(_format, 0, Memory.Span);
    //    for (int i = 0; i < values.Length; i++)
    //    {
    //        object value = values[i];
    //        if (value.GetType() == typeof(int))
    //        {
    //            packet.Write((int)value);
    //        }
    //    }
    //}
    public HReadOnlyPacket AsReadOnlyPacket()
    {
        return new HReadOnlyPacket(Format, Memory.Span);
    }

    public void EnsureMinimumCapacity(int minimumCapacity)
    {
        if (minimumCapacity > Memory.Length)
        {
            if (minimumCapacity > _owner.Memory.Length)
            {
                IMemoryOwner<byte> enlargedOwner = MemoryPool<byte>.Shared.Rent(minimumCapacity);
                Memory.CopyTo(enlargedOwner.Memory);

                _owner.Dispose();
                _owner = enlargedOwner;
            }
            Memory = _owner.Memory[..minimumCapacity];
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
                _owner.Dispose();
            }
            _disposed = true;
        }
    }
}