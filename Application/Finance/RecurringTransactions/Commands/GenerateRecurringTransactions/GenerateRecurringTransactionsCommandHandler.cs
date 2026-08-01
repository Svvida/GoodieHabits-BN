using Domain.Calculators;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Finance.RecurringTransactions.Commands.GenerateRecurringTransactions
{
    public class GenerateRecurringTransactionsCommandHandler(
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<GenerateRecurringTransactionsCommandHandler> logger)
        : IRequestHandler<GenerateRecurringTransactionsCommand, int>
    {
        public async Task<int> Handle(GenerateRecurringTransactionsCommand request, CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(clock.GetCurrentInstant().ToDateTimeUtc());

            // Pre-filtered in SQL, so nothing with an up-to-date watermark is ever loaded.
            var templates = await unitOfWork.RecurringTransactions
                .GetForMaterializationAsync(today, cancellationToken).ConfigureAwait(false);

            if (templates.Count == 0)
            {
                logger.LogInformation("No recurring transaction templates require materialization.");
                return 0;
            }

            var totalGenerated = 0;

            foreach (var template in templates)
            {
                var occurrences = RecurrenceCalculator.GetMissingOccurrences(template, today);
                if (occurrences.Count == 0)
                    continue;

                // Guards a second run inside the same process, before the watermark has been persisted.
                var existing = await unitOfWork.FinanceTransactions
                    .GetForRecurringTemplateAsync(template.Id, cancellationToken).ConfigureAwait(false);
                var occupiedMonths = existing
                    .Select(t => (t.OccurredOn.Year, t.OccurredOn.Month))
                    .ToHashSet();

                foreach (var occurredOn in occurrences)
                {
                    if (!occupiedMonths.Add((occurredOn.Year, occurredOn.Month)))
                        continue;

                    var transaction = FinanceTransaction.CreateRecurring(template, occurredOn);
                    await unitOfWork.FinanceTransactions.AddAsync(transaction, cancellationToken).ConfigureAwait(false);
                    totalGenerated++;
                }

                // Advance regardless: a month the user has already deleted must not be recreated on the next
                // run. That is the whole reason this watermark exists rather than a pure existence check.
                template.MarkMaterialized(occurrences[^1]);
            }

            if (totalGenerated == 0)
            {
                logger.LogInformation("Recurring transaction templates were already up to date.");
            }

            // One unit of work for every template, including the watermark advances.
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return totalGenerated;
        }
    }
}
