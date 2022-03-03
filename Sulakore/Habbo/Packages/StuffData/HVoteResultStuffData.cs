using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages.StuffData;

public class HVoteResultStuffData : HStuffData
{
    public string State { get; set; }
    public int Result { get; set; }

    public HVoteResultStuffData()
        : base(HStuffDataFormat.VoteResult)
    { }
    public HVoteResultStuffData(ref HReadOnlyPacket packet)
        : this()
    {
        State = packet.Read<string>();
        Result = packet.Read<int>();
    }
}