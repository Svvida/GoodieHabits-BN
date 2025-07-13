using System.Security.Claims;
using Domain;
using Domain.Exceptions;

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
    }
}