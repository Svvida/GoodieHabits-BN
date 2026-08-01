using FluentValidation;

namespace Application.Finance.Transactions.Commands.UpdatePaidStatus
{
    public class UpdatePaidStatusCommandValidator : AbstractValidator<UpdatePaidStatusCommand>
    {
        public UpdatePaidStatusCommandValidator()
        {
            RuleFor(c => c.TransactionId)
                .GreaterThan(0).WithMessage("TransactionId must be greater than 0.");
        }
    }
}
