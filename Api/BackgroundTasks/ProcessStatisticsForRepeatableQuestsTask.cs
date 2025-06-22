using Application.Interfaces.Quests;

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
            _logger.LogInformation("Starting processing of repeatable quests statistics.");
            await using var scope = _scopeFactory.CreateAsyncScope();

            try
            {
                var statisticsService = scope.ServiceProvider.GetRequiredService<IQuestStatisticsService>();
                int affectedRows = await statisticsService.ProcessStatisticsForQuestsAndSaveAsync(cancellationToken).ConfigureAwait(false);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("ProcessStatistics task successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    _logger.LogInformation("ProcessStatistics found no stats to process.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing repeatable quests statistics.");
            }
        }
    }
}
