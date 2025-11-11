using Application.Common.Interfaces;

namespace Application.UserProfiles.Queries.GetUserProfileForPublicDisplay
{
    public record GetUserProfileForPublicDisplayQuery(int CurrentUserProfileId, int ViewedUserProfileId) : IQuery<BaseUserProfileDto>;
}
