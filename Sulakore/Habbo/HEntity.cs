using System;
using System.Globalization;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HEntity : HData
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public HPoint Tile { get; set; }
        public string Name { get; set; }
        public string Motto { get; set; }
        public HGender Gender { get; set; }
        public string FigureId { get; set; }
        public string FavoriteGroup { get; set; }
        public HEntityUpdate LastUpdate { get; private set; }

        public HEntity(HPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();
            Motto = packet.ReadUTF8();
            FigureId = packet.ReadUTF8();
            Index = packet.ReadInt32();

            Tile = new HPoint(packet.ReadInt32(), packet.ReadInt32(),
                double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture));

            packet.ReadInt32();
            int type = packet.ReadInt32();

            switch (type)
            {
                case 1:
                {
                    Gender = (HGender)packet.ReadUTF8().ToLower()[0];
                    packet.ReadInt32();
                    packet.ReadInt32();
                    FavoriteGroup = packet.ReadUTF8();
                    packet.ReadUTF8();
                    packet.ReadInt32();
                    packet.ReadBoolean();
                    break;
                }
                case 2:
                {
                    packet.ReadInt32();
                    packet.ReadInt32();
                    packet.ReadUTF8();
                    packet.ReadInt32();
                    packet.ReadBoolean();
                    packet.ReadBoolean();
                    packet.ReadBoolean();
                    packet.ReadBoolean();
                    packet.ReadBoolean();
                    packet.ReadBoolean();
                    packet.ReadInt32();
                    packet.ReadUTF8();
                    break;
                }
                case 4:
                {
                    packet.ReadUTF8();
                    packet.ReadInt32();
                    packet.ReadUTF8();
                    for (int j = packet.ReadInt32(); j > 0; j--)
                    {
                        packet.ReadUInt16();
                    }
                    break;
                }
            }
        }

        public void Update(HEntityUpdate update)
        {
            if (!TryUpdate(update))
            {
                throw new ArgumentException("Entity index does not match.", nameof(update));
            }
        }
        public bool TryUpdate(HEntityUpdate update)
        {
            if (Index != update.Index) return false;

            Tile = update.Tile;
            LastUpdate = update;
            return true;
        }

        public static HEntity[] Parse(HPacket packet)
        {
            var entities = new HEntity[packet.ReadInt32()];
            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = new HEntity(packet);
            }
            return entities;
        }
    }
}