using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public class HStringArrayStuffData : HStuffData
{
    public string[] Data { get; set; }

    public HStringArrayStuffData()
        : base(HStuffDataFormat.StringArray)
    { }
    public HStringArrayStuffData(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
        : this()
    {
        Data = new string[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Data.Length; i++)
        {
            Data[i] = format.ReadUTF8(ref packetSpan);
        }
    }
}