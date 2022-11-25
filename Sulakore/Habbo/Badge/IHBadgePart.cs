using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sulakore.Habbo.Badge;

public interface IHBadgePart : ISpanFormattable
{
    HBadgeColor Color { get; init; }
    HBadgePosition Position { get; init; }

    protected static bool TryParse(ReadOnlySpan<char> value, out int partId, out HBadgeColor color, out HBadgePosition position)
    {
        partId = default;
        color = default;
        position = default;

        if (value.Length < 6)
            return false;

        // SAFETY: Reinterpreting the integers to same sized enums on the fly.
        if (int.TryParse(value.Slice(1, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out partId) &&
            int.TryParse(value.Slice(3, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out Unsafe.As<HBadgeColor, int>(ref color)) &&
            int.TryParse(value.Slice(5, 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out Unsafe.As<HBadgePosition, int>(ref position)))
        {
            return true;
        }

        return false;
    }
}