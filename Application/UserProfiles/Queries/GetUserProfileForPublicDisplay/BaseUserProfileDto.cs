using System.Text.Json.Serialization;
using Application.Accounts.Dtos;
using Application.Statistics.Dtos;
using Domain.Enums;

namespace Application.UserProfiles.Queries.GetUserProfileForPublicDisplay
{
    [JsonDerivedType(typeof(PublicUserProfileDto), "NotFriends")]
    [JsonDerivedType(typeof(PublicUserProfileDto), "InvitationSent")]
    [JsonDerivedType(typeof(PublicUserProfileDto), "InvitationReceived")]
    [JsonDerivedType(typeof(FriendUserProfileDto), "Friends")]
    [JsonDerivedType(typeof(BlockedByUserProfileDto), "BlockedBy")]
    [JsonDerivedType(typeof(BlockingUserProfileDto), "Blocking")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "friendshipStatus")]
    public abstract record BaseUserProfileDto(int UserProfileId, string Nickname, string? AvatarUrl, FriendshipStatus FriendshipStatus);

    // For strangers or when an invite is pending
    public record PublicUserProfileDto(int UserProfileId, string Nickname, string? AvatarUrl, FriendshipStatus FriendshipStatus, bool CanSendInvitation, XpProgressDto XpStats)
        : BaseUserProfileDto(UserProfileId, Nickname, AvatarUrl, FriendshipStatus);

    // For friends
    public record FriendUserProfileDto(int UserProfileId, string Nickname, string? AvatarUrl, FriendshipStatus FriendshipStatus, string? Bio, DateTime JoinDate, int FriendsCount, List<BadgeDto> Badges, XpProgressDto XpStats)
        : BaseUserProfileDto(UserProfileId, Nickname, AvatarUrl, FriendshipStatus);

    // For when the viewer is blocking the user
    public record BlockingUserProfileDto(int UserProfileId, string Nickname, string? AvatarUrl, FriendshipStatus FriendshipStatus)
        : BaseUserProfileDto(UserProfileId, Nickname, AvatarUrl, FriendshipStatus);

    // For when the viewer has been blocked by the user
    public record BlockedByUserProfileDto(int UserProfileId, string Nickname, string? AvatarUrl, FriendshipStatus FriendshipStatus) // Minimal data
        : BaseUserProfileDto(UserProfileId, Nickname, AvatarUrl, FriendshipStatus);
}
