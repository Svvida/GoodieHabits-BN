using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Leaderboard.GetTopXp
{
    public class GetTopXpQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTopXpQuery, GetTopXpResponse>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<GetTopXpResponse> Handle(GetTopXpQuery request, CancellationToken cancellationToken)
        {
            var profiles = await _unitOfWork.UserProfiles.GetTenProfilesWithMostXpAsync(cancellationToken).ConfigureAwait(false);

            return _mapper.Map<GetTopXpResponse>(profiles);
        }
    }
}
