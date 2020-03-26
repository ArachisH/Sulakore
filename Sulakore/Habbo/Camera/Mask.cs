using System.Drawing;

namespace Sulakore.Habbo.Camera
{
    public class Mask
    {
        public string? Name { get; set; }
        public Point Location { get; set; }
        public bool? FlipH { get; set; }
        public bool? FlipV { get; set; }
    }
}