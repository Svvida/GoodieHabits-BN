using FluentValidation;

namespace Application.Finance.Budgets.Commands.UpdateBudget
{
    public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
    {
        public UpdateBudgetCommandValidator()
        {
            RuleFor(c => c.BudgetId)
                .GreaterThan(0).WithMessage("BudgetId must be greater than 0.");

            RuleFor(c => c.LimitAmount)
                .GreaterThan(0).WithMessage("LimitAmount must be greater than 0.");
        }
    }
}
