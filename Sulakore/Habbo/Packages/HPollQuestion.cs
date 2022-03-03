using Sulakore.Network.Protocol;

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

    public HPollQuestion(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        packet.Read<int>();
        Type = (HPollType)packet.Read<int>();
        Text = packet.Read<string>();
        Category = packet.Read<int>();
        AnswerType = packet.Read<int>();

        Choices = new HPollChoice[packet.Read<int>()];
        for (int i = 0; i < Choices.Length; i++)
        {
            Choices[i] = new HPollChoice(ref packet);
        }
    }
}