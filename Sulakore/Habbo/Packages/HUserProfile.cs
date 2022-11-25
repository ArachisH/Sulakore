using Sulakore.Network.Formats;

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

    public HUserProfile(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Username = format.ReadUTF8(ref packetSpan);
        Motto = format.ReadUTF8(ref packetSpan);
        Figure = format.ReadUTF8(ref packetSpan);
        CreationDate = DateTime.Parse(format.ReadUTF8(ref packetSpan));
        AchievementScore = format.Read<int>(ref packetSpan);
        FriendCount = format.Read<int>(ref packetSpan);

        IsFriend = format.Read<bool>(ref packetSpan);
        IsFriendRequestSent = format.Read<bool>(ref packetSpan);
        IsOnline = format.Read<bool>(ref packetSpan);

        Groups = new HGroupEntry[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Groups.Length; i++)
        {
            Groups[i] = new HGroupEntry(format, ref packetSpan);
        }

        SinceLastAccessInSeconds = format.Read<int>(ref packetSpan);
        OpenProfileView = format.Read<bool>(ref packetSpan);
    }
}