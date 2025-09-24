using Application.Accounts.Commands.ResetPassword;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Accounts.Commands.ResetPassword
{
    public class ResetPasswordCommandValidatorTests
    {
        private readonly ResetPasswordCommandValidator _validator;

        public ResetPasswordCommandValidatorTests()
        {
            _validator = new ResetPasswordCommandValidator();
        }

        [Theory]
        [InlineData("valid@email.com", false)]
        [InlineData("invalid-email", true)]
        [InlineData("", true)]
        public void ShouldHaveError_WhenEmailIsInvalid(string email, bool expectedToHaveError)
        {
            var command = new ResetPasswordCommand(email, "123456", "NewPass1", "NewPass1");
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

        [Theory]
        [InlineData("123456", false)] // Valid
        [InlineData("12345", true)] // Too short
        [InlineData("1234567", true)] // Too long
        [InlineData("123abc", true)] // Non-digit chars
        [InlineData("", true)] // Empty
        [InlineData(null, true)] // Null
        public void ShouldHaveError_WhenResetCodeIsInvalid(string resetCode, bool expectedToHaveError)
        {
            var command = new ResetPasswordCommand("test@example.com", resetCode, "NewPass1", "NewPass1");
            var result = _validator.TestValidate(command);

            if (expectedToHaveError)
            {
                result.ShouldHaveValidationErrorFor(x => x.ResetCode);
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(x => x.ResetCode);
            }
        }

        [Theory]
        // Valid password examples (assuming Password() rule covers length/chars)
        [InlineData("ValidP@ss1", false)]
        [InlineData("short", true)] // Too short (from .Password() rule)
        [InlineData("Invalid Pass", true)] // Invalid char (from .Password() rule)
        public void ShouldHaveError_WhenNewPasswordIsInvalid(string newPassword, bool expectedToHaveError)
        {
            var command = new ResetPasswordCommand("test@example.com", "123456", newPassword, newPassword);
            var result = _validator.TestValidate(command);

            if (expectedToHaveError)
            {
                result.ShouldHaveValidationErrorFor(x => x.NewPassword);
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
            }
        }

        [Fact]
        public void ShouldHaveError_WhenConfirmNewPasswordDoesNotMatchNewPassword()
        {
            var command = new ResetPasswordCommand("test@example.com", "123456", "NewPass1", "Mismatch1");
            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword)
                  .WithErrorMessage("The confirmation password does not match your new password.");
        }

        [Fact]
        public void ShouldNotHaveAnyValidationErrors_WhenCommandIsValid()
        {
            var command = new ResetPasswordCommand("test@example.com", "123456", "NewPass1", "NewPass1");
            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
