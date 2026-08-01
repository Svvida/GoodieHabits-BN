using Domain.Models;
using FluentValidation;

namespace Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction
{
    public class UpdateRecurringTransactionCommandValidator : AbstractValidator<UpdateRecurringTransactionCommand>
    {
        public UpdateRecurringTransactionCommandValidator()
        {
            RuleFor(c => c.RecurringTransactionId)
                .GreaterThan(0).WithMessage("RecurringTransactionId must be greater than 0.");

            // Partial update: each rule only applies when the client actually sent that field.
            RuleFor(c => c.Amount!.Value)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.")
                .When(c => c.Amount.HasValue);

            RuleFor(c => c.DayOfMonth!.Value)
                .InclusiveBetween(RecurringTransaction.MinDayOfMonth, RecurringTransaction.MaxDayOfMonth)
                .WithMessage($"DayOfMonth must be between {RecurringTransaction.MinDayOfMonth} and {RecurringTransaction.MaxDayOfMonth}.")
                .When(c => c.DayOfMonth.HasValue);

            RuleFor(c => c.Note)
                .MaximumLength(FinanceTransaction.NoteMaxLength)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
