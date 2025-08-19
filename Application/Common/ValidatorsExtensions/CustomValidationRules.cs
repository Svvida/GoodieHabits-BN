using Application.Common.Interfaces;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Common.ValidatorsExtensions
{
    public static class CustomValidationRules
    {
        public static IRuleBuilderOptions<T, int> QuestMustBeOwnedByCurrentUser<T>(
            this IRuleBuilder<T, int> ruleBuilder, IUnitOfWork unitOfWork) where T : ICurrentUserQuestCommand
        {
            return (IRuleBuilderOptions<T, int>)ruleBuilder.CustomAsync(async (questId, context, cancellationToken) =>
            {
                if (context.InstanceToValidate is not ICurrentUserQuestCommand command)
                {
                    context.AddFailure("Command", "Invalid command instance.");
                    return;
                }

                var isOwner = await unitOfWork.Quests.IsQuestOwnedByUserAsync(questId, command.AccountId, cancellationToken).ConfigureAwait(false);

                if (!isOwner)
                {
                    context.AddFailure("You do not have permission to access this quest.");
                }
            });
        }

        public static IRuleBuilderOptions<T, int> QuestLabelMustBeOwnedByCurrentUser<T>(
            this IRuleBuilder<T, int> ruleBuilder, IUnitOfWork unitOfWork) where T : ICurrentUserQuestLabelCommand
        {
            return (IRuleBuilderOptions<T, int>)ruleBuilder.CustomAsync(async (questId, context, cancellationToken) =>
            {
                if (context.InstanceToValidate is not ICurrentUserQuestLabelCommand command)
                {
                    context.AddFailure("Command", "Invalid command instance.");
                    return;
                }

                var isOwner = await unitOfWork.QuestLabels.IsLabelOwnedByUserAsync(questId, command.AccountId, cancellationToken).ConfigureAwait(false);

                if (!isOwner)
                {
                    context.AddFailure("You do not have permission to access this label.");
                }
            });
        }
    }
}
