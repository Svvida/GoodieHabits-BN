using FluentValidation;

namespace Application.Finance.Analytics.Queries.GetMonthlySummary
{
    public class GetMonthlySummaryQueryValidator : AbstractValidator<GetMonthlySummaryQuery>
    {
        public GetMonthlySummaryQueryValidator()
        {
            RuleFor(q => q.Year).InclusiveBetween(2000, 9999).WithMessage("Year must be between 2000 and 9999.");
            RuleFor(q => q.Month).InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
        }
    }
}
