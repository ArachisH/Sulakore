using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HGroupEntry
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string BadgeCode { get; set; }
    public string PrimaryColor { get; set; }
    public string SecondaryColor { get; set; }

    public bool Favorite { get; set; }
    public int OwnerId { get; set; }
    public bool HasForum { get; set; }

    public HGroupEntry(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Name = packet.Read<string>();
        BadgeCode = packet.Read<string>();
        PrimaryColor = packet.Read<string>();
        SecondaryColor = packet.Read<string>();

        Favorite = packet.Read<bool>();
        OwnerId = packet.Read<int>();
        HasForum = packet.Read<bool>();
    }
}