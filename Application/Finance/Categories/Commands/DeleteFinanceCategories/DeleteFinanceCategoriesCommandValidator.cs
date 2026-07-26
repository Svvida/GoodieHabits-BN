using FluentValidation;

namespace Application.Finance.Categories.Commands.DeleteFinanceCategories
{
    public class DeleteFinanceCategoriesCommandValidator : AbstractValidator<DeleteFinanceCategoriesCommand>
    {
        public DeleteFinanceCategoriesCommandValidator()
        {
            RuleFor(c => c.CategoryIds)
                .NotEmpty().WithMessage("At least one category id is required.");

            RuleForEach(c => c.CategoryIds)
                .GreaterThan(0).WithMessage("Category ids must be greater than 0.");
        }
    }
}
