using Application.Common.Interfaces;
using Domain.Common;
using Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public class LevelingService : ILevelingService
    {
        private readonly LevelingOptions _options;
        private readonly List<LevelDefinition> _sortedCurve; // Cache sorted curve

        public LevelingService(IOptions<LevelingOptions> options)
        {
            _options = options.Value;

            if (_options.Curve == null || _options.Curve.Count == 0)
                throw new InvalidArgumentException("Leveling curve configuration in missing or empty");

            _sortedCurve = _options.Curve.OrderBy(l => l.Level).ToList();
        }

        public LevelInfo CalculateLevelInfo(int totalXp)
        {
            LevelDefinition currentLevelDef = _sortedCurve
                .LastOrDefault(l => totalXp >= l.RequiredTotalXp)
                ?? _sortedCurve.First();

            int currentLevelNumber = currentLevelDef.Level;
            bool isMaxLevel = currentLevelNumber >= _options.MaxLevel || currentLevelNumber >= _sortedCurve.Last().Level;

            LevelDefinition? nextLevelDef = null;
            if (!isMaxLevel)
                nextLevelDef = _sortedCurve.FirstOrDefault(l => l.Level == currentLevelNumber + 1);

            return new LevelInfo
            {
                CurrentLevel = currentLevelNumber,
                CurrentLevelRequiredXp = currentLevelDef.RequiredTotalXp,
                NextLevelRequiredXp = nextLevelDef?.RequiredTotalXp, // Null if max level or next level not defined
                IsMaxLevel = isMaxLevel || nextLevelDef == null
            };
        }

        public int GetLevelForXp(int totalXp)
        {
            LevelInfo levelInfo = CalculateLevelInfo(totalXp);
            return levelInfo.CurrentLevel;
        }

        public bool IsMaxLevel(int totalXp)
        {
            LevelInfo levelInfo = CalculateLevelInfo(totalXp);
            return levelInfo.IsMaxLevel;
        }

        public int GetRequiredXpForNextLevel(int totalXp)
        {
            LevelInfo levelInfo = CalculateLevelInfo(totalXp);
            if (levelInfo.IsMaxLevel)
                return levelInfo.CurrentLevelRequiredXp; // Return current level requirement if max level
            return levelInfo.NextLevelRequiredXp ?? levelInfo.CurrentLevelRequiredXp; // Fallback to current level if next is null
        }
    }
}