using System;
using System.Globalization;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HEntity
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Motto { get; set; }
        public HGender Gender { get; set; }
        public int EntityType { get; set; }
        public string FigureId { get; set; }
        public string FavoriteGroup { get; set; }

        private HPoint _tile;
        public HPoint Tile => _lastUpdate?.Tile ?? _tile;

        public HAction Action => _lastUpdate?.Action ?? HAction.None;
        public bool IsController => _lastUpdate?.IsController ?? false;

        private HEntityUpdate _lastUpdate;
        public HEntityUpdate LastUpdate
        {
            get => _lastUpdate;
            set
            {
                if (value.Index != Index)
                {
                    throw new Exception("Entity update data index does not match with current entity index.");
                }
                _lastUpdate = value;
            }
        }

        public HEntity(HPacket packet)
        {

            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();
            Motto = packet.ReadUTF8();
            FigureId = packet.ReadUTF8();
            Index = packet.ReadInt32();

            _tile = new HPoint(packet.ReadInt32(), packet.ReadInt32(),
                double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture));

            packet.ReadInt32();
            EntityType = packet.ReadInt32();

            switch (EntityType)
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