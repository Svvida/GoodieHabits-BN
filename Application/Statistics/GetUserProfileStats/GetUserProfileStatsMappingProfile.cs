using AutoMapper;
using Domain.Models;

namespace Application.Statistics.GetUserProfileStats
{
    public class GetUserProfileStatsMappingProfile : Profile
    {
        public GetUserProfileStatsMappingProfile()
        {
            CreateMap<UserProfile, GetUserProfileStatsResponse>()
                .ForCtorParam(nameof(GetUserProfileStatsResponse.QuestStats), opt => opt.MapFrom(src => src))
                .ForCtorParam(nameof(GetUserProfileStatsResponse.GoalStats), opt => opt.MapFrom<GetUserProfileGoalsResolver>())
                .ForCtorParam(nameof(GetUserProfileStatsResponse.XpStats), opt => opt.MapFrom(src => src));


            CreateMap<UserProfile, UserProfileQuestStatsDto>()
                .ForCtorParam(nameof(UserProfileQuestStatsDto.CurrentTotal), opt => opt.MapFrom(src => src.ExistingQuests))
                .ForCtorParam(nameof(UserProfileQuestStatsDto.Completed), opt => opt.MapFrom(src => src.CurrentlyCompletedExistingQuests))
                .ForCtorParam(nameof(UserProfileQuestStatsDto.InProgress), opt => opt.MapFrom(src => Math.Max(src.ExistingQuests - src.CurrentlyCompletedExistingQuests, 0)));
        }
    }
}