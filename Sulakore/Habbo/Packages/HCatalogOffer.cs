using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HCatalogOffer
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

    public HCatalogOffer(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        DisplayName = packet.Read<string>();
        IsRentable = packet.Read<bool>();

        CreditCost = packet.Read<int>();
        OtherCurrencyCost = packet.Read<int>();
        OtherCurrencyType = packet.Read<int>();
        CanGift = packet.Read<bool>();

        Products = new HCatalogProduct[packet.Read<int>()];
        for (int i = 0; i < Products.Length; i++)
        {
            Products[i] = new HCatalogProduct(ref packet);
        }

        ClubLevel = packet.Read<int>();
        AllowBundle = packet.Read<bool>();
        IsPet = packet.Read<bool>();

        PreviewImage = packet.Read<string>();
    }
}