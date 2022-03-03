using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HCrackableStuffData : HStuffData
{
    public string State { get; set; }
    public int Hits { get; set; }
    public int Target { get; set; }

    public HCrackableStuffData()
        : base(HStuffDataFormat.Crackable)
    { }
    public HCrackableStuffData(ref HReadOnlyPacket packet)
        : this()
    {
        State = packet.Read<string>();
        Hits = packet.Read<int>();
        Target = packet.Read<int>();
    }
}