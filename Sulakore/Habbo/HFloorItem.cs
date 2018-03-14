﻿using System.Globalization;
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

            var loc1 = packet.ReadUTF8();
            var loc3 = packet.ReadInt32();

            Category = packet.ReadInt32();
            Stuff = ReadData(packet, Category);

            var loc4 = packet.ReadInt32();
            var loc5 = packet.ReadInt32();

            OwnerId = packet.ReadInt32();
            if (TypeId < 0)
            {
                var loc6 = packet.ReadUTF8();
            }
        }

        public void Update(HFloorItem furni)
        {
            Tile = furni.Tile;
            Stuff = furni.Stuff;
            Facing = furni.Facing;
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
    }
}