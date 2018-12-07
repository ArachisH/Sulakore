using System;

namespace Sulakore.Habbo
{
    [Flags]
    public enum HRoomFlags
    {
        None = 0,
        HasCustomThumbnail = 1 << 0,
        HasGroup = 1 << 1,
        HasAdvertisement = 1 << 2,
        ShowOwner = 1 << 3,
        AllowPets = 1 << 4,
        ShowRoomAd = 1 << 5
    }
}
