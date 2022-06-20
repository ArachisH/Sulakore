using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Sulakore.Habbo.Badge;

public sealed record HBadgeSymbolPart(HBadgeSymbol Symbol, HBadgeColor Color, HBadgePosition Position) : IHBadgePart
{
    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out HBadgeSymbolPart? symbolPart)
    {
        symbolPart = default;
        if (value.Length < 6)
            return false;

        int type = value[0] - 's';
        if (type != 0 && type != 1)
            return false;

        if (!int.TryParse(value.Slice(1, 2), out int symbolId) ||
            !int.TryParse(value.Slice(3, 2), out int color) ||
            !int.TryParse(value.Slice(5, 1), out int position)) 
            return false;

        // If type == 's', 0-99 else if type == 't', 100-199
        symbolId += type * 100;
        symbolPart = new((HBadgeSymbol)symbolId, (HBadgeColor)color, (HBadgePosition)position);
        return true;
    }

    public string ToString(string? format, IFormatProvider? provider = default)
        => string.Create(CultureInfo.InvariantCulture, stackalloc char[8],
            $"{(char)('s' + (byte)Symbol / 100)}{(int)Symbol % 100:00}{(int)Color:00}{(int)Position}");

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
        => destination.TryWrite(CultureInfo.InvariantCulture,
            $"{(char)('s' + (byte)Symbol / 100)}{(int)Symbol % 100:00}{(int)Color:00}{(int)Position}", out charsWritten);
}
