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
            : base(packet)
        {
            Id = packet.ReadInt32();
            TypeId = packet.ReadInt32();

            var tile = new HPoint(packet.ReadInt32(), packet.ReadInt32());
            Facing = (HDirection)packet.ReadInt32();

            tile.Z = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);
            Tile = tile;

            Remnants.Enqueue(packet.ReadUTF8());
            Remnants.Enqueue(packet.ReadInt32());

            Category = packet.ReadInt32();
            Stuff = ReadData(packet, Category);

            SecondsToExpiration = packet.ReadInt32();
            UsagePolicy = packet.ReadInt32();

            OwnerId = packet.ReadInt32();
            if (TypeId < 0)
            {
                Remnants.Enqueue(packet.ReadUTF8());
            }
        }

        public void Update(HFloorItem furni)
        {
            Tile = furni.Tile;
            Stuff = furni.Stuff;
            Facing = furni.Facing;
        }

        public override void WriteTo(HPacket packet)
        {
            packet.Write(Id);
            packet.Write(TypeId);
            packet.Write(Tile.X, Tile.X);
            packet.Write((int)Facing);
            packet.Write(Tile.Z.ToString(CultureInfo.InvariantCulture));
            packet.Write((string)Remnants.Dequeue());
            packet.Write((int)Remnants.Dequeue());
            packet.Write(Category);
            packet.Write(Format.GetBytes(Stuff));
            packet.Write(SecondsToExpiration);
            packet.Write(UsagePolicy);
            packet.Write(OwnerId);
            if (TypeId < 0)
            {
                packet.Write((string)Remnants.Dequeue());
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

            var furniture = new HFloorItem[packet.ReadInt32()];
            for (int i = 0; i < furniture.Length; i++)
            {
                var furni = new HFloorItem(packet);
                furni.OwnerName = owners[furni.OwnerId];

                furniture[i] = furni;
            }
            return furniture;
        }
        public static HPacket ToPacket(ushort packetId, HFormat format, IList<HFloorItem> floorItems)
        {
            HPacket packet = format.CreatePacket(packetId);

            packet.Write(0);
            var owners = new Dictionary<int, string>();
            foreach (HFloorItem floorItem in floorItems)
            {
                if (owners.ContainsKey(floorItem.OwnerId)) continue;
                owners.Add(floorItem.OwnerId, floorItem.OwnerName);

                packet.Write(floorItem.OwnerId);
                packet.Write(floorItem.OwnerName);
            }
            packet.Write(owners.Count, 0);

            packet.Write(floorItems.Count);
            foreach (HFloorItem floorItem in floorItems)
            {
                floorItem.WriteTo(packet);
            }

            return packet;
        }
    }
}