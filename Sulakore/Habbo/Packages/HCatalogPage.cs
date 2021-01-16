using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
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

        public HCatalogPage(HReadOnlyPacket packet)
        {
            Id = packet.ReadInt32();
            CatalogType = packet.ReadString();
            LayoutCode = packet.ReadString();

            Images = new string[packet.ReadInt32()];
            for (int i = 0; i < Images.Length; i++)
            {
                Images[i] = packet.ReadString();
            }

            Texts = new string[packet.ReadInt32()];
            for (int i = 0; i < Texts.Length; i++)
            {
                Texts[i] = packet.ReadString();
            }

            Offers = new HCatalogOffer[packet.ReadInt32()];
            for (int i = 0; i < Offers.Length; i++)
            {
                Offers[i] = new HCatalogOffer(packet);
            }

            OfferId = packet.ReadInt32();
            AcceptSeasonCurrencyAsCredits = packet.ReadBoolean();
            
            IsFrontPage = packet.Available > 0;
        }
    }
}