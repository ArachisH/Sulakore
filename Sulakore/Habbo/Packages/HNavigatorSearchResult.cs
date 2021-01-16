using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HNavigatorSearchResult
    {
        public string SearchCode { get; set; }
        public string Text { get; set; }

        public int ActionAllowed { get; set; }
        public bool ForceClosed { get; set; }
        public int ViewMode { get; set; }

        public HRoomEntry[] Rooms { get; set; }

        public HNavigatorSearchResult(HReadOnlyPacket packet)
        {
            SearchCode = packet.ReadString();
            Text = packet.ReadString();

            ActionAllowed = packet.ReadInt32();
            ForceClosed = packet.ReadBoolean();
            ViewMode = packet.ReadInt32();

            Rooms = new HRoomEntry[packet.ReadInt32()];
            for (int i = 0; i < Rooms.Length; i++)
            {
                Rooms[i] = new HRoomEntry(packet);
            }
        }

        public static HNavigatorSearchResult[] Parse(HReadOnlyPacket packet)
        {
            string searchCode = packet.ReadString();
            string filter = packet.ReadString();

            var results = new HNavigatorSearchResult[packet.ReadInt32()];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new HNavigatorSearchResult(packet);
            }
            return results;
        }
    }
}
