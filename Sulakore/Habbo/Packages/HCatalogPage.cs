using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HCatalogPage
{
    public int Id { get; set; }
    public string CatalogType { get; set; }
    public string LayoutCode { get; set; }

    public string[] Images { get; set; }
    public string[] Texts { get; set; }

    public HCatalogOffer[] Offers { get; set; }

    public int OfferId { get; set; }
    public bool AcceptSeasonCurrencyAsCredits { get; set; }

    public bool IsFrontPage { get; set; }

    public HCatalogPage(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        CatalogType = packet.Read<string>();
        LayoutCode = packet.Read<string>();

        Images = new string[packet.Read<int>()];
        for (int i = 0; i < Images.Length; i++)
        {
            Images[i] = packet.Read<string>();
        }

        Texts = new string[packet.Read<int>()];
        for (int i = 0; i < Texts.Length; i++)
        {
            Texts[i] = packet.Read<string>();
        }

        Offers = new HCatalogOffer[packet.Read<int>()];
        for (int i = 0; i < Offers.Length; i++)
        {
            Offers[i] = new HCatalogOffer(ref packet);
        }

        OfferId = packet.Read<int>();
        AcceptSeasonCurrencyAsCredits = packet.Read<bool>();

        IsFrontPage = packet.Available > 0;
    }
}