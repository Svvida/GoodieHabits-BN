namespace Application.Dtos.UserProfileStats
{
    public class XpProgressDto
    {
        public int CurrentXp { get; set; }
        public int Level { get; set; }
        public int NextLevelXpRequirement { get; set; }
        public bool IsMaxLevel { get; set; }
    }
}
