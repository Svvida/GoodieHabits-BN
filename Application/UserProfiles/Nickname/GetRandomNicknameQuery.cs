using Application.Dtos.UserProfile;
using MediatR;

namespace Application.UserProfiles.Nickname
{
    public record GetRandomNicknameQuery() : IRequest<GetNicknameDto>;
}
