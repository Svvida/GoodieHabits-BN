using Application.Finance.Transactions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Queries.GetTransactionById
{
    public class GetTransactionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
    {
        public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
        {
            var transaction = await unitOfWork.FinanceTransactions
                .GetOwnedByIdAsync(request.TransactionId, request.UserProfileId, true, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Transaction with ID {request.TransactionId} not found.");

            return mapper.Map<TransactionDto>(transaction);
        }
    }
}
