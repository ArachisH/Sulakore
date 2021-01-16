using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData
{
    public class HHighScoreStuffData : HStuffData
    {
        public string State { get; set; }
        public HScoreType ScoreType { get; set; }
        public HScoreClearType ClearType { get; set; }

        public HHighScoreData[] Entries { get; set; }

        public HHighScoreStuffData()
            : base(HStuffDataFormat.HighScore)
        { }
        public HHighScoreStuffData(HReadOnlyPacket packet)
            : this()
        {
            State = packet.ReadString();
            ScoreType = (HScoreType)packet.ReadInt32();
            ClearType = (HScoreClearType)packet.ReadInt32();

            Entries = new HHighScoreData[packet.ReadInt32()];
            for (int i = 0; i < Entries.Length; i++)
            {
                Entries[i] = new HHighScoreData(packet);
            }
        }
    }
}
