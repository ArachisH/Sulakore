using Sulakore.Network.Formats;

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

    public HCatalogPage(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        CatalogType = format.ReadUTF8(ref packetSpan);
        LayoutCode = format.ReadUTF8(ref packetSpan);

        Images = new string[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Images.Length; i++)
        {
            Images[i] = format.ReadUTF8(ref packetSpan);
        }

        Texts = new string[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Texts.Length; i++)
        {
            Texts[i] = format.ReadUTF8(ref packetSpan);
        }

        Offers = new HCatalogOffer[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Offers.Length; i++)
        {
            Offers[i] = new HCatalogOffer(format, ref packetSpan);
        }

        OfferId = format.Read<int>(ref packetSpan);
        AcceptSeasonCurrencyAsCredits = format.Read<bool>(ref packetSpan);

        IsFrontPage = !packetSpan.IsEmpty;
    }
}