using Domain.Common;

namespace Application.Statistics.Calculators
{
    public interface ILevelCalculator
    {
        LevelInfo CalculateLevelInfo(int totalXp);
    }
}
