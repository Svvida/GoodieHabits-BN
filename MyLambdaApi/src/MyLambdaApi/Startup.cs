using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Accounts.ChangePassword;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Quests;
using Application.Configurations;
using Application.Dtos.Quests;
using Application.Quests.CreateQuest.Validators;
using Application.Services;
using Application.Services.Quests;
using Application.Statistics.Calculators;
using Application.Validators.Accounts;
using Application.Validators.Auth;
using Application.Validators.QuestLabels;
using Application.Validators.Quests;
using Application.Validators.UserGoal;
using Domain.Common;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyLambdaApi.Converters;
using MyLambdaApi.Filters;
using MyLambdaApi.Middlewares;
using NodaTime;

namespace MyLambdaApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("SwaggerCors",
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        //Add Controllers
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new TrimmingJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonConverterForUtcDateTime());
                //options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // Might be useful if Lambda unproperly serialize emoji
            });

        // Add Swagger for API Documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
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

        // Register Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IQuestService, QuestService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQuestResetService, QuestResetService>();
        services.AddScoped<IQuestLabelService, QuestLabelService>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<ITokenValidator, TokenValidator>();
        services.AddSingleton<ILevelCalculator, LevelCalculator>();
        services.AddScoped<IUserGoalService, UserGoalService>();
        services.AddScoped<IQuestStatisticsService, QuestStatisticsService>();
        services.AddSingleton<IClock>(SystemClock.Instance); // Use NodaTime's SystemClock
        services.AddScoped<IQuestOccurrenceGenerator, QuestOccurrencesGenerator>();
        services.AddScoped<IQuestStatisticsCalculator, QuestStatisticsCalculator>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IGoalExpirationService, GoalExpirationService>();

        // Register Validators
        services.AddValidatorsFromAssemblyContaining<CreateQuestCommandValidator<BaseCreateQuestDto>>();
        services.AddValidatorsFromAssemblyContaining<BaseUpdateQuestValidator<BaseUpdateQuestDto>>();
        services.AddValidatorsFromAssemblyContaining<QuestCompletionPatchValidator<QuestCompletionPatchDto>>();
        services.AddValidatorsFromAssemblyContaining<CreateAccountValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateAccountValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateQuestLabelValidator>();
        services.AddValidatorsFromAssemblyContaining<PatchQuestLabelValidator>();
        services.AddValidatorsFromAssemblyContaining<ChangePasswordCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<PasswordConfirmationValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateUserGoalValidator>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        // Register AutoMapper profiles
        //var profileAssembly = typeof(AccountProfile).Assembly;

        //services.AddAutoMapper(cfg =>
        //{
        //    cfg.LicenseKey = services..Configuration["AutomapperKey"] ?? string.Empty;
        //}, profileAssembly);

        //// Register MediatR
        //builder.Services.AddMediatR(cfg =>
        //{
        //    cfg.LicenseKey = builder.Configuration["AutomapperKey"] ?? string.Empty;
        //});

        // Configure EF Core with SQL Server
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Register Password Hasher
        services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100000;
        });

        // Register configurations
        services.Configure<JwtSettings>(Configuration.GetSection("Jwt"));
        services.Configure<LevelingOptions>(Configuration.GetSection(LevelingOptions.SectionName));

        // Configure Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:AccessToken:Key"]!)),
                    ClockSkew = TimeSpan.FromSeconds(30) // 30s tolerance for the expiration date
                };
            });

        // Configure Authorization
        services.AddScoped<QuestAuthorizationFilter>();
        services.AddScoped<QuestLabelAuthorizationFilter>();

        // Register Token Handler
        services.AddSingleton<JwtSecurityTokenHandler>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "GoodieHabits API V1");
                options.RoutePrefix = string.Empty; // Set to empty to make Swagger at root
            });
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("GoodieHabbi API is running!");
            });
        });
    }
}