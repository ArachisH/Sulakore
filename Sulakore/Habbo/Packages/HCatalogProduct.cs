using Sulakore.Network.Formats;

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

    public HCatalogProduct(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Type = (HProductType)format.ReadUTF8(ref packetSpan)[0];
        switch (Type)
        {
            case HProductType.Badge:
            {
                ExtraData = format.ReadUTF8(ref packetSpan);
                ProductCount = 1;
                break;
            }
            default:
            {
                ClassId = format.Read<int>(ref packetSpan);
                ExtraData = format.ReadUTF8(ref packetSpan);
                ProductCount = format.Read<int>(ref packetSpan);
                if (IsLimited = format.Read<bool>(ref packetSpan))
                {
                    LimitedTotal = format.Read<int>(ref packetSpan);
                    LimitedRemaining = format.Read<int>(ref packetSpan);
                }
                break;
            }
        }
    }
}