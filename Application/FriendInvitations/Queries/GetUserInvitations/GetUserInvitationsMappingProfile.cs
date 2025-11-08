using Application.Common.Interfaces;
using Domain.Models;
using Mapster;

namespace Application.FriendInvitations.Queries.GetUserInvitations
{
    public class GetUserInvitationsMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, SenderDto>()
                .Map(dest => dest.UserProfileId, src => src.Id)
                .Map(dest => dest.AvatarUrl, src => MapContext.Current.GetService<IUrlBuilder>().BuildInvitationAvatarUrl(src.Avatar));

            config.NewConfig<UserProfile, ReceiverDto>()
                .Map(dest => dest.UserProfileId, src => src.Id)
                .Map(dest => dest.AvatarUrl, src => MapContext.Current.GetService<IUrlBuilder>().BuildInvitationAvatarUrl(src.Avatar));

            config.NewConfig<FriendInvitation, FriendInvitationDto>()
                .Map(dest => dest.InvitationId, src => src.Id)
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.Sender, src => src.Sender)
                .Map(dest => dest.Receiver, src => src.Receiver);
        }
    }
}
