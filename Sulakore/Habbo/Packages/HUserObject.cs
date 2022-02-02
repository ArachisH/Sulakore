using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HUserObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Figure { get; set; }
        public HGender Gender { get; set; }

        public string CustomData { get; set; }
        public string RealName { get; set; }
        public bool DirectMail { get; set; }

        public int RespectTotal { get; set; }
        public int RespectLeft { get; set; }
        public int ScratchesLeft { get; set; }

        public bool StreamPublishingAllowed { get; set; }
        public DateTime? LastAccess { get; set; }

        public bool NameChangeAllowed { get; set; }
        public bool AccountSafetyLocked { get; set; }

        public HUserObject(HPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();

            Figure = packet.ReadUTF8();
            Gender = (HGender)packet.ReadUTF8()[0];

            CustomData = packet.ReadUTF8();
            RealName = packet.ReadUTF8();
            DirectMail = packet.ReadBoolean();
            
            RespectTotal = packet.ReadInt32();
            RespectLeft = packet.ReadInt32();
            ScratchesLeft = packet.ReadInt32();

            StreamPublishingAllowed = packet.ReadBoolean();

            if (DateTime.TryParse(packet.ReadUTF8(), out DateTime lastAccess)) //only save if parsing was successful to not save a default DateTime value
                LastAccess = lastAccess;

            NameChangeAllowed = packet.ReadBoolean();
            AccountSafetyLocked = packet.ReadBoolean();
        }
    }
}
