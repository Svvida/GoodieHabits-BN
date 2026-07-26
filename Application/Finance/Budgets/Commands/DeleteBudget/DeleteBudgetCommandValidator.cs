using FluentValidation;

namespace Application.Finance.Budgets.Commands.DeleteBudget
{
    public class DeleteBudgetCommandValidator : AbstractValidator<DeleteBudgetCommand>
    {
        public DeleteBudgetCommandValidator()
        {
            RuleFor(c => c.BudgetId)
                .GreaterThan(0).WithMessage("BudgetId must be greater than 0.");
        }
    }
}
