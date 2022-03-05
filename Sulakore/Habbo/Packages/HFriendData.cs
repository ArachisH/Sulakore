using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

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

    public HFriendData(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Username = format.ReadUTF8(ref packetSpan);
        Gender = format.Read<int>(ref packetSpan) == 1 ? HGender.Male : HGender.Female;

        IsOnline = format.Read<bool>(ref packetSpan);
        CanFollow = format.Read<bool>(ref packetSpan);
        Figure = format.ReadUTF8(ref packetSpan);
        CategoryId = format.Read<int>(ref packetSpan);
        Motto = format.ReadUTF8(ref packetSpan);
        RealName = format.ReadUTF8(ref packetSpan);
        format.ReadUTF8(ref packetSpan);

        IsPersisted = format.Read<bool>(ref packetSpan);
        format.Read<bool>(ref packetSpan);
        IsPocketHabboUser = format.Read<bool>(ref packetSpan);
        RelationshipStatus = (HRelationship)format.Read<short>(ref packetSpan);
    }

    public static HFriendData[] Parse(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        int removedFriends = format.Read<int>(ref packetSpan);
        int addedFriends = format.Read<int>(ref packetSpan);

        var friends = new HFriendData[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < friends.Length; i++)
        {
            friends[i] = new HFriendData(format, ref packetSpan);
        }
        return friends;
    }
}