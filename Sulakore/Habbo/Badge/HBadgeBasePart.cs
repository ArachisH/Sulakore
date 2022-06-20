using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Sulakore.Habbo.Badge;

public record HBadgeBasePart(HBadgeBase Type, HBadgeColor Color, HBadgePosition Position = default) : IHBadgePart
{
    public static bool TryParse(ReadOnlySpan<char> s, [NotNullWhen(true)] out HBadgeBasePart? value)
    {
        value = default;
        if (s.Length < 6 && s[0] != 'b')
            return false;

        if (!int.TryParse(s.Slice(1, 2), out int type) ||
            !int.TryParse(s.Slice(3, 2), out int color) ||
            !int.TryParse(s.Slice(5, 1), out int position)) return false;

        value = new((HBadgeBase)type, (HBadgeColor)color, (HBadgePosition)position);
        return true;
    }

    public string ToString(string? format, IFormatProvider? provider = default)
        => string.Create(CultureInfo.InvariantCulture, stackalloc char[8], $"b{(int)Type:00}{(int)Color:00}{(int)Position}");

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
        => destination.TryWrite(CultureInfo.InvariantCulture, $"b{(int)Type:00}{(int)Color:00}{(int)Position}", out charsWritten);
}
