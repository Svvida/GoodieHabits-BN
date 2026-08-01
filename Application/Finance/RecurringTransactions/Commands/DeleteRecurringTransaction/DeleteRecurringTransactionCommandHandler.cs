using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.RecurringTransactions.Commands.DeleteRecurringTransaction
{
    public class DeleteRecurringTransactionCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteRecurringTransactionCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteRecurringTransactionCommand request, CancellationToken cancellationToken)
        {
            var template = await unitOfWork.RecurringTransactions
                .GetOwnedByIdAsync(request.RecurringTransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Recurring transaction with ID {request.RecurringTransactionId} not found.");

            // Transactions already generated are the user's own records and outlive the template. The FK is
            // Restrict, so they have to be detached in the same unit of work or the delete would fail outright.
            var materialized = await unitOfWork.FinanceTransactions
                .GetForRecurringTemplateAsync(template.Id, cancellationToken).ConfigureAwait(false);

            foreach (var transaction in materialized)
                transaction.ClearRecurringTemplate();

            unitOfWork.RecurringTransactions.Remove(template);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
