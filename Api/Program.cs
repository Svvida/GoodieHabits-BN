using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
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

            // Configure Host
            ConfigureHost(builder);

            // Configure Services
            ConfigureServices(builder);

            // Build Application
            var app = builder.Build();

            // Seed Data
            //await SeedDatabaseAsync(app);

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
            // Add Controllers
            builder.Services.AddControllers();

            // Add Swagger for API Documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.UseAllOfToExtendReferenceSchemas();

                c.SelectSubTypesUsing(type =>
                {
                    if (type == typeof(RepeatInterval))
                    {
                        return new[] { typeof(DailyRepeatInterval), typeof(WeeklyRepeatInterval), typeof(MonthlyRepeatInterval) };
                    }
                    return Enumerable.Empty<Type>();
                });
            });

            // Register Repositories
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IOneTimeQuestRepository, OneTimeQuestRepository>();
            builder.Services.AddScoped<IRepeatableQuestRepository, RepeatableQuestRepository>();
            builder.Services.AddScoped<ISeasonalQuestRepository, SeasonalQuestRepository>();
            builder.Services.AddScoped<IUserSeasonalQuestRepository, UserSeasonalQuestRepository>();

            // Register Services
            builder.Services.AddScoped<IAccountService, AccountService>();

            // Register Database Seeder
            builder.Services.AddScoped<DataSeeder>();

            // Configure EF Core with SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

        private static async Task SeedDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var seeder = serviceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            // Enable Swagger in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable HTTPS Redirection
            app.UseHttpsRedirection();

            // Enable Authorization
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();
        }
    }
}
