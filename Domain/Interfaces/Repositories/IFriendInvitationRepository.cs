using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IFriendInvitationRepository : IBaseRepository<FriendInvitation>
    {
        Task<FriendInvitation?> GetFriendInvitationByUserProfileIdsAsync(int userProfileId1, int userProfileId2, bool loadProfiles, CancellationToken cancellationToken = default);
        Task<List<FriendInvitation>> GetUserInvitationsAsync(int userProfileId, InvitationDirection? direction, CancellationToken cancellationToken = default);
        Task<bool> IsFriendInvitationExistByProfileIdsAsync(int userProfileId1, int userProfileId2, CancellationToken cancellationToken = default);
        Task<FriendInvitation?> GetUserInvitationByIdAsync(int userProfileId, int invitationId, CancellationToken cancellationToken = default);
        Task<bool> IsUserInvitationExistByIdAsync(int userProfileId, int invitationId, CancellationToken cancellationToken = default);
        Task<FriendshipEligibilityStatus> CheckFriendshipEligibilityAsync(int senderUserProfileId, int receiverUserProfileId, CancellationToken cancellationToken = default);
    }
}
