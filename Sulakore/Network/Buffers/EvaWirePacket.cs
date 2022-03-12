using System.Buffers;

using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public sealed class EvaWirePacket : HPacket
{
    public EvaWirePacket(short id)
        : base(HFormat.EvaWire, id)
    { }
    internal EvaWirePacket(short id, Memory<byte> buffer)
        : base(HFormat.EvaWire, id, buffer)
    { }
    internal EvaWirePacket(short id, int length, IMemoryOwner<byte> owner)
        : base(HFormat.EvaWire, id, length, owner)
    { }

    public static HPacket Create(short id, out HPacketWriter packetOut) => Create(HFormat.EvaWire, id, out packetOut);
}