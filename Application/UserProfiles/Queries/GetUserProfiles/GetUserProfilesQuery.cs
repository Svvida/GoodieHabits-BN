using Application.Common.Interfaces;

namespace Application.UserProfiles.Queries.GetUserProfiles
{
    public record GetUserProfilesQuery(string? Nickname, int Limit) : IQuery<List<UserProfileSummaryDto>>;
}
