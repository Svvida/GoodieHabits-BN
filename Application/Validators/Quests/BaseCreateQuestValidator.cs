using System.Globalization;
using Application.Dtos.Quests;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators.Quests
{
    public class BaseCreateQuestValidator<T> : AbstractValidator<T> where T : BaseCreateQuestDto
    {
        public BaseCreateQuestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .Length(1, 100).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("{PropertyName} must not exceed {MaxLength} characters.");

            RuleFor(x => x.Emoji)
                .Must(emoji => IsSingleEmoji(emoji!)).When(x => !string.IsNullOrEmpty(x.Emoji))
                .WithMessage("You must provide a valid single emoji.");

            RuleFor(x => x.EndDate)
                .GreaterThan(_ => DateTime.UtcNow.Date).When(x => x.EndDate.HasValue)
                .WithMessage("{PropertyName} must be greater than or equal to today's date");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("{PropertyName} must be greater than {ComparisonProperty}");

            RuleFor(x => x.Priority)
                .IsEnumName(typeof(PriorityEnum), caseSensitive: true).When(x => x.Priority != null)
                .WithMessage("{PropertyName} must be a valid priority type: 'Low', 'Medium', 'High'.");
        }

        private static bool IsSingleEmoji(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var enumerator = StringInfo.GetTextElementEnumerator(input);
            int count = 0;

            while (enumerator.MoveNext())
            {
                count++;
                if (count > 1)
                    return false;
            }

            return count == 1;
        }
    }
}
