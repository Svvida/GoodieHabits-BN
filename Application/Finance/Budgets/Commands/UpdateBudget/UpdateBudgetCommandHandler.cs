using Application.Finance.Budgets.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Budgets.Commands.UpdateBudget
{
    public class UpdateBudgetCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateBudgetCommand, BudgetDto>
    {
        public async Task<BudgetDto> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
        {
            var budget = await unitOfWork.Budgets
                .GetOwnedByIdAsync(request.BudgetId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Budget with ID {request.BudgetId} not found.");

            budget.UpdateLimit(request.LimitAmount);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<BudgetDto>(budget);
        }
    }
}
