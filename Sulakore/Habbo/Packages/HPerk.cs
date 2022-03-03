using Sulakore.Network.Protocol;

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

    public static HPerk[] Parse(ref HReadOnlyPacket packet)
    {
        var perkAllowances = new HPerk[packet.Read<int>()];
        for (int i = 0; i < perkAllowances.Length; i++)
        {
            perkAllowances[i] = new HPerk(packet.Read<string>(),
                packet.Read<string>(), packet.Read<bool>());
        }
        return perkAllowances;
    }
}