using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Commands.RunMaintenance
{
    public class RunMaintenanceCommandHandler(
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<RunMaintenanceCommandHandler> logger)
        : IRequestHandler<RunMaintenanceCommand, int>
    {
        public async Task<int> Handle(RunMaintenanceCommand request, CancellationToken cancellationToken)
        {
            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();

            var profiles = await unitOfWork.UserProfiles
                .GetProfilesNeedingMaintenanceAsync(DateOnly.FromDateTime(nowUtc), cancellationToken)
                .ConfigureAwait(false);

            if (!profiles.Any())
            {
                logger.LogInformation("Maintenance: every profile is already up to date.");
                return 0;
            }

            int maintained = 0;

            foreach (var profile in profiles)
            {
                // Each profile re-checks against its own local date, so a profile the SQL pre-filter caught
                // only because of UTC skew is a no-op here.
                if (profile.RunDailyMaintenance(nowUtc) >= 0)
                    maintained++;
            }

            logger.LogInformation("Maintenance ran for {Count} profile(s).", maintained);

            return await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
