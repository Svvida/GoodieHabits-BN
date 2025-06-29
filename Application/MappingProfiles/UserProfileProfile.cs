using Application.Dtos.UserProfile;
using Application.Dtos.UserProfileStats;
using Application.MappingActions;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class UserProfileProfile : Profile
    {
        public UserProfileProfile()
        {
            // Entity -> DTO
            CreateMap<UserProfile, GetUserProfileStatsDto>()
                .ForMember(dest => dest.Quests, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Goals, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.XpProgress, opt => opt.MapFrom(src => src));

            CreateMap<UserProfile, QuestStatsDto>()
                .ForMember(dest => dest.CurrentTotal, opt => opt.MapFrom(src => src.ExistingQuests))
                .ForMember(dest => dest.Completed, opt => opt.MapFrom(src => src.CurrentlyCompletedExistingQuests))
                .ForMember(dest => dest.InProgress, opt => opt.MapFrom(src => Math.Max(src.ExistingQuests - src.CurrentlyCompletedExistingQuests, 0)));

            CreateMap<UserProfile, GoalStatsDto>()
                .AfterMap<SetUserProfileGoalsAction>();

            CreateMap<UserProfile, XpProgressDto>()
                .AfterMap<SetUserLevelAction>();

            CreateMap<UserProfile, GetUserProfileInfoDto>()
                .ForMember(dest => dest.Badges, opt => opt.MapFrom(src => src.UserProfile_Badges));
        }
    }
}
