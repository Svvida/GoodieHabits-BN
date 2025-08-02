using Domain.Models;

namespace Domain.Interfaces.Authentication
{
    public interface ITokenGenerator
    {
        string GenerateAccessToken(Account account);
        string GenerateRefreshToken(Account account);
    }
}
