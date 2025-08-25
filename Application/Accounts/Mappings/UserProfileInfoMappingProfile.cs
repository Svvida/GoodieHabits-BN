using Application.Accounts.Dtos;
using Application.Common.Interfaces;
using Domain.Models;
using Mapster;

namespace Application.Accounts.Mappings
{
    public class UserProfileInfoMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, UserProfileInfoDto>()
                .Map(dest => dest.Badges, src => src.UserProfile_Badges.Select(b => b.Adapt<BadgeDto>()))
                .Map(dest => dest.Avatar, src => MapContext.Current.GetService<IUrlBuilder>().BuildAvatarUrl(src.Avatar));

            config.NewConfig<UserProfile_Badge, BadgeDto>()
                .Map(dest => dest.Text, src => src.Badge.Text);
        }
    }
}
