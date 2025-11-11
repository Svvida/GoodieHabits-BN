using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.UserProfiles.Queries.GetUserProfileForPublicDisplay
{
    public class GetUserProfileForPublicDisplayQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetUserProfileForPublicDisplayQuery, BaseUserProfileDto>
    {
        public async Task<BaseUserProfileDto> Handle(GetUserProfileForPublicDisplayQuery query, CancellationToken cancellationToken)
        {
            var viewedProfile = await unitOfWork.UserProfiles.GetUserProfileByIdForPublicDisplayAsync(query.ViewedUserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("User Profile not found.");

            var friendshipStatus = await DetermineFriendshipStatusAsync(query.CurrentUserProfileId, query.ViewedUserProfileId, unitOfWork, cancellationToken).ConfigureAwait(false);

            return MapToDto(viewedProfile, friendshipStatus, mapper);
        }

        private static async Task<FriendshipStatus> DetermineFriendshipStatusAsync(int currentUserId, int viewedUserId, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
        {
            if (await unitOfWork.UserBlocks.IsUserBlockExistsByProfileIdsAsync(currentUserId, viewedUserId, cancellationToken).ConfigureAwait(false))
                return FriendshipStatus.Blocking;

            if (await unitOfWork.UserBlocks.IsUserBlockExistsByProfileIdsAsync(viewedUserId, currentUserId, cancellationToken).ConfigureAwait(false))
                return FriendshipStatus.BlockedBy;

            if (await unitOfWork.Friends.IsFriendshipExistsByUserProfileIdsAsync(currentUserId, viewedUserId, cancellationToken).ConfigureAwait(false))
                return FriendshipStatus.Friends;

            var invitation = await unitOfWork.FriendInvitations.GetPendingInvitationAsync(currentUserId, viewedUserId, cancellationToken).ConfigureAwait(false);
            if (invitation != null)
            {
                return invitation.SenderUserProfileId == currentUserId
                    ? FriendshipStatus.InvitationSent
                    : FriendshipStatus.InvitationReceived;
            }

            return FriendshipStatus.NotFriends;
        }

        private static BaseUserProfileDto MapToDto(UserProfile profile, FriendshipStatus status, IMapper mapper)
        {
            switch (status)
            {
                case FriendshipStatus.Friends:
                    var friendDto = mapper.Map<FriendUserProfileDto>(profile);
                    return friendDto with { FriendshipStatus = status };

                case FriendshipStatus.Blocking:
                    var blockingDto = mapper.Map<BlockingUserProfileDto>(profile);
                    return blockingDto with { FriendshipStatus = status };

                case FriendshipStatus.BlockedBy:
                    var blockedByDto = mapper.Map<BlockedByUserProfileDto>(profile);
                    return blockedByDto with { FriendshipStatus = status, AvatarUrl = "" };

                case FriendshipStatus.NotFriends:
                case FriendshipStatus.InvitationSent:
                case FriendshipStatus.InvitationReceived:
                default:
                    var publicDto = mapper.Map<PublicUserProfileDto>(profile);
                    bool canSend = status == FriendshipStatus.NotFriends;
                    return publicDto with { FriendshipStatus = status, CanSendInvitation = canSend };
            }
        }
    }
}