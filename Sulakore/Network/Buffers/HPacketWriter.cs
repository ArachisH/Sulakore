using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public readonly ref struct HPacketWriter // This should probably also be a non-readonly ref struct, and not keep the position in HPacket since others will have access to the same object.
{
    private readonly IBufferWriter<byte> _output;

    public HFormat Format { get; init; }

    public HPacketWriter(HPacket packet)
        : this(packet.Format, packet)
    { }
    public HPacketWriter(HFormat format, IBufferWriter<byte> output)
    {
        _output = output;

        Format = format;
    }

    public void Write<T>(T value) where T : struct
    {
        int size = Format.GetSize(value);
        Span<byte> buffer = _output.GetSpan(size);
        Format.Write(buffer, value);
    }
    public void Write(ReadOnlySpan<char> value)
    {
        int size = Format.GetSize(value);
        Span<byte> buffer = _output.GetSpan(size);
        Format.Write(buffer, value);
    }
}