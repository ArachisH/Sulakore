using Sulakore.Network.Formats;

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

    public static HStuffData Parse(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        int value = format.Read<int>(ref packetSpan);
        HStuffData stuffData = (HStuffDataFormat)(value & 0xFF) switch
        {
            HStuffDataFormat.Legacy => new HLegacyStuffData(format, ref packetSpan),
            HStuffDataFormat.Map => new HMapStuffData(format, ref packetSpan),
            HStuffDataFormat.StringArray => new HStringArrayStuffData(format, ref packetSpan),
            HStuffDataFormat.VoteResult => new HVoteResultStuffData(format, ref packetSpan),
            HStuffDataFormat.Empty => new HStuffData(),
            HStuffDataFormat.IntArray => new HIntArrayStuffData(format, ref packetSpan),
            HStuffDataFormat.HighScore => new HHighScoreStuffData(format, ref packetSpan),
            HStuffDataFormat.Crackable => new HCrackableStuffData(format, ref packetSpan),
            _ => throw new NotImplementedException((value & 0xFF).ToString()),
        };

        stuffData.Flags = (HStuffDataFlags)(value & 0xFF00);
        if (stuffData.Flags.HasFlag(HStuffDataFlags.HasUniqueSerialNumber))
        {
            stuffData.UniqueSerialNumber = format.Read<int>(ref packetSpan);
            stuffData.UniqueSeriesSize = format.Read<int>(ref packetSpan);
        }
        return stuffData;
    }
}