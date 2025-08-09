using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.ResetCompletedQuests
{
    public class ResetCompletedQuestsCommandHandler(IUnitOfWork unitOfWork, ILogger<ResetCompletedQuestsCommandHandler> logger) : IRequestHandler<ResetCompletedQuestsCommand, int>
    {
        public async Task<int> Handle(ResetCompletedQuestsCommand request, CancellationToken cancellationToken)
        {
            DateTime nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var accounts = await unitOfWork.Accounts.GetAccountsWithQuestsToResetAsync(nowUtc, cancellationToken).ConfigureAwait(false);

            if (!accounts.Any())
            {
                logger.LogInformation("No quests eligible for reset at this time.");
                return 0;
            }

            int totalResetQuests = 0;
            foreach (var account in accounts)
            {
                int resetQuestsForAccount = account.ResetQuests(nowUtc);
                totalResetQuests += resetQuestsForAccount;
                if (resetQuestsForAccount > 0)
                {
                    logger.LogInformation("Account {AccountId} has {ResetQuests} reset quests.", account.Id, resetQuestsForAccount);
                }
            }

            logger.LogInformation("Total of {TotalQuests} quests reset across {AccountCount} accounts.", totalResetQuests, accounts.Count());

            return await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}