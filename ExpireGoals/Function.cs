using Amazon.Lambda.Core;
using Domain.Interfaces.Resetting;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Resetting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpireGoalsLambda;

public class Function
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Function> _logger;

    public Function()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddConsole();
        });

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("FATAL: ConnectionStrings__DefaultConnection environment variable not set.");
            throw new InvalidOperationException("Connection string is not set.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            Console.WriteLine("Initializing DbContext with connection string...");
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IGoalExpirationRepository, GoalExpirationRepository>();

        _serviceProvider = services.BuildServiceProvider();

        _logger = _serviceProvider.GetRequiredService<ILogger<Function>>();
        _logger.LogInformation("Function instance initialized.");
    }
    public async Task FunctionHandler(ILambdaContext context)
    {
        _logger.LogInformation("FunctionHandler invoked. Request ID: {AwsRequestId}", context.AwsRequestId);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var resetRepo = scope.ServiceProvider.GetRequiredService<IGoalExpirationRepository>();

            _logger.LogInformation("Attempting to expire goals...");

            var expiredGoals = await resetRepo.PrepareGoalsForExpirationAsync(CancellationToken.None);

            _logger.LogInformation($"{expiredGoals} goals expired successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during expiring goals. Request ID: {AwsRequestId}", context.AwsRequestId);
            throw;
        }
    }
}
