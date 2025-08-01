using Application.Dtos.Stats;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Statistics.Queries.GetUserExtendedStats
{
    public class GetUserExtendedStatsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserExtendedStatsQuery, GetUserExtendedStatsDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<GetUserExtendedStatsDto> Handle(GetUserExtendedStatsQuery request, CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.UserProfiles.GetUserProfileWithGoalsAsync(request.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.AccountId} not found.");

            return _mapper.Map<GetUserExtendedStatsDto>(profile);
        }
    }
}
