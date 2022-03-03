using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HUserObject
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Figure { get; set; }
    public HGender Gender { get; set; }

    public string CustomData { get; set; }
    public string RealName { get; set; }
    public bool DirectMail { get; set; }

    public int RespectTotal { get; set; }
    public int RespectLeft { get; set; }
    public int ScratchesLeft { get; set; }

    public bool StreamPublishingAllowed { get; set; }
    public DateTime LastAccess { get; set; }

    public bool NameChangeAllowed { get; set; }
    public bool AccountSafetyLocked { get; set; }

    public HUserObject(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Name = packet.Read<string>();

        Figure = packet.Read<string>();
        Gender = (HGender)packet.Read<string>()[0];

        CustomData = packet.Read<string>();
        RealName = packet.Read<string>();
        DirectMail = packet.Read<bool>();

        RespectTotal = packet.Read<int>();
        RespectLeft = packet.Read<int>();
        ScratchesLeft = packet.Read<int>();

        StreamPublishingAllowed = packet.Read<bool>();

        if (DateTime.TryParse(packet.Read<string>(), out DateTime lastAccess))
        {
            LastAccess = lastAccess;
        }

        NameChangeAllowed = packet.Read<bool>();
        AccountSafetyLocked = packet.Read<bool>();
    }
}