using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HPollChoice
{
    public string Value { get; set; }
    public string Text { get; set; }
    public int Type { get; set; }

    public HPollChoice(ref HReadOnlyPacket packet)
    {
        Value = packet.Read<string>();
        Text = packet.Read<string>();
        Type = packet.Read<int>();
    }
}