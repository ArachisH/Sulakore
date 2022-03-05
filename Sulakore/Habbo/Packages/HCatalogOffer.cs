using Sulakore.Network.Formats;

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

    public HCatalogOffer(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        DisplayName = format.ReadUTF8(ref packetSpan);
        IsRentable = format.Read<bool>(ref packetSpan);

        CreditCost = format.Read<int>(ref packetSpan);
        OtherCurrencyCost = format.Read<int>(ref packetSpan);
        OtherCurrencyType = format.Read<int>(ref packetSpan);
        CanGift = format.Read<bool>(ref packetSpan);

        Products = new HCatalogProduct[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Products.Length; i++)
        {
            Products[i] = new HCatalogProduct(format, ref packetSpan);
        }

        ClubLevel = format.Read<int>(ref packetSpan);
        AllowBundle = format.Read<bool>(ref packetSpan);
        IsPet = format.Read<bool>(ref packetSpan);

        PreviewImage = format.ReadUTF8(ref packetSpan);
    }
}