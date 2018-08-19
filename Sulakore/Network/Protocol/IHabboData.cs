namespace Sulakore.Network.Protocol
{
    public interface IHabboData
    {
        byte[] ToBytes();
        HPacket ToPacket();
        void WriteTo(HPacket packet);
    }
}