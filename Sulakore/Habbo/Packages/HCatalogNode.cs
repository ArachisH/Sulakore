using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HCatalogNode
    {
        public bool Visible { get; set; }

        public int Icon { get; set; }
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string Localization { get; set; }
        
        public int[] OfferIds { get; set; }
        public HCatalogNode[] Children { get; set; }

        public HCatalogNode(HPacket packet)
        {
            Visible = packet.ReadBoolean();
            
            Icon = packet.ReadInt32();
            PageId = packet.ReadInt32();
            PageName = packet.ReadUTF8();
            Localization = packet.ReadUTF8();

            OfferIds = new int[packet.ReadInt32()];
            for (int i = 0; i < OfferIds.Length; i++)
            {
                OfferIds[i] = packet.ReadInt32();
            }

            Children = new HCatalogNode[packet.ReadInt32()];
            for (int i = 0; i < Children.Length; i++)
            {
                Children[i] = new HCatalogNode(packet);
            }
        }

        public static HCatalogNode Parse(HPacket packet)
        {
            var root = new HCatalogNode(packet);
            bool newAdditionsAvailable = packet.ReadBoolean();
            string catalogType = packet.ReadUTF8();
            
            return root;
        }
    }
}
