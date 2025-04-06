namespace Application.Models
{
    public record LevelInfo
    {
        public int CurrentLevel { get; init; }
        public int CurrentLevelRequiredXp { get; init; } // Total XP required to reach the current level
        public int? NextLevelRequiredXp { get; init; } // Total XP required to reach the next level (null if max level)
        public bool IsMaxLevel { get; init; }
    }
}
