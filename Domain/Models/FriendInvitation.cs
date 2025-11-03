using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    public class FriendInvitation
    {
        public int Id { get; private set; }
        public int SenderUserProfileId { get; private set; }
        public int ReceiverUserProfileId { get; private set; }
        public FriendInvitationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? RespondedAt { get; private set; }

        public UserProfile Sender { get; private set; } = null!;
        public UserProfile Receiver { get; private set; } = null!;

        // EF Core parameterless constructor
        protected FriendInvitation() { }
        private FriendInvitation(int senderId, int receiverId, DateTime nowUtc)
        {
            SenderUserProfileId = senderId;
            ReceiverUserProfileId = receiverId;
            Status = FriendInvitationStatus.Pending;
            CreatedAt = nowUtc;
        }
        public static FriendInvitation Create(int senderId, int receiverId, DateTime nowUtc)
        {
            return new FriendInvitation(senderId, receiverId, nowUtc);
        }

        internal void SetAccepted(DateTime nowUtc)
        {
            if (Status != FriendInvitationStatus.Pending)
                throw new FriendInvitationException("Only pending invitations can be accepted.");
            Status = FriendInvitationStatus.Accepted; // This is a temporary state before deletion
            RespondedAt = nowUtc;
        }

        public void Cancel()
        {
            if (Status != FriendInvitationStatus.Pending)
                throw new FriendInvitationException("Only pending invitations can be cancelled.");
            Status = FriendInvitationStatus.Cancelled;
        }

        public void Reject(DateTime nowUtc)
        {
            if (Status != FriendInvitationStatus.Pending)
                throw new FriendInvitationException("Only pending invitations can be rejected.");
            Status = FriendInvitationStatus.Rejected;
            RespondedAt = nowUtc;
        }
    }
}
