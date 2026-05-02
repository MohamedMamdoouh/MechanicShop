using System.Text;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Domain.Customers;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Data.Interceptors;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Infrastructure.Identity.Models;
using MechanicShop.Infrastructure.Identity.Policies;
using MechanicShop.Infrastructure.Realtime;
using MechanicShop.Infrastructure.Services;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using SendGrid;
namespace MechanicShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddSettings();
        services.AddDatabase(configuration);
        services.AddIdentityServices();
        services.AddJwtAuthentication();
        services.AddAuthorizationPolicies();
        services.AddApplicationServices();
        services.AddSendGridClient();
        services.AddRealtime();
        services.AddBackgroundJobs();
        services.AddCaching();
        services.AddInfrastructureHealthChecks();
        services.AddSingleton<IPhoneValidator, PhoneValidatorService>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    private static void AddSettings(this IServiceCollection services)
    {
        services
            .AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<TokenSettings>()
            .BindConfiguration(TokenSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<AppSettings>()
            .BindConfiguration(AppSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<SendGridSettings>()
            .BindConfiguration(SendGridSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ApplicationDbContextInitialiser>();
    }

    private static void AddIdentityServices(this IServiceCollection services)
    {
        services
            .AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddScoped<IIdentityService, IdentityService>();
    }

    private static void AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((bearerOptions, jwtSettings) =>
            {
                var jwt = jwtSettings.Value;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                bearerOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) &&
                            ctx.HttpContext.Request.Path.StartsWithSegments(WorkOrderHub.HubUrl))
                        {
                            ctx.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IRefreshTokenFactory, RefreshTokenFactory>();
        services.AddScoped<ITokenSessionService, TokenSessionService>();
    }

    private static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, LaborAssignedHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy("LaborAssigned", policy =>
                policy.AddRequirements(new LaborAssignedRequirement()));
    }

    private static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
        services.AddScoped<IWorkOrderNotifier, SignalRWorkOrderNotifier>();
    }

    private static void AddSendGridClient(this IServiceCollection services)
    {
        services.AddSingleton<ISendGridClient>(sp =>
        {
            var apiKey = sp.GetRequiredService<IOptions<SendGridSettings>>().Value.ApiKey;
            return new SendGridClient(apiKey);
        });
    }

    private static void AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
    }

    private static void AddInfrastructureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "database", tags: ["db", "ready"]);
    }

    private static void AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<OverdueBookingCleanupService>();
    }

    private static void AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHybridCache();
    }
}