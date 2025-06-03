using Domain.Interfaces.Resetting;

namespace Api.BackgroundTasks
{
    public class ResetQuestsTask : StartupTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ResetQuestsTask> _logger;

        public ResetQuestsTask(IServiceProvider serviceProvider, ILogger<ResetQuestsTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var resetService = scope.ServiceProvider.GetRequiredService<IResetQuestsRepository>();
            int resetCount = await resetService.ResetQuestsAsync(cancellationToken);
            _logger.LogInformation("ResetQuestsTask completed. Reset {Count} quests.", resetCount);
        }
    }
}
