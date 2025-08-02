using System.Security.Claims;

namespace Domain.Interfaces.Authentication
{
    public interface ITokenValidator
    {
        ClaimsPrincipal ValidateRefreshToken(string refreshToken);
    }
}
