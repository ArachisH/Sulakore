﻿using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages.StuffData;

public sealed class HVoteResultStuffData : HStuffData
{
    public string State { get; set; }
    public int Result { get; set; }

    public HVoteResultStuffData()
        : base(HStuffDataFormat.VoteResult)
    { }
    public HVoteResultStuffData(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
        : this()
    {
        State = format.ReadUTF8(ref packetSpan);
        Result = format.Read<int>(ref packetSpan);
    }
}