using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public class HMapStuffData : HStuffData
{
    public Dictionary<string, string> Data { get; set; }

    public HMapStuffData()
        : base(HStuffDataFormat.Map)
    { }
    public HMapStuffData(HFormat format, ref ReadOnlySpan<byte> packetSpan)
        : this()
    {
        int length = format.Read<int>(ref packetSpan);
        Data = new(length);

        for (int i = 0; i < length; i++)
        {
            Data[format.ReadUTF8(ref packetSpan)] = format.ReadUTF8(ref packetSpan);
        }
    }
}