using System.Collections.Generic;

using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HWallItem
    {
        public int Id { get; set; }
        public int TypeId { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public int State { get; set; }
        public string Data { get; set; }
        public string Location { get; set; }
        public int UsagePolicy { get; set; }
        public string Placement { get; set; }
        public int SecondsToExpiration { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        public HWallItem(HPacket packet)
        {
            Id = int.Parse(packet.ReadUTF8());
            TypeId = packet.ReadInt32();
            Location = packet.ReadUTF8();
            Data = packet.ReadUTF8();
            SecondsToExpiration = packet.ReadInt32();
            int local7 = packet.ReadInt32();
            int local8 = packet.ReadInt32();

            if (!float.IsNaN(float.Parse(Data)))
            {
                State = int.Parse(Data);
            }

            string[] locations = Location.Split(' ');
            if (Location.IndexOf(":") == 0)
            {
                //false
                if (locations.Length >= 3)
                {
                    string local14 = locations[0];
                    string local15 = locations[1];
                    Placement = locations[2];
                    if (local14.Length > 3 && local15.Length > 2)
                    {
                        local14 = local14.Substring(3);
                        local15 = local15.Substring(2);

                        locations = local14.Split(',');
                        if (locations.Length >= 2)
                        {
                            int local16 = int.Parse(locations[0]);
                            int local17 = int.Parse(locations[1]);
                            locations = local15.Split(',');
                            if (locations.Length >= 2)
                            {
                                X = int.Parse(locations[0]);
                                Y = int.Parse(locations[1]);
                            }
                        }
                    }
                }
            }
            else if (locations.Length >= 2)
            {
                //true
                Placement = locations[0];
                if (Placement == "rightwall" || Placement == "frontwall")
                {
                    Placement = "r";
                }
                else Placement = "l";

                string[] local21 = locations[1].Split(',');
                if (local21.Length >= 3)
                {
                    Y = float.Parse(local21[0]);
                    Z = float.Parse(local21[1]);
                }
            }
        }

        public static HWallItem[] Parse(HPacket packet)
        {
            int ownersCount = packet.ReadInt32();
            var owners = new Dictionary<int, string>(ownersCount);
            for (int i = 0; i < ownersCount; i++)
            {
                owners.Add(packet.ReadInt32(), packet.ReadUTF8());
            }

            var wallItems = new HWallItem[packet.ReadInt32()];
            for (int i = 0; i < wallItems.Length; i++)
            {
                var wallItem = new HWallItem(packet);
                wallItem.OwnerName = owners[wallItem.OwnerId];

                wallItems[i] = wallItem;
            }
            return wallItems;
        }
    }
}