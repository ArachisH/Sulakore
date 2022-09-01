using System.Drawing;
using System.Text.Json.Serialization;

#nullable enable
namespace Sulakore.Habbo.Camera;

public sealed record Plane
{
    public IList<Point> CornerPoints { get; set; } = new List<Point>();

    [JsonPropertyName("bottomAligned")]
    public bool IsBottomAligned { get; set; }

    public IList<Mask>? Masks { get; set; }

    [JsonPropertyName("texCols")]
    public IList<TextureColumn> TextureColumns { get; set; } = new List<TextureColumn>();

    public double Z { get; set; }

    [JsonConverter(typeof(PhotoColorConverter))]
    public Color Color { get; set; }
}