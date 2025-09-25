using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.UserGoals.Commands.ExpireGoals
{
    public class ExpireGoalsCommandHandler(IUnitOfWork unitOfWork, ILogger<ExpireGoalsCommandHandler> logger) : IRequestHandler<ExpireGoalsCommand, int>
    {
        public async Task<int> Handle(ExpireGoalsCommand command, CancellationToken cancellationToken)
        {
            var nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var profiles = await unitOfWork.UserProfiles.GetProfilesWithGoalsToExpireAsync(nowUtc, cancellationToken).ConfigureAwait(false);

            if (!profiles.Any())
            {
                logger.LogInformation("No goals to expire at this time.");
                return 0;
            }

            int totalExpiredGoals = 0;
            foreach (var profile in profiles)
            {
                int expiredGoalsForAccount = profile.ExpireGoals(nowUtc);
                totalExpiredGoals += expiredGoalsForAccount;
                if (expiredGoalsForAccount > 0)
                    logger.LogInformation("User Profile {ProfileId} has {ExpiredGoals} newly expired goals.", profile.Id, expiredGoalsForAccount);
            }

            logger.LogInformation("Total of {TotalGoals} goals expired across {ProfilesCount} profiles.", totalExpiredGoals, profiles.Count());

            return await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
