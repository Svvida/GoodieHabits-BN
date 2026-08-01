using Application.Finance.Transactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Commands.CreateTransaction
{
    public class CreateTransactionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateTransactionCommand, TransactionDto>
    {
        public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
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

            var transaction = FinanceTransaction.Create(
                request.UserProfileId,
                request.Type,
                request.Amount,
                request.OccurredOn,
                request.CategoryId,
                request.Note,
                // Omitted means paid, regardless of the date — a date-dependent implicit default would surprise
                // API clients, and the app always sends an explicit value.
                request.IsPaid ?? true);

            await unitOfWork.FinanceTransactions.AddAsync(transaction, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<TransactionDto>(transaction);
        }
    }
}
