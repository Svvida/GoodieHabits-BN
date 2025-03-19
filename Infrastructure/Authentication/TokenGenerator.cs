using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain;
using Domain.Interfaces.Authentication;
using Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        public TokenGenerator(JwtSecurityTokenHandler jwtSecurityTokenHandler)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler ?? new JwtSecurityTokenHandler();
        }

        public string GenerateJwtToken(Account account, string issuer, string audience, string accessTokenKey, int expirationMinutes)
        {
            var claims = new Claim[]
            {
                new(JwtClaimTypes.AccountId, account.Id.ToString()),
                new(JwtClaimTypes.Email, account.Email),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                issuer: issuer,
                audience: audience,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
                );

            return _jwtSecurityTokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken(Account account, string issuer, string refreshTokenKey, int expirationDays)
        {
            var claims = new Claim[]
            {
                new(JwtClaimTypes.AccountId, account.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshTokenKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                issuer: issuer,
                expires: DateTime.UtcNow.AddDays(expirationDays),
                signingCredentials: credentials
                );

            return _jwtSecurityTokenHandler.WriteToken(token);
        }
    }
}
