using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HCatalogOffer : HData
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        
        public bool IsRentable { get; set; }

        public int CreditCost { get; set; }
        public int OtherCurrencyCost { get; set; }
        public int OtherCurrencyType { get; set; }

        public bool CanGift { get; set; }
        
        public HCatalogProduct[] Products { get; set; }
        
        public int ClubLevel { get; set; }
        public bool IsPet { get; set; }
        public bool AllowBundle { get; set; }

        public string PreviewImage { get; set; }

        public HCatalogOffer(HPacket packet)
        {
            Id = packet.ReadInt32();
            DisplayName = packet.ReadUTF8();
            IsRentable = packet.ReadBoolean();

            CreditCost = packet.ReadInt32();
            OtherCurrencyCost = packet.ReadInt32();
            OtherCurrencyType = packet.ReadInt32();
            CanGift = packet.ReadBoolean();

            Products = new HCatalogProduct[packet.ReadInt32()];
            for(int i = 0; i < Products.Length; i++)
            {
                Products[i] = new HCatalogProduct(packet);
            }

            ClubLevel = packet.ReadInt32();
            IsPet = packet.ReadBoolean();
            AllowBundle = packet.ReadBoolean();

            PreviewImage = packet.ReadUTF8();
        }
    }
}
