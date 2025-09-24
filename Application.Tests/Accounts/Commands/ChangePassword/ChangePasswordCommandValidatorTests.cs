using Application.Accounts.Commands.ChangePassword;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Accounts.Commands.ChangePassword
{
    public class ChangePasswordCommandValidatorTests
    {
        private readonly ChangePasswordCommandValidator _validator;

        public ChangePasswordCommandValidatorTests()
        {
            // For validators without external dependencies (like IUnitOfWork),
            // you can often just instantiate them directly.
            _validator = new ChangePasswordCommandValidator();
        }

        [Theory]
        // Valid password examples
        [InlineData("ValidP@ss1", false)]
        [InlineData("another_password-1", false)]
        // Invalid: Too short
        [InlineData("short", true)]
        // Invalid: Too long
        [InlineData("aVeryLongPasswordThatExceedsTheFiftyCharacterLimitForSecurityPurposesAndAlsoMakesItHardToType", true)]
        // Invalid: Contains disallowed character (space)
        [InlineData("Invalid Pass", true)]
        // Invalid: Empty
        [InlineData("", true)]
        // Invalid: Null (though `NotEmpty` handles this)
        [InlineData(null, true)]
        public void ShouldHaveError_WhenPasswordIsInvalid(string password, bool expectedToHaveError)
        {
            // Testing OldPassword rules
            var command = new ChangePasswordCommand(password, "NewPass1", "NewPass1", 1);
            var result = _validator.TestValidate(command);

            if (expectedToHaveError)
            {
                result.ShouldHaveValidationErrorFor(x => x.OldPassword);
                result.Errors.Should().Contain(e => e.PropertyName == "OldPassword");
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(x => x.OldPassword);
            }

            // Testing NewPassword rules (they are the same)
            command = new ChangePasswordCommand("OldPass1", password, password, 1);
            result = _validator.TestValidate(command);

            if (expectedToHaveError)
            {
                result.ShouldHaveValidationErrorFor(x => x.NewPassword);
                result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
            }
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordIsSameAsOldPassword()
        {
            var command = new ChangePasswordCommand("SamePass1", "SamePass1", "SamePass1", 1);
            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("New password must be different from the old password.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenNewPasswordIsDifferentFromOldPassword()
        {
            var command = new ChangePasswordCommand("OldPass1", "NewPass1", "NewPass1", 1);
            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public void ShouldHaveError_WhenConfirmNewPasswordDoesNotMatchNewPassword()
        {
            var command = new ChangePasswordCommand("OldPass1", "NewPass1", "NotNewPass1", 1);
            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword)
                  .WithErrorMessage("The confirmation password does not match your new password.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenConfirmNewPasswordMatchesNewPassword()
        {
            var command = new ChangePasswordCommand("OldPass1", "NewPass1", "NewPass1", 1);
            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.ConfirmNewPassword);
        }

        [Fact]
        public void ShouldNotHaveAnyValidationErrors_WhenCommandIsValid()
        {
            var command = new ChangePasswordCommand("OldPass1", "NewPass1", "NewPass1", 1);
            var result = _validator.TestValidate(command);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
