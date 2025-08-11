using Application.Quests.RecalculateRepeatableQuestStatistics;
using MediatR;

namespace Api.BackgroundTasks
{
    public class RecalculateRepeatableQuestStatisticsTask(IServiceScopeFactory scopeFactory, ILogger<RecalculateRepeatableQuestStatisticsTask> logger) : StartupTask
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting processing of repeatable quests statistics.");
            await using var scope = scopeFactory.CreateAsyncScope();

            try
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                int affectedRows = await sender.Send(new RecalculateRepeatableQuestStatisticsCommand(), cancellationToken).ConfigureAwait(false);

                if (affectedRows > 0)
                {
                    logger.LogInformation("ProcessStatistics task successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    logger.LogInformation("ProcessStatistics found no stats to process.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing repeatable quests statistics.");
            }
            finally
            {
                logger.LogInformation("Finished processing repeatable quests statistics.");
            }
        }
    }
}
