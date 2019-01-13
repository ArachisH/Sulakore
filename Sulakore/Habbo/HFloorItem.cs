using System.Globalization;
using System.Collections.Generic;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HFloorItem : HData
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public HPoint Tile { get; set; }
        public HDirection Facing { get; set; }

        public int Category { get; set; }
        public object[] Stuff { get; set; }

        public int SecondsToExpiration { get; set; }
        public int UsagePolicy { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        public HFloorItem(HPacket packet)
        {
            Id = packet.ReadInt32();
            TypeId = packet.ReadInt32();

            var tile = new HPoint(packet.ReadInt32(), packet.ReadInt32());
            Facing = (HDirection)packet.ReadInt32();

            tile.Z = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);
            Tile = tile;

            packet.ReadUTF8();
            packet.ReadInt32();

            Category = packet.ReadInt32();
            Stuff = ReadData(packet, Category);

            SecondsToExpiration = packet.ReadInt32();
            UsagePolicy = packet.ReadInt32();

            OwnerId = packet.ReadInt32();
            if (TypeId < 0)
            {
                packet.ReadUTF8();
            }
        }

        public static HFloorItem[] Parse(HPacket packet)
        {
            int ownersCount = packet.ReadInt32();
            var owners = new Dictionary<int, string>(ownersCount);
            for (int i = 0; i < ownersCount; i++)
            {
                owners.Add(packet.ReadInt32(), packet.ReadUTF8());
            }

            var floorItems = new HFloorItem[packet.ReadInt32()];
            for (int i = 0; i < floorItems.Length; i++)
            {
                var floorItem = new HFloorItem(packet);
                floorItem.OwnerName = owners[floorItem.OwnerId];

                floorItems[i] = floorItem;
            }
            return floorItems;
        }
    }
}