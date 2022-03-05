using Sulakore.Network.Formats;

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

    public HUserSearchResult(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Name = format.ReadUTF8(ref packetSpan);
        Motto = format.ReadUTF8(ref packetSpan);

        IsOnline = format.Read<bool>(ref packetSpan);
        CanFollow = format.Read<bool>(ref packetSpan);

        format.ReadUTF8(ref packetSpan);

        Gender = format.Read<int>(ref packetSpan) == 1 ? HGender.Male : HGender.Female;
        Figure = format.ReadUTF8(ref packetSpan);

        RealName = format.ReadUTF8(ref packetSpan);
    }

    public static (HUserSearchResult[] friends, HUserSearchResult[] others) Parse(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        var friends = new HUserSearchResult[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < friends.Length; i++)
        {
            friends[i] = new HUserSearchResult(format, ref packetSpan);
        }

        var others = new HUserSearchResult[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < others.Length; i++)
        {
            others[i] = new HUserSearchResult(format, ref packetSpan);
        }
        return (friends, others);
    }
}