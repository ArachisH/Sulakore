using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HIntArrayStuffData : HStuffData
{
    public int[] Data { get; set; }

    public HIntArrayStuffData()
        : base(HStuffDataFormat.IntArray)
    { }
    public HIntArrayStuffData(ref HReadOnlyPacket packet)
        : this()
    {
        Data = new int[packet.Read<int>()];
        for (int i = 0; i < Data.Length; i++)
        {
            Data[i] = packet.Read<int>();
        }
    }
}