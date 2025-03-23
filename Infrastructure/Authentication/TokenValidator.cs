using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Exceptions;
using Domain.Interfaces.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication
{
    public class TokenValidator : ITokenValidator
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        public TokenValidator(JwtSecurityTokenHandler jwtSecurityTokenHandler)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler ?? new JwtSecurityTokenHandler();
        }

        public ClaimsPrincipal ValidateRefreshToken(string refreshToken, string issuer, string refreshTokenKey)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidArgumentException("Refresh token is required.");

            if (!_jwtSecurityTokenHandler.CanReadToken(refreshToken))
                throw new InvalidArgumentException("Invalid refresh token format.");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshTokenKey)),
            };

            ClaimsPrincipal? principal;
            try
            {
                principal = _jwtSecurityTokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out SecurityToken validatedToken);
            }
            catch (Exception)
            {
                throw new InvalidArgumentException("Invalid refresh token");
            }

            return principal ?? throw new UnauthorizedException("Invalid refresh token");
        }
    }
}
