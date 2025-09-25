using System.Security.Claims;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Api.Helpers
{
    public static class JwtHelpers
    {
        public static int GetCurrentUserId(this ClaimsPrincipal user)
        {
            string? accountIdString = user.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
            {
                throw new UnauthorizedException("Invalid access token: missing account identifier.");
            }
            return accountId;
        }

        public static int GetCurrentUserProfileId(this ClaimsPrincipal user)
        {
            string? userProfileIdString = user.FindFirst(JwtClaimTypes.UserProfileId)?.Value;
            if (string.IsNullOrWhiteSpace(userProfileIdString) || !int.TryParse(userProfileIdString, out int userProfileId))
            {
                throw new UnauthorizedException("Invalid access token: missing user profile identifier.");
            }
            return userProfileId;
        }
    }
}