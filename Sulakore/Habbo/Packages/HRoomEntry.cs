using Sulakore.Network.Formats;

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

    public HRoomEntry(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Name = format.ReadUTF8(ref packetSpan);

        OwnerId = format.Read<int>(ref packetSpan);
        OwnerName = format.ReadUTF8(ref packetSpan);

        DoorMode = (HDoorMode)format.Read<int>(ref packetSpan);
        UserCount = format.Read<int>(ref packetSpan);
        MaxUserCount = format.Read<int>(ref packetSpan);

        Description = format.ReadUTF8(ref packetSpan);
        TradeMode = (HTradeMode)format.Read<int>(ref packetSpan);
        Ranking = format.Read<int>(ref packetSpan);
        Stars = format.Read<int>(ref packetSpan);
        Category = (HRoomCategory)format.Read<int>(ref packetSpan);

        Tags = new string[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Tags.Length; i++)
        {
            Tags[i] = format.ReadUTF8(ref packetSpan);
        }

        HRoomFlags roomFlags = (HRoomFlags)format.Read<int>(ref packetSpan);

        if (roomFlags.HasFlag(HRoomFlags.HasCustomThumbnail))
        {
            ThumbnailUrl = format.ReadUTF8(ref packetSpan);
        }
        if (roomFlags.HasFlag(HRoomFlags.HasGroup))
        {
            GroupId = format.Read<int>(ref packetSpan);
            GroupName = format.ReadUTF8(ref packetSpan);
            GroupBadgeCode = format.ReadUTF8(ref packetSpan);
        }
        if (roomFlags.HasFlag(HRoomFlags.HasAdvertisement))
        {
            AdName = format.ReadUTF8(ref packetSpan);
            AdDescription = format.ReadUTF8(ref packetSpan);
            AdExpiresInMinutes = format.Read<int>(ref packetSpan);
        }

        ShowOwner = roomFlags.HasFlag(HRoomFlags.ShowOwner);
        AllowPets = roomFlags.HasFlag(HRoomFlags.AllowPets);
        ShowEntryAd = roomFlags.HasFlag(HRoomFlags.ShowRoomAd);
    }
}