using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public sealed class EvaWirePacket : HPacket
{
    public EvaWirePacket(short id, out HPacketWriter packetOut)
        : base(IHFormat.EvaWire, id, out packetOut)
    { }
}