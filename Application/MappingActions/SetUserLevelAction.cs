using Application.Dtos.Profiles;
using Application.Interfaces;
using Application.Models;
using AutoMapper;
using Domain.Models;

namespace Application.MappingActions
{
    public class SetUserLevelAction : IMappingAction<UserProfile, XpProgressDto>
    {
        private readonly ILevelingService _levelingService;
        public SetUserLevelAction(ILevelingService levelingService)
        {
            _levelingService = levelingService;
        }
        public void Process(UserProfile source, XpProgressDto destination, ResolutionContext context)
        {
            LevelInfo levelInfo = _levelingService.CalculateLevelInfo(source.TotalXp);

            destination.Level = levelInfo.CurrentLevel;
            destination.CurrentXp = source.TotalXp;
            destination.IsMaxLevel = levelInfo.IsMaxLevel;

            if (levelInfo.IsMaxLevel)
                destination.NextLevelXpRequirement = levelInfo.CurrentLevelRequiredXp;
            else
                destination.NextLevelXpRequirement = levelInfo.NextLevelRequiredXp ?? levelInfo.CurrentLevelRequiredXp; // fallback to current level if next level is null

        }
    }
}
