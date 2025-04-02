using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Api.Converters;
using Api.Filters;
using Api.Middlewares;
using Application.Configurations;
using Application.Dtos.Quests;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Quests;
using Application.MappingProfiles;
using Application.Services;
using Application.Services.Quests;
using Application.Validators;
using Application.Validators.Accounts;
using Application.Validators.QuestLabels;
using Application.Validators.Quests;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Interfaces.Quests;
using Domain.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Quests;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

            // Configure Middleware
            ConfigureMiddleware(app);

            // Reset daily questes on startup
            await ResetQuests(app);

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

            //Add Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonConverterForUtcDateTime());
                });

            // Add Swagger for API Documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.UseAllOfToExtendReferenceSchemas();
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter ONLY token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                options.AddSecurityDefinition("x-time-zone", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "x-time-zone",
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Specify the user's timezone in IANA format (e.g., 'America/New_York')"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "x-time-zone"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddFluentValidationRulesToSwagger();

            // Register Repositories
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IQuestRepository, QuestRepository>();
            builder.Services.AddScoped<IResetQuestsRepository, ResetQuestsRepository>();
            builder.Services.AddScoped<IQuestLabelRepository, QuestLabelRepository>();
            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            // Register Services
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IQuestService, QuestService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IQuestResetService, QuestResetService>();
            builder.Services.AddScoped<IQuestLabelService, QuestLabelService>();
            builder.Services.AddScoped<IQuestLabelsHandler, QuestLabelsHandler>();
            builder.Services.AddScoped<IQuestWeekdaysHandler, QuestWeekdaysHandler>();
            builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
            builder.Services.AddScoped<ITokenValidator, TokenValidator>();

            // Register Validators
            builder.Services.AddValidatorsFromAssemblyContaining<BaseCreateQuestValidator<BaseCreateQuestDto>>();
            builder.Services.AddValidatorsFromAssemblyContaining<BaseUpdateQuestValidator<BaseUpdateQuestDto>>();
            builder.Services.AddValidatorsFromAssemblyContaining<BaseQuestCompletionPatchValidator<BaseQuestCompletionPatchDto>>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateAccountValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateQuestLabelValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<PatchQuestLabelValidator>();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            // Register AutoMapper profiles
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<OneTimeQuestProfile>();
                cfg.AddProfile<DailyQuestProfile>();
                cfg.AddProfile<WeeklyQuestProfile>();
                cfg.AddProfile<MonthlyQuestProfile>();
                cfg.AddProfile<SeasonalQuestProfile>();
                cfg.AddProfile<AccountProfile>();
                cfg.AddProfile<QuestLabelProfile>();
            });

            // Configure EF Core with SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information));

            // Register Password Hasher
            builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
            builder.Services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                options.IterationCount = 100000;
            });

            // Register configurations
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // Configure Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // Default is not necessary since we use only one scheme
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:AccessToken:Key"]!)),
                        ClockSkew = TimeSpan.FromSeconds(30) // 30s tolerance for the expiration date
                    };
                });

            // Configure Authorization
            builder.Services.AddScoped<QuestAuthorizationFilter>();
            builder.Services.AddScoped<QuestLabelAuthorizationFilter>();

            // Register Token Handler
            builder.Services.AddSingleton<JwtSecurityTokenHandler>();

            // Register filters
            builder.Services.AddScoped<TimeZoneUpdateFilter>();
        }

        private async static Task ResetQuests(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var questsResetService = serviceProvider.GetRequiredService<IResetQuestsRepository>();
            await questsResetService.ResetQuestsAsync();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            try
            {
                app.UseHttpsRedirection();

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
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GoodieHabits API V1");
                    options.RoutePrefix = string.Empty; // Root path
                });

                // Enable HTTPS Redirection
                app.UseHttpsRedirection();

                //Enable Authentication
                app.UseAuthentication();

                // Enable Authorization
                app.UseAuthorization();

                // Map Controllers
                app.MapControllers();
                Log.Information("Middleware configured successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while configuring the middleware.");
            }
        }
    }
}
