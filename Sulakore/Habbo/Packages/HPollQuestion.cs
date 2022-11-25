using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HPollQuestion
{
    public int Id { get; set; }
    public string Text { get; set; }
    public HPollType Type { get; set; }
    public int Category { get; set; }
    public int AnswerType { get; set; }
    public int AnswerCount { get; set; }

    public HPollChoice[] Choices { get; set; }

    public HPollQuestion(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        format.Read<int>(ref packetSpan);
        Type = (HPollType)format.Read<int>(ref packetSpan);
        Text = format.ReadUTF8(ref packetSpan);
        Category = format.Read<int>(ref packetSpan);
        AnswerType = format.Read<int>(ref packetSpan);

        Choices = new HPollChoice[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Choices.Length; i++)
        {
            Choices[i] = new HPollChoice(format, ref packetSpan);
        }
    }
}