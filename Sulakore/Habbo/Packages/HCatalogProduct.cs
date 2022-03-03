using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HCatalogProduct
{
    public HProductType Type { get; set; }
    public int? ClassId { get; set; }

    public string ExtraData { get; set; }
    public int ProductCount { get; set; }

    public bool IsLimited { get; set; }
    public int? LimitedTotal { get; set; }
    public int? LimitedRemaining { get; set; }

    public HCatalogProduct(ref HReadOnlyPacket packet)
    {
        Type = (HProductType)packet.Read<string>()[0];
        switch (Type)
        {
            case HProductType.Badge:
            {
                ExtraData = packet.Read<string>();
                ProductCount = 1;
                break;
            }
            default:
            {
                ClassId = packet.Read<int>();
                ExtraData = packet.Read<string>();
                ProductCount = packet.Read<int>();
                if (IsLimited = packet.Read<bool>())
                {
                    LimitedTotal = packet.Read<int>();
                    LimitedRemaining = packet.Read<int>();
                }
                break;
            }
        }
    }
}