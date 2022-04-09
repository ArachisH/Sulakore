using Sulakore.Network.Formats;

namespace Sulakore.Network.Buffers;

public static class EvaWirePacket // : IPacketFactory?
{
    public static HPacket Create(short id, out HPacketWriter packetOut)
    {
        return new HPacket(IHFormat.EvaWire, id, out packetOut);
    }
}