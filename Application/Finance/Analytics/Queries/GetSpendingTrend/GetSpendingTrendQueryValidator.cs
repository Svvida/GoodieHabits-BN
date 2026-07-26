using FluentValidation;

namespace Application.Finance.Analytics.Queries.GetSpendingTrend
{
    public class GetSpendingTrendQueryValidator : AbstractValidator<GetSpendingTrendQuery>
    {
        public GetSpendingTrendQueryValidator()
        {
            RuleFor(q => q.EndYear).InclusiveBetween(2000, 9999).WithMessage("EndYear must be between 2000 and 9999.");
            RuleFor(q => q.EndMonth).InclusiveBetween(1, 12).WithMessage("EndMonth must be between 1 and 12.");
            RuleFor(q => q.Months).InclusiveBetween(1, 60).WithMessage("Months must be between 1 and 60.");
        }
    }
}
