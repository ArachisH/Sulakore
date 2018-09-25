using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HRoomEntry : HData
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        public int DoorMode { get; set; }
        public int UserCount { get; set; }
        public int MaxUserCount { get; set; }

        public string Description { get; set; }
        public int TradeMode { get; set; }
        public int Ranking { get; set; }
        public int Category { get; set; }
        public int Stars { get; set; }

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

        public HRoomEntry(HPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();

            OwnerId = packet.ReadInt32();
            OwnerName = packet.ReadUTF8();

            DoorMode = packet.ReadInt32();
            UserCount = packet.ReadInt32();
            MaxUserCount = packet.ReadInt32();

            Description = packet.ReadUTF8();
            TradeMode = packet.ReadInt32();
            Ranking = packet.ReadInt32();
            Category = packet.ReadInt32();
            Stars = packet.ReadInt32();

            Tags = new string[packet.ReadInt32()];
            for (int i = 0; i < Tags.Length; i++)
            {
                Tags[i] = packet.ReadUTF8();
            }

            HRoomFlags roomEntryBitmask = (HRoomFlags)packet.ReadInt32();

            if (roomEntryBitmask.HasFlag(HRoomFlags.HasCustomThumbnail))
            {
                ThumbnailUrl = packet.ReadUTF8();
            }
            if (roomEntryBitmask.HasFlag(HRoomFlags.HasGroup))
            {
                GroupId = packet.ReadInt32();
                GroupName = packet.ReadUTF8();
                GroupBadgeCode = packet.ReadUTF8();
            }
            if (roomEntryBitmask.HasFlag(HRoomFlags.HasAdvertisement))
            {
                AdName = packet.ReadUTF8();
                AdDescription = packet.ReadUTF8();
                AdExpiresInMinutes = packet.ReadInt32();
            }

            ShowOwner = roomEntryBitmask.HasFlag(HRoomFlags.ShowOwner);
            AllowPets = roomEntryBitmask.HasFlag(HRoomFlags.AllowPets);
            ShowEntryAd = roomEntryBitmask.HasFlag(HRoomFlags.ShowRoomAd);
        }
    }
}