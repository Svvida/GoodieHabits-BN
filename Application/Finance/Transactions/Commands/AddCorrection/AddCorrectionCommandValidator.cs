using Domain.Models;
using FluentValidation;

namespace Application.Finance.Transactions.Commands.AddCorrection
{
    public class AddCorrectionCommandValidator : AbstractValidator<AddCorrectionCommand>
    {
        public AddCorrectionCommandValidator()
        {
            RuleFor(c => c.TransactionId)
                .GreaterThan(0).WithMessage("TransactionId must be greater than 0.");

            RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(c => c.OccurredOn)
                .GreaterThan(DateOnly.MinValue).WithMessage("OccurredOn is required.");

            RuleFor(c => c.Note)
                .MaximumLength(FinanceTransaction.NoteMaxLength)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
