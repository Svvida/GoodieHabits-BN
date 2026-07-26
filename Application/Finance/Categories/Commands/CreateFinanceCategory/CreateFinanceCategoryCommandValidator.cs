using Application.Finance.Common;
using Domain.Models;
using FluentValidation;

namespace Application.Finance.Categories.Commands.CreateFinanceCategory
{
    public class CreateFinanceCategoryCommandValidator : AbstractValidator<CreateFinanceCategoryCommand>
    {
        public CreateFinanceCategoryCommandValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(FinanceCategory.NameMaxLength)
                .WithMessage("Name must not exceed {MaxLength} characters.");

            RuleFor(c => c.Type)
                .IsInEnum().WithMessage("Type is invalid.");

            RuleFor(c => c.ParentCategoryId)
                .GreaterThan(0).When(c => c.ParentCategoryId.HasValue)
                .WithMessage("ParentCategoryId must be greater than 0.");

            RuleFor(c => c.Color)
                .Must(FinanceValidationRules.BeValidHexColor).When(c => c.Color is not null)
                .WithMessage("Color must be a valid hex color code (e.g. #RRGGBB).");
        }
    }
}
