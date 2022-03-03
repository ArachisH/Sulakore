using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HPoll
{
    public int Id { get; set; }

    public string StartMessage { get; set; }
    public string EndMessage { get; set; }

    public HPollQuestion[] Questions { get; set; }

    public HPoll(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();

        StartMessage = packet.Read<string>();
        EndMessage = packet.Read<string>();

        Questions = new HPollQuestion[packet.Read<int>()];
        for (int i = 0; i < Questions.Length; i++)
        {
            Questions[i] = new HPollQuestion(ref packet);
        }
        packet.Read<bool>();
    }
}