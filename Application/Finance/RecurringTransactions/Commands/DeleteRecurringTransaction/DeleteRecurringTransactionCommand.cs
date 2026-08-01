using Application.Common.Interfaces;
using MediatR;

namespace Application.Finance.RecurringTransactions.Commands.DeleteRecurringTransaction
{
    public record DeleteRecurringTransactionCommand(int RecurringTransactionId, int UserProfileId) : ICommand<Unit>;
}
