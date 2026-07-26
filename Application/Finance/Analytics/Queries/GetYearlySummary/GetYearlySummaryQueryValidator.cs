using FluentValidation;

namespace Application.Finance.Analytics.Queries.GetYearlySummary
{
    public class GetYearlySummaryQueryValidator : AbstractValidator<GetYearlySummaryQuery>
    {
        public GetYearlySummaryQueryValidator()
        {
            RuleFor(q => q.Year).InclusiveBetween(2000, 9999).WithMessage("Year must be between 2000 and 9999.");
        }
    }
}
