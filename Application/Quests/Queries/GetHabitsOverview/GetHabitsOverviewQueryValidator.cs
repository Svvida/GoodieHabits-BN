using FluentValidation;

namespace Application.Quests.Queries.GetHabitsOverview
{
    public class GetHabitsOverviewQueryValidator : AbstractValidator<GetHabitsOverviewQuery>
    {
        /// <summary>Narrower than the single-quest cap: this query fans out across every habit.</summary>
        private const int MaxRangeDays = 732;

        public GetHabitsOverviewQueryValidator()
        {
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
