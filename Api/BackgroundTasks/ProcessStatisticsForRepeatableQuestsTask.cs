using Application.Common.Interfaces.Quests;

namespace Api.BackgroundTasks
{
    public class ProcessStatisticsForRepeatableQuestsTask(IServiceScopeFactory scopeFactory, ILogger<ProcessStatisticsForRepeatableQuestsTask> logger) : StartupTask
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting processing of repeatable quests statistics.");
            await using var scope = scopeFactory.CreateAsyncScope();

            try
            {
                var statisticsService = scope.ServiceProvider.GetRequiredService<IQuestStatisticsService>();
                int affectedRows = await statisticsService.ProcessStatisticsForQuestsAndSaveAsync(cancellationToken).ConfigureAwait(false);

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
        }
    }
}
