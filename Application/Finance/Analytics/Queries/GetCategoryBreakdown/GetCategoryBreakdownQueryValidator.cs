using Domain.Enums;
using FluentValidation;

namespace Application.Finance.Analytics.Queries.GetCategoryBreakdown
{
    public class GetCategoryBreakdownQueryValidator : AbstractValidator<GetCategoryBreakdownQuery>
    {
        public GetCategoryBreakdownQueryValidator()
        {
            RuleFor(q => q.Type).IsInEnum().WithMessage("Type is invalid.");
            RuleFor(q => q.Period).IsInEnum().WithMessage("Period is invalid.");
            RuleFor(q => q.Year).InclusiveBetween(2000, 9999).WithMessage("Year must be between 2000 and 9999.");

            When(q => q.Period == BudgetPeriodEnum.Monthly, () =>
            {
                RuleFor(q => q.Month)
                    .NotNull().WithMessage("Month is required for a monthly breakdown.")
                    .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
            });
        }
    }
}
