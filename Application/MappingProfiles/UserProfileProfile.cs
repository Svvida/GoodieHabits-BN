using Application.Dtos.Profiles;
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
            CreateMap<UserProfile, GetUserProfileDto>()
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Badges, opt => opt.MapFrom(src => src.UserProfile_Badges))
                .ForMember(dest => dest.QuestsStats, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.GoalsStats, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.XpProgress, opt => opt.MapFrom(src => src));

            CreateMap<UserProfile, QuestStatsDto>()
                .ForMember(dest => dest.TotalCreated, opt => opt.MapFrom(src => src.TotalQuests))
                .ForMember(dest => dest.Completed, opt => opt.MapFrom(src => src.CompletedQuests))
                .ForMember(dest => dest.ExistingQuests, opt => opt.MapFrom(src => src.ExistingQuests))
                .ForMember(dest => dest.CompletedExistingQuests, opt => opt.MapFrom(src => src.CompletedExistingQuests));

            CreateMap<UserProfile, GoalStatsDto>()
                .ForMember(dest => dest.TotalCreated, opt => opt.MapFrom(src => src.TotalGoals))
                .ForMember(dest => dest.Completed, opt => opt.MapFrom(src => src.CompletedGoals))
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.ActiveGoals))
                .ForMember(dest => dest.Expired, opt => opt.MapFrom(src => src.ExpiredGoals));

            CreateMap<UserProfile, XpProgressDto>()
                .AfterMap<SetUserLevelAction>();
        }
    }
}
