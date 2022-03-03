using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HStuffData
{
    public HStuffDataFormat Format { get; set; }
    public HStuffDataFlags Flags { get; set; }

    public int UniqueSerialNumber { get; set; }
    public int UniqueSeriesSize { get; set; }

    public HStuffData()
        : this(HStuffDataFormat.Empty)
    { }
    protected HStuffData(HStuffDataFormat format)
    {
        Format = format;
    }

    public static HStuffData Parse(ref HReadOnlyPacket packet)
    {
        int value = packet.Read<int>();
        HStuffData stuffData = (HStuffDataFormat)(value & 0xFF) switch
        {
            HStuffDataFormat.Legacy => new HLegacyStuffData(ref packet),
            HStuffDataFormat.Map => new HMapStuffData(ref packet),
            HStuffDataFormat.StringArray => new HStringArrayStuffData(ref packet),
            HStuffDataFormat.VoteResult => new HVoteResultStuffData(ref packet),
            HStuffDataFormat.Empty => new HStuffData(),
            HStuffDataFormat.IntArray => new HIntArrayStuffData(ref packet),
            HStuffDataFormat.HighScore => new HHighScoreStuffData(ref packet),
            HStuffDataFormat.Crackable => new HCrackableStuffData(ref packet),
            _ => throw new NotImplementedException((value & 0xFF).ToString()),
        };

        stuffData.Flags = (HStuffDataFlags)(value & 0xFF00);
        if (stuffData.Flags.HasFlag(HStuffDataFlags.HasUniqueSerialNumber))
        {
            stuffData.UniqueSerialNumber = packet.Read<int>();
            stuffData.UniqueSeriesSize = packet.Read<int>();
        }
        return stuffData;
    }
}