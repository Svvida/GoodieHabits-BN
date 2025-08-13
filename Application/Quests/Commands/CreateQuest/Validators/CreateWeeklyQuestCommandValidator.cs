using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.Commands.CreateQuest.Validators
{
    public class CreateWeeklyQuestCommandValidator : CreateQuestCommandValidator<CreateWeeklyQuestCommand>
    {
        public CreateWeeklyQuestCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            RuleFor(x => x.Weekdays)
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleForEach(x => x.Weekdays)
                .NotEmpty().WithMessage("Each weekday must be a valid name.")
                .IsEnumName(typeof(WeekdayEnum), caseSensitive: true)
                .WithMessage("{PropertyName} must be a valid weekday name: 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'. Value '{PropertyValue}' does not meet requirements.")
                .When(x => x.Weekdays.Count > 0);
        }
    }
}
