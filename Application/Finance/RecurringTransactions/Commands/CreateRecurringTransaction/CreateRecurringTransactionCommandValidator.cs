using Domain.Models;
using FluentValidation;

namespace Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction
{
    public class CreateRecurringTransactionCommandValidator : AbstractValidator<CreateRecurringTransactionCommand>
    {
        public CreateRecurringTransactionCommandValidator()
        {
            RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(c => c.DayOfMonth)
                .InclusiveBetween(RecurringTransaction.MinDayOfMonth, RecurringTransaction.MaxDayOfMonth)
                .WithMessage($"DayOfMonth must be between {RecurringTransaction.MinDayOfMonth} and {RecurringTransaction.MaxDayOfMonth}.");

            RuleFor(c => c.Note)
                .MaximumLength(FinanceTransaction.NoteMaxLength)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
