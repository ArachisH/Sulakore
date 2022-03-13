using System.Globalization;

using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HWallItem
{
    public int Id { get; set; }
    public int TypeId { get; set; }

    public HPoint Local { get; set; }
    public HPoint Global { get; set; }

    public float Y { get; set; }
    public float Z { get; set; }

    public int State { get; set; }
    public string Data { get; set; }
    public string Location { get; set; }
    public HUsagePolicy UsagePolicy { get; set; }
    public string Placement { get; set; }
    public int SecondsToExpiration { get; set; }

    public int OwnerId { get; set; }
    public string OwnerName { get; set; }

    public HWallItem(IHFormat format, ref ReadOnlySpan<byte> packetData)
    {
        Id = int.Parse(format.ReadUTF8(ref packetData));
        TypeId = format.Read<int>(ref packetData);
        Location = format.ReadUTF8(ref packetData);
        Data = format.ReadUTF8(ref packetData);
        SecondsToExpiration = format.Read<int>(ref packetData);
        UsagePolicy = (HUsagePolicy)format.Read<int>(ref packetData);
        OwnerId = format.Read<int>(ref packetData);

        if (float.TryParse(Data, out _))
        {
            State = int.Parse(Data);
        }

        string[] locations = Location.Split(' ');
        if (Location.IndexOf(":") == 0 && locations.Length >= 3)
        {
            Placement = locations[2];

            if (locations[0].Length <= 3 || locations[1].Length <= 2) return;
            string firstLoc = locations[0][3..];
            string secondLoc = locations[1][2..];

            locations = firstLoc.Split(',');
            if (locations.Length >= 2)
            {
                Global = new HPoint(int.Parse(locations[0]), int.Parse(locations[1]));
                locations = secondLoc.Split(',');

                if (locations.Length >= 2)
                {
                    Local = new HPoint(int.Parse(locations[0]), int.Parse(locations[1]));
                }
            }
        }
        else if (locations.Length >= 2)
        {
            Placement = locations[0];
            if (Placement == "rightwall" || Placement == "frontwall")
            {
                Placement = "r";
            }
            else Placement = "l";

            string[] coords = locations[1].Split(',');
            if (coords.Length >= 3)
            {
                Y = float.Parse(coords[0], CultureInfo.InvariantCulture);
                Z = float.Parse(coords[1], CultureInfo.InvariantCulture);
            }
        }
    }

    public static HWallItem[] Parse(IHFormat format, ref ReadOnlySpan<byte> packetData)
    {
        int ownersCount = format.Read<int>(ref packetData);
        var owners = new Dictionary<int, string>(ownersCount);
        for (int i = 0; i < ownersCount; i++)
        {
            owners.Add(
                format.Read<int>(ref packetData),
                format.ReadUTF8(ref packetData));
        }

        var wallItems = new HWallItem[format.Read<int>(ref packetData)];
        for (int i = 0; i < wallItems.Length; i++)
        {
            var wallItem = new HWallItem(format, ref packetData);
            wallItem.OwnerName = owners[wallItem.OwnerId];

            wallItems[i] = wallItem;
        }

        return wallItems;
    }
}