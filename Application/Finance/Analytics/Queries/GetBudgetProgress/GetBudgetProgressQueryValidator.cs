using FluentValidation;

namespace Application.Finance.Analytics.Queries.GetBudgetProgress
{
    public class GetBudgetProgressQueryValidator : AbstractValidator<GetBudgetProgressQuery>
    {
        public GetBudgetProgressQueryValidator()
        {
            RuleFor(q => q.Year).InclusiveBetween(2000, 9999).WithMessage("Year must be between 2000 and 9999.");
            RuleFor(q => q.Month)
                .InclusiveBetween(1, 12).When(q => q.Month.HasValue)
                .WithMessage("Month must be between 1 and 12.");
        }
    }
}
