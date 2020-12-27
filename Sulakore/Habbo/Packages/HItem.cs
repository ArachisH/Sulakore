using Sulakore.Network.Protocol;
using Sulakore.Habbo.Packages.StuffData;

namespace Sulakore.Habbo.Packages
{
#nullable enable
    public class HItem
    {
        public int RoomItemId { get; set; }
        public HProductType Type { get; set; }
        public int Id { get; set; }
        public int TypeId { get; set; }
        public HFurniCategory Category { get; set; }

        public HStuffData StuffData { get; set; }

        public bool IsRecyclable { get; set; }
        public bool IsTradable { get; set; }
        public bool IsGroupable { get; set; }
        public bool IsSellable { get; set; }

        public int SecondsToExpiration { get; set; }
        public bool CanPlaceInMarketplace { get; set; }

        public bool HasRentPeriodStarted { get; set; }
        public int RoomId { get; set; }
        
        public string? SlotId { get; set; }
        public int? Extra { get; set; }

        public HItem(HPacket packet)
        {
            RoomItemId = packet.ReadInt32();
            Type = (HProductType)packet.ReadUTF8()[0];
            Id = packet.ReadInt32();
            TypeId = packet.ReadInt32();
            Category = (HFurniCategory)packet.ReadInt32();

            StuffData = HStuffData.Parse(packet);

            IsRecyclable =  packet.ReadBoolean();
            IsTradable = packet.ReadBoolean();
            IsGroupable = packet.ReadBoolean();
            IsSellable = packet.ReadBoolean();

            CanPlaceInMarketplace = packet.ReadBoolean();
            SecondsToExpiration = packet.ReadInt32();

            HasRentPeriodStarted = packet.ReadBoolean();
            RoomId = packet.ReadInt32();

            if (Type == HProductType.Stuff)
            {
                SlotId = packet.ReadUTF8();
                Extra = packet.ReadInt32();
            }
        }

        public static HItem[] Parse(HPacket packet)
        {
            packet.ReadInt32();
            packet.ReadInt32();
            var items = new HItem[packet.ReadInt32()];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new HItem(packet);
            }
            return items;
        }
    }
}