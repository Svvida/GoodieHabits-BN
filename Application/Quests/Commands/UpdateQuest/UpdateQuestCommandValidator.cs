using Application.Common;
using Application.Common.ValidatorsExtensions;
using Application.Quests.Commands.CreateQuest;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.Commands.UpdateQuest
{
    public class UpdateQuestCommandValidator : AbstractValidator<UpdateQuestCommand>
    {
        public UpdateQuestCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.QuestId).QuestMustBeOwnedByCurrentUser(unitOfWork);

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .Length(1, 100).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.");

            RuleFor(x => x.Description)
                .MaximumLength(10000).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Must(desc => Checkers.IsSafeHtml(desc!)).WithMessage("{PropertyName} contains unsafe HTML.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Emoji)
                .Must(emoji => Checkers.IsSingleEmoji(emoji!)).When(x => !string.IsNullOrEmpty(x.Emoji))
                .WithMessage("You must provide a valid single emoji.");

            // Unlike creation, an update may legitimately keep a start date that is already in the past.
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("{PropertyName} must be greater than {ComparisonProperty}");

            RuleFor(x => x.Priority)
                .IsEnumName(typeof(PriorityEnum), caseSensitive: true).When(x => x.Priority != null)
                .WithMessage("{PropertyName} must be a valid priority type: 'Low', 'Medium', 'High'.");

            RuleFor(x => x.Difficulty)
                .IsEnumName(typeof(DifficultyEnum), caseSensitive: true).When(x => x.Difficulty != null)
                .WithMessage("{PropertyName} must be a valid difficulty type: 'Easy', 'Medium', 'Hard', 'Impossible'.");

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(1, 1440).When(x => x.DurationMinutes.HasValue)
                .WithMessage("{PropertyName} must be between 1 and 1440 minutes.");

            RuleFor(x => x.Schedule).NotNull().SetValidator(new QuestScheduleRequestValidator());
            RuleFor(x => x.Target!).SetValidator(new QuestTargetRequestValidator()).When(x => x.Target is not null);

            RuleFor(x => x.Labels)
                .MustAsync(async (dto, labels, cancellationToken) =>
                {
                    var labelsCount = await unitOfWork.QuestLabels
                        .CountOwnedLabelsAsync(labels, dto.UserProfileId, cancellationToken)
                        .ConfigureAwait(false);

                    return labelsCount == labels.Count;
                }).WithMessage("One or more provided labels do not exist or do not belong to the user.")
                  .When(x => x.Labels is not null && x.Labels.Count != 0);
        }
    }
}
