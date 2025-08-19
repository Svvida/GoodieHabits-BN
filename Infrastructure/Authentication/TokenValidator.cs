using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Exceptions;
using Domain.Interfaces.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication
{
    public class TokenValidator(JwtSecurityTokenHandler jwtSecurityTokenHandler, IOptions<JwtSettings> jwtSettingsOptions) : ITokenValidator
    {
        private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;

        public ClaimsPrincipal ValidateRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidArgumentException("Refresh token is required.");

            if (!jwtSecurityTokenHandler.CanReadToken(refreshToken))
                throw new InvalidArgumentException("Invalid refresh token format.");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = _jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.RefreshToken.Key)),
            };

            ClaimsPrincipal? principal;
            try
            {
                principal = jwtSecurityTokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out SecurityToken validatedToken);
            }
            catch (Exception)
            {
                throw;
            }

            return principal ?? throw new UnauthorizedException("Invalid refresh token");
        }
    }
}
