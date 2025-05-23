using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Api.Converters;
using Api.Filters;
using Api.Middlewares;
using Application.Configurations;
using Application.Configurations.Leveling;
using Application.Dtos.Quests;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Quests;
using Application.MappingProfiles;
using Application.Services;
using Application.Services.Quests;
using Application.Validators.Accounts;
using Application.Validators.Auth;
using Application.Validators.QuestLabels;
using Application.Validators.Quests;
using Application.Validators.UserGoal;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Interfaces.Quests;
using Domain.Interfaces.Resetting;
using Domain.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Quests;
using Infrastructure.Repositories.Resetting;
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

            // Reset daily questes on startup (Run in background)
            Task resetQuestsTask = Task.Run(async () =>
            {
                try
                {
                    await ResetQuests(app);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error resetting quests in background.");
                }
            });

            // Expire goals on startup (Run in background)
            Task expireGoalsTask = Task.Run(async () =>
            {
                try
                {
                    await ExpireGoals(app);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error expiring goals in background.");
                }
            });

            Log.Information("Application started");

            // Wait for tasks to complete (with a timeout) during shutdown
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Timeout after 30 seconds

            try
            {
                await Task.WhenAll(resetQuestsTask, expireGoalsTask).WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log.Warning("Background tasks did not complete within the timeout.");
            }

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
            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("SwaggerCors",
            //        builder =>
            //        {
            //            builder
            //                .AllowAnyOrigin()
            //                .AllowAnyHeader()
            //                .AllowAnyMethod();
            //        });
            //});

            //Add Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new TrimmingJsonConverter());
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
            builder.Services.AddScoped<IUserGoalRepository, UserGoalRepository>();
            builder.Services.AddScoped<IGoalExpirationRepository, GoalExpirationRepository>();

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
            builder.Services.AddSingleton<ILevelingService, LevelingService>();
            builder.Services.AddScoped<IUserGoalService, UserGoalService>();

            // Register Validators
            builder.Services.AddValidatorsFromAssemblyContaining<BaseCreateQuestValidator<BaseCreateQuestDto>>();
            builder.Services.AddValidatorsFromAssemblyContaining<BaseUpdateQuestValidator<BaseUpdateQuestDto>>();
            builder.Services.AddValidatorsFromAssemblyContaining<BaseQuestCompletionPatchValidator<BaseQuestCompletionPatchDto>>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateAccountValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateQuestLabelValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<PatchQuestLabelValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<DeleteAccountValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserGoalValidator>();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            // Register AutoMapper profiles
            builder.Services.AddAutoMapper(typeof(AccountProfile).Assembly); // Automatically register all profiles in the assembly

            // Configure EF Core with SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Register Password Hasher
            builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
            builder.Services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                options.IterationCount = 100000;
            });

            // Register configurations
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<LevelingOptions>(builder.Configuration.GetSection(LevelingOptions.SectionName));

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
        }

        private async static Task ResetQuests(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var questsResetService = serviceProvider.GetRequiredService<IResetQuestsRepository>();
            int resetedQuests = await questsResetService.ResetQuestsAsync();
            Log.Information("{Count} quests reset.", resetedQuests);
        }

        private async static Task ExpireGoals(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var expireGoalsService = serviceProvider.GetRequiredService<IGoalExpirationRepository>();
            var expiredGoals = await expireGoalsService.ExpireGoalsAsync();
            Log.Information("{Count} goals expired.", expiredGoals);
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();
            //app.UseCors("SwaggerCors");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "GoodieHabits API V1");
                options.RoutePrefix = string.Empty; // Set to empty to make Swagger at root
            });
        }
    }
}
