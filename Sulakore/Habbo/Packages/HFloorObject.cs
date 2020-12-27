using System.Globalization;
using System.Collections.Generic;

using Sulakore.Network.Protocol;
using Sulakore.Habbo.Packages.StuffData;

namespace Sulakore.Habbo.Packages
{
#nullable enable
    public class HFloorObject
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public HPoint Tile { get; set; }
        public HDirection Facing { get; set; }

        public double Height { get; set; }
        public int Extra { get; set; }

        public HStuffData StuffData { get; set; }

        public int SecondsToExpiration { get; set; }
        public HUsagePolicy UsagePolicy { get; set; }

        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }

        public string? StaticClass { get; set; }

        public HFloorObject(HPacket packet)
        {
            Id = packet.ReadInt32();
            TypeId = packet.ReadInt32();

            var tile = new HPoint(packet.ReadInt32(), packet.ReadInt32());
            Facing = (HDirection)packet.ReadInt32();

            tile.Z = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);
            Tile = tile;

            Height = double.Parse(packet.ReadUTF8(), CultureInfo.InvariantCulture);
            Extra = packet.ReadInt32();

            StuffData = HStuffData.Parse(packet);

            SecondsToExpiration = packet.ReadInt32();
            UsagePolicy = (HUsagePolicy)packet.ReadInt32();

            OwnerId = packet.ReadInt32();
            if (TypeId < 0)
            {
                StaticClass = packet.ReadUTF8();
            }
        }

        public static HFloorObject[] Parse(HPacket packet)
        {
            int ownersCount = packet.ReadInt32();
            var owners = new Dictionary<int, string>(ownersCount);
            for (int i = 0; i < ownersCount; i++)
            {
                owners.Add(packet.ReadInt32(), packet.ReadUTF8());
            }

            var floorObjects = new HFloorObject[packet.ReadInt32()];
            for (int i = 0; i < floorObjects.Length; i++)
            {
                var floorObject = new HFloorObject(packet);
                floorObject.OwnerName = owners[floorObject.OwnerId];

                floorObjects[i] = floorObject;
            }
            return floorObjects;
        }
    }
}