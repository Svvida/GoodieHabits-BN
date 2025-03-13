using Application.Dtos.Labels;
using FluentValidation;

namespace Application.Validators.QuestLabels
{
    public class PatchQuestLabelValidator : AbstractValidator<PatchQuestLabelDto>
    {
        public PatchQuestLabelValidator()
        {
            RuleFor(x => x.Value)
                .MaximumLength(25).WithMessage("{PropertyName} must not exceed {MaxLength} characters");

            RuleFor(x => x.BackgroundColor)
                .MaximumLength(7).WithMessage("{PropertyName} must not exceed {MaxLength} characters");

            RuleFor(x => x.TextColor)
                .MaximumLength(7).WithMessage("{PropertyName} must not exceed {MaxLength} characters");
        }
    }
}
