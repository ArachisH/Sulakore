using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public class HPerk
{
    public string Code { get; set; }
    public string ErrorMessage { get; set; }
    public bool IsAllowed { get; set; }

    public HPerk(string code, string errorMessage, bool isAllowed)
    {
        Code = code;
        ErrorMessage = errorMessage;
        IsAllowed = isAllowed;
    }

    public static HPerk[] Parse(IHFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        var perkAllowances = new HPerk[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < perkAllowances.Length; i++)
        {
            perkAllowances[i] = new HPerk(format.ReadUTF8(ref packetSpan),
                format.ReadUTF8(ref packetSpan), format.Read<bool>(ref packetSpan));
        }
        return perkAllowances;
    }
}