using Application.Interfaces.Quests;
using Domain.Interfaces.Quests;

namespace Api.BackgroundTasks
{
    public class ProcessStatisticsForRepeatableQuestsTask : StartupTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessStatisticsForRepeatableQuestsTask> _logger;

        public ProcessStatisticsForRepeatableQuestsTask(IServiceProvider serviceProvider, ILogger<ProcessStatisticsForRepeatableQuestsTask> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var questRepository = scope.ServiceProvider.GetRequiredService<IQuestRepository>();
            var statisticsService = scope.ServiceProvider.GetRequiredService<IQuestStatisticsService>();

            var repeatableQuests = await questRepository.GetRepeatableQuestsAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Start processing occurrences for repeatable quests.");
            await statisticsService.ProcessStatisticsForQuestsAsync(repeatableQuests, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Statistics processing completed for repeatable quests.");
        }
    }
}
