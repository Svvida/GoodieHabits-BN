using Application.Quests.Commands.RunMaintenance;
using MediatR;

namespace Api.BackgroundTasks
{
    /// <summary>
    /// Safety net, not the mechanism. Correctness comes from the per-user pass on the read path, because the
    /// app pool this runs in shuts down after fifteen idle minutes and cannot be relied on to tick.
    /// </summary>
    public class RunMaintenanceTask(IServiceScopeFactory scopeFactory, ILogger<RunMaintenanceTask> logger) : StartupTask
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("RunMaintenanceTask started.");
            await using var scope = scopeFactory.CreateAsyncScope();

            try
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                int affectedRows = await sender.Send(new RunMaintenanceCommand(), cancellationToken).ConfigureAwait(false);

                logger.LogInformation("RunMaintenanceTask saved {Count} change(s).", affectedRows);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while running quest maintenance.");
            }
            finally
            {
                logger.LogInformation("RunMaintenanceTask finished.");
            }
        }
    }
}
