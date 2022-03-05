using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public class HIntArrayStuffData : HStuffData
{
    public int[] Data { get; set; }

    public HIntArrayStuffData()
        : base(HStuffDataFormat.IntArray)
    { }
    public HIntArrayStuffData(HFormat format, ref ReadOnlySpan<byte> packetSpan)
        : this()
    {
        Data = new int[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Data.Length; i++)
        {
            Data[i] = format.Read<int>(ref packetSpan);
        }
    }
}