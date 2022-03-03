using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HUserSearchResult
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Motto { get; set; }

    public bool IsOnline { get; set; }
    public bool CanFollow { get; set; }

    public HGender Gender { get; set; }
    public string Figure { get; set; }

    public string RealName { get; set; }

    public HUserSearchResult(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Name = packet.Read<string>();
        Motto = packet.Read<string>();

        IsOnline = packet.Read<bool>();
        CanFollow = packet.Read<bool>();

        packet.Read<string>();

        Gender = packet.Read<int>() == 1 ? HGender.Male : HGender.Female; //TODO: HExtension, ffs sulake
        Figure = packet.Read<string>();

        RealName = packet.Read<string>();
    }

    public static (HUserSearchResult[] friends, HUserSearchResult[] others) Parse(ref HReadOnlyPacket packet)
    {
        var friends = new HUserSearchResult[packet.Read<int>()];
        for (int i = 0; i < friends.Length; i++)
        {
            friends[i] = new HUserSearchResult(ref packet);
        }

        var others = new HUserSearchResult[packet.Read<int>()];
        for (int i = 0; i < others.Length; i++)
        {
            others[i] = new HUserSearchResult(ref packet);
        }
        return (friends, others);
    }
}