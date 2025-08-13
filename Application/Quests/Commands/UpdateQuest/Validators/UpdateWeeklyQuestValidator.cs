using Application.Quests.Commands.UpdateQuest;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.Commands.UpdateQuest.Validators
{
    public class UpdateWeeklyQuestValidator : UpdateQuestCommandValidator<UpdateWeeklyQuestCommand>
    {
        public UpdateWeeklyQuestValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
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
