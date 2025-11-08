namespace Application.FriendInvitations.Queries.GetUserInvitations
{
    public record FriendInvitationDto(int InvitationId, string Status, DateTime CreatedAt, SenderDto Sender, ReceiverDto Receiver);

    public record SenderDto(int UserProfileId, string Nickname, string? AvatarUrl);

    public record ReceiverDto(int UserProfileId, string Nickname, string? AvatarUrl);
}
