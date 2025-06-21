using Application.Interfaces.Quests;

namespace Api.BackgroundTasks
{
    public class ProcessOccurrencesTask : StartupTask
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProcessOccurrencesTask> _logger;

        public ProcessOccurrencesTask(IServiceScopeFactory scopeFactory, ILogger<ProcessOccurrencesTask> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var questStatisticsService = serviceProvider.GetRequiredService<IQuestStatisticsService>();
            _logger.LogInformation("Start processing occurrences.");
            await questStatisticsService.ProcessOccurrencesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Occurrences processed.");
        }
    }
}