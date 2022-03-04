using System.Drawing;

namespace Sulakore.Habbo;

public struct HPoint : IEquatable<HPoint>, ISpanFormattable
{
    public static readonly HPoint Empty;
    
    public int X { get; init; }
    public int Y { get; init; }
    public double Z { get; init; }

    public readonly bool IsEmpty => Equals(Empty);

    public static bool operator !=(HPoint left, HPoint right) => !(left == right);
    public static bool operator ==(HPoint left, HPoint right) => left.Equals(right);

    public static implicit operator Point(HPoint point) => new(point.X, point.Y);
    public static implicit operator HPoint(Point point) => new(point.X, point.Y);

    public static implicit operator (int x, int y)(HPoint point) => (point.X, point.Y);
    public static implicit operator (int x, int y, double z)(HPoint point) => (point.X, point.Y, point.Z);

    public static implicit operator HPoint((int x, int y) point) => new(point.x, point.y);
    public static implicit operator HPoint((int x, int y, double z) point) => new(point.x, point.y, point.z);

    public HPoint(int x, int y)
        : this(x, y, 0)
    { }
    public HPoint(int x, int y, char level)
        : this(x, y, ToZ(level))
    { }
    public HPoint(int x, int y, double z)
        => (X, Y, Z) = (x, y, z);

    public readonly override int GetHashCode() => HashCode.Combine(X, Y);

    public readonly bool Equals(HPoint point) => X == point.X && Y == point.Y;
    public readonly override bool Equals(object obj) => obj is HPoint point && Equals(point);

    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        => destination.TryWrite($"X={X}, Y={Y}, Z={Z}", out charsWritten);

    public readonly string ToString(string format, IFormatProvider formatProvider)
        => string.Create(formatProvider, $"X={X}, Y={Y}, Z={Z}");
    public readonly override string ToString() => ToString();

    public static char ToLevel(double z)
    {
        if (z >= 0 && z <= 9) return (char)(z + 48);
        if (z >= 10 && z <= 29) return (char)(z + 87);
        return 'x';
    }
    public static double ToZ(char level)
    {
        if (level >= '0' && level <= '9') return level - 48;
        if (level >= 'a' && level <= 't') return level - 87;
        return 0;
    }
}
