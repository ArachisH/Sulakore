using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HCatalogNode
{
    public bool Visible { get; set; }

    public int Icon { get; set; }
    public int PageId { get; set; }
    public string PageName { get; set; }
    public string Localization { get; set; }

    public int[] OfferIds { get; set; }
    public HCatalogNode[] Children { get; set; }

    public HCatalogNode(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Visible = format.Read<bool>(ref packetSpan);

        Icon = format.Read<int>(ref packetSpan);
        PageId = format.Read<int>(ref packetSpan);
        PageName = format.ReadUTF8(ref packetSpan);
        Localization = format.ReadUTF8(ref packetSpan);

        OfferIds = new int[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < OfferIds.Length; i++)
        {
            OfferIds[i] = format.Read<int>(ref packetSpan);
        }

        Children = new HCatalogNode[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Children.Length; i++)
        {
            Children[i] = new HCatalogNode(format, ref packetSpan);
        }
    }

    public static HCatalogNode Parse(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        var root = new HCatalogNode(format, ref packetSpan);
        bool newAdditionsAvailable = format.Read<bool>(ref packetSpan);
        string catalogType = format.ReadUTF8(ref packetSpan);
        return root;
    }
}