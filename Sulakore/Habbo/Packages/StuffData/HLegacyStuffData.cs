using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public class HLegacyStuffData : HStuffData
{
    public string Data { get; set; }

    public HLegacyStuffData()
        : base(HStuffDataFormat.Legacy)
    { }
    public HLegacyStuffData(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
        : this()
    {
        Data = format.ReadUTF8(ref packetSpan);
    }
}