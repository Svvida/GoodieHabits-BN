namespace Domain.Enums
{
    public enum FriendshipStatus
    {
        NotFriends,       // No relationship exists
        InvitationSent,   // I have sent them an invitation
        InvitationReceived, // I have received an invitation from them
        Friends,          // We are friends
        Blocking,         // I am blocking them
        BlockedBy,        // They have blocked me
    }
}
