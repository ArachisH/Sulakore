using System;
using System.Drawing;
using System.Diagnostics;

namespace Sulakore.Habbo
{
    [DebuggerDisplay(@"\{X = {X} Y = {Y} Z = {Z}\}")]
    public struct HPoint : IEquatable<HPoint>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public double Z { get; set; }
        public readonly bool IsEmpty => Equals(Empty);

        public static readonly HPoint Empty;

        public static bool operator !=(HPoint left, HPoint right) => !(left == right);
        public static bool operator ==(HPoint left, HPoint right) => left.Equals(right);

        public static implicit operator Point(HPoint point) => new Point(point.X, point.Y);
        public static implicit operator HPoint(Point point) => new HPoint(point.X, point.Y);

        public static implicit operator (int x, int y)(HPoint point) => (point.X, point.Y);
        public static implicit operator (int x, int y, double z)(HPoint point) => (point.X, point.Y, point.Z);

        public static implicit operator HPoint((int x, int y) point) => new HPoint(point.x, point.y);
        public static implicit operator HPoint((int x, int y, double z) point) => new HPoint(point.x, point.y, point.z);

        public HPoint(int x, int y)
            : this(x, y, 0)
        { }
        public HPoint(int x, int y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public HPoint(int x, int y, char level)
            : this(x, y, ToZ(level))
        { }

        public readonly override int GetHashCode() => HashCode.Combine(X, Y);
        public readonly override string ToString() => $"{{X={X},Y={Y},Z={Z}}}";

        public readonly bool Equals(HPoint point) => X == point.X && Y == point.Y;
        public readonly override bool Equals(object obj) => obj is HPoint point && Equals(point);

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
}