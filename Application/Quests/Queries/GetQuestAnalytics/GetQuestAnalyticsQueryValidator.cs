using FluentValidation;

namespace Application.Quests.Queries.GetQuestAnalytics
{
    public class GetQuestAnalyticsQueryValidator : AbstractValidator<GetQuestAnalyticsQuery>
    {
        /// <summary>Five years of daily periods is ~1,800 rows — a sane ceiling for a single request.</summary>
        private const int MaxRangeDays = 1830;

        public GetQuestAnalyticsQueryValidator()
        {
            RuleFor(x => x.QuestId)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(x => x.To)
                .GreaterThanOrEqualTo(x => x.From!.Value)
                .When(x => x.From.HasValue && x.To.HasValue)
                .WithMessage("To must be on or after From.");

            RuleFor(x => x)
                .Must(x => x.From!.Value.AddDays(MaxRangeDays) >= x.To!.Value)
                .When(x => x.From.HasValue && x.To.HasValue)
                .WithMessage($"The requested range must not exceed {MaxRangeDays} days.");
        }
    }
}
