using System.Security.Claims;
using Domain;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public class RefreshAccessTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenValidator tokenValidator,
        ITokenGenerator tokenGenerator,
        ILogger<RefreshAccessTokenCommandHandler> logger) : IRequestHandler<RefreshAccessTokenCommand, RefreshAccessTokenResponse>
    {
        public async Task<RefreshAccessTokenResponse> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
        {
            ClaimsPrincipal principal = tokenValidator.ValidateRefreshToken(request.RefreshToken);

            if (!int.TryParse(principal.FindFirst(JwtClaimTypes.AccountId)?.Value, out var accountId))
                // The accountId claim is missing or not a valid integer.
                throw new UnauthorizedException("Invalid token claims.");

            var account = await unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken)
                ?? throw new UnauthorizedException("Invalid token.");

            if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
            {
                var normalizedTimeZone = request.TimeZoneId.Trim();

                if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(normalizedTimeZone) is null)
                    logger.LogWarning("Invalid time zone '{TimeZone}' received for user {UserId}.", normalizedTimeZone, account.Id);
                else
                {
                    if (!string.Equals(account.TimeZone, normalizedTimeZone, StringComparison.Ordinal))
                    {
                        account.UpdateTimeZone(normalizedTimeZone);
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                        logger.LogInformation("Updated timezone for user {UserId} to {TimeZone}.", account.Id, normalizedTimeZone);
                    }
                }
            }

            return new RefreshAccessTokenResponse(AccessToken: tokenGenerator.GenerateAccessToken(account));
        }
    }
}
