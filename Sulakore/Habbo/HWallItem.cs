using System.Collections.Generic;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HWallItem : HData
    {
        public int Id { get; set; }
        public int TypeId { get; set; }

        public string Location { get; set; }
        public string State { get; set; }
        public int SecondsToExpiration { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        public HWallItem(HPacket packet)
        {
            Id = int.Parse(packet.ReadUTF8());
            TypeId = packet.ReadInt32();

            Location = packet.ReadUTF8();
            State = packet.ReadUTF8();

            SecondsToExpiration = packet.ReadInt32();

            int loc1 = packet.ReadInt32(); //usagePolicy?

            OwnerId = packet.ReadInt32();
        }

        public void Update(HWallItem furni)
        {
            Location = furni.Location;
            State = furni.State;
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
    }
}
