namespace Sulakore.Habbo.Camera
{
    public class Sprite
    {
        public bool? FlipH { get; set; }

        public int X { get; set; }
        public int? Width { get; set; }
        public int Y { get; set; }
        public int? Height { get; set; }
        public double Z { get; set; }

        public bool? Frame { get; set; }
        public double? Skew { get; set; }
        public int? Alpha { get; set; }
        public int? Color { get; set; }
        public string BlendMode { get; set; }
        
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}