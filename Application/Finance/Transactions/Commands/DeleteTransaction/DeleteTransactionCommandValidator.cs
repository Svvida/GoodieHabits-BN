using FluentValidation;

namespace Application.Finance.Transactions.Commands.DeleteTransaction
{
    public class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
    {
        public DeleteTransactionCommandValidator()
        {
            RuleFor(c => c.TransactionId)
                .GreaterThan(0).WithMessage("TransactionId must be greater than 0.");
        }
    }
}
