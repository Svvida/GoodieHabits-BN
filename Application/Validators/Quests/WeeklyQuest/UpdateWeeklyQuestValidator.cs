using Application.Dtos.Quests.WeeklyQuest;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators.Quests.WeeklyQuest
{
    public class UpdateWeeklyQuestValidator : BaseUpdateQuestValidator<UpdateWeeklyQuestDto>
    {
        public UpdateWeeklyQuestValidator()
        {
            RuleFor(x => x.Weekdays)
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleForEach(x => x.Weekdays)
                .NotEmpty().WithMessage("Each weekday must be a valid name.")
                .IsEnumName(typeof(WeekdayEnum), caseSensitive: true)
                .WithMessage("{PropertyName} must be a valid weekday name: 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'.")
                .When(x => x.Weekdays.Count > 0);
        }
    }
}
