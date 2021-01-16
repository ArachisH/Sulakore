using System;
using System.Globalization;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
#nullable enable
    public class HEntity
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Motto { get; set; }
        public HGender Gender { get; set; }
        public HEntityType EntityType { get; set; }
        public string FigureId { get; set; }
        public string? FavoriteGroup { get; set; }

        private HPoint _tile;
        public HPoint Tile => _lastUpdate?.Tile ?? _tile;

        public HAction Action => _lastUpdate?.Action ?? HAction.None;
        public bool IsController => _lastUpdate?.IsController ?? false;

        private HEntityUpdate? _lastUpdate;
        public HEntityUpdate? LastUpdate
        {
            get => _lastUpdate;
            set
            {
                if (value?.Index != Index)
                {
                    throw new Exception("Entity update data index does not match with current entity index.");
                }
                _lastUpdate = value;
            }
        }

        public HEntity(HReadOnlyPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadString();
            Motto = packet.ReadString();
            FigureId = packet.ReadString();
            Index = packet.ReadInt32();

            _tile = new HPoint(packet.ReadInt32(), packet.ReadInt32(),
                double.Parse(packet.ReadUTF16(), provider: CultureInfo.InvariantCulture));

            packet.ReadInt32();
            EntityType = (HEntityType)packet.ReadInt32();

            switch (EntityType)
            {
                case HEntityType.User:
                {
                    Gender = (HGender)char.ToLowerInvariant(packet.ReadUTF16()[0]);
                    packet.ReadInt32();
                    packet.ReadInt32();
                    FavoriteGroup = packet.ReadString();
                    packet.ReadUTF8();
                    packet.ReadInt32();
                    packet.ReadBoolean();
                    break;
                }
                case HEntityType.Pet:
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
                case HEntityType.RentableBot:
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

        public static HEntity[] Parse(HReadOnlyPacket packet)
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