using Application.Common.Interfaces;

namespace Application.Finance.Transactions.Commands.DeleteTransaction
{
    public record DeleteTransactionCommand(int TransactionId, int UserProfileId) : ICommand;
}
