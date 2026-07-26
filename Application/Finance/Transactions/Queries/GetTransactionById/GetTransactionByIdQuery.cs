using Application.Common.Interfaces;
using Application.Finance.Transactions.Dtos;

namespace Application.Finance.Transactions.Queries.GetTransactionById
{
    public record GetTransactionByIdQuery(int TransactionId, int UserProfileId) : IQuery<TransactionDto>;
}
