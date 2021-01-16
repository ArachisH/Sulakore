using System;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HUserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Motto { get; set; }
        public string Figure { get; set; }
        public DateTime CreationDate { get; set; }
        public int AchievementScore { get; set; }
        public int FriendCount { get; set; }

        public bool IsFriend { get; set; }
        public bool IsFriendRequestSent { get; set; }
        public bool IsOnline { get; set; }

        public HGroupEntry[] Groups { get; set; }

        public int SinceLastAccessInSeconds { get; set; }
        public bool OpenProfileView { get; set; }

        public HUserProfile(HReadOnlyPacket packet)
        {
            Id = packet.ReadInt32();
            Username = packet.ReadString();
            Motto = packet.ReadString();
            Figure = packet.ReadString();
            CreationDate = DateTime.Parse(packet.ReadUTF16());
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
            SinceLastAccessInSeconds = packet.ReadInt32();
            OpenProfileView = packet.ReadBoolean();
        }
    }
}