using Application.UserGoals.Commands.ExpireGoals;
using MediatR;

namespace Api.BackgroundTasks
{
    public class ExpireGoalsTask(IServiceScopeFactory scopeFactory, ILogger<ExpireGoalsTask> logger) : StartupTask
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("ExpireGoalsTask is starting.");
            await using var scope = scopeFactory.CreateAsyncScope();

            try
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                int affectedRows = await sender.Send(new ExpireGoalsCommand(), cancellationToken);

                if (affectedRows > 0)
                {
                    logger.LogInformation("ExpireGoalsTask successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    logger.LogInformation("ExpireGoalsTask found no goals to expire.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing ExpireGoalsTask: {Message}", ex.Message);
            }
            finally
            {
                logger.LogInformation("ExpireGoalsTask has finished.");
            }
        }
    }
}
