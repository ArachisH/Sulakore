using Sulakore.Network.Protocol;

namespace Sulakore.Habbo
{
    public class HAchievement
    {
        public string Name { get; set; }
        public HAchievementLevel[] Levels { get; set; }

        public HAchievement(HPacket packet)
        {
            Name = packet.ReadUTF8();
            Levels = new HAchievementLevel[packet.ReadInt32()];
            for (int i = 0; i < Levels.Length; i++)
            {
                Levels[i] = new HAchievementLevel(Name, packet);
            }
        }

        public static HAchievement[] Parse(HPacket packet)
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
