using Application.Interfaces.Quests;
using Domain.Interfaces.Quests;

namespace Api.BackgroundTasks
{
    public class ProcessStatisticsForRepeatableQuestsTask : StartupTask
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProcessStatisticsForRepeatableQuestsTask> _logger;

        public ProcessStatisticsForRepeatableQuestsTask(IServiceScopeFactory scopeFactory, ILogger<ProcessStatisticsForRepeatableQuestsTask> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var questRepository = scope.ServiceProvider.GetRequiredService<IQuestRepository>();
            var statisticsService = scope.ServiceProvider.GetRequiredService<IQuestStatisticsService>();

            var repeatableQuests = await questRepository.GetRepeatableQuestsAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Start processing occurrences for repeatable quests.");
            await statisticsService.ProcessStatisticsForQuestsAsync(repeatableQuests, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Statistics processing completed for repeatable quests.");
        }
    }
}
