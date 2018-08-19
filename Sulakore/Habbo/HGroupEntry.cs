using System;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HGroupEntry : IHabboData
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

        public void WriteTo(HPacket packet)
        {
            packet.Write(Id);
            packet.Write(Name);
            packet.Write(BadgeCode);
            packet.Write(PrimaryColor);
            packet.Write(SecondaryColor);

            packet.Write(Favorite);
            packet.Write(OwnerId);
            packet.Write(HasForum);
        }
        
        public byte[] ToBytes() => throw new NotImplementedException();
        public HPacket ToPacket() => throw new NotSupportedException();
    }
}
