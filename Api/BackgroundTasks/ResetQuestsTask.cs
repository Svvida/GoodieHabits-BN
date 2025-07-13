using Domain.Interfaces;

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
            _logger.LogInformation("ResetQuestsTask started.");
            await using var scope = _scopeFactory.CreateAsyncScope();

            try
            {
                var questResetService = scope.ServiceProvider.GetRequiredService<IQuestResetService>();

                int affectedRows = await questResetService.ResetCompletedQuestsAsync(cancellationToken).ConfigureAwait(false);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("ResetQuestsTask successfully saved changes to the database. Affected rows: {Count}.", affectedRows);
                }
                else
                {
                    _logger.LogInformation("ResetQuestsTask found no quests to reset.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing ResetQuestsTask: {Message}", ex.Message);
            }
        }
    }
}
