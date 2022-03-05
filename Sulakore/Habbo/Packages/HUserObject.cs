using Sulakore.Network.Formats;

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

    public HUserObject(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Name = format.ReadUTF8(ref packetSpan);

        Figure = format.ReadUTF8(ref packetSpan);
        Gender = (HGender)format.ReadUTF8(ref packetSpan)[0];

        CustomData = format.ReadUTF8(ref packetSpan);
        RealName = format.ReadUTF8(ref packetSpan);
        DirectMail = format.Read<bool>(ref packetSpan);

        RespectTotal = format.Read<int>(ref packetSpan);
        RespectLeft = format.Read<int>(ref packetSpan);
        ScratchesLeft = format.Read<int>(ref packetSpan);

        StreamPublishingAllowed = format.Read<bool>(ref packetSpan);

        if (DateTime.TryParse(format.ReadUTF8(ref packetSpan), out DateTime lastAccess))
        {
            LastAccess = lastAccess;
        }

        NameChangeAllowed = format.Read<bool>(ref packetSpan);
        AccountSafetyLocked = format.Read<bool>(ref packetSpan);
    }
}