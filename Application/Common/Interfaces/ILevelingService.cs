using Application.Models;

namespace Application.Common.Interfaces
{
    public interface ILevelingService
    {
        LevelInfo CalculateLevelInfo(int totalXp);
    }
}
