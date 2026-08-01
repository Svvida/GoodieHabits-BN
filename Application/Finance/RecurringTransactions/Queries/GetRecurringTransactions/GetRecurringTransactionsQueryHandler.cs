using Application.Finance.RecurringTransactions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.RecurringTransactions.Queries.GetRecurringTransactions
{
    public class GetRecurringTransactionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetRecurringTransactionsQuery, IEnumerable<RecurringTransactionDto>>
    {
        public async Task<IEnumerable<RecurringTransactionDto>> Handle(
            GetRecurringTransactionsQuery request, CancellationToken cancellationToken)
        {
            var templates = await unitOfWork.RecurringTransactions
                .GetUserTemplatesAsync(request.UserProfileId, true, cancellationToken).ConfigureAwait(false);

            return templates.Select(mapper.Map<RecurringTransactionDto>).ToList();
        }
    }
}
