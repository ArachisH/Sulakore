using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HPollChoice
{
    public string Value { get; set; }
    public string Text { get; set; }
    public int Type { get; set; }

    public HPollChoice(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Value = format.ReadUTF8(ref packetSpan);
        Text = format.ReadUTF8(ref packetSpan);
        Type = format.Read<int>(ref packetSpan);
    }
}