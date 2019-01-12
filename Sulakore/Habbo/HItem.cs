﻿using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HItem : HData
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int RoomId { get; set; }
        public int Category { get; set; }
        public string SlotId { get; set; }
        public object[] Stuff { get; set; }
        public bool IsTradeable { get; set; }
        public bool IsGroupable { get; set; }
        public int SecondsToExpiration { get; set; }
        public bool HasRentPeriodStarted { get; set; }
        public bool CanPlaceInMarketplace { get; set; }

        public HItem(HPacket packet)
        {
            packet.ReadInt32();

            string loc2 = packet.ReadUTF8();

            Id = packet.ReadInt32();
            TypeId = packet.ReadInt32();
            packet.ReadInt32();

            Category = packet.ReadInt32();
            Stuff = ReadData(packet, Category);

            IsGroupable = packet.ReadBoolean();
            IsTradeable = packet.ReadBoolean();
            packet.ReadBoolean();
            CanPlaceInMarketplace = packet.ReadBoolean();
            SecondsToExpiration = packet.ReadInt32();

            HasRentPeriodStarted = packet.ReadBoolean();
            RoomId = packet.ReadInt32();

            if (loc2 == "S")
            {
                SlotId = packet.ReadUTF8();
                packet.ReadInt32();
            }
        }

        public static HItem[] Parse(HPacket packet)
        {
            packet.ReadInt32();
            packet.ReadInt32();
            var items = new HItem[packet.ReadInt32()];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new HItem(packet);
            }
            return items;
        }
    }
}