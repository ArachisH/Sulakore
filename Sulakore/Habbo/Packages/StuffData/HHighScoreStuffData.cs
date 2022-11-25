using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public sealed class HHighScoreStuffData : HStuffData
{
    public string State { get; set; }
    public HScoreType ScoreType { get; set; }
    public HScoreClearType ClearType { get; set; }

    public HHighScoreData[] Entries { get; set; }

    public HHighScoreStuffData()
        : base(HStuffDataFormat.HighScore)
    { }
    public HHighScoreStuffData(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
        : this()
    {
        State = format.ReadUTF8(ref packetSpan);
        ScoreType = (HScoreType)format.Read<int>(ref packetSpan);
        ClearType = (HScoreClearType)format.Read<int>(ref packetSpan);

        Entries = new HHighScoreData[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Entries.Length; i++)
        {
            Entries[i] = new HHighScoreData(format, ref packetSpan);
        }
    }
}