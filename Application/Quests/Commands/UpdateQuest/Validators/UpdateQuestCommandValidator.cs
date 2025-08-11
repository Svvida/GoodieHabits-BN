using Application.Common;
using Application.Common.ValidatorsExtensions;
using Application.Quests.Commands.UpdateQuest;
using Domain.Enum;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Quests.Commands.UpdateQuest.Validators
{
    public class UpdateQuestCommandValidator<T> : AbstractValidator<T> where T : UpdateQuestCommand
    {
        public UpdateQuestCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.QuestId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
                .QuestMustBeOwnedByCurrentUser(unitOfWork)
                .WithMessage("Quest not found or you do not have permission to access it.");

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

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("{PropertyName} must be greater than {ComparisonProperty}");

            RuleFor(x => x.Priority)
                .IsEnumName(typeof(PriorityEnum), caseSensitive: true).When(x => x.Priority != null)
                .WithMessage("{PropertyName} must be a valid priority type: 'Low', 'Medium', 'High'.");

            RuleFor(x => x.Difficulty)
                .IsEnumName(typeof(DifficultyEnum), caseSensitive: true).When(x => x.Difficulty != null)
                .WithMessage("{PropertyName} must be a valid difficulty type: 'Easy', 'Medium', 'Hard', 'Impossible'.");
        }
    }
}
