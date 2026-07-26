using Application.Finance.Budgets.Commands.CreateBudget;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace Application.Tests.Finance.Budgets
{
    public class CreateBudgetCommandValidatorTests
    {
        private readonly CreateBudgetCommandValidator _validator = new();

        private static CreateBudgetCommand ValidMonthly() =>
            new(null, BudgetPeriodEnum.Monthly, 2026, 3, 500m, 1);

        [Fact]
        public void Should_Pass_ForValidMonthly()
        {
            _validator.TestValidate(ValidMonthly()).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Pass_ForValidYearly()
        {
            var command = new CreateBudgetCommand(5, BudgetPeriodEnum.Yearly, 2026, null, 5000m, 1);
            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveError_WhenMonthlyWithoutMonth()
        {
            var command = ValidMonthly() with { Month = null };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Month);
        }

        [Fact]
        public void Should_HaveError_WhenMonthlyMonthOutOfRange()
        {
            var command = ValidMonthly() with { Month = 13 };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Month);
        }

        [Fact]
        public void Should_HaveError_WhenYearlyWithMonth()
        {
            var command = new CreateBudgetCommand(null, BudgetPeriodEnum.Yearly, 2026, 4, 5000m, 1);
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Month);
        }

        [Fact]
        public void Should_HaveError_WhenLimitNotPositive()
        {
            var command = ValidMonthly() with { LimitAmount = 0m };
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.LimitAmount);
        }
    }
}
