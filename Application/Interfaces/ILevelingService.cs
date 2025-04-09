using Application.Models;

namespace Application.Interfaces
{
    public interface ILevelingService
    {
        LevelInfo CalculateLevelInfo(int totalXp);
    }
}
