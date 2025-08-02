using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Accounts.Commands.UpdateTimeZoneIfChanged
{
    public class UpdateTimeZoneIfChangedCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateTimeZoneIfChangedCommandHandler> logger) : IRequestHandler<UpdateTimeZoneIfChangedCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateTimeZoneIfChangedCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.TimeZoneId))
                return Unit.Value;

            var normalizedTimeZone = request.TimeZoneId.Trim();

            if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(normalizedTimeZone) is null)
            {
                logger.LogWarning($"Invalid time zone received: {normalizedTimeZone} for user with ID: {request.AccountId}. Not saving time zone to database.");
                return Unit.Value;
            }

            var account = await unitOfWork.Accounts.GetByIdAsync(request.AccountId, cancellationToken).ConfigureAwait(false);
            if (account is null)
                return Unit.Value;

            if (!string.Equals(account.TimeZone, normalizedTimeZone, StringComparison.Ordinal))
            {
                account.UpdateTimeZone(normalizedTimeZone);
                logger.LogInformation("Updated timezone for user {UserId} to {TimeZone}.", request.AccountId, normalizedTimeZone);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return Unit.Value;
        }
    }
}
