using Application.Supplements.Catalog.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Supplements.Catalog.Queries.GetSupplements
{
    public class GetSupplementsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetSupplementsQuery, IEnumerable<SupplementDto>>
    {
        public async Task<IEnumerable<SupplementDto>> Handle(GetSupplementsQuery request, CancellationToken cancellationToken)
        {
            var supplements = await unitOfWork.Supplements
                .GetUserSupplementsAsync(request.UserProfileId, request.IncludeInactive, cancellationToken)
                .ConfigureAwait(false);

            return supplements.Select(mapper.Map<SupplementDto>).ToList();
        }
    }
}
