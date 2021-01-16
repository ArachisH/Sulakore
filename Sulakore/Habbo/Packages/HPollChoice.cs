using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HPollChoice
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }

        public HPollChoice(HReadOnlyPacket packet)
        {
            Value = packet.ReadString();
            Text = packet.ReadString();
            Type = packet.ReadInt32();
        }
    }
}
