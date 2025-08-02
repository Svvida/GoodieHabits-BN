using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain;
using Domain.Interfaces.Authentication;
using Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication
{
    public class TokenGenerator(JwtSecurityTokenHandler jwtSecurityTokenHandler, IOptions<JwtSettings> jwtSettingsOptions) : ITokenGenerator
    {
        private readonly JwtSettings jwtSettings = jwtSettingsOptions.Value;
        public string GenerateAccessToken(Account account)
        {
            var claims = new Claim[]
            {
                new(JwtClaimTypes.AccountId, account.Id.ToString()),
                new(JwtClaimTypes.Email, account.Email),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.AccessToken.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.AccessToken.ExpirationMinutes),
                signingCredentials: credentials
                );

            return jwtSecurityTokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken(Account account)
        {
            var claims = new Claim[]
            {
                new(JwtClaimTypes.AccountId, account.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.RefreshToken.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                issuer: jwtSettings.Issuer,
                expires: DateTime.UtcNow.AddDays(jwtSettings.RefreshToken.ExpirationDays),
                signingCredentials: credentials
                );

            return jwtSecurityTokenHandler.WriteToken(token);
        }
    }
}
