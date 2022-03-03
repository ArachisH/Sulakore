using System.Globalization;

using Sulakore.Network.Protocol;
using Sulakore.Habbo.Packages.StuffData;

namespace Sulakore.Habbo.Packages;

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
    public string OwnerName { get; set; }

    public string StaticClass { get; set; }

    public HFloorObject(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        TypeId = packet.Read<int>();

        var tile = new HPoint(packet.Read<int>(), packet.Read<int>());
        Facing = (HDirection)packet.Read<int>();

        tile.Z = double.Parse(packet.Read<string>(), CultureInfo.InvariantCulture);
        Tile = tile;

        Height = double.Parse(packet.Read<string>(), CultureInfo.InvariantCulture);
        Extra = packet.Read<int>();

        StuffData = HStuffData.Parse(ref packet);

        SecondsToExpiration = packet.Read<int>();
        UsagePolicy = (HUsagePolicy)packet.Read<int>();

        OwnerId = packet.Read<int>();
        if (TypeId < 0)
        {
            StaticClass = packet.Read<string>();
        }
    }

    public static HFloorObject[] Parse(ref HReadOnlyPacket packet)
    {
        int ownersCount = packet.Read<int>();
        var owners = new Dictionary<int, string>(ownersCount);
        for (int i = 0; i < ownersCount; i++)
        {
            owners.Add(packet.Read<int>(), packet.Read<string>());
        }

        var floorObjects = new HFloorObject[packet.Read<int>()];
        for (int i = 0; i < floorObjects.Length; i++)
        {
            var floorObject = new HFloorObject(ref packet);
            floorObject.OwnerName = owners[floorObject.OwnerId];

            floorObjects[i] = floorObject;
        }
        return floorObjects;
    }
}