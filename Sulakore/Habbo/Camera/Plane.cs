using System.Drawing;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable enable
namespace Sulakore.Habbo.Camera
{
    public class Plane
    {
        public IList<Point> CornerPoints { get; set; } = new List<Point>();

        [JsonPropertyName("bottomAligned")]
        public bool IsBottomAligned { get; set; }

        public IList<Mask>? Masks { get; set; }
        public IList<TextureColumn> TexCols { get; set; } = new List<TextureColumn>();

        public double Z { get; set; }
        public int Color { get; set; }
    }
}