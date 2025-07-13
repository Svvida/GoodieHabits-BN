using Application.Dtos.Stats;
using Application.Dtos.UserProfileStats;
using Application.Helpers;
using Application.MappingActions;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class ExtendedStatsProfile : Profile
    {
        public ExtendedStatsProfile()
        {
            CreateMap<UserProfile, GetUserExtendedStatsDto>()
                .ForMember(dest => dest.QuestStats, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.GoalStats, opt => opt.MapFrom<ExtendedGoalsResolver>())
                .ForMember(dest => dest.XpStats, opt => opt.MapFrom(src => src));

            CreateMap<UserProfile, QuestExtendedStatsDto>()
                .ForMember(dest => dest.CurrentTotal, opt => opt.MapFrom(src => src.ExistingQuests))
                .ForMember(dest => dest.CurrentEverCompleted, opt => opt.MapFrom(src => src.EverCompletedExistingQuests))
                .ForMember(dest => dest.CurrentCompleted, opt => opt.MapFrom(src => src.CurrentlyCompletedExistingQuests))
                .ForMember(dest => dest.TotalCreated, opt => opt.MapFrom(src => src.TotalQuests))
                .ForMember(dest => dest.TotalCompleted, opt => opt.MapFrom(src => src.CompletedQuests));

            CreateMap<UserProfile, XpProgressDto>()
                .AfterMap<SetUserLevelAction>();
        }
    }
}
