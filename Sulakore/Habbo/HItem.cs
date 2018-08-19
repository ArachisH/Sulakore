using Sulakore.Network.Protocol;
using System.Collections.Generic;

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
        public int SecondsToExpiration { get; set; }
        public bool HasRentPeriodStarted { get; set; }

        public HItem(HPacket packet)
            : base(packet)
        {
            Remnants.Enqueue(packet.ReadInt32());

            string unknown1 = packet.ReadUTF8();
            Remnants.Enqueue(unknown1);

            Id = packet.ReadInt32();
            TypeId = packet.ReadInt32();
            Remnants.Enqueue(packet.ReadInt32());

            Category = packet.ReadInt32();
            Stuff = ReadData(packet, Category);

            Remnants.Enqueue(packet.ReadBoolean());
            Remnants.Enqueue(packet.ReadBoolean());
            Remnants.Enqueue(packet.ReadBoolean());
            Remnants.Enqueue(packet.ReadBoolean());
            SecondsToExpiration = packet.ReadInt32();

            HasRentPeriodStarted = packet.ReadBoolean();
            RoomId = packet.ReadInt32();

            if (unknown1 == "S")
            {
                SlotId = packet.ReadUTF8();
                Remnants.Enqueue(packet.ReadInt32());
            }
        }

        public override void WriteTo(HPacket packet)
        {
            packet.Write((int)Remnants.Dequeue());

            string unknown1 = (string)Remnants.Dequeue();
            packet.Write(unknown1);
        }

        public static HItem[] Parse(HPacket packet)
        {
            int loc1 = packet.ReadInt32();
            int loc2 = packet.ReadInt32();

            var items = new HItem[packet.ReadInt32()];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new HItem(packet);
            }
            return items;
        }
        public static HPacket ToPacket(ushort packetId, HFormat format, IList<HItem> items)
        {
            HPacket packet = format.CreatePacket(packetId);

            packet.Write(0);
            packet.Write(0);

            packet.Write(items.Count);
            foreach (HItem item in items)
            {
                item.WriteTo(packet);
            }

            return packet;
        }
    }
}