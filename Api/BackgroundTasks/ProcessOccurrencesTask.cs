using Application.Interfaces.Quests;

namespace Api.BackgroundTasks
{
    public class ProcessOccurrencesTask : StartupTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessOccurrencesTask> _logger;

        public ProcessOccurrencesTask(IServiceProvider serviceProvider, ILogger<ProcessOccurrencesTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var questStatisticsService = serviceProvider.GetRequiredService<IQuestStatisticsService>();
            _logger.LogInformation("Start processing occurrences.");
            await questStatisticsService.ProcessOccurrencesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Occurrences processed.");
        }
    }
}