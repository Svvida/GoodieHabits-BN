using Application.Accounts.Commands.UpdateAccount;
using Domain.Models;
using FluentValidation.TestHelper;

namespace Application.Tests.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandValidatorTests : TestBase<UpdateAccountCommandValidator>
    {
        private readonly UpdateAccountCommandValidator _validator;

        public UpdateAccountCommandValidatorTests()
        {
            // Instantiate the validator with the real UnitOfWork from TestBase
            // This means unique DB for each test class, and any setup in TestBase runs.
            _validator = new UpdateAccountCommandValidator(_unitOfWork);
        }

        [Fact]
        public async Task ShouldHaveError_WhenAccountIdIsEmpty()
        {
            var command = new UpdateAccountCommand("login", "email@example.com", "nick", "bio", 0, 0);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.AccountId);
        }

        [Theory]
        [InlineData("us", true)] // Too short
        [InlineData("toolongloginnamee", true)] // Too long (16+ chars)
        [InlineData("valid_login", false)]
        [InlineData("user.login", false)]
        [InlineData("user!login", false)]
        [InlineData("user-login", false)]
        [InlineData("user@login", true)] // Invalid char
        public async Task ShouldHaveError_WhenLoginIsInvalidLengthOrFormat(string login, bool expectedToHaveError)
        {
            var command = new UpdateAccountCommand(login, "email@example.com", "nick", "bio", 1, 1);
            var result = await _validator.TestValidateAsync(command);

            if (expectedToHaveError)
            {
                result.ShouldHaveValidationErrorFor(x => x.Login);
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(x => x.Login);
            }
        }

        [Fact]
        public async Task ShouldHaveError_WhenLoginIsNotUnique()
        {
            // Arrange: Seed an existing account with the same login
            var existingAccount = Account.Create("hashed_password", "existing@test.com", "oldnick");
            existingAccount.UpdateLogin("takenlogin");
            await _unitOfWork.Accounts.AddAsync(existingAccount, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new UpdateAccountCommand(
                AccountId: 999, // Different account ID, so it should be unique
                UserProfileId: 999,
                Login: "takenlogin",
                Email: "new@example.com",
                Nickname: "newnick",
                Bio: "bio");

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login)
                  .WithErrorMessage("This Login is already taken.");
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenLoginIsSameAsOwnLogin()
        {
            // Arrange: Seed an account with a specific login
            var existingAccount = Account.Create("own@test.com", "hashedpass", "ownnick");
            existingAccount.UpdateLogin("myownlogin");
            await _unitOfWork.Accounts.AddAsync(existingAccount, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new UpdateAccountCommand(
                AccountId: existingAccount.Id, // Same account ID
                UserProfileId: existingAccount.Profile.Id,
                Login: "myownlogin", // Same login
                Email: "new@example.com",
                Nickname: "newnick",
                Bio: "bio");

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Login);
        }

        [Theory]
        [InlineData("invalid-email", true)]
        [InlineData("valid@email.com", false)]
        [InlineData("a@b.c", true)] // Short invalid
        [InlineData("email@example.com", false)]
        public async Task ShouldHaveError_WhenEmailIsInvalid(string email, bool expectedToHaveError)
        {
            var command = new UpdateAccountCommand("login", email, "nick", "bio", 1, 1);
            var result = await _validator.TestValidateAsync(command);

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
        public async Task ShouldHaveError_WhenEmailIsNotUnique()
        {
            // Arrange: Seed an existing account with the same email
            var existingAccount = Account.Create("hashed_password", "existing@test.com", "oldnick");
            await _unitOfWork.Accounts.AddAsync(existingAccount, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new UpdateAccountCommand(
                AccountId: 999, // Different account ID
                UserProfileId: 999,
                Login: "newlogin",
                Email: "existing@test.com", // This email is taken
                Nickname: "newnick",
                Bio: "bio");

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("This Email address is already in use.");
        }

        [Fact]
        public async Task ShouldHaveError_WhenNicknameIsNotUnique()
        {
            // Arrange: Seed an existing account with a profile
            var existingAccount = Account.Create("nick@test.com", "hashedpass", "taken_nickname");
            await _unitOfWork.Accounts.AddAsync(existingAccount, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            var command = new UpdateAccountCommand(
                AccountId: 999, // Different account ID
                UserProfileId: 999,
                Login: "unique_login",
                Email: "unique_email@example.com",
                Nickname: "taken_nickname", // This nickname is taken
                Bio: "bio");

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Nickname)
                  .WithErrorMessage("This Nickname is already taken.");
        }


        [Fact]
        public async Task ShouldNotHaveError_WhenBioIsValid()
        {
            var command = new UpdateAccountCommand("login", "email@example.com", "nick", "A safe bio.", 1, 1);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Bio);
        }

        [Fact]
        public async Task ShouldHaveError_WhenBioContainsUnsafeHtml()
        {
            var command = new UpdateAccountCommand("login", "email@example.com", "nick", "<script>alert('xss')</script>", 1, 1);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Bio)
                  .WithErrorMessage("Bio contains unsafe HTML");
        }

        [Fact]
        public async Task ShouldHaveError_WhenBioExceedsMaxLength()
        {
            var longBio = new string('a', 31); // MaxLength is 30
            var command = new UpdateAccountCommand("login", "email@example.com", "nick", longBio, 1, 1);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Bio)
                  .WithErrorMessage("Bio cannot exceed 30 characters");
        }
    }
}
