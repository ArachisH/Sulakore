using Sulakore.Network.Protocol;
using System.Collections.Generic;

namespace Sulakore.Habbo
{
    public class HWallItem : HData
    {
        public int Id { get; set; }
        public int TypeId { get; set; }

        public string Location { get; set; }
        public string State { get; set; }
        public int SecondsToExpiration { get; set; }
        public int UsagePolicy { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        public HWallItem(HPacket packet)
            : base(packet)
        {
            Id = int.Parse(packet.ReadUTF8());
            TypeId = packet.ReadInt32();

            Location = packet.ReadUTF8();
            State = packet.ReadUTF8();
            SecondsToExpiration = packet.ReadInt32();
            UsagePolicy = packet.ReadInt32();

            OwnerId = packet.ReadInt32();
        }

        public void Update(HWallItem furni)
        {
            Location = furni.Location;
            State = furni.State;
        }

        public override void WriteTo(HPacket packet)
        {
            packet.Write(Id);
            packet.Write(TypeId);

            packet.Write(Location);
            packet.Write(State);

            packet.Write(SecondsToExpiration);
            packet.Write(UsagePolicy);
            packet.Write(OwnerId);
        }

        public static HWallItem[] Parse(HPacket packet)
        {
            int ownersCount = packet.ReadInt32();
            var owners = new Dictionary<int, string>(ownersCount);
            for (int i = 0; i < ownersCount; i++)
            {
                owners.Add(packet.ReadInt32(), packet.ReadUTF8());
            }

            var furniture = new HWallItem[packet.ReadInt32()];
            for (int i = 0; i < furniture.Length; i++)
            {
                var furni = new HWallItem(packet);
                furni.OwnerName = owners[furni.OwnerId];

                furniture[i] = furni;
            }
            return furniture;
        }
        public static HPacket ToPacket(ushort packetId, HFormat format, IList<HWallItem> wallItems)
        {
            HPacket packet = format.CreatePacket(packetId);

            packet.Write(0);
            var owners = new Dictionary<int, string>();
            foreach (HWallItem wallItem in wallItems)
            {
                if (owners.ContainsKey(wallItem.OwnerId)) continue;
                owners.Add(wallItem.OwnerId, wallItem.OwnerName);

                packet.Write(wallItem.OwnerId);
                packet.Write(wallItem.OwnerName);
            }
            packet.Write(owners.Count, 0);

            packet.Write(wallItems.Count);
            foreach (HWallItem wallItem in wallItems)
            {
                wallItem.WriteTo(packet);
            }

            return packet;
        }
    }
}
