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
            CreateMap<Account, GetAccountWithProfileDto>()
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile));

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
