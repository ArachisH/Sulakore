using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HPoll
{
    public int Id { get; set; }

    public string StartMessage { get; set; }
    public string EndMessage { get; set; }

    public HPollQuestion[] Questions { get; set; }

    public HPoll(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);

        StartMessage = format.ReadUTF8(ref packetSpan);
        EndMessage = format.ReadUTF8(ref packetSpan);

        Questions = new HPollQuestion[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Questions.Length; i++)
        {
            Questions[i] = new HPollQuestion(format, ref packetSpan);
        }
        format.Read<bool>(ref packetSpan);
    }
}