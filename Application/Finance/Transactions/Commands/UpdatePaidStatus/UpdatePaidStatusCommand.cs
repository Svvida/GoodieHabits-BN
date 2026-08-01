using Application.Common.Interfaces;
using Application.Finance.Transactions.Dtos;

namespace Application.Finance.Transactions.Commands.UpdatePaidStatus
{
    public record UpdatePaidStatusCommand(
        int TransactionId,
        bool IsPaid,
        int UserProfileId) : ICommand<TransactionDto>;
}
