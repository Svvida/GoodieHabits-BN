using Application.Common.Interfaces;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserProfiles.Queries.GetUserProfiles
{
    public class GetUserProfilesQueryHandler(IUnitOfWork unitOfWork, IUrlBuilder urlBuilder) : IRequestHandler<GetUserProfilesQuery, List<UserProfileSummaryDto>>
    {
        public async Task<List<UserProfileSummaryDto>> Handle(GetUserProfilesQuery query, CancellationToken cancellationToken)
        {
            var result = await unitOfWork.UserProfiles
                .SearchUserProfilesByNickname(query.Nickname)
                .OrderBy(u => u.Nickname)
                .Take(query.Limit)
                .Select(u => new UserProfileSummaryDto(
                    u.Id,
                    u.Nickname,
                    u.UploadedAvatarUrl // Temporarily holds the Public ID
                 ))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return [.. result.Select(dto => dto with
            {
                AvatarUrl = urlBuilder.BuildThumbnailAvatarUrl(dto.AvatarUrl)
            })];
        }
    }
}
