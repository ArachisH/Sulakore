using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HMapStuffData : HStuffData
{
    public Dictionary<string, string> Data { get; set; }

    public HMapStuffData()
        : base(HStuffDataFormat.Map)
    { }
    public HMapStuffData(ref HReadOnlyPacket packet)
        : this()
    {
        int length = packet.Read<int>();
        Data = new(length);

        for (int i = 0; i < length; i++)
        {
            Data[packet.Read<string>()] = packet.Read<string>();
        }
    }
}