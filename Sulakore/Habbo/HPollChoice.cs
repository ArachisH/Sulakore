using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HPollChoice
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }

        public HPollChoice(HPacket packet)
        {
            Value = packet.ReadUTF8();
            Text = packet.ReadUTF8();
            Type = packet.ReadInt32();
        }
    }
}
