using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HPoll
    {
        public int Id { get; set; }

        public string StartMessage { get; set; }
        public string EndMessage { get; set; }

        public HPollQuestion[] Questions { get; set; }

        public HPoll(HPacket packet)
        {
            Id = packet.ReadInt32();

            StartMessage = packet.ReadUTF8();
            EndMessage = packet.ReadUTF8();
            
            Questions = new HPollQuestion[packet.ReadInt32()];
            for (int i = 0; i < Questions.Length; i++)
            {
                Questions[i] = new HPollQuestion(packet);
            }
            packet.ReadBoolean();
        }
    }
}
