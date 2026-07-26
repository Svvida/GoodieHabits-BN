using Application.Finance.Budgets.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Budgets.Queries.GetBudgets
{
    public class GetBudgetsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetBudgetsQuery, IEnumerable<BudgetDto>>
    {
        public async Task<IEnumerable<BudgetDto>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
        {
            var budgets = await unitOfWork.Budgets
                .GetUserBudgetsAsync(request.UserProfileId, request.Year, request.Month, true, cancellationToken)
                .ConfigureAwait(false);

            return budgets.Select(mapper.Map<BudgetDto>).ToList();
        }
    }
}
