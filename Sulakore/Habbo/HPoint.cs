namespace Sulakore.Habbo
{
    public class HPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public double Z { get; set; }

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

        public static double ToZ(char level)
        {
            if (level >= '0' && level <= '9')
            {
                return (level - 48);
            }
            else if (level >= 'a' && level <= 't')
            {
                return (level - 87);
            }
            return 0;
        }
        public static char ToLevel(double z)
        {
            char level = 'x';
            if (z >= 0 && z <= 9)
            {
                level = (char)(z + 48);
            }
            else if (z >= 10 && z <= 29)
            {
                level = (char)(z + 87);
            }
            return level;
        }

        public bool Equals(HPoint tile)
        {
            return (X == tile.X && Y == tile.Y);
        }
        public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";
    }
}