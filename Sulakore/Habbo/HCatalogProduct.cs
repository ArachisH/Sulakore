using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HCatalogProduct : HData
    {
        public HProductType Type { get; set; }
        public int ClassId { get; set; }

        public string ExtraData { get; set; }
        public int ProductCount { get; set; }

        public bool IsLimited { get; set; }
        public int LimitedTotal { get; set; }
        public int LimitedRemaining { get; set; }

        public HCatalogProduct(HPacket packet)
            : base(packet)
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

        public override void WriteTo(HPacket packet)
        {
            packet.Write((char)Type);
            switch (Type)
            {
                case HProductType.Badge:
                {
                    packet.Write(ExtraData);
                    break;
                }
                default:
                {
                    packet.Write(ClassId);
                    packet.Write(ExtraData);
                    packet.Write(ProductCount);

                    packet.Write(IsLimited);
                    if (IsLimited)
                    {
                        packet.Write(LimitedTotal);
                        packet.Write(LimitedRemaining);
                    }
                    break;
                }
            }
        }
    }
}