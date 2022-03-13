using Sulakore.Network.Formats;

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

    public HGroupEntry(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Name = format.ReadUTF8(ref packetSpan);
        BadgeCode = format.ReadUTF8(ref packetSpan);
        PrimaryColor = format.ReadUTF8(ref packetSpan);
        SecondaryColor = format.ReadUTF8(ref packetSpan);

        Favorite = format.Read<bool>(ref packetSpan);
        OwnerId = format.Read<int>(ref packetSpan);
        HasForum = format.Read<bool>(ref packetSpan);
    }
}