using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HNavigatorSearchResult
{
    public string SearchCode { get; set; }
    public string Text { get; set; }

    public int ActionAllowed { get; set; }
    public bool ForceClosed { get; set; }
    public int ViewMode { get; set; }

    public HRoomEntry[] Rooms { get; set; }

    public HNavigatorSearchResult(ref HReadOnlyPacket packet)
    {
        SearchCode = packet.Read<string>();
        Text = packet.Read<string>();

        ActionAllowed = packet.Read<int>();
        ForceClosed = packet.Read<bool>();
        ViewMode = packet.Read<int>();

        Rooms = new HRoomEntry[packet.Read<int>()];
        for (int i = 0; i < Rooms.Length; i++)
        {
            Rooms[i] = new HRoomEntry(ref packet);
        }
    }

    public static HNavigatorSearchResult[] Parse(ref HReadOnlyPacket packet)
    {
        string searchCode = packet.Read<string>();
        string filter = packet.Read<string>();

        var results = new HNavigatorSearchResult[packet.Read<int>()];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = new HNavigatorSearchResult(ref packet);
        }
        return results;
    }
}