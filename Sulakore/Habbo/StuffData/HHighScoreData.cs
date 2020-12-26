using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.StuffData
{
    public class HHighScoreData
    {
        public int Score { get; set; }
        public string[] Users { get; set; }

        public HHighScoreData(HPacket packet)
        {
            Score = packet.ReadInt32();
            Users = new string[packet.ReadInt32()];
            for (int i = 0; i < Users.Length; i++)
            {
                Users[i] = packet.ReadUTF8();
            }
        }
    }
}
