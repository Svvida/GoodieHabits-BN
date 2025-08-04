using Application.Common.Interfaces;
using Application.Models;
using Application.Statistics.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.Statistics.Mappings
{
    public class SetUserLevelAction(ILevelingService levelingService) : IMappingAction<UserProfile, XpProgressDto>
    {
        public void Process(UserProfile source, XpProgressDto destination, ResolutionContext context)
        {
            LevelInfo levelInfo = levelingService.CalculateLevelInfo(source.TotalXp);

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
