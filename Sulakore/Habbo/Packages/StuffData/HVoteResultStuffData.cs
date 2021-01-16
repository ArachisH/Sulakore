using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData
{
    public class HVoteResultStuffData : HStuffData
    {
        public string State { get; set; }
        public int Result { get; set; }

        public HVoteResultStuffData()
            : base(HStuffDataFormat.VoteResult)
        { }
        public HVoteResultStuffData(HReadOnlyPacket packet)
            : this()
        {
            State = packet.ReadString();
            Result = packet.ReadInt32();
        }
    }
}
