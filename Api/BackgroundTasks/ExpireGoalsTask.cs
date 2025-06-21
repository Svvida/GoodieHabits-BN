using Domain.Interfaces.Resetting;

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
            using var scope = _scopeFactory.CreateScope();
            var goalExpirationRepository = scope.ServiceProvider.GetRequiredService<IGoalExpirationRepository>();
            int expiredCount = await goalExpirationRepository.ExpireGoalsAsync(cancellationToken);
            _logger.LogInformation("ExpireGoalsTask completed. Expired {Count} goals.", expiredCount);
        }
    }
}
