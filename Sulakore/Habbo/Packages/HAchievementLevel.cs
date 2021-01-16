using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HAchievementLevel
    {
        public string BadgeId { get; }

        public int Level { get; set; }
        public int PointLimit { get; set; }

        public HAchievementLevel(HReadOnlyPacket packet, string name)
        {
            Level = packet.ReadInt32();
            PointLimit = packet.ReadInt32();

            BadgeId = "ACH_" + name + Level;
        }
    }
}
