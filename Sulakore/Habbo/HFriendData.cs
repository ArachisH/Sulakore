using Sulakore.Network.Protocol;
using System.Collections.Generic;

namespace Sulakore.Habbo
{
    public class HFriendData : HData
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
            : base(packet)
        {
            Id = packet.ReadInt32();
            Username = packet.ReadUTF8();
            Gender = (packet.ReadInt32() == 1 ? HGender.Male : HGender.Female);

            IsOnline = packet.ReadBoolean();
            CanFollow = packet.ReadBoolean();
            Figure = packet.ReadUTF8();
            CategoryId = packet.ReadInt32();
            Motto = packet.ReadUTF8();
            RealName = packet.ReadUTF8();
            Remnants.Enqueue(packet.ReadUTF8());

            IsPersisted = packet.ReadBoolean();
            Remnants.Enqueue(packet.ReadBoolean());
            IsPocketHabboUser = packet.ReadBoolean();
            RelationshipStatus = (HRelationship)packet.ReadUInt16();
        }

        public void Update(HFriendData friend)
        {
            IsOnline = friend.IsOnline;
            Figure = friend.Figure;
            Motto = friend.Motto;
            RelationshipStatus = friend.RelationshipStatus;
        }

        public override void WriteTo(HPacket packet)
        {
            packet.Write(Id);
            packet.Write(Username);
            packet.Write(Gender == HGender.Male ? 1 : 0); // TODO GEEKER: Check if 0 is for Female, or is it 2? idk

            packet.Write(IsOnline);
            packet.Write(CanFollow);
            packet.Write(Figure);
            packet.Write(CategoryId);
            packet.Write(Motto);
            packet.Write(RealName);
            packet.Write((string)Remnants.Dequeue());

            packet.Write(IsPersisted);
            packet.Write((bool)Remnants.Dequeue());
            packet.Write(IsPocketHabboUser);
            packet.Write((ushort)RelationshipStatus);
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
        public static HPacket ToPacket(ushort packetId, HFormat format, IList<HFriendData> friends)
        {
            HPacket packet = format.CreatePacket(packetId);

            packet.Write(0);
            packet.Write(0);

            packet.Write(friends.Count);
            foreach (HFriendData friend in friends)
            {
                friend.WriteTo(packet);
            }

            return packet;
        }
    }
}
