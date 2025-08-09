using Application.Quests.ResetCompletedQuests;
using MediatR;

namespace Api.BackgroundTasks
{
    public class ResetQuestsTask(IServiceScopeFactory scopeFactory, ILogger<ResetQuestsTask> logger) : StartupTask
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("ResetQuestsTask started.");
            await using var scope = scopeFactory.CreateAsyncScope();

            try
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                int affectedRows = await sender.Send(new ResetCompletedQuestsCommand(), cancellationToken).ConfigureAwait(false);

                if (affectedRows > 0)
                {
                    logger.LogInformation("ResetQuestsTask successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    logger.LogInformation("ResetQuestsTask found no quests to reset.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing ResetQuestsTask: {Message}", ex.Message);
            }
            finally
            {
                logger.LogInformation("ResetQuestsTask completed.");
            }
        }
    }
}
