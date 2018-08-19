using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HUserProfile : HData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Motto { get; set; }
        public string Figure { get; set; }
        public string CreationDate { get; set; }
        public int AchievementScore { get; set; }
        public int FriendCount { get; set; }

        public bool IsFriend { get; set; }
        public bool IsFriendRequestSent { get; set; }
        public bool IsOnline { get; set; }

        public HGroupEntry[] Groups { get; set; }

        public int LastAccessSinceInSeconds { get; set; }
        public bool OpenProfileView { get; set; }

        public HUserProfile(HPacket packet)
            : base(packet)
        {
            Id = packet.ReadInt32();
            Username = packet.ReadUTF8();
            Motto = packet.ReadUTF8();
            Figure = packet.ReadUTF8();
            CreationDate = packet.ReadUTF8();
            AchievementScore = packet.ReadInt32();
            FriendCount = packet.ReadInt32();

            IsFriend = packet.ReadBoolean();
            IsFriendRequestSent = packet.ReadBoolean();
            IsOnline = packet.ReadBoolean();

            Groups = new HGroupEntry[packet.ReadInt32()];
            for (int i = 0; i < Groups.Length; i++)
            {
                Groups[i] = new HGroupEntry(packet);
            }
            LastAccessSinceInSeconds = packet.ReadInt32();
            OpenProfileView = packet.ReadBoolean();
        }

        public override void WriteTo(HPacket packet)
        {
            packet.Write(Id);
            packet.Write(Username);
            packet.Write(Motto);
            packet.Write(Figure);
            packet.Write(CreationDate);
            packet.Write(AchievementScore);
            packet.Write(FriendCount);

            packet.Write(IsFriend);
            packet.Write(IsFriendRequestSent);
            packet.Write(IsOnline);

            packet.Write(Groups.Length);
            foreach(var group in Groups)
            {
                group.WriteTo(packet);
            }
            packet.Write(LastAccessSinceInSeconds);
            packet.Write(OpenProfileView);
        }
    }
}
