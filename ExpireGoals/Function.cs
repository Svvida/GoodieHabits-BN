using Amazon.Lambda.Core;
using Application.Common.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpireGoalsLambda;

public class Function
{
    // Static field to hold the scope factory. It's created only once
    private static readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Function> _logger;

    // Static constructor: Runs only once when the class is first loaded by the runtime.
    static Function()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        // Build the service provider and get the scope factory. Store it statically.
        var serviceProvider = services.BuildServiceProvider();
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    // Instance constructor: This will run for each "cold start".
    // It should be lightweight and only resolve services needed for the instance itself, like logger.
    public Function()
    {
        using var scope = _scopeFactory.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<Function>>();
        _logger.LogInformation("New Function instance created (cold start).");
    }
    private static void ConfigureServices(IServiceCollection services)
    {
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

        // DbContextFactory for better DbContext management in non-HTTP contexts like AWS Lambda
        services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Register all services with scoped lifetime
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGoalExpirationService, GoalExpirationService>();
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
        _logger.LogInformation("FunctionHandler invoked. Request ID: {AwsRequestId}", context.AwsRequestId);

        await using var scope = _scopeFactory.CreateAsyncScope();

        try
        {
            var expirationService = scope.ServiceProvider.GetRequiredService<IGoalExpirationService>();

            _logger.LogInformation("Attempting to expire goals...");

            int affectedRows = await expirationService.ExpireGoalsAndSaveAsync(CancellationToken.None);

            _logger.LogInformation("{Count} goals expired successfully.", affectedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during expiring goals. Request ID: {AwsRequestId}", context.AwsRequestId);
            throw;
        }
    }
}
