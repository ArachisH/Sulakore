namespace Sulakore.Habbo.Camera;

public sealed record Filter
{
    public int Alpha { get; set; }
    public string Name { get; set; } = default!;
}