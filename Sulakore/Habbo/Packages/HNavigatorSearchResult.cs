using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HNavigatorSearchResult
{
    public string SearchCode { get; set; }
    public string Text { get; set; }

    public int ActionAllowed { get; set; }
    public bool ForceClosed { get; set; }
    public int ViewMode { get; set; }

    public HRoomEntry[] Rooms { get; set; }

    public HNavigatorSearchResult(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        SearchCode = format.ReadUTF8(ref packetSpan);
        Text = format.ReadUTF8(ref packetSpan);

        ActionAllowed = format.Read<int>(ref packetSpan);
        ForceClosed = format.Read<bool>(ref packetSpan);
        ViewMode = format.Read<int>(ref packetSpan);

        Rooms = new HRoomEntry[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < Rooms.Length; i++)
        {
            Rooms[i] = new HRoomEntry(format, ref packetSpan);
        }
    }

    public static HNavigatorSearchResult[] Parse(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        string searchCode = format.ReadUTF8(ref packetSpan);
        string filter = format.ReadUTF8(ref packetSpan);

        var results = new HNavigatorSearchResult[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = new HNavigatorSearchResult(format, ref packetSpan);
        }
        return results;
    }
}