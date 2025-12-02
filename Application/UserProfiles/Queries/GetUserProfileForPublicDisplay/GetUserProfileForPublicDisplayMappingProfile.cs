using Application.Common.Interfaces;
using Domain.Models;
using Mapster;

namespace Application.UserProfiles.Queries.GetUserProfileForPublicDisplay
{
    public class GetUserProfileForPublicDisplayMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, BlockedByUserProfileDto>()
                .Map(dest => dest.UserProfileId, src => src.Id)
                .Map(dest => dest.AvatarUrl, src => MapContext.Current.GetService<IUrlBuilder>().BuildProfilePageAvatarUrl(src.CurrentAvatarUrl));

            config.NewConfig<UserProfile, BlockingUserProfileDto>()
                .Map(dest => dest.UserProfileId, src => src.Id)
                .Map(dest => dest.AvatarUrl, src => MapContext.Current.GetService<IUrlBuilder>().BuildProfilePageAvatarUrl(src.CurrentAvatarUrl));

            config.NewConfig<UserProfile, FriendUserProfileDto>()
                .Map(dest => dest.UserProfileId, src => src.Id)
                .Map(dest => dest.AvatarUrl, src => MapContext.Current.GetService<IUrlBuilder>().BuildProfilePageAvatarUrl(src.CurrentAvatarUrl))
                .Map(dest => dest.JoinDate, src => src.CreatedAt)
                .Map(dest => dest.Badges, src => src.UserProfile_Badges)
                .Map(dest => dest.XpStats, src => src);

            config.NewConfig<UserProfile, PublicUserProfileDto>()
                .Map(dest => dest.UserProfileId, src => src.Id)
                .Map(dest => dest.AvatarUrl, src => MapContext.Current.GetService<IUrlBuilder>().BuildProfilePageAvatarUrl(src.CurrentAvatarUrl))
                .Map(dest => dest.XpStats, src => src);
        }
    }
}
