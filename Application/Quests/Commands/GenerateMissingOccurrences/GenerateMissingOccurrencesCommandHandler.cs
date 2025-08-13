using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Commands.GenerateMissingOccurrences
{
    public class GenerateMissingOccurrencesCommandHandler(IUnitOfWork unitOfWork, ILogger<GenerateMissingOccurrencesCommandHandler> logger)
        : IRequestHandler<GenerateMissingOccurrencesCommand, int>
    {
        public async Task<int> Handle(GenerateMissingOccurrencesCommand request, CancellationToken cancellationToken)
        {
            var nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            var quests = await unitOfWork.Quests.GetRepeatableQuestsForOccurrencesProcessingAsync(nowUtc, cancellationToken).ConfigureAwait(false);

            if (!quests.Any())
            {
                logger.LogInformation("No quests found requiring new occurrences.");
                return 0;
            }

            int totalGenerated = 0;

            foreach (var quest in quests)
            {
                var generatedForQuest = quest.GenerateMissingOccurrences(nowUtc);

                if (generatedForQuest == 0)
                {
                    logger.LogDebug("No occurrences generated for quest {QuestId}", quest.Id);
                    continue;
                }

                logger.LogDebug("Generated {Occurreces} new occurreces fro quest {QuestId}", generatedForQuest, quest.Id);
                totalGenerated += generatedForQuest;
            }

            if (totalGenerated == 0)
            {
                logger.LogInformation("No new occurrences needed to be generated for any quests.");
                return 0;
            }

            return await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
