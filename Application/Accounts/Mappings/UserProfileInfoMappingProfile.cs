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
                .Map(dest => dest.Badges, src => src.UserProfile_Badges)
                .Map(dest => dest.Avatar, src => MapContext.Current.GetService<IUrlBuilder>().BuildProfilePageAvatarUrl(src.Avatar));

            config.NewConfig<UserProfile_Badge, BadgeDto>()
                .Map(dest => dest.Type, src => src.Badge.Type)
                .Map(dest => dest.Text, src => src.Badge.Text)
                .Map(dest => dest.ColorHex, src => src.Badge.ColorHex)
                .Map(dest => dest.Description, src => src.Badge.Description);
        }
    }
}
