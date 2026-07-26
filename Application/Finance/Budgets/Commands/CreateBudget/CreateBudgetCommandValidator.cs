using Domain.Enums;
using FluentValidation;

namespace Application.Finance.Budgets.Commands.CreateBudget
{
    public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
    {
        public CreateBudgetCommandValidator()
        {
            RuleFor(c => c.Period)
                .IsInEnum().WithMessage("Period is invalid.");

            RuleFor(c => c.Year)
                .InclusiveBetween(2000, 9999).WithMessage("Year must be between 2000 and 9999.");

            RuleFor(c => c.LimitAmount)
                .GreaterThan(0).WithMessage("LimitAmount must be greater than 0.");

            RuleFor(c => c.CategoryId)
                .GreaterThan(0).When(c => c.CategoryId.HasValue)
                .WithMessage("CategoryId must be greater than 0.");

            When(c => c.Period == BudgetPeriodEnum.Monthly, () =>
            {
                RuleFor(c => c.Month)
                    .NotNull().WithMessage("Month is required for a monthly budget.")
                    .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
            });

            When(c => c.Period == BudgetPeriodEnum.Yearly, () =>
            {
                RuleFor(c => c.Month)
                    .Null().WithMessage("A yearly budget must not specify a month.");
            });
        }
    }
}
