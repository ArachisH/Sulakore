using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages;

public class HPetInformation
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int Level { get; set; }
    public int MaxLevel { get; set; }

    public int Experience { get; set; }
    public int MaxExperience { get; set; }

    public int Energy { get; set; }
    public int MaxEnergy { get; set; }

    public int Nutrition { get; set; }
    public int MaxNutrition { get; set; }

    public int OwnerId { get; set; }
    public string OwnerName { get; set; }

    public int Scratches { get; set; }
    public int Age { get; set; }
    public int BreedId { get; set; }

    public bool HasFreeSaddle { get; set; }
    public bool IsRiding { get; set; }

    public int[] SkillThresholds { get; set; }
    public int AccessRights { get; set; }

    public bool CanBreed { get; set; }
    public bool CanHarvest { get; set; }
    public bool CanRevive { get; set; }

    public int RarityLevel { get; set; }
    public int MaxWellBeingSeconds { get; set; }
    public int RemainingWellBeingSeconds { get; set; }
    public int RemainingGrowingSeconds { get; set; }
    public bool HasBreedingPermission { get; set; }

    public HPetInformation(ref HReadOnlyPacket packet)
    {
        Id = packet.Read<int>();
        Name = packet.Read<string>();

        Level = packet.Read<int>();
        MaxLevel = packet.Read<int>();

        Experience = packet.Read<int>();
        MaxExperience = packet.Read<int>();

        Energy = packet.Read<int>();
        MaxEnergy = packet.Read<int>();

        Nutrition = packet.Read<int>();
        MaxNutrition = packet.Read<int>();

        Scratches = packet.Read<int>();
        OwnerId = packet.Read<int>();
        Age = packet.Read<int>();
        OwnerName = packet.Read<string>();

        BreedId = packet.Read<int>();

        HasFreeSaddle = packet.Read<bool>();
        IsRiding = packet.Read<bool>();

        SkillThresholds = new int[packet.Read<int>()];
        for (int i = 0; i < SkillThresholds.Length; i++)
        {
            SkillThresholds[i] = packet.Read<int>();
        }
        AccessRights = packet.Read<int>();

        CanBreed = packet.Read<bool>();
        CanHarvest = packet.Read<bool>();
        CanRevive = packet.Read<bool>();

        RarityLevel = packet.Read<int>();
        MaxWellBeingSeconds = packet.Read<int>();
        RemainingWellBeingSeconds = packet.Read<int>();
        RemainingGrowingSeconds = packet.Read<int>();

        HasBreedingPermission = packet.Read<bool>();
    }
}