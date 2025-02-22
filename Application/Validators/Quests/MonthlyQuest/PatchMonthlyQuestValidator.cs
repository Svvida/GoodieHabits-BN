using Application.Dtos.Quests.MonthlyQuest;
using FluentValidation;

namespace Application.Validators.Quests.MonthlyQuest
{
    public class PatchMonthlyQuestValidator : BasePatchQuestValidator<PatchMonthlyQuestDto>
    {
        public PatchMonthlyQuestValidator()
        {
            RuleFor(x => x.StartDay)
                .InclusiveBetween(1, 31)
                .WithMessage("{PropertyName} must be between 1 and 31");

            RuleFor(x => x.EndDay)
                .InclusiveBetween(1, 31)
                .WithMessage("{PropertyName} must be between 1 and 31")
                .GreaterThanOrEqualTo(x => x.StartDay)
                .WithMessage("{PropertyName} must be greater than or equal to {ComparisonProperty}");
        }
    }
}
