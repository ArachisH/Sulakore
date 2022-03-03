using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HHighScoreStuffData : HStuffData
{
    public string State { get; set; }
    public HScoreType ScoreType { get; set; }
    public HScoreClearType ClearType { get; set; }

    public HHighScoreData[] Entries { get; set; }

    public HHighScoreStuffData()
        : base(HStuffDataFormat.HighScore)
    { }
    public HHighScoreStuffData(ref HReadOnlyPacket packet)
        : this()
    {
        State = packet.Read<string>();
        ScoreType = (HScoreType)packet.Read<int>();
        ClearType = (HScoreClearType)packet.Read<int>();

        Entries = new HHighScoreData[packet.Read<int>()];
        for (int i = 0; i < Entries.Length; i++)
        {
            Entries[i] = new HHighScoreData(ref packet);
        }
    }
}