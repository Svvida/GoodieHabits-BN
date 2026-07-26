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
                .GetOwnedByIdAsync(request.TransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Transaction with ID {request.TransactionId} not found.");

            unitOfWork.FinanceTransactions.Remove(transaction);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
