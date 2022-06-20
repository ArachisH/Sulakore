namespace Sulakore.Habbo.Badge;

public interface IHBadgePart : ISpanFormattable
{
    HBadgeColor Color { get; init; }
    HBadgePosition Position { get; init; }
}