using Application.Accounts.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Accounts.Mappings
{
    public class UserProfileInfoMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, UserProfileInfoDto>()
                .Map(dest => dest.Badges, src => src.UserProfile_Badges.Select(b => b.Adapt<BadgeDto>()));

            config.NewConfig<UserProfile_Badge, BadgeDto>()
                .Map(dest => dest.Text, src => src.Badge.Text);
        }
    }
}
