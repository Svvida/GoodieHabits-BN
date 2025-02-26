using Application.Dtos.Quests.WeeklyQuest;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators.Quests.WeeklyQuest
{
    public class PatchWeeklyQuestValidator : BasePatchQuestValidator<PatchWeeklyQuestDto>
    {
        public PatchWeeklyQuestValidator()
        {
            RuleFor(x => x.Weekdays)
                .NotEmpty().WithMessage("{PropertyName} can't be empty.")
                .When(x => x.Weekdays is not null)
                .ForEach(weekday =>
                {
                    weekday.IsEnumName(typeof(WeekdayEnum), caseSensitive: true)
                        .WithMessage("{PropertyName} must be a valid weekday name: 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'.");
                });
        }
    }
}
