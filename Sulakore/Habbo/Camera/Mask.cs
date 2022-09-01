using System.Drawing;
using System.Text.Json.Serialization;

namespace Sulakore.Habbo.Camera;

public sealed record Mask
{
    public string? Name { get; set; }
    public Point Location { get; set; }
    
    [JsonPropertyName("flipH")]
    public bool? FlipHorizantally { get; set; }

    [JsonPropertyName("flipV")]
    public bool? FlipVertically { get; set; }
}