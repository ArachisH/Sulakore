using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HPollQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public HPollType Type { get; set; }
        public int Category { get; set; }
        public int AnswerType { get; set; }
        public int AnswerCount { get; set; }

        public HPollChoice[] Choices { get; set; }

        public HPollQuestion(HPacket packet)
        {
            Id = packet.ReadInt32();
            packet.ReadInt32();
            Type = (HPollType)packet.ReadInt32();
            Text = packet.ReadUTF8();
            Category = packet.ReadInt32();
            AnswerType = packet.ReadInt32();

            Choices = new HPollChoice[packet.ReadInt32()];
            for (int i = 0; i < Choices.Length; i++)
            {
                Choices[i] = new HPollChoice(packet);
            }
        }
    }
}
