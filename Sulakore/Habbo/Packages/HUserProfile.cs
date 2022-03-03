using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

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

    public HUserProfile(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Username = packet.Read<string>();
        Motto = packet.Read<string>();
        Figure = packet.Read<string>();
        CreationDate = DateTime.Parse(packet.Read<string>());
        AchievementScore = packet.Read<int>();
        FriendCount = packet.Read<int>();

        IsFriend = packet.Read<bool>();
        IsFriendRequestSent = packet.Read<bool>();
        IsOnline = packet.Read<bool>();

        Groups = new HGroupEntry[packet.Read<int>()];
        for (int i = 0; i < Groups.Length; i++)
        {
            Groups[i] = new HGroupEntry(ref packet);
        }

        SinceLastAccessInSeconds = packet.Read<int>();
        OpenProfileView = packet.Read<bool>();
    }
}