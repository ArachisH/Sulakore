using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
#nullable enable
    public class HCatalogProduct
    {
        public HProductType Type { get; set; }
        public int? ClassId { get; set; }

        public string ExtraData { get; set; }
        public int ProductCount { get; set; }

        public bool IsLimited { get; set; }
        public int? LimitedTotal { get; set; }
        public int? LimitedRemaining { get; set; }

        public HCatalogProduct(HPacket packet)
        {
            Type = (HProductType)packet.ReadUTF8()[0];
            switch (Type)
            {
                case HProductType.Badge:
                {
                    ExtraData = packet.ReadUTF8();
                    ProductCount = 1;
                    break;
                }
                default:
                {
                    ClassId = packet.ReadInt32();
                    ExtraData = packet.ReadUTF8();
                    ProductCount = packet.ReadInt32();
                    if (IsLimited = packet.ReadBoolean())
                    {
                        LimitedTotal = packet.ReadInt32();
                        LimitedRemaining = packet.ReadInt32();
                    }
                    break;
                }
            }
        }
    }
}