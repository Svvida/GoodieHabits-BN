using Application.UserProfiles.Queries.GetUserProfiles;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.UserProfiles.Queries
{
    public class GetUserProfilesQueryValidatorTests
    {
        private readonly GetUserProfilesQueryValidator _validator;

        public GetUserProfilesQueryValidatorTests()
        {
            _validator = new GetUserProfilesQueryValidator();
        }

        [Theory]
        [InlineData(null, true)] // Null is allowed
        [InlineData("", true)] // Empty is allowed
        [InlineData("ValidNickname", true)] // Valid nickname
        [InlineData("ThisNicknameIsWayTooLongAndExceedsTheMaximumAllowedLengthOfFiftyCharacters", false)] // Too long
        public void ShouldValidateNicknameLength(string nickname, bool isValid)
        {
            // Arrange
            var query = new GetUserProfilesQuery(nickname, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(x => x.Nickname);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.Nickname)
                    .WithErrorMessage("Nickname search term cannot exceed 50 characters.");
            }
        }

        [Theory]
        [InlineData(1, true)] // Minimum valid limit
        [InlineData(50, true)] // Maximum valid limit
        [InlineData(0, false)] // Too small
        [InlineData(-1, false)] // Negative value
        [InlineData(51, false)] // Too large
        public void ShouldValidateLimitRange(int limit, bool isValid)
        {
            // Arrange
            var query = new GetUserProfilesQuery("ValidNickname", limit);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(x => x.Limit);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.Limit)
                    .WithErrorMessage(limit <= 0
                        ? "Limit must be greater than 0."
                        : "Maximum limit is 50.");
            }
        }

        [Fact]
        public void ShouldNotHaveAnyValidationErrors_WhenQueryIsValid()
        {
            // Arrange
            var query = new GetUserProfilesQuery("ValidNickname", 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
