using Domain.Common;

namespace Application.Common.Interfaces
{
    public interface ILevelingService
    {
        LevelInfo CalculateLevelInfo(int totalXp);
        int GetLevelForXp(int totalXp);
        bool IsMaxLevel(int totalXp);
        int GetRequiredXpForNextLevel(int totalXp);
    }
}
