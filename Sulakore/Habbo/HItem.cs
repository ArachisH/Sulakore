using System.Collections.Generic;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HItem : HData
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int RoomId { get; set; }
        public int Category { get; set; }
        public string SlotId { get; set; }
        public int SecondsToExpiration { get; set; }
        public bool HasRentPeriodStarted { get; set; }

        public HItem(HPacket packet)
        {
            packet.ReadInt32();
            string unknown1 = packet.ReadUTF8();

            Id = packet.ReadInt32();
            TypeId = packet.ReadInt32();
            packet.ReadInt32();

            Category = packet.ReadInt32();
            ReadData(packet, Category);

            packet.ReadBoolean();
            packet.ReadBoolean();
            packet.ReadBoolean();
            packet.ReadBoolean();
            SecondsToExpiration = packet.ReadInt32();

            HasRentPeriodStarted = packet.ReadBoolean();
            RoomId = packet.ReadInt32();

            if (unknown1 == "S")
            {
                SlotId = packet.ReadUTF8();
                packet.ReadInt32();
            }
        }

        public static IEnumerable<HItem> Parse(HPacket packet)
        {
            packet.ReadInt32();
            packet.ReadInt32();
            int itemCount = packet.ReadInt32();
            for (int i = 0; i < itemCount; i++)
            {
                yield return new HItem(packet);
            }
        }
    }
}