using Domain.Interfaces.Resetting;

namespace Api.BackgroundTasks
{
    public class ExpireGoalsTask : StartupTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpireGoalsTask> _logger;
        public ExpireGoalsTask(IServiceProvider serviceProvider, ILogger<ExpireGoalsTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var goalExpirationRepository = scope.ServiceProvider.GetRequiredService<IGoalExpirationRepository>();
            int expiredCount = await goalExpirationRepository.ExpireGoalsAsync(cancellationToken);
            _logger.LogInformation("ExpireGoalsTask completed. Expired {Count} goals.", expiredCount);
        }
    }
}
