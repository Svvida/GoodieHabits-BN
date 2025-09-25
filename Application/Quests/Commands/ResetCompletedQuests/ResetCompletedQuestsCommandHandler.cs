using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Commands.ResetCompletedQuests
{
    public class ResetCompletedQuestsCommandHandler(IUnitOfWork unitOfWork, ILogger<ResetCompletedQuestsCommandHandler> logger) : IRequestHandler<ResetCompletedQuestsCommand, int>
    {
        public async Task<int> Handle(ResetCompletedQuestsCommand request, CancellationToken cancellationToken)
        {
            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var profiles = await unitOfWork.UserProfiles.GetProfilesWithQuestsToResetAsync(nowUtc, cancellationToken).ConfigureAwait(false);

            if (!profiles.Any())
            {
                logger.LogInformation("No quests eligible for reset at this time.");
                return 0;
            }

            int totalResetQuests = 0;
            foreach (var profile in profiles)
            {
                int resetQuestsForAccount = profile.ResetQuests(nowUtc);
                totalResetQuests += resetQuestsForAccount;
                if (resetQuestsForAccount > 0)
                    logger.LogInformation("User Profile {ProfileId} has {ResetQuests} reset quests.", profile.Id, resetQuestsForAccount);
            }

            logger.LogInformation("Total of {TotalQuests} quests reset across {ProfilesCount} profiles.", totalResetQuests, profiles.Count());

            return await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}