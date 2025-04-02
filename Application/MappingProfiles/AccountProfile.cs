using Application.Dtos.Accounts;
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
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.Profile.Nickname))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Profile.Avatar))
                .ForMember(dest => dest.CompletedQuests, opt => opt.MapFrom(src => src.Profile.CompletedQuests))
                .ForMember(dest => dest.TotalQuests, opt => opt.MapFrom(src => src.Profile.TotalQuests))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Profile.UserLevel))
                .ForMember(dest => dest.Xp, opt => opt.MapFrom(src => src.Profile.CurrentExperience))
                .ForMember(dest => dest.TotalXP, opt => opt.MapFrom(src => src.Profile.TotalExperience))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Profile.Bio))
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Badges, opt => opt.MapFrom(src => src.Profile.UserProfile_Badges));

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
