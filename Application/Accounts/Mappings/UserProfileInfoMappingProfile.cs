using Application.Accounts.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.Accounts.Mappings
{
    public class UserProfileInfoMappingProfile : Profile
    {
        public UserProfileInfoMappingProfile()
        {
            CreateMap<UserProfile, UserProfileInfoDto>()
                .ForCtorParam(nameof(UserProfileInfoDto.Badges), opt => opt.MapFrom(src => src.UserProfile_Badges));

            CreateMap<UserProfile_Badge, BadgeDto>()
                .ForCtorParam(nameof(BadgeDto.Text), opt => opt.MapFrom(src => src.Badge.Text));
        }
    }
}
