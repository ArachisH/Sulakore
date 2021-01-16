using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
    public class HAchievement
    {
        public string Name { get; set; }
        public HAchievementLevel[] Levels { get; set; }

        public HAchievement(HReadOnlyPacket packet)
        {
            Name = packet.ReadString();
            Levels = new HAchievementLevel[packet.ReadInt32()];
            for (int i = 0; i < Levels.Length; i++)
            {
                Levels[i] = new HAchievementLevel(packet, Name);
            }
        }

        public static HAchievement[] Parse(HReadOnlyPacket packet)
        {
            var achievements = new HAchievement[packet.ReadInt32()];
            for (int i = 0; i < achievements.Length; i++)
            {
                achievements[i] = new HAchievement(packet);
            }
            return achievements;
        }
    }
}
