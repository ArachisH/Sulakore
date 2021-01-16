using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
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

        public static HPerk[] Parse(HReadOnlyPacket packet)
        {
            var perkAllowances = new HPerk[packet.ReadInt32()];
            for (int i = 0; i < perkAllowances.Length; i++)
            {
                perkAllowances[i] = new HPerk(packet.ReadString(),
                    packet.ReadString(), packet.ReadBoolean());
            }
            return perkAllowances;
        }
    }
}
