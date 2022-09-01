using System.Drawing;
using System.Text.Json.Serialization;

namespace Sulakore.Habbo.Camera;

public sealed record Sprite
{
    [JsonPropertyName("flipH")]
    public bool? FlipHorizantally { get; set; }

    public int X { get; set; }
    public int? Width { get; set; }
    public int Y { get; set; }
    public int? Height { get; set; }
    public double Z { get; set; }

    public bool? Frame { get; set; }
    public double? Skew { get; set; }
    public int? Alpha { get; set; }

    [JsonConverter(typeof(PhotoColorConverter.Nullable))]
    public Color? Color { get; set; }

    public string BlendMode { get; set; } = default!;
    public string Name { get; set; } = default!;
}