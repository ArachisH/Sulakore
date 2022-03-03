using Sulakore.Network.Protocol;

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

    public HFriendData(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Username = packet.Read<string>();
        Gender = packet.Read<int>() == 1 ? HGender.Male : HGender.Female;

        IsOnline = packet.Read<bool>();
        CanFollow = packet.Read<bool>();
        Figure = packet.Read<string>();
        CategoryId = packet.Read<int>();
        Motto = packet.Read<string>();
        RealName = packet.Read<string>();
        packet.Read<string>();

        IsPersisted = packet.Read<bool>();
        packet.Read<bool>();
        IsPocketHabboUser = packet.Read<bool>();
        RelationshipStatus = (HRelationship)packet.Read<short>();
    }

    public static HFriendData[] Parse(ref HReadOnlyPacket packet)
    {
        int removedFriends = packet.Read<int>();
        int addedFriends = packet.Read<int>();

        var friends = new HFriendData[packet.Read<int>()];
        for (int i = 0; i < friends.Length; i++)
        {
            friends[i] = new HFriendData(ref packet);
        }
        return friends;
    }
}