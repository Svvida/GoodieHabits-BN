using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.UserGoals.ExpireGoals
{
    public class ExpireGoalsCommandHandler(IUnitOfWork unitOfWork, ILogger<ExpireGoalsCommandHandler> logger) : IRequestHandler<ExpireGoalsCommand, int>
    {
        public async Task<int> Handle(ExpireGoalsCommand command, CancellationToken cancellationToken)
        {
            var nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var accounts = await unitOfWork.Accounts.GetAccountsWithGoalsToExpireAsync(nowUtc, cancellationToken).ConfigureAwait(false);

            if (!accounts.Any())
            {
                logger.LogInformation("No goals to expire at this time.");
                return 0;
            }

            int totalExpiredGoals = 0;
            foreach (var account in accounts)
            {
                int expiredGoalsForAccount = account.ExpireGoals(nowUtc);
                totalExpiredGoals += expiredGoalsForAccount;
                if (expiredGoalsForAccount > 0)
                {
                    logger.LogInformation("Account {AccountId} has {ExpiredGoals} newly expired goals.", account.Id, expiredGoalsForAccount);
                }
            }

            logger.LogInformation("Total of {TotalGoals} goals expired across {AccountCount} accounts.", totalExpiredGoals, accounts.Count());

            return await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
