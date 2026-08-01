using Application.Finance.RecurringTransactions.Commands.GenerateRecurringTransactions;
using MediatR;

namespace Api.BackgroundTasks
{
    public class GenerateRecurringTransactionsTask(
        IServiceScopeFactory scopeFactory,
        ILogger<GenerateRecurringTransactionsTask> logger) : StartupTask
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("GenerateRecurringTransactions task started.");
            await using var scope = scopeFactory.CreateAsyncScope();

            try
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                int affectedRows = await sender.Send(new GenerateRecurringTransactionsCommand(), cancellationToken).ConfigureAwait(false);

                if (affectedRows > 0)
                {
                    logger.LogInformation("GenerateRecurringTransactions saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    logger.LogInformation("GenerateRecurringTransactions found no transactions to materialize.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while materializing recurring transactions.");
            }
            finally
            {
                logger.LogInformation("GenerateRecurringTransactions task finished.");
            }
        }
    }
}
