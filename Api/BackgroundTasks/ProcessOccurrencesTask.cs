using Application.Quests.Commands.GenerateMissingOccurrences;
using MediatR;

namespace Api.BackgroundTasks
{
    public class ProcessOccurrencesTask(IServiceScopeFactory scopeFactory, ILogger<ProcessOccurrencesTask> logger) : StartupTask
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("ProcessOccurrences task started.");
            await using var scope = scopeFactory.CreateAsyncScope();

            try
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                int affectedRows = await sender.Send(new GenerateMissingOccurrencesCommand(), cancellationToken).ConfigureAwait(false);

                if (affectedRows > 0)
                {
                    logger.LogInformation("ProcessOccurrences successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    logger.LogInformation("ProcessOccurrences found no new occurrences to add.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing occurrences.");
            }
            finally
            {
                logger.LogInformation("ProcessOccurrences task finished.");
            }
        }
    }
}