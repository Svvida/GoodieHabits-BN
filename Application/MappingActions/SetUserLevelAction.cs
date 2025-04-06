using Application.Dtos.Accounts;
using Application.Interfaces;
using Application.Models;
using AutoMapper;
using Domain.Models;

namespace Application.MappingActions
{
    public class SetUserLevelAction : IMappingAction<Account, GetAccountDto>
    {
        private readonly ILevelingService _levelingService;
        public SetUserLevelAction(ILevelingService levelingService)
        {
            _levelingService = levelingService;
        }
        public void Process(Account source, GetAccountDto destination, ResolutionContext context)
        {
            if (source.Profile != null)
            {
                LevelInfo levelInfo = _levelingService.CalculateLevelInfo(source.Profile.TotalXp);

                destination.Level = levelInfo.CurrentLevel;
                destination.UserXp = source.Profile.TotalXp;
                destination.IsMaxLevel = levelInfo.IsMaxLevel;

                if (levelInfo.IsMaxLevel)
                    destination.NextLevelTotalXpRequired = levelInfo.CurrentLevelRequiredXp;
                else
                    destination.NextLevelTotalXpRequired = levelInfo.NextLevelRequiredXp ?? levelInfo.CurrentLevelRequiredXp; // fallback to current level if next level is null
            }
        }
    }
}
