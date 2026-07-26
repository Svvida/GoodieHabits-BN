using Application.Finance.Common;
using Domain.Models;
using FluentValidation;

namespace Application.Finance.Categories.Commands.UpdateFinanceCategory
{
    public class UpdateFinanceCategoryCommandValidator : AbstractValidator<UpdateFinanceCategoryCommand>
    {
        public UpdateFinanceCategoryCommandValidator()
        {
            RuleFor(c => c.CategoryId)
                .GreaterThan(0).WithMessage("CategoryId must be greater than 0.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(FinanceCategory.NameMaxLength)
                .WithMessage("Name must not exceed {MaxLength} characters.");

            RuleFor(c => c.Color)
                .Must(FinanceValidationRules.BeValidHexColor).When(c => c.Color is not null)
                .WithMessage("Color must be a valid hex color code (e.g. #RRGGBB).");
        }
    }
}
