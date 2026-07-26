using Application.Finance.Categories.Commands.CreateFinanceCategory;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace Application.Tests.Finance.Categories
{
    public class CreateFinanceCategoryCommandValidatorTests
    {
        private readonly CreateFinanceCategoryCommandValidator _validator = new();

        private static CreateFinanceCategoryCommand Valid() =>
            new("Groceries", FinanceTransactionTypeEnum.Expense, null, "#F28E2B", "food", false, 1);

        [Fact]
        public void Should_Pass_ForValidCommand()
        {
            _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveError_WhenNameEmpty()
        {
            var command = Valid() with { Name = "" };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Name);
        }

        [Fact]
        public void Should_HaveError_WhenTypeInvalid()
        {
            var command = Valid() with { Type = (FinanceTransactionTypeEnum)99 };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Type);
        }

        [Fact]
        public void Should_HaveError_WhenColorInvalid()
        {
            var command = Valid() with { Color = "red" };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Color);
        }

        [Fact]
        public void Should_HaveError_WhenParentCategoryIdNotPositive()
        {
            var command = Valid() with { ParentCategoryId = 0 };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.ParentCategoryId);
        }
    }
}
