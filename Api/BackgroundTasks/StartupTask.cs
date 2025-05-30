namespace Api.BackgroundTasks
{
    public abstract class StartupTask : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        protected abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
