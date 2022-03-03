using Sulakore.Network.Protocol;

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

    public HCatalogNode(ref HReadOnlyPacket packet)
    {
        Visible = packet.Read<bool>();

        Icon = packet.Read<int>();
        PageId = packet.Read<int>();
        PageName = packet.Read<string>();
        Localization = packet.Read<string>();

        OfferIds = new int[packet.Read<int>()];
        for (int i = 0; i < OfferIds.Length; i++)
        {
            OfferIds[i] = packet.Read<int>();
        }

        Children = new HCatalogNode[packet.Read<int>()];
        for (int i = 0; i < Children.Length; i++)
        {
            Children[i] = new HCatalogNode(ref packet);
        }
    }

    public static HCatalogNode Parse(ref HReadOnlyPacket packet)
    {
        var root = new HCatalogNode(ref packet);
        bool newAdditionsAvailable = packet.Read<bool>();
        string catalogType = packet.Read<string>();

        return root;
    }
}