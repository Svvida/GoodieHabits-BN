using Application.Supplements.Intakes.Common;
using Application.Supplements.Intakes.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Supplements.Intakes.Queries.GetIntakes
{
    public class GetIntakesQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetIntakesQuery, IEnumerable<SupplementIntakeDto>>
    {
        public async Task<IEnumerable<SupplementIntakeDto>> Handle(GetIntakesQuery request, CancellationToken cancellationToken)
        {
            var intakes = await unitOfWork.SupplementIntakes
                .GetForPeriodAsync(request.UserProfileId, request.From, request.To, cancellationToken)
                .ConfigureAwait(false);

            // The catalog is fetched once and joined in memory rather than included per row: a user has a
            // handful of supplements and hundreds of intakes.
            var supplements = await unitOfWork.Supplements
                .GetUserSupplementsAsync(request.UserProfileId, true, cancellationToken)
                .ConfigureAwait(false);

            var catalog = supplements.ToDictionary(s => s.Id);

            return intakes.Select(i => ChecklistBuilder.BuildIntake(i, catalog)).ToList();
        }
    }
}
