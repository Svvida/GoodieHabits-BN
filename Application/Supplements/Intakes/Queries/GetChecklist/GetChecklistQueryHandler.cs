using Application.Supplements.Intakes.Common;
using Application.Supplements.Intakes.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Supplements.Intakes.Queries.GetChecklist
{
    public class GetChecklistQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetChecklistQuery, SupplementChecklistDto>
    {
        public async Task<SupplementChecklistDto> Handle(GetChecklistQuery request, CancellationToken cancellationToken)
            => await ChecklistBuilder
                .BuildAsync(unitOfWork, request.UserProfileId, request.Date, request.Timings, cancellationToken)
                .ConfigureAwait(false);
    }
}
