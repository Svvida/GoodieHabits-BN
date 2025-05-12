using Application.Dtos.UserGoal;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators.UserGoal
{
    public class CreateUserGoalValidator : AbstractValidator<CreateUserGoalDto>
    {
        public CreateUserGoalValidator()
        {
            RuleFor(x => x.GoalType)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .IsEnumName(typeof(GoalTypeEnum), caseSensitive: false)
                .WithMessage("{PropertyName} must be a valid type of goal.");

            RuleFor(x => x.QuestType)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .IsEnumName(typeof(QuestTypeEnum), caseSensitive: false)
                .WithMessage("{PropertyName} must be a valid type of quest.");
        }
    }
}
