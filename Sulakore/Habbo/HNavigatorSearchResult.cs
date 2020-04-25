using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HNavigatorSearchResult
    {
        public string SearchCode { get; set; }
        public string Text { get; set; }

        public int ActionAllowed { get; set; }
        public bool ForceClosed { get; set; }
        public int ViewMode { get; set; }

        public HRoomEntry[] Rooms { get; set; }

        public HNavigatorSearchResult(HPacket packet)
        {
            SearchCode = packet.ReadUTF8();
            Text = packet.ReadUTF8();

            ActionAllowed = packet.ReadInt32();
            ForceClosed = packet.ReadBoolean();
            ViewMode = packet.ReadInt32();

            Rooms = new HRoomEntry[packet.ReadInt32()];
            for (int i = 0; i < Rooms.Length; i++)
            {
                Rooms[i] = new HRoomEntry(packet);
            }
        }

        public static HNavigatorSearchResult[] Parse(HPacket packet)
        {
            string searchCode = packet.ReadUTF8();
            string filter = packet.ReadUTF8();

            var results = new HNavigatorSearchResult[packet.ReadInt32()];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new HNavigatorSearchResult(packet);
            }
            return results;
        }
    }
}
