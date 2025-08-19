using Domain.ValueObjects;

namespace Application.Statistics.Calculators
{
    public interface ILevelCalculator
    {
        LevelInfo CalculateLevelInfo(int totalXp);
    }
}
