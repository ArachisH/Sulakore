namespace Sulakore.Habbo.Camera;

public sealed record TextureColumn
{
    public ICollection<string> AssetNames { get; } = new List<string>();
}