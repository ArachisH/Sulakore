using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HGroupEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BadgeCode { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }

        public bool Favorite { get; set; }
        public int OwnerId { get; set; }
        public bool HasForum { get; set; }

        public HGroupEntry(HPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();
            BadgeCode = packet.ReadUTF8();
            PrimaryColor = packet.ReadUTF8();
            SecondaryColor = packet.ReadUTF8();

            Favorite = packet.ReadBoolean();
            OwnerId = packet.ReadInt32();
            HasForum = packet.ReadBoolean();
        }
    }
}
