using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.StuffData
{
    public class HLegacyStuffData : HStuffData
    {
        public string Data { get; set; }

        public HLegacyStuffData()
            : base(HStuffDataFormat.Legacy)
        { }
        public HLegacyStuffData(HPacket packet)
            : this()
        {
            Data = packet.ReadUTF8();
        }
    }
}
