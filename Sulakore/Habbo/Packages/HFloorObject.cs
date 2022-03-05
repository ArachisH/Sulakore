using System.Globalization;

using Sulakore.Habbo.Packages.StuffData;
using Sulakore.Network.Formats;

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

    public HFloorObject(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        TypeId = format.Read<int>(ref packetSpan);

        int x = format.Read<int>(ref packetSpan);
        int y = format.Read<int>(ref packetSpan);
        Facing = (HDirection)format.Read<int>(ref packetSpan);
        Tile = new HPoint(x, y, float.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture));

        Height = double.Parse(format.ReadUTF8(ref packetSpan), CultureInfo.InvariantCulture);
        Extra = format.Read<int>(ref packetSpan);

        StuffData = HStuffData.Parse(format, ref packetSpan);

        SecondsToExpiration = format.Read<int>(ref packetSpan);
        UsagePolicy = (HUsagePolicy)format.Read<int>(ref packetSpan);

        OwnerId = format.Read<int>(ref packetSpan);
        if (TypeId < 0)
        {
            StaticClass = format.ReadUTF8(ref packetSpan);
        }
    }

    public static HFloorObject[] Parse(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        int ownersCount = format.Read<int>(ref packetSpan);
        var owners = new Dictionary<int, string>(ownersCount);
        for (int i = 0; i < ownersCount; i++)
        {
            owners.Add(format.Read<int>(ref packetSpan), format.ReadUTF8(ref packetSpan));
        }

        var floorObjects = new HFloorObject[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < floorObjects.Length; i++)
        {
            var floorObject = new HFloorObject(format, ref packetSpan);
            floorObject.OwnerName = owners[floorObject.OwnerId];

            floorObjects[i] = floorObject;
        }
        return floorObjects;
    }
}