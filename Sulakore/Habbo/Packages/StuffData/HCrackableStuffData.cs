using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public sealed class HCrackableStuffData : HStuffData
{
    public string State { get; set; }
    public int Hits { get; set; }
    public int Target { get; set; }

    public HCrackableStuffData()
        : base(HStuffDataFormat.Crackable)
    { }
    public HCrackableStuffData(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
        : this()
    {
        State = format.ReadUTF8(ref packetSpan);
        Hits = format.Read<int>(ref packetSpan);
        Target = format.Read<int>(ref packetSpan);
    }
}