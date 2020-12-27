using System;

using Sulakore.Network.Protocol;

#nullable enable
namespace Sulakore.Habbo.Packages.StuffData
{
    public class HStuffData
    {
        public HStuffDataFormat Format { get; set; }
        public HStuffDataFlags Flags { get; set; }

        public int? UniqueSerialNumber { get; set; }
        public int? UniqueSeriesSize { get; set; }

        public HStuffData()
            : this(HStuffDataFormat.Empty)
        { }
        protected HStuffData(HStuffDataFormat format)
        {
            Format = format;
        }

        public static HStuffData Parse(HPacket packet)
        {
            int value = packet.ReadInt32();
            HStuffData? stuffData = (HStuffDataFormat)(value & 0xFF) switch
            {
                HStuffDataFormat.Legacy => new HLegacyStuffData(packet),
                HStuffDataFormat.Map => new HMapStuffData(packet),
                HStuffDataFormat.StringArray => new HStringArrayStuffData(packet),
                HStuffDataFormat.VoteResult => new HVoteResultStuffData(packet),
                HStuffDataFormat.Empty => new HStuffData(),
                HStuffDataFormat.IntArray => new HIntArrayStuffData(packet),
                HStuffDataFormat.HighScore => new HHighScoreStuffData(packet),
                HStuffDataFormat.Crackable => new HCrackableStuffData(packet),

                _ => throw new NotImplementedException((value & 0xFF).ToString()),
            };

            stuffData.Flags = (HStuffDataFlags)(value & 0xFF00);
            if (stuffData.Flags.HasFlag(HStuffDataFlags.HasUniqueSerialNumber))
            {
                stuffData.UniqueSerialNumber = packet.ReadInt32();
                stuffData.UniqueSeriesSize = packet.ReadInt32();
            }
            return stuffData;
        }
    }
}