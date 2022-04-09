using System.Buffers;
using System.Diagnostics;

namespace Sulakore.Network;

internal static class BufferHelper
{
    [DebuggerStepThrough]
    public static IMemoryOwner<byte> Rent(int minBufferSize, out Memory<byte> trimmedRegion)
    {
        var trimmedOwner = MemoryPool<byte>.Shared.Rent(minBufferSize);
        trimmedRegion = trimmedOwner.Memory[..minBufferSize];
        return trimmedOwner;
    }
}