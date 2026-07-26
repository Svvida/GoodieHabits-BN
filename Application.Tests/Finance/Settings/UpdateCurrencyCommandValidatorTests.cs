using Application.Finance.Settings.Commands.UpdateCurrency;
using FluentValidation.TestHelper;

namespace Application.Tests.Finance.Settings
{
    public class UpdateCurrencyCommandValidatorTests
    {
        private readonly UpdateCurrencyCommandValidator _validator = new();

        [Theory]
        [InlineData("USD")]
        [InlineData("usd")]   // allow-list is case-insensitive
        [InlineData("PLN")]
        public void Should_Pass_ForSupportedCurrency(string currency)
        {
            var command = new UpdateCurrencyCommand(currency, 1);
            _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_HaveError_WhenEmpty()
        {
            var command = new UpdateCurrencyCommand("", 1);
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Currency);
        }

        [Fact]
        public void Should_HaveError_WhenUnsupported()
        {
            var command = new UpdateCurrencyCommand("XYZ", 1);
            _validator.TestValidate(command).ShouldHaveValidationErrorFor(c => c.Currency);
        }
    }
}
