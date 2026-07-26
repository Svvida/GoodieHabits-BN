using Application.Common.Dtos;
using Application.Finance.Transactions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Queries.GetTransactions
{
    public class GetTransactionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionDto>>
    {
        public async Task<PagedResult<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await unitOfWork.FinanceTransactions
                .GetUserTransactionsAsync(
                    request.UserProfileId,
                    request.From,
                    request.To,
                    request.Type,
                    request.CategoryId,
                    request.Page,
                    request.PageSize,
                    cancellationToken)
                .ConfigureAwait(false);

            var dtos = items.Select(mapper.Map<TransactionDto>).ToList();

            return new PagedResult<TransactionDto>(dtos, request.Page, request.PageSize, totalCount);
        }
    }
}
