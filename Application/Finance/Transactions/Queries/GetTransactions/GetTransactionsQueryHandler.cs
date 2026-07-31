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

            // Corrections are excluded from the page itself and attached to their parent here — a correction may
            // fall outside the requested filter (a different month, typically) and must still travel with it.
            var corrections = await unitOfWork.FinanceTransactions
                .GetCorrectionsForParentsAsync(items.Select(t => t.Id), cancellationToken).ConfigureAwait(false);

            if (corrections.Count > 0)
            {
                var byParent = corrections
                    .GroupBy(c => c.CorrectsTransactionId!.Value)
                    .ToDictionary(group => group.Key, group => group.Select(mapper.Map<TransactionDto>).ToList());

                foreach (var dto in dtos)
                {
                    if (byParent.TryGetValue(dto.Id, out var parentCorrections))
                        dto.Corrections = parentCorrections;
                }
            }

            return new PagedResult<TransactionDto>(dtos, request.Page, request.PageSize, totalCount);
        }
    }
}
