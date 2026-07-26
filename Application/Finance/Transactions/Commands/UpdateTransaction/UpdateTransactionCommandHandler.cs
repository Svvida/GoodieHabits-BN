using Application.Finance.Transactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Commands.UpdateTransaction
{
    public class UpdateTransactionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateTransactionCommand, TransactionDto>
    {
        public async Task<TransactionDto> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await unitOfWork.FinanceTransactions
                .GetOwnedByIdAsync(request.TransactionId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Transaction with ID {request.TransactionId} not found.");

            if (request.CategoryId is int categoryId)
            {
                var category = await unitOfWork.FinanceCategories
                    .GetAssignableByIdAsync(categoryId, request.UserProfileId, true, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"Category with ID {categoryId} not found.");

                if (category.Type != request.Type)
                    throw new ConflictException(
                        $"Category '{category.Name}' is a {category.Type} category and cannot be used for a {request.Type} transaction.");
            }

            transaction.ChangeType(request.Type);
            transaction.UpdateAmount(request.Amount);
            transaction.UpdateDate(request.OccurredOn);
            transaction.Recategorize(request.CategoryId);
            transaction.UpdateNote(request.Note);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<TransactionDto>(transaction);
        }
    }
}
