using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HLegacyStuffData : HStuffData
{
    public string Data { get; set; }

    public HLegacyStuffData()
        : base(HStuffDataFormat.Legacy)
    { }
    public HLegacyStuffData(ref HReadOnlyPacket packet)
        : this()
    {
        Data = packet.Read<string>();
    }
}