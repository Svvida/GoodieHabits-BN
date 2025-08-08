using Application.UserGoals.ExpireGoals;
using MediatR;

namespace Api.BackgroundTasks
{
    public class ExpireGoalsTask : StartupTask
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpireGoalsTask> _logger;
        public ExpireGoalsTask(IServiceScopeFactory scopeFactory, ILogger<ExpireGoalsTask> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("ExpireGoalsTask is starting.");
            await using var scope = _scopeFactory.CreateAsyncScope();

            try
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                int affectedRows = await sender.Send(new ExpireGoalsCommand(), cancellationToken);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("ExpireGoalsTask successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    _logger.LogInformation("ExpireGoalsTask found no goals to expire.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing ExpireGoalsTask: {Message}", ex.Message);
            }
        }
    }
}
