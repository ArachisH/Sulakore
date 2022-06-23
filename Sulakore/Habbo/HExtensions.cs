namespace Sulakore.Habbo;

public static class HExtensions
{
    public static HSign GetRandomSign() => (HSign)Random.Shared.Next(0, 19);
    public static HTheme GetRandomTheme() => (HTheme)(Random.Shared.Next(3, 9) & 7);

    public static HDirection ToLeft(this HDirection facing)
    {
        return (HDirection)(((int)facing - 1) & 7);
    }
    public static HDirection ToRight(this HDirection facing)
    {
        return (HDirection)(((int)facing + 1) & 7);
    }

    public static HHotel ToHotel(this ReadOnlySpan<char> value)
    {
        if (value.Length < 2) return HHotel.Unknown;

        if (value.Length >= 4 && value.StartsWith("hh", StringComparison.OrdinalIgnoreCase)) value = value.Slice(2, 2); // hhxx
        else if (value.Length >= 7 && value.StartsWith("game-", StringComparison.OrdinalIgnoreCase)) value = value.Slice(5, 2); // game-xx

        if (value.Length != 2 && value.Length != 5)
        {
            // Slice to the domain
            int hostIndex = value.LastIndexOf("habbo", StringComparison.OrdinalIgnoreCase);
            if (hostIndex != -1)
                value = value.Slice(hostIndex + "habbo".Length);

            // Is second-level .com TLD
            int comDotIndex = value.IndexOf("com.", StringComparison.OrdinalIgnoreCase);
            if (comDotIndex != -1)
                value = value.Slice(comDotIndex + "com.".Length);
            
            // Corner-case where value was domain including the dot
            if (value[0] == '.') value = value.Slice(1);

            if (value.StartsWith("com", StringComparison.OrdinalIgnoreCase))
                return HHotel.US;
            
            // Slice out rest of value
            value = value.Slice(0, 2);
        }

        if (Enum.TryParse(value, ignoreCase: true, out HHotel hotel) && Enum.IsDefined(hotel))
            return hotel;

        return HHotel.Unknown;
    }

    public static string ToRegion(this HHotel hotel) 
        => hotel.ToString().ToLower();
    public static string ToDomain(this HHotel hotel)
        => hotel switch
        {
            HHotel.TR => "com.tr",
            HHotel.BR => "com.br",
            HHotel.Unknown => throw new ArgumentException($"Hotel cannot be '{nameof(HHotel.Unknown)}'.", nameof(hotel)),
            
            _ => hotel.ToString().ToLower()
        };
    public static Uri ToUri(this HHotel hotel, string subdomain = "www")
    {
        return new Uri($"https://{subdomain}.habbo.{hotel.ToDomain()}");
    }
}