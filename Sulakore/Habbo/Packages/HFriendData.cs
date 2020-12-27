using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HFriendData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public HGender Gender { get; set; }
        public bool IsOnline { get; set; }
        public bool CanFollow { get; set; }
        public string Figure { get; set; }

        public int CategoryId { get; set; }

        public string Motto { get; set; }
        public string RealName { get; set; }

        public bool IsPersisted { get; set; }
        public bool IsPocketHabboUser { get; set; }
        public HRelationship RelationshipStatus { get; set; }

        public HFriendData(HPacket packet)
        {
            Id = packet.ReadInt32();
            Username = packet.ReadUTF8();
            Gender = packet.ReadInt32() == 1 ? HGender.Male : HGender.Female;

            IsOnline = packet.ReadBoolean();
            CanFollow = packet.ReadBoolean();
            Figure = packet.ReadUTF8();
            CategoryId = packet.ReadInt32();
            Motto = packet.ReadUTF8();
            RealName = packet.ReadUTF8();
            packet.ReadUTF8();

            IsPersisted = packet.ReadBoolean();
            packet.ReadBoolean();
            IsPocketHabboUser = packet.ReadBoolean();
            RelationshipStatus = (HRelationship)packet.ReadUInt16();
        }

        public static HFriendData[] Parse(HPacket packet)
        {
            int removedFriends = packet.ReadInt32();
            int addedFriends = packet.ReadInt32();

            var friends = new HFriendData[packet.ReadInt32()];
            for (int i = 0; i < friends.Length; i++)
            {
                friends[i] = new HFriendData(packet);
            }
            return friends;
        }
    }
}
