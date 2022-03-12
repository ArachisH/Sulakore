using Sulakore.Network.Buffers;
using Sulakore.Network.Formats;

namespace Sulakore.Habbo.Packages;

public sealed class HAchievement
{
    public string Name { get; set; }
    public ACHLevel[] Levels { get; set; }

    public HAchievement(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Name = format.ReadUTF8(ref packetSpan);
        Levels = new ACHLevel[format.Read<int>(ref packetSpan)];
    }

    public static HAchievement[] Parse(HPacket packet)
    {
        return Parse(packet.Format, packet.Buffer.Span);
    }
    public static HAchievement[] Parse(HFormat format, ReadOnlySpan<byte> packetSpan)
    {
        var achievements = new HAchievement[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < achievements.Length; i++)
        {
            var achievement = new HAchievement(format, ref packetSpan);
            achievements[i] = achievement;

            for (int j = 0; j < achievement.Levels.Length; j++)
            {
                var level = new ACHLevel(format, ref packetSpan);
                level.BadgeId = $"ACH_{achievement.Name}{level.Level}";

                achievement.Levels[j] = level;
            }
        }
        return achievements;
    }

    public sealed class ACHLevel
    {
        public int Level { get; set; }
        public int PointLimit { get; set; }
        public string BadgeId { get; set; }

        public ACHLevel(HFormat format, ref ReadOnlySpan<byte> packetSpan)
        {
            Level = format.Read<int>(ref packetSpan);
            PointLimit = format.Read<int>(ref packetSpan);
        }
    }
}