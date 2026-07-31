using Application.Finance.Transactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Commands.UpdateTransaction
{
    public class UpdateTransactionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateTransactionCommand, TransactionDto>
    {
        public async Task<TransactionDto> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
        {
            // Corrections are loaded because recategorizing a parent has to carry them along.
            var transaction = await unitOfWork.FinanceTransactions
                .GetOwnedWithCorrectionsAsync(request.TransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Transaction with ID {request.TransactionId} not found.");

            if (transaction.IsCorrection)
                await UpdateCorrectionAsync(transaction, request, cancellationToken).ConfigureAwait(false);
            else
                await UpdateOrdinaryAsync(transaction, request, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<TransactionDto>(transaction);
        }

        private async Task UpdateOrdinaryAsync(
            FinanceTransaction transaction, UpdateTransactionCommand request, CancellationToken cancellationToken)
        {
            if (request.CategoryId is int categoryId)
            {
                var category = await unitOfWork.FinanceCategories
                    .GetAssignableByIdAsync(categoryId, request.UserProfileId, true, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"Category with ID {categoryId} not found.");

                if (category.Type != request.Type)
                    throw new ConflictException(
                        $"Category '{category.Name}' is a {category.Type} category and cannot be used for a {request.Type} transaction.");
            }

            // CorrectedAmount > 0 is an exact test for "has corrections", so it holds even without the collection.
            if (request.Type != transaction.Type && transaction.CorrectedAmount > 0)
                throw new ConflictException(
                    "Type cannot be changed while corrections exist for this transaction. Remove them first.");

            if (request.Amount < transaction.CorrectedAmount)
                throw new ConflictException(
                    $"Amount cannot be lower than the {transaction.CorrectedAmount} already corrected against this transaction.");

            transaction.ChangeType(request.Type);
            transaction.UpdateAmount(request.Amount);
            transaction.UpdateDate(request.OccurredOn);
            transaction.Recategorize(request.CategoryId);
            transaction.UpdateNote(request.Note);

            // Corrections inherit the parent's category, so recategorizing cascades — blocking it instead would
            // mean "you can't recategorize a dinner you got a refund for".
            foreach (var correction in transaction.Corrections)
                correction.Recategorize(request.CategoryId);
        }

        private async Task UpdateCorrectionAsync(
            FinanceTransaction correction, UpdateTransactionCommand request, CancellationToken cancellationToken)
        {
            // The client sends the whole transaction back, so an unchanged inherited value is accepted silently;
            // only an actual attempt to change type or category is a conflict.
            if (request.Type != correction.Type)
                throw new ConflictException("A correction inherits its type from the transaction it corrects.");

            if (request.CategoryId != correction.CategoryId)
                throw new ConflictException("A correction inherits its category from the transaction it corrects.");

            if (request.Amount != correction.Amount)
            {
                var parent = await unitOfWork.FinanceTransactions
                    .GetOwnedByIdAsync(correction.CorrectsTransactionId!.Value, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"Transaction with ID {correction.CorrectsTransactionId} not found.");

                var correctable = parent.Amount - parent.CorrectedAmount + correction.Amount;
                if (request.Amount > correctable)
                    throw new ConflictException(
                        $"Corrections cannot exceed the transaction amount ({parent.Amount}); {correctable} is the most this correction can be.");

                parent.RevertCorrection(correction.Amount);
                parent.ApplyCorrection(request.Amount);
            }

            correction.UpdateAmount(request.Amount);
            correction.UpdateDate(request.OccurredOn);
            correction.UpdateNote(request.Note);
        }
    }
}
