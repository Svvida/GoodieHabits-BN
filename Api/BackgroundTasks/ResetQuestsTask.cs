using Domain.Interfaces.Resetting;

namespace Api.BackgroundTasks
{
    public class ResetQuestsTask : StartupTask
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ResetQuestsTask> _logger;

        public ResetQuestsTask(IServiceScopeFactory scopeFactory, ILogger<ResetQuestsTask> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var resetService = scope.ServiceProvider.GetRequiredService<IResetQuestsRepository>();
            int resetCount = await resetService.ResetQuestsAsync(cancellationToken);
            _logger.LogInformation("ResetQuestsTask completed. Reset {Count} quests.", resetCount);
        }
    }
}
