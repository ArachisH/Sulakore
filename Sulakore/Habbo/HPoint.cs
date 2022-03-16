using System.Drawing;
using System.Numerics;
using System.Globalization;

using Sulakore.Network.Formats;

namespace Sulakore.Habbo;

/// <summary>
/// Represents an ordered pair of x-, y-, and z- coordinates that defines a point in a three-dimensional space.
/// </summary>
public struct HPoint : IEquatable<HPoint>, IHFormattable, ISpanFormattable
{
    public const float DEFAULT_EPSILON = 0.01f;

    private static readonly HPoint _origin = new();
    public static ref readonly HPoint Origin => ref _origin;
    
    public int X { readonly get; set; }
    public int Y { readonly get; set; }
    public float Z { readonly get; set; }

    public HPoint()
        : this(0, 0, 0)
    { }
    public HPoint(int x, int y)
        : this(x, y, 0)
    { }
    public HPoint(int x, int y, char level)
        : this(x, y, ToZ(level))
    { }
    public HPoint(int x, int y, float z)
        => (X, Y, Z) = (x, y, z);

    public readonly override string ToString()
        => ToString(format: null);
    
    public readonly override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public readonly override bool Equals(object obj)
    {
        if (obj is HPoint other)
            return Equals(other);
        else if (obj is ValueTuple<int, int> t2)
            return Equals(t2);
        else if (obj is ValueTuple<int, int, float> t3)
            return Equals(t3);
        else return false;
    }

    public readonly bool Equals(HPoint other) => Equals(other);
    public readonly bool Equals(HPoint other, float epsilon = DEFAULT_EPSILON) => X == other.X && Y == other.Y
        && Math.Abs(other.Z - Z) < epsilon;

    public readonly bool Equals((int X, int Y) point) => X == point.X && Y == point.Y;
    public readonly bool Equals((int X, int Y, int Z) point, float epsilon = DEFAULT_EPSILON) => X == point.X && Y == point.Y
        && Math.Abs(point.Z - Z) < epsilon;
    public readonly bool Equals((int X, int Y, float Z) point, float epsilon = DEFAULT_EPSILON) => X == point.X && Y == point.Y 
        && Math.Abs(point.Z - Z) < epsilon;

    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider provider = default)
        => destination.TryWrite(CultureInfo.InvariantCulture, $"{X}, {Y}, {Z}", out charsWritten);
    public readonly string ToString(string format, IFormatProvider formatProvider = default)
        => string.Create(CultureInfo.InvariantCulture, stackalloc char[64], $"{X}, {Y}, {Z}");

    public readonly bool TryFormat(Span<byte> destination, IHFormat format, out int bytesWritten, ReadOnlySpan<char> formatString)
        => destination.TryWrite(format, $"{X}{Y}", out bytesWritten);

    public static bool operator !=(HPoint left, HPoint right) => !(left == right);
    public static bool operator ==(HPoint left, HPoint right) => left.Equals(right);

    public static implicit operator Point(HPoint point) => new(point.X, point.Y);
    public static implicit operator HPoint(Point point) => new(point.X, point.Y);

    public static implicit operator Vector2(HPoint point) => new(point.X, point.Y);
    public static implicit operator Vector3(HPoint point) => new(point.X, point.Y, point.Z);

    public static implicit operator (int X, int Y)(HPoint point) => (point.X, point.Y);
    public static implicit operator (int X, int Y, float Z)(HPoint point) => (point.X, point.Y, point.Z);

    public static implicit operator HPoint((int X, int Y) point) => new(point.X, point.Y);
    public static implicit operator HPoint((int X, int Y, float Z) point) => new(point.X, point.Y, point.Z);

    public static HPoint operator +(HPoint point, (int X, int Y) offset) => new(point.X + offset.X, point.Y + offset.Y);
    public static HPoint operator +(HPoint point, (int X, int Y, int Z) offset) => new(point.X + offset.X, point.Y + offset.Y, point.Z + offset.Z);
    public static HPoint operator +(HPoint point, (int X, int Y, float Z) offset) => new(point.X + offset.X, point.Y + offset.Y, point.Z + offset.Z);
    public static HPoint operator +(HPoint left, HPoint right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static HPoint operator -(HPoint point, (int X, int Y) offset) => new(point.X - offset.X, point.Y - offset.Y);
    public static HPoint operator -(HPoint point, (int X, int Y, int Z) offset) => new(point.X - offset.X, point.Y - offset.Y, point.Z - offset.Z);
    public static HPoint operator -(HPoint point, (int X, int Y, float Z) offset) => new(point.X - offset.X, point.Y - offset.Y, point.Z - offset.Z);
    public static HPoint operator -(HPoint left, HPoint right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static char ToLevel(float z)
    {
        if (z >= 0 && z <= 9) return (char)(z + 48);
        if (z >= 10 && z <= 29) return (char)(z + 87);
        return 'x';
    }
    public static float ToZ(char level)
    {
        if (level >= '0' && level <= '9') return level - 48;
        if (level >= 'a' && level <= 't') return level - 87;
        return 0;
    }
}
