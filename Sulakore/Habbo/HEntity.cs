using System;
using System.Globalization;
using System.Collections.Generic;

using Sulakore.Network.Protocol;
using System.Linq;

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
        public int EntityType { get; set; }
        public string FigureId { get; set; }
        public string FavoriteGroup { get; set; }
        public HEntityUpdate LastUpdate { get; private set; }

        public HEntity(HPacket packet)
            : base(packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();
            Motto = packet.ReadUTF8();
            FigureId = packet.ReadUTF8();
            Index = packet.ReadInt32();

            Tile = new HPoint(packet.ReadInt32(), packet.ReadInt32(),
                double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture));

            Remnants.Enqueue(packet.ReadInt32());
            EntityType = packet.ReadInt32();

            switch (EntityType)
            {
                case 1:
                {
                    Gender = (HGender)packet.ReadUTF8().ToLower()[0];
                    Remnants.Enqueue(packet.ReadInt32());
                    Remnants.Enqueue(packet.ReadInt32());
                    FavoriteGroup = packet.ReadUTF8();
                    Remnants.Enqueue(packet.ReadUTF8());
                    Remnants.Enqueue(packet.ReadInt32());
                    Remnants.Enqueue(packet.ReadBoolean());
                    break;
                }
                case 2:
                {
                    Remnants.Enqueue(packet.ReadInt32());
                    Remnants.Enqueue(packet.ReadInt32());
                    Remnants.Enqueue(packet.ReadUTF8());
                    Remnants.Enqueue(packet.ReadInt32());
                    Remnants.Enqueue(packet.ReadBoolean());
                    Remnants.Enqueue(packet.ReadBoolean());
                    Remnants.Enqueue(packet.ReadBoolean());
                    Remnants.Enqueue(packet.ReadBoolean());
                    Remnants.Enqueue(packet.ReadBoolean());
                    Remnants.Enqueue(packet.ReadBoolean());
                    Remnants.Enqueue(packet.ReadInt32());
                    Remnants.Enqueue(packet.ReadUTF8());
                    break;
                }
                case 4:
                {
                    Remnants.Enqueue(packet.ReadUTF8());
                    Remnants.Enqueue(packet.ReadInt32());
                    Remnants.Enqueue(packet.ReadUTF8());
                    for (int j = packet.ReadInt32(); j > 0; j--)
                    {
                        Remnants.Enqueue(packet.ReadUInt16());
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

        public override void WriteTo(HPacket packet)
        {
            packet.Write(Id);
            packet.Write(Name);
            packet.Write(Motto);
            packet.Write(FigureId);
            packet.Write(Index);

            packet.Write(Tile.X);
            packet.Write(Tile.Z);
            packet.Write(Tile.Z.ToString(CultureInfo.InvariantCulture));

            packet.Write((int)Remnants.Dequeue());
            packet.Write(EntityType);

            switch (EntityType)
            {
                case 1:
                {
                    packet.Write(Gender.ToString().Substring(0, 1));
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write(FavoriteGroup);
                    packet.Write((string)Remnants.Dequeue());
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write((bool)Remnants.Dequeue());
                    break;
                }
                case 2:
                {
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write((string)Remnants.Dequeue());
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write((bool)Remnants.Dequeue());
                    packet.Write((bool)Remnants.Dequeue());
                    packet.Write((bool)Remnants.Dequeue());
                    packet.Write((bool)Remnants.Dequeue());
                    packet.Write((bool)Remnants.Dequeue());
                    packet.Write((bool)Remnants.Dequeue());
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write((string)Remnants.Dequeue());
                    break;
                }
                case 3:
                {
                    packet.Write((string)Remnants.Dequeue());
                    packet.Write((int)Remnants.Dequeue());
                    packet.Write((string)Remnants.Dequeue());

                    var j = (int)Remnants.Dequeue();
                    packet.Write(j);
                    for (int i = 0; i < j; i++)
                    {
                        packet.Write((ushort)Remnants.Dequeue());
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
        public static HPacket ToPacket(ushort packetId, HFormat format, IList<HEntity> entities)
        {
            HPacket packet = format.CreatePacket(packetId);

            packet.Write(entities.Count);
            foreach (HEntity entity in entities)
            {
                entity.WriteTo(packet);
            }

            return packet;
        }
    }
}