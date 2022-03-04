﻿using System.Reflection;

namespace Sulakore.Habbo;

public static class HExtensions
{
    private static readonly Random _rng = new();
    private const BindingFlags BINDINGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

    public static HSign GetRandomSign()
    {
        return (HSign)_rng.Next(0, 19);
    }
    public static HTheme GetRandomTheme()
    {
        return (HTheme)((_rng.Next(1, 7) + 2) & 7);
    }

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

        if (value.StartsWith("hh", StringComparison.OrdinalIgnoreCase)) value = value[2..4];
        else if (value.StartsWith("game-", StringComparison.OrdinalIgnoreCase)) value = value[5..7];

        if (value.Length != 2 && value.Length != 5)
        {
            // Slice to the domain
            int hostIndex = value.LastIndexOf("habbo", StringComparison.OrdinalIgnoreCase);
            if (hostIndex != -1)
                value = value[(hostIndex + "habbo".Length)..];

            // Is second-level .com TLD
            int comDotIndex = value.IndexOf("com.", StringComparison.OrdinalIgnoreCase);
            if (comDotIndex != -1)
                value = value[(comDotIndex + "com.".Length)..];
            
            // Corner-case where value was domain including the dot
            if (value[0] == '.') value = value[1..];

            if (value.StartsWith("com", StringComparison.OrdinalIgnoreCase))
                return HHotel.US;
            
            // Slice out rest of value
            value = value[..2];
        }

        if (Enum.TryParse(value, ignoreCase: true, out HHotel hotel))
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

    public static IEnumerable<MemberInfo> GetAllMembers(this Type type)
    {
        return type.Excavate(t => t.GetMembers(BINDINGS));
    }
    public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
    {
        return type.Excavate(t => t.GetMethods(BINDINGS));
    }
    public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
    {
        return type.Excavate(t => t.GetProperties(BINDINGS));
    }
    public static IEnumerable<T> Excavate<T>(this Type type, Func<Type, IEnumerable<T>> excavator)
    {
        IEnumerable<T> excavated = null;
        while (type != null)
        {
            IEnumerable<T> batch = excavator(type);
            excavated = excavated?.Concat(batch) ?? batch;
            type = type.BaseType;
        }
        return excavated;
    }
}
