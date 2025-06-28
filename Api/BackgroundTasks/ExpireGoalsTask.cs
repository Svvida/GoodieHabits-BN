using Application.Interfaces;

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
                var expirationService = scope.ServiceProvider.GetRequiredService<IGoalExpirationService>();
                int affectedRows = await expirationService.ExpireGoalsAndSaveAsync(cancellationToken);

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
