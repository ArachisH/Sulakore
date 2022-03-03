using Sulakore.Network.Protocol;

using static Sulakore.Habbo.Packages.HAchievement;

namespace Sulakore.Habbo.Packages;

public sealed record HAchievement(string Name, ACHLevel[] Levels)
{
    public static HAchievement[] Parse(ref HReadOnlyPacket packet)
    {
        var achieve_ments = new HAchievement[packet.Read<int>()];
        for (int i = 0; i < achieve_ments.Length; i++)
        {
            string name = packet.Read<string>();
            var levels = new ACHLevel[packet.Read<int>()];

            achieve_ments[i] = new HAchievement(name, levels);
            for (int j = 0; j < levels.Length; j++)
            {
                int level = packet.Read<int>();
                int pointLimit = packet.Read<int>();

                levels[j] = new ACHLevel(level, pointLimit, $"ACH_{name}{level}");
            }
        }
        return achieve_ments;
    }

    public sealed record ACHLevel(int Level, int PointLimit, string BadgeId);
}