using Application.Accounts.Commands.ForgotPassword;
using Application.Accounts.Commands.RequestPasswordReset;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Accounts.Commands.RequestPasswordReset
{
    public class RequestPasswordResetCommandValidatorTests
    {

        private readonly RequestPasswordResetCommandValidator _validator;

        public RequestPasswordResetCommandValidatorTests()
        {
            _validator = new RequestPasswordResetCommandValidator();
        }

        [Theory]
        [InlineData("invalid-email", true)]
        [InlineData("valid@email.com", false)]
        [InlineData("a@b.c", true)]
        [InlineData("", true)]
        public void ShouldHaveError_WhenEmailIsInvalid(string email, bool expectedToHaveError)
        {
            var command = new RequestPasswordResetCommand(email);
            var result = _validator.TestValidate(command);

            if (expectedToHaveError)
            {
                result.ShouldHaveValidationErrorFor(x => x.Email);
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(x => x.Email);
            }
        }

        [Fact]
        public void ShouldNotHaveAnyValidationErrors_WhenCommandIsValid()
        {
            var command = new RequestPasswordResetCommand("valid@example.com");
            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
