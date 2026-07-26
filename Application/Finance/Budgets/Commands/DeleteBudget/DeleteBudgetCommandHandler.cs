using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Budgets.Commands.DeleteBudget
{
    public class DeleteBudgetCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteBudgetCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
        {
            var budget = await unitOfWork.Budgets
                .GetOwnedByIdAsync(request.BudgetId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Budget with ID {request.BudgetId} not found.");

            unitOfWork.Budgets.Remove(budget);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
