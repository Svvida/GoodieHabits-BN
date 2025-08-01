using Domain.Enum;
using Domain.Interfaces;
using FluentValidation;

namespace Application.UserGoals.Commands.CreateUserGoal
{
    public class CreateUserGoalCommandValidator : AbstractValidator<CreateUserGoalCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserGoalCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(cmd => cmd.GoalType)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .IsEnumName(typeof(GoalTypeEnum), caseSensitive: false)
                .WithMessage("{PropertyName} must be a valid type of goal. 'Daily', 'Weekly', 'Monthly', 'Yearly'.");

            RuleFor(cmd => cmd.QuestType)
                .NotEmpty()
                .WithMessage("{PropertyName} is required.")
                .IsEnumName(typeof(QuestTypeEnum), caseSensitive: false)
                .WithMessage("{PropertyName} must be a valid type of quest. 'OneTime', 'Daily', 'Weekly', 'Monthly', 'Seasonal'.");

            RuleFor(cmd => cmd.QuestId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0)
                .WithMessage("{PropertyName} must be greater than 0.")
                .MustAsync(_unitOfWork.Quests.ExistsByIdAsync)
                .WithMessage("Quest with ID {PropertyValue} does not exist.")
                .CustomAsync(async (questId, context, cancellationToken) =>
                {
                    var quest = await _unitOfWork.Quests.GetByIdAsync(questId, cancellationToken).ConfigureAwait(false);
                    if (quest is null)
                        return;

                    bool isAlreadyActiveGoal = await _unitOfWork.UserGoals.IsQuestActiveGoalAsync(questId, cancellationToken).ConfigureAwait(false);
                    if (isAlreadyActiveGoal)
                        context.AddFailure("Quest", $"Quest with ID {questId} is already an active goal.");

                    if (quest.IsCompleted)
                        context.AddFailure("Quest", $"Quest with ID {questId} is already completed. Cannot create a goal for it.");
                });

            RuleFor(cmd => cmd)
                .MustAsync(async (cmd, cancellationToken) =>
                {
                    if (!Enum.TryParse<GoalTypeEnum>(cmd.GoalType, true, out var goalType))
                        return true;

                    var activeGoalsCount = await _unitOfWork.UserGoals.GetActiveGoalsCountByTypeAsync(cmd.AccountId, goalType, cancellationToken).ConfigureAwait(false);

                    return activeGoalsCount < 1;
                })
                .WithMessage(cmd => $"You have reached the limit of active '{cmd.GoalType}' goals.")
                .When(cmd => Enum.TryParse<GoalTypeEnum>(cmd.GoalType, true, out _), ApplyConditionTo.CurrentValidator);
        }
    }
}
