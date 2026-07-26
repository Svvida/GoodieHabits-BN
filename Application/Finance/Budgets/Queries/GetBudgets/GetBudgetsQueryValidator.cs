using FluentValidation;

namespace Application.Finance.Budgets.Queries.GetBudgets
{
    public class GetBudgetsQueryValidator : AbstractValidator<GetBudgetsQuery>
    {
        public GetBudgetsQueryValidator()
        {
            RuleFor(q => q.Year)
                .InclusiveBetween(2000, 9999).WithMessage("Year must be between 2000 and 9999.");

            RuleFor(q => q.Month)
                .InclusiveBetween(1, 12).When(q => q.Month.HasValue)
                .WithMessage("Month must be between 1 and 12.");
        }
    }
}
