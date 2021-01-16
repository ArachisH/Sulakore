using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData
{
    public class HLegacyStuffData : HStuffData
    {
        public string Data { get; set; }

        public HLegacyStuffData()
            : base(HStuffDataFormat.Legacy)
        { }
        public HLegacyStuffData(HReadOnlyPacket packet)
            : this()
        {
            Data = packet.ReadString();
        }
    }
}
