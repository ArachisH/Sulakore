using Sulakore.Network.Protocol;

namespace Sulakore.Habbo.Packages
{
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

        public HPetInformation(HPacket packet)
        {
            Id = packet.ReadInt32();
            Name = packet.ReadUTF8();

            Level = packet.ReadInt32();
            MaxLevel = packet.ReadInt32();

            Experience = packet.ReadInt32();
            MaxExperience = packet.ReadInt32();

            Energy = packet.ReadInt32();
            MaxEnergy = packet.ReadInt32();

            Nutrition = packet.ReadInt32();
            MaxNutrition = packet.ReadInt32();

            Scratches = packet.ReadInt32();
            OwnerId = packet.ReadInt32();
            Age = packet.ReadInt32();
            OwnerName = packet.ReadUTF8();

            BreedId = packet.ReadInt32();

            HasFreeSaddle = packet.ReadBoolean();
            IsRiding = packet.ReadBoolean();

            SkillThresholds = new int[packet.ReadInt32()];
            for (int i = 0; i < SkillThresholds.Length; i++)
            {
                SkillThresholds[i] = packet.ReadInt32();
            }
            AccessRights = packet.ReadInt32();

            CanBreed = packet.ReadBoolean();
            CanHarvest = packet.ReadBoolean();
            CanRevive = packet.ReadBoolean();

            RarityLevel = packet.ReadInt32();
            MaxWellBeingSeconds = packet.ReadInt32();
            RemainingWellBeingSeconds = packet.ReadInt32();
            RemainingGrowingSeconds = packet.ReadInt32();

            HasBreedingPermission = packet.ReadBoolean();
        }
    }
}
