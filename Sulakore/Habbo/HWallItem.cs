using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HWallItem
    {
        public int Id { get; set; }
        public int TypeId { get; set; }

        public string State { get; set; }
        public string Location { get; set; }
        public int UsagePolicy { get; set; }
        public int SecondsToExpiration { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        public HWallItem(HPacket packet)
        {
            Id = int.Parse(packet.ReadUTF8());
            TypeId = packet.ReadInt32();

            Location = packet.ReadUTF8();
            State = packet.ReadUTF8();
            SecondsToExpiration = packet.ReadInt32();
            UsagePolicy = packet.ReadInt32();

            OwnerId = packet.ReadInt32();
        }
    }
}