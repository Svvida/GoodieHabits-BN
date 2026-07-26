using Application.Finance.Categories.Commands.DeleteFinanceCategories;
using FluentValidation.TestHelper;

namespace Application.Tests.Finance.Categories
{
    public class DeleteFinanceCategoriesCommandValidatorTests
    {
        private readonly DeleteFinanceCategoriesCommandValidator _validator = new();

        [Fact]
        public void Should_Pass_ForValidIds()
        {
            var command = new DeleteFinanceCategoriesCommand(new[] { 1, 2, 3 }, 1);
            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveError_WhenListEmpty()
        {
            var command = new DeleteFinanceCategoriesCommand(Array.Empty<int>(), 1);
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.CategoryIds);
        }

        [Fact]
        public void Should_HaveError_WhenAnyIdNotPositive()
        {
            var command = new DeleteFinanceCategoriesCommand(new[] { 1, 0 }, 1);
            _validator.TestValidate(command).ShouldHaveValidationErrorFor("CategoryIds[1]");
        }
    }
}
