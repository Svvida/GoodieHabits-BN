using Api.Middlewares;
using Application.Helpers;
using Application.Interfaces;
using Application.MappingProfiles;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Quests;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Logger
            ConfigureLogger();
            Log.Information("Starting application");

            var builder = WebApplication.CreateBuilder(args);

            Log.Information("Active Environment: {Environment}", builder.Environment.EnvironmentName);
            Log.Information("Using Connection String: {ConnectionString}", builder.Configuration.GetConnectionString("DefaultConnection"));

            // Configure Host
            ConfigureHost(builder);

            // Load configuration for the current environment
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // Configure Services
            ConfigureServices(builder);

            // Build Application
            var app = builder.Build();

            // Seed Data
            await SeedDatabaseAsync(app);

            // Configure Middleware
            ConfigureMiddleware(app);

            Log.Information("Application started");
            await app.RunAsync();
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                    .Build())
                .CreateLogger();
        }

        private static void ConfigureHost(WebApplicationBuilder builder)
        {
            // Use Serilog as the logging provider
            builder.Host.UseSerilog();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWithCredentials",
                    builder =>
                    {
                        builder
                            .SetIsOriginAllowed(origin => true)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            // Add Controllers
            builder.Services.AddControllers();

            // Add Swagger for API Documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.UseAllOfToExtendReferenceSchemas();
            });

            // Register Repositories
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IOneTimeQuestRepository, OneTimeQuestRepository>();
            builder.Services.AddScoped<IDailyQuestRepository, DailyQuestRepository>();
            builder.Services.AddScoped<IWeeklyQuestRepository, WeeklyQuestRepository>();
            builder.Services.AddScoped<IMonthlyQuestRepository, MonthlyQuestRepository>();
            builder.Services.AddScoped<ISeasonalQuestRepository, SeasonalQuestRepository>();
            builder.Services.AddScoped<IQuestMetadataRepository, QuestMetadataRepository>();

            // Register Services
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IOneTimeQuestService, OneTimeQuestService>();
            builder.Services.AddScoped<IDailyQuestService, DailyQuestService>();
            builder.Services.AddScoped<IWeeklyQuestService, WeeklyQuestService>();
            builder.Services.AddScoped<IMonthlyQuestService, MonthlyQuestService>();
            builder.Services.AddScoped<ISeasonalQuestService, SeasonalQuestService>();
            builder.Services.AddScoped<IQuestMetadataService, QuestMetadataService>();

            // Register AutoMapper profiles
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<OneTimeQuestProfile>();
                cfg.AddProfile<DailyQuestProfile>();
                cfg.AddProfile<WeeklyQuestProfile>();
                cfg.AddProfile<MonthlyQuestProfile>();
                cfg.AddProfile<SeasonalQuestProfile>();
                cfg.AddProfile<QuestMetadataProfile>();
            });
            builder.Services.AddTransient<QuestMetadataResolver>();

            // Register Database Seeder
            builder.Services.AddScoped<DataSeeder>();

            // Configure EF Core with SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));
        }

        private static async Task SeedDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Check if seeding is enabled
            var seedData = configuration.GetValue<bool>("SeedData");
            if (!seedData)
            {
                Log.Information("Seeding is disabled in the configuration.");
                return;
            }

            Log.Information("Seeding is enabled. Starting seeding process...");

            var seeder = serviceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // Handle OPTIONS requests explicitly for Preflight
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    var origin = context.Request.Headers.Origin.ToString();
                    context.Response.Headers.Append("Access-Control-Allow-Origin", origin); // Reflect the request origin dynamically
                    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE, OPTIONS");
                    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                    context.Response.StatusCode = 204; // No Content
                    return;
                }
                await next();
            });

            app.UseCors("AllowWithCredentials"); // CORS must be applied before Routing

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRouting();

            // Enable Swagger in all environments
            app.UseSwagger();
            app.UseSwaggerUI();

            // Enable HTTPS Redirection
            app.UseHttpsRedirection();


            // Enable Authorization
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();
        }
    }
}
