using Sulakore.Habbo.Packages.StuffData;
using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HItem
{
    public int RoomItemId { get; set; }
    public HProductType Type { get; set; }
    public int Id { get; set; }
    public int TypeId { get; set; }
    public HFurniCategory Category { get; set; }

    public HStuffData StuffData { get; set; }

    public bool IsRecyclable { get; set; }
    public bool IsTradable { get; set; }
    public bool IsGroupable { get; set; }
    public bool IsSellable { get; set; }

    public int SecondsToExpiration { get; set; }
    public bool CanPlaceInMarketplace { get; set; }

    public bool HasRentPeriodStarted { get; set; }
    public int RoomId { get; set; }

    public string SlotId { get; set; }
    public int Extra { get; set; }

    public HItem(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        RoomItemId = format.Read<int>(ref packetSpan);
        Type = (HProductType)format.ReadUTF8(ref packetSpan)[0];
        Id = format.Read<int>(ref packetSpan);
        TypeId = format.Read<int>(ref packetSpan);
        Category = (HFurniCategory)format.Read<int>(ref packetSpan);

        StuffData = HStuffData.Parse(format, ref packetSpan);

        IsRecyclable = format.Read<bool>(ref packetSpan);
        IsTradable = format.Read<bool>(ref packetSpan);
        IsGroupable = format.Read<bool>(ref packetSpan);
        IsSellable = format.Read<bool>(ref packetSpan);

        CanPlaceInMarketplace = format.Read<bool>(ref packetSpan);
        SecondsToExpiration = format.Read<int>(ref packetSpan);

        HasRentPeriodStarted = format.Read<bool>(ref packetSpan);
        RoomId = format.Read<int>(ref packetSpan);

        if (Type == HProductType.Stuff)
        {
            SlotId = format.ReadUTF8(ref packetSpan);
            Extra = format.Read<int>(ref packetSpan);
        }
    }

    public static HItem[] Parse(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        format.Read<int>(ref packetSpan);
        format.Read<int>(ref packetSpan);
        var items = new HItem[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new HItem(format, ref packetSpan);
        }
        return items;
    }
}