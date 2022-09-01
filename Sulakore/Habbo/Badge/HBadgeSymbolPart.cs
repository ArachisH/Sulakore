using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Sulakore.Habbo.Badge;

public sealed record HBadgeSymbolPart(HBadgeSymbol Symbol, HBadgeColor Color, HBadgePosition Position) : IHBadgePart
{
    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out HBadgeSymbolPart? symbolPart)
    {
        symbolPart = default;

        if (!IHBadgePart.TryParse(value, out int symbol, out var color, out var position))
            return false;

        // If type is 's', symbolId is in range 0-99.
        // else if the type is 't'; 100-199
        switch (value[0])
        {
            case 't': symbol += 100; break;
            case 's': break;
            default: return false;
        }

        symbolPart = new((HBadgeSymbol)symbol, color, position);
        return true;
    }

    public override string ToString() => ToString(null);

    public string ToString(string? format, IFormatProvider? provider = default)
        => string.Create(CultureInfo.InvariantCulture, stackalloc char[8],
            $"{(char)('s' + (byte)Symbol / 100)}{(int)Symbol % 100:00}{(int)Color:00}{(int)Position}");

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
        => destination.TryWrite(CultureInfo.InvariantCulture,
            $"{(char)('s' + (byte)Symbol / 100)}{(int)Symbol % 100:00}{(int)Color:00}{(int)Position}", out charsWritten);
}
