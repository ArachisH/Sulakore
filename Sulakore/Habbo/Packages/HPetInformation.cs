using Sulakore.Network.Formats;

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

    public HPetInformation(HFormat format, ref ReadOnlySpan<byte> packetSpan)
    {
        Id = format.Read<int>(ref packetSpan);
        Name = format.ReadUTF8(ref packetSpan);

        Level = format.Read<int>(ref packetSpan);
        MaxLevel = format.Read<int>(ref packetSpan);

        Experience = format.Read<int>(ref packetSpan);
        MaxExperience = format.Read<int>(ref packetSpan);

        Energy = format.Read<int>(ref packetSpan);
        MaxEnergy = format.Read<int>(ref packetSpan);

        Nutrition = format.Read<int>(ref packetSpan);
        MaxNutrition = format.Read<int>(ref packetSpan);

        Scratches = format.Read<int>(ref packetSpan);
        OwnerId = format.Read<int>(ref packetSpan);
        Age = format.Read<int>(ref packetSpan);
        OwnerName = format.ReadUTF8(ref packetSpan);

        BreedId = format.Read<int>(ref packetSpan);

        HasFreeSaddle = format.Read<bool>(ref packetSpan);
        IsRiding = format.Read<bool>(ref packetSpan);

        SkillThresholds = new int[format.Read<int>(ref packetSpan)];
        for (int i = 0; i < SkillThresholds.Length; i++)
        {
            SkillThresholds[i] = format.Read<int>(ref packetSpan);
        }
        AccessRights = format.Read<int>(ref packetSpan);

        CanBreed = format.Read<bool>(ref packetSpan);
        CanHarvest = format.Read<bool>(ref packetSpan);
        CanRevive = format.Read<bool>(ref packetSpan);

        RarityLevel = format.Read<int>(ref packetSpan);
        MaxWellBeingSeconds = format.Read<int>(ref packetSpan);
        RemainingWellBeingSeconds = format.Read<int>(ref packetSpan);
        RemainingGrowingSeconds = format.Read<int>(ref packetSpan);

        HasBreedingPermission = format.Read<bool>(ref packetSpan);
    }
}