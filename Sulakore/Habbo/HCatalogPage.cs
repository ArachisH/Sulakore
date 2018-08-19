using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HCatalogPage : HData
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

        public HCatalogPage(HPacket packet)
            : base(packet)
        {
            Id = packet.ReadInt32();
            CatalogType = packet.ReadUTF8();
            LayoutCode = packet.ReadUTF8();

            Images = new string[packet.ReadInt32()];
            for (int i = 0; i < Images.Length; i++)
            {
                Images[i] = packet.ReadUTF8();
            }

            Texts = new string[packet.ReadInt32()];
            for (int i = 0; i < Texts.Length; i++)
            {
                Texts[i] = packet.ReadUTF8();
            }

            Offers = new HCatalogOffer[packet.ReadInt32()];
            for (int i = 0; i < Offers.Length; i++)
            {
                Offers[i] = new HCatalogOffer(packet);
            }

            OfferId = packet.ReadInt32();
            AcceptSeasonCurrencyAsCredits = packet.ReadBoolean();

            //TODO: ?????????????
            IsFrontPage = (packet.ReadableBytes > 0);
        }

        public override void WriteTo(HPacket packet)
        {
            packet.Write(Id);
            packet.Write(CatalogType);
            packet.Write(LayoutCode);

            packet.Write(Images.Length);
            foreach (string image in Images)
            {
                packet.Write(image);
            }

            packet.Write(Texts.Length);
            foreach (string text in Texts)
            {
                packet.Write(text);
            }

            packet.Write(Offers.Length);
            foreach (HCatalogOffer offer in Offers)
            {
                offer.WriteTo(packet);
            }

            packet.Write(OfferId);
            packet.Write(AcceptSeasonCurrencyAsCredits);

            // TODO: ????????????????????
            // IsFrontPage??
        }
    }
}