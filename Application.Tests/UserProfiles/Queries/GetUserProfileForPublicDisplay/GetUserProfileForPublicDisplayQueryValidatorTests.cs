using Application.UserProfiles.Queries.GetUserProfileForPublicDisplay;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.UserProfiles.Queries.GetUserProfileForPublicDisplay
{
    public class GetUserProfileForPublicDisplayQueryValidatorTests : TestBase<GetUserProfileForPublicDisplayQueryValidator>
    {
        private readonly GetUserProfileForPublicDisplayQueryValidator _validator;

        public GetUserProfileForPublicDisplayQueryValidatorTests()
        {
            _validator = new GetUserProfileForPublicDisplayQueryValidator(_unitOfWork);
        }

        [Theory]
        [InlineData(0, 1, false)] // Invalid: CurrentUserProfileId must be greater than 0
        [InlineData(1, 0, false)] // Invalid: ViewedUserProfileId must be greater than 0
        [InlineData(1, 1, false)] // Invalid: ViewedUserProfileId must not equal CurrentUserProfileId
        [InlineData(2, 1, true)]  // Valid: Both IDs are valid and different
        public async Task ShouldValidateUserProfileIds(int currentUserProfileId, int viewedUserProfileId, bool isValid)
        {
            // Arrange
            await AddAccountAsync("test@email.com", "password", "ViewedUser");
            var query = new GetUserProfileForPublicDisplayQuery(currentUserProfileId, viewedUserProfileId);

            // Act
            var result = await _validator.TestValidateAsync(query);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                if (currentUserProfileId <= 0)
                {
                    result.ShouldHaveValidationErrorFor(x => x.CurrentUserProfileId)
                        .WithErrorMessage("CurrentUserProfileId must be greater than 0.");
                }

                if (viewedUserProfileId <= 0)
                {
                    result.ShouldHaveValidationErrorFor(x => x.ViewedUserProfileId)
                        .WithErrorMessage("ViewedUserProfileId must be greater than 0.");
                }

                if (currentUserProfileId == viewedUserProfileId && currentUserProfileId > 0)
                {
                    result.ShouldHaveValidationErrorFor(x => x.ViewedUserProfileId)
                        .WithErrorMessage("Viewed User Profile Id must not equal Current User Profile Id.");
                }
            }
        }

        [Fact]
        public async Task ShouldHaveError_WhenViewedUserProfileDoesNotExist()
        {
            // Arrange
            var currentUser = await AddAccountAsync("current@example.com", "hashed_password", "CurrentUser");
            var nonExistentViewedUserProfileId = 999; // Assume this ID does not exist

            var query = new GetUserProfileForPublicDisplayQuery(currentUser.Profile.Id, nonExistentViewedUserProfileId);

            // Act
            var result = await _validator.TestValidateAsync(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ViewedUserProfileId)
                .WithErrorMessage("Viewed User Profile Id must exists.");
        }

        [Fact]
        public async Task ShouldNotHaveAnyValidationErrors_WhenQueryIsValid()
        {
            // Arrange
            var currentUser = await AddAccountAsync("current@example.com", "hashed_password", "CurrentUser");
            var viewedUser = await AddAccountAsync("viewed@example.com", "hashed_password", "ViewedUser");

            var query = new GetUserProfileForPublicDisplayQuery(currentUser.Profile.Id, viewedUser.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(query);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
