using Application.Dtos.Accounts;
using Application.MappingActions;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            // Entity -> DTO
            CreateMap<Account, GetAccountDto>()
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.CreatedAt))
                // Map from the Profile entity
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.Profile.Nickname))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Profile.Avatar))
                .ForMember(dest => dest.CompletedQuests, opt => opt.MapFrom(src => src.Profile.CompletedQuests))
                .ForMember(dest => dest.TotalQuests, opt => opt.MapFrom(src => src.Profile.TotalQuests))
                .ForMember(dest => dest.CompletedGoals, opt => opt.MapFrom(src => src.Profile.CompletedGoals))
                .ForMember(dest => dest.ExpiredGoals, opt => opt.MapFrom(src => src.Profile.ExpiredGoals))
                .ForMember(dest => dest.AbandonedGoals, opt => opt.MapFrom(src => src.Profile.AbandonedGoals))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Profile.Bio))
                .ForMember(dest => dest.UserXp, opt => opt.MapFrom(src => src.Profile.TotalXp))
                .ForMember(dest => dest.Badges, opt => opt.MapFrom(src => src.Profile.UserProfile_Badges))
                // Calculate and Map Leveling Info
                .AfterMap<SetUserLevelAction>();

            // Create DTO -> Entity
            CreateMap<CreateAccountDto, Account>()
                .ForMember(dest => dest.HashPassword, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => new UserProfile()));

            // Patch DTO -> Entity
            CreateMap<UpdateAccountDto, Account>()
                .ForPath(dest => dest.Profile.Nickname, opt => opt.MapFrom(src => src.Nickname))
                .ForPath(dest => dest.Profile.Bio, opt => opt.MapFrom(src => src.Bio));
        }
    }
}
