namespace Domain.Common
{
    public class LevelingOptions
    {
        public const string SectionName = "Leveling";

        public int MaxLevel { get; set; } = 10;
        public List<LevelDefinition> Curve { get; set; } = [];
    }

    public class LevelDefinition
    {
        public int Level { get; set; }
        public int RequiredTotalXp { get; set; }
    }
}
