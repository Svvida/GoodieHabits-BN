using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Api.BackgroundTasks;
using Api.Converters;
using Api.Middlewares;
using Application.Auth.Commands.Register;
using Application.Badges;
using Application.Badges.Strategies;
using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Badges;
using Application.Common.Interfaces.Email;
using Application.Common.Interfaces.Notifications;
using Application.Quests;
using Application.Statistics.Calculators;
using Application.UserProfiles.Nickname;
using Domain.Common;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Models;
using FluentValidation;
using Infrastructure.Authentication;
using Infrastructure.Email;
using Infrastructure.Email.Senders;
using Infrastructure.Notifications;
using Infrastructure.Persistence;
using Infrastructure.Photos;
using Infrastructure.Services;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NodaTime;
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
            var applicationAssembly = typeof(Application.AssemblyReference).Assembly;
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("SignalRCors", policy =>
                {
                    policy.SetIsOriginAllowed(origin => true) // Allow any origin since it is mobile app API
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // This is crucial for SignalR
                });
            });

            //Add Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new TrimmingJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new JsonConverterForUtcDateTime());
                    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
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

                options.EnableAnnotations();
            });

            // Register Repositories
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            builder.Services.AddSingleton<ITokenGenerator, TokenGenerator>();
            builder.Services.AddSingleton<ITokenValidator, TokenValidator>();
            builder.Services.AddSingleton<ILevelCalculator, LevelCalculator>();
            builder.Services.AddSingleton<IClock>(SystemClock.Instance); // Use NodaTime's SystemClock
            builder.Services.AddScoped<INicknameGenerator, NicknameGenerator>();
            builder.Services.AddScoped<IQuestMapper, QuestMapper>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IForgotPasswordEmailSender, ForgotPasswordEmailSender>();
            builder.Services.AddScoped<IPhotoService, CloudinaryPhotoService>();
            builder.Services.AddScoped<IUrlBuilder, CloudinaryUrlBuilder>();
            builder.Services.AddSingleton<INotificationSender, SignalRNotificationSender>();

            // Register Badge awarding strategies
            // Orchestrator
            builder.Services.AddScoped<IBadgeAwardingService, BadgeAwardingService>();
            // Strategies
            builder.Services.AddScoped<IBadgeAwardingStrategy, BalancedHeroBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, Complete500QuestsBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, CompleteDailySevenBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, CompleteDailyThirtyBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, CompleteMonthlyTwelveBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, Create100QuestsBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, CreateFirstQuestBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, CreateTwentyQuestsBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, FailureComebackBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, GoalCompleteFiftyBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, GoalCompleteFirstBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, GoalCompleteTenBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, GoalCompleteYearlyBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, GoalCreateFirstBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, GoalCreateTenBadgeStrategy>();
            builder.Services.AddScoped<IBadgeAwardingStrategy, StreakRecoveryBadgeStrategy>();

            // Register Validators
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();

            // Register Mapster
            var typeAdapterConfig = new TypeAdapterConfig();

            typeAdapterConfig.Scan(applicationAssembly); // Scan the current assembly for mappings

            builder.Services.AddSingleton(typeAdapterConfig);

            builder.Services.AddScoped<IMapper, ServiceMapper>();

            // Register MediatR
            builder.Services.AddMediatR(cfg =>
            {
                cfg.LicenseKey = builder.Configuration["AutomapperKey"] ?? string.Empty;
                cfg.RegisterServicesFromAssembly(applicationAssembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // Register SignalR
            builder.Services.AddSignalR();
            // Custom provider as we don't use 'sub' claim
            builder.Services.AddSingleton<IUserIdProvider, UserProfileIdProvider>();

            // Configure EF Core with SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment()); // Enable sensitive data logging only in development
            });

            // Register Password Hasher
            builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
            builder.Services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                options.IterationCount = 100000;
            });

            // Register configurations
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
            builder.Services.Configure<LevelingOptions>(builder.Configuration.GetSection(LevelingOptions.SectionName));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(CloudinarySettings.SectionName));

            // Configure Cloudinary settings
            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;

                var account = new CloudinaryDotNet.Account(
                    settings.CloudName,
                    settings.ApiKey,
                    settings.ApiSecret);

                var cloudinary = new CloudinaryDotNet.Cloudinary(account)
                {
                    Api = { Secure = true }
                };

                return cloudinary;
            });

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

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/hubs/notifications"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            // Register Token Handler
            builder.Services.AddSingleton<JwtSecurityTokenHandler>();

            // Register Startup Tasks
            builder.Services.AddHostedService<ResetQuestsTask>();
            builder.Services.AddHostedService<ExpireGoalsTask>();
            builder.Services.AddHostedService<ProcessOccurrencesTask>();
            builder.Services.AddHostedService<RecalculateRepeatableQuestStatisticsTask>();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();
            //app.UseCors("SwaggerCors");
            app.UseCors("SignalRCors");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<NotificationHub>("/api/hubs/notifications");

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
