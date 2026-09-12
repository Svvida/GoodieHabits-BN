using Application.Common;
using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentValidation;
using NodaTime;

namespace Application.Quests.Commands.CreateQuest
{
    public class CreateQuestCommandValidator : AbstractValidator<CreateQuestCommand>
    {
        public CreateQuestCommandValidator(IUnitOfWork unitOfWork, IClock clock)
        {
            // One day of slack absorbs the gap between the server's UTC date and the user's local one.
            var earliestAllowedDate = DateOnly.FromDateTime(clock.GetCurrentInstant().ToDateTimeUtc()).AddDays(-1);

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

            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(_ => earliestAllowedDate).When(x => x.StartDate.HasValue)
                .WithMessage("{PropertyName} must be greater then or equal to today's date.");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(_ => earliestAllowedDate).When(x => x.EndDate.HasValue)
                .WithMessage("{PropertyName} must be greater than or equal to today's date")
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

    /// <summary>
    /// Field-level rules for a schedule. The structural invariants (a month window needs both ends, a year
    /// window must be a real MMDD) live in <see cref="QuestSchedule"/> so there is one definition of them;
    /// these are the ones worth reporting as a field error rather than a 400 from the domain.
    /// </summary>
    public class QuestScheduleRequestValidator : AbstractValidator<QuestScheduleRequest>
    {
        public QuestScheduleRequestValidator()
        {
            RuleFor(x => x.Unit)
                .IsEnumName(typeof(PeriodUnitEnum), caseSensitive: false)
                .WithMessage("{PropertyName} must be one of: 'None', 'Day', 'Week', 'Month', 'Year'.");

            RuleFor(x => x.Interval)
                .InclusiveBetween(1, QuestSchedule.MaxInterval)
                .WithMessage($"{{PropertyName}} must be between 1 and {QuestSchedule.MaxInterval}.");

            RuleForEach(x => x.Weekdays)
                .IsEnumName(typeof(WeekdayEnum), caseSensitive: false)
                .WithMessage("{PropertyValue} is not a valid weekday.")
                .When(x => x.Weekdays is not null);

            RuleFor(x => x.MonthWindowStartDay)
                .InclusiveBetween(1, 31).When(x => x.MonthWindowStartDay.HasValue)
                .WithMessage("{PropertyName} must be between 1 and 31.");

            RuleFor(x => x.MonthWindowEndDay)
                .InclusiveBetween(1, 31).When(x => x.MonthWindowEndDay.HasValue)
                .WithMessage("{PropertyName} must be between 1 and 31.")
                .GreaterThanOrEqualTo(x => x.MonthWindowStartDay!.Value)
                .When(x => x.MonthWindowStartDay.HasValue && x.MonthWindowEndDay.HasValue)
                .WithMessage("{PropertyName} cannot be before {ComparisonProperty}.");
        }
    }

    public class QuestTargetRequestValidator : AbstractValidator<QuestTargetRequest>
    {
        public QuestTargetRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.")
                .LessThanOrEqualTo(QuestTarget.MaxAmount).WithMessage($"{{PropertyName}} cannot exceed {QuestTarget.MaxAmount}.");

            RuleFor(x => x.Unit)
                .MaximumLength(QuestTarget.UnitMaxLength)
                .WithMessage($"{{PropertyName}} cannot exceed {QuestTarget.UnitMaxLength} characters.");

            RuleFor(x => x.Mode)
                .IsEnumName(typeof(TargetModeEnum), caseSensitive: false)
                .WithMessage("{PropertyName} must be 'AtLeast' or 'AtMost'.");

            // Reserved but not implemented: reject it here so the slot can ship without the behaviour.
            RuleFor(x => x.Mode)
                .Must(mode => !string.Equals(mode, nameof(TargetModeEnum.AtMost), StringComparison.OrdinalIgnoreCase))
                .WithMessage("Limit habits ('AtMost') are not supported yet.");

            RuleFor(x => x.MaxCompletionsPerDay)
                .GreaterThanOrEqualTo(1).When(x => x.MaxCompletionsPerDay.HasValue)
                .WithMessage("{PropertyName} must be at least 1.");
        }
    }
}
