﻿using Application.Dtos.Quests;
using Application.Validators.Helpers;
using Application.Validators.QuestLabels;
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
                .MaximumLength(10000).WithMessage("{PropertyName} must not exceed {MaxLength} characters.")
                .Must(desc => Checkers.IsSafeHtml(desc!)).WithMessage("{PropertyName} contains unsafe HTML.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Emoji)
                .Must(emoji => Checkers.IsSingleEmoji(emoji!)).When(x => !string.IsNullOrEmpty(x.Emoji))
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

            RuleForEach(x => x.Labels)
                .SetValidator(new CreateQuestLabelValidator());
        }
    }
}
