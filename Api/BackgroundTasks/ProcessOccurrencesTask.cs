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
            _logger.LogInformation("ProcessOccurrences task started.");
            await using var scope = _scopeFactory.CreateAsyncScope();

            try
            {
                var occurrencesGenerator = scope.ServiceProvider.GetRequiredService<IQuestOccurrenceGenerator>();

                int affectedRows = await occurrencesGenerator.GenerateAndSaveMissingOccurrencesForQuestsAsync(cancellationToken).ConfigureAwait(false);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("ProcessOccurrences successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    _logger.LogInformation("ProcessOccurrences found no new occurrences to add.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing occurrences.");
            }
        }
    }
}