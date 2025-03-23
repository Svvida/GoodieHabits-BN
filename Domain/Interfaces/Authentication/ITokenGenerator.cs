using Domain.Models;

namespace Domain.Interfaces.Authentication
{
    public interface ITokenGenerator
    {
        string GenerateJwtToken(Account account, string issuer, string audience, string accessTokenKey, int expirationMinutes);
        string GenerateRefreshToken(Account account, string issuer, string refreshTokenKey, int expirationDays);
    }
}
