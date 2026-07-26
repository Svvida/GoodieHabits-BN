using Domain.Models;
using FluentValidation;

namespace Application.Finance.Transactions.Commands.UpdateTransaction
{
    public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
    {
        public UpdateTransactionCommandValidator()
        {
            RuleFor(c => c.TransactionId)
                .GreaterThan(0).WithMessage("TransactionId must be greater than 0.");

            RuleFor(c => c.Type)
                .IsInEnum().WithMessage("Type is invalid.");

            RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(c => c.OccurredOn)
                .GreaterThan(DateOnly.MinValue).WithMessage("OccurredOn is required.");

            RuleFor(c => c.CategoryId)
                .GreaterThan(0).When(c => c.CategoryId.HasValue)
                .WithMessage("CategoryId must be greater than 0.");

            RuleFor(c => c.Note)
                .MaximumLength(FinanceTransaction.NoteMaxLength)
                .WithMessage("Note must not exceed {MaxLength} characters.");
        }
    }
}
