using Application.Statistics.Calculators;
using Application.Statistics.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Statistics.Mappings
{
    public class XpProgressMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, XpProgressDto>()
                .MapToConstructor(true)
                .ConstructUsing(src => MapToXpProgressDto(src));
        }

        private static XpProgressDto MapToXpProgressDto(UserProfile src)
        {
            var levelingService = MapContext.Current.GetService<ILevelCalculator>();
            var levelInfo = levelingService.CalculateLevelInfo(src.TotalXp);

            return new XpProgressDto
            {
                Level = levelInfo.CurrentLevel,
                CurrentXp = src.TotalXp,
                IsMaxLevel = levelInfo.IsMaxLevel,
                NextLevelXpRequirement = levelInfo.IsMaxLevel
                    ? levelInfo.CurrentLevelRequiredXp
                    : levelInfo.NextLevelRequiredXp ?? levelInfo.CurrentLevelRequiredXp // fallback to current level if next level is null
            };
        }
    }
}
