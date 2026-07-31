using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Transactions.Commands.DeleteTransaction
{
    public class DeleteTransactionCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteTransactionCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
        {
            // Owned lookup ensures a user can only hard-delete their own transaction.
            var transaction = await unitOfWork.FinanceTransactions
                .GetOwnedWithCorrectionsAsync(request.TransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Transaction with ID {request.TransactionId} not found.");

            if (transaction.IsCorrection)
            {
                // Give the money back to the parent's net figure before the row disappears.
                var parent = await unitOfWork.FinanceTransactions
                    .GetOwnedByIdAsync(transaction.CorrectsTransactionId!.Value, request.UserProfileId, false, cancellationToken).ConfigureAwait(false);

                parent?.RevertCorrection(transaction.Amount);
            }
            else if (transaction.Corrections.Count > 0)
            {
                // The self-FK is Restrict, so the corrections have to go in the same unit of work.
                unitOfWork.FinanceTransactions.RemoveRange(transaction.Corrections);
            }

            unitOfWork.FinanceTransactions.Remove(transaction);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
