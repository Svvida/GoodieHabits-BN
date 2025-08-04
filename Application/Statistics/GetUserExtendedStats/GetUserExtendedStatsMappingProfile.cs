using AutoMapper;
using Domain.Models;

namespace Application.Statistics.GetUserExtendedStats
{
    public class GetUserExtendedStatsMappingProfile : Profile
    {
        public GetUserExtendedStatsMappingProfile()
        {
            CreateMap<UserProfile, GetUserExtendedStatsResponse>()
                .ForCtorParam(nameof(GetUserExtendedStatsResponse.QuestStats), opt => opt.MapFrom(src => src))
                .ForCtorParam(nameof(GetUserExtendedStatsResponse.GoalStats), opt => opt.MapFrom<GetUserExtendedGoalsResolver>())
                .ForCtorParam(nameof(GetUserExtendedStatsResponse.XpStats), opt => opt.MapFrom(src => src));

            CreateMap<UserProfile, QuestExtendedStatsDto>()
                .ForCtorParam(nameof(QuestExtendedStatsDto.CurrentTotal), opt => opt.MapFrom(src => src.ExistingQuests))
                .ForCtorParam(nameof(QuestExtendedStatsDto.CurrentEverCompleted), opt => opt.MapFrom(src => src.EverCompletedExistingQuests))
                .ForCtorParam(nameof(QuestExtendedStatsDto.CurrentCompleted), opt => opt.MapFrom(src => src.CurrentlyCompletedExistingQuests))
                .ForCtorParam(nameof(QuestExtendedStatsDto.TotalCreated), opt => opt.MapFrom(src => src.TotalQuests))
                .ForCtorParam(nameof(QuestExtendedStatsDto.TotalCompleted), opt => opt.MapFrom(src => src.CompletedQuests));
        }
    }
}