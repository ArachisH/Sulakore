using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HRoomEntry
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int OwnerId { get; set; }
    public string OwnerName { get; set; }

    public HDoorMode DoorMode { get; set; }
    public int UserCount { get; set; }
    public int MaxUserCount { get; set; }

    public string Description { get; set; }
    public HTradeMode TradeMode { get; set; }
    public int Ranking { get; set; }
    public int Stars { get; set; }
    public HRoomCategory Category { get; set; }

    public string[] Tags { get; set; }

    public string ThumbnailUrl { get; set; }

    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public string GroupBadgeCode { get; set; }

    public string AdName { get; set; }
    public string AdDescription { get; set; }
    public int AdExpiresInMinutes { get; set; }

    public bool ShowOwner { get; set; }
    public bool AllowPets { get; set; }
    public bool ShowEntryAd { get; set; }

    public HRoomEntry(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Name = packet.Read<string>();

        OwnerId = packet.Read<int>();
        OwnerName = packet.Read<string>();

        DoorMode = (HDoorMode)packet.Read<int>();
        UserCount = packet.Read<int>();
        MaxUserCount = packet.Read<int>();

        Description = packet.Read<string>();
        TradeMode = (HTradeMode)packet.Read<int>();
        Ranking = packet.Read<int>();
        Stars = packet.Read<int>();
        Category = (HRoomCategory)packet.Read<int>();

        Tags = new string[packet.Read<int>()];
        for (int i = 0; i < Tags.Length; i++)
        {
            Tags[i] = packet.Read<string>();
        }

        HRoomFlags roomFlags = (HRoomFlags)packet.Read<int>();

        if (roomFlags.HasFlag(HRoomFlags.HasCustomThumbnail))
        {
            ThumbnailUrl = packet.Read<string>();
        }
        if (roomFlags.HasFlag(HRoomFlags.HasGroup))
        {
            GroupId = packet.Read<int>();
            GroupName = packet.Read<string>();
            GroupBadgeCode = packet.Read<string>();
        }
        if (roomFlags.HasFlag(HRoomFlags.HasAdvertisement))
        {
            AdName = packet.Read<string>();
            AdDescription = packet.Read<string>();
            AdExpiresInMinutes = packet.Read<int>();
        }

        ShowOwner = roomFlags.HasFlag(HRoomFlags.ShowOwner);
        AllowPets = roomFlags.HasFlag(HRoomFlags.AllowPets);
        ShowEntryAd = roomFlags.HasFlag(HRoomFlags.ShowRoomAd);
    }
}