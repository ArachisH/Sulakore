using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public sealed class HHighScoreData
{
    public int Score { get; set; }
    public string[] Users { get; set; }

    public HHighScoreData(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Score = format.Read<int>(ref packetSpan);
        Users = new string[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Users.Length; i++)
        {
            Users[i] = format.ReadUTF8(ref packetSpan);
        }
    }
}