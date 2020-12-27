using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
#nullable enable
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
        
        public string? ThumbnailUrl { get; set; }
        
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupBadgeCode { get; set; }
        
        public string? AdName { get; set; }
        public string? AdDescription { get; set; }
        public int? AdExpiresInMinutes { get; set; }

        public bool ShowOwner { get; set; }
        public bool AllowPets { get; set; }
        public bool ShowEntryAd { get; set; }

        public HRoomEntry(HPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();

            OwnerId = packet.ReadInt32();
            OwnerName = packet.ReadUTF8();

            DoorMode = (HDoorMode)packet.ReadInt32();
            UserCount = packet.ReadInt32();
            MaxUserCount = packet.ReadInt32();

            Description = packet.ReadUTF8();
            TradeMode = (HTradeMode)packet.ReadInt32();
            Ranking = packet.ReadInt32();
            Stars = packet.ReadInt32();
            Category = (HRoomCategory)packet.ReadInt32();

            Tags = new string[packet.ReadInt32()];
            for (int i = 0; i < Tags.Length; i++)
            {
                Tags[i] = packet.ReadUTF8();
            }

            HRoomFlags roomFlags = (HRoomFlags)packet.ReadInt32();

            if (roomFlags.HasFlag(HRoomFlags.HasCustomThumbnail))
            {
                ThumbnailUrl = packet.ReadUTF8();
            }
            if (roomFlags.HasFlag(HRoomFlags.HasGroup))
            {
                GroupId = packet.ReadInt32();
                GroupName = packet.ReadUTF8();
                GroupBadgeCode = packet.ReadUTF8();
            }
            if (roomFlags.HasFlag(HRoomFlags.HasAdvertisement))
            {
                AdName = packet.ReadUTF8();
                AdDescription = packet.ReadUTF8();
                AdExpiresInMinutes = packet.ReadInt32();
            }

            ShowOwner = roomFlags.HasFlag(HRoomFlags.ShowOwner);
            AllowPets = roomFlags.HasFlag(HRoomFlags.AllowPets);
            ShowEntryAd = roomFlags.HasFlag(HRoomFlags.ShowRoomAd);
        }
    }
}