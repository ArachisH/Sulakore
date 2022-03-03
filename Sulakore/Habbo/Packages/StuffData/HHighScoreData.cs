using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HHighScoreData
{
    public int Score { get; set; }
    public string[] Users { get; set; }

    public HHighScoreData(ref HReadOnlyPacket packet)
    {
        Score = packet.Read<int>();
        Users = new string[packet.Read<int>()];
        for (int i = 0; i < Users.Length; i++)
        {
            Users[i] = packet.Read<string>();
        }
    }
}