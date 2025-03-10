using Application.Dtos.Labels;
using FluentValidation;

namespace Application.Validators.QuestLabels
{
    public class CreateQuestLabelValidator : AbstractValidator<CreateQuestLabelDto>
    {
        public CreateQuestLabelValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters");

            RuleFor(x => x.BackgroundColor)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters");

            RuleFor(x => x.TextColor)
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed {MaxLength} characters");
        }
    }
}
