using Sulakore.Network.Protocol;
using Sulakore.Habbo.Packages.StuffData;

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

    public HItem(ref HReadOnlyPacket packet)
    {
        RoomItemId = packet.Read<int>();
        Type = (HProductType)packet.Read<string>()[0];
        Id = packet.Read<int>();
        TypeId = packet.Read<int>();
        Category = (HFurniCategory)packet.Read<int>();

        StuffData = HStuffData.Parse(ref packet);

        IsRecyclable = packet.Read<bool>();
        IsTradable = packet.Read<bool>();
        IsGroupable = packet.Read<bool>();
        IsSellable = packet.Read<bool>();

        CanPlaceInMarketplace = packet.Read<bool>();
        SecondsToExpiration = packet.Read<int>();

        HasRentPeriodStarted = packet.Read<bool>();
        RoomId = packet.Read<int>();

        if (Type == HProductType.Stuff)
        {
            SlotId = packet.Read<string>();
            Extra = packet.Read<int>();
        }
    }

    public static HItem[] Parse(ref HReadOnlyPacket packet)
    {
        packet.Read<int>();
        packet.Read<int>();
        var items = new HItem[packet.Read<int>()];
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new HItem(ref packet);
        }
        return items;
    }
}