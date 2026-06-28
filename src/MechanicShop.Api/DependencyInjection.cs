using System.Globalization;
using System.IO.Compression;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using MechanicShop.Api.Infrastructure;
using MechanicShop.Api.OpenApi.Transformers;
using MechanicShop.Api.Services;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.JsonWebTokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace MechanicShop.Api;

public static class DependencyInjection
{
    private const string UnknownIp = "unknown";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();
        services.AddApiVersioning();
        services.AddOutputCacheWithPolicies();
        services.AddRateLimiter();
        services.AddResponseCompressionWithProviders();
        services.AddCustomProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApiDocs();
        services.AddCorsPolicies(configuration);
        services.AddTelemetry(configuration);
        services.AddControllerWithJsonConfiguration();
        services.AddRequestSizeLimits();

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app, IConfiguration configuration)
    {
        var corsPolicyName = configuration["AppSettings:CorsPolicyName"] ?? "DefaultCorsPolicy";

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseHttpsRedirection();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<RequestLogContextMiddleware>();
        app.UseResponseCompression();
        app.UseOutputCache();
        app.UseCors(corsPolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    private static void AddApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }

    private static void AddOpenApiDocs(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
            options.AddDocumentTransformer<VersionInfoTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemaTransformer>();
            options.AddOperationTransformer<BearerSecuritySchemaTransformer>();
        });

        // Add Swagger generator for Swagger UI
        services.AddSwaggerGen();
    }

    private static void AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("AppSettings:CorsAllowedOrigins")
            .Get<string[]>() ?? [];

        var policyName = configuration["AppSettings:CorsPolicyName"] ?? "DefaultCorsPolicy";

        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private static void AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["OpenTelemetry:Endpoint"];

        services
            .AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("MechanicShop.Api"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(endpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();

                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(endpoint));
                }
            });
    }

    private static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        return services;
    }

    private static IServiceCollection AddOutputCacheWithPolicies(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddPolicy(CachePolicies.AuthUser, builder =>
            {
                builder.AddPolicy<AuthUserOutputCachePolicy>();
                builder.Expire(TimeSpan.FromMinutes(1));
                builder.SetVaryByQuery("page", "size");
                builder.Cache();
            });
        });
        return services;
    }

    private static IServiceCollection AddRequestSizeLimits(this IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 3 * 1024 * 1024;
        });

        return services;
    }

    private static IServiceCollection AddResponseCompressionWithProviders(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Append("application/json");
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Fastest);

        services.Configure<GzipCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Fastest);

        return services;
    }

    private static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
        });

        return services;
    }

    private static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global backstop: 300 req/min per IP — applied to every request before named policies
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? UnknownIp;
                return RateLimitPartition.GetSlidingWindowLimiter($"{RateLimitPolicies.Global}:{ip}",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        PermitLimit = 300,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            // 5 login attempts per 60s per IP — brute-force protection
            options.AddPolicy(RateLimitPolicies.Auth, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? UnknownIp;
                return RateLimitPartition.GetFixedWindowLimiter($"{RateLimitPolicies.Auth}:{ip}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromSeconds(60),
                        PermitLimit = 5,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            // 10 refresh attempts per 60s per IP
            options.AddPolicy(RateLimitPolicies.Refresh, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? UnknownIp;
                return RateLimitPartition.GetFixedWindowLimiter($"{RateLimitPolicies.Refresh}:{ip}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromSeconds(60),
                        PermitLimit = 10,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            // 30 write operations per 60s per authenticated user — all mutation endpoints
            options.AddPolicy(RateLimitPolicies.Writes, context =>
            {
                var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? UnknownIp;
                return RateLimitPartition.GetSlidingWindowLimiter($"{RateLimitPolicies.Writes}:{userId}",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromSeconds(60),
                        SegmentsPerWindow = 6,
                        PermitLimit = 30,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            // 10 PDF generations per 60s per authenticated user — expensive operation
            options.AddPolicy(RateLimitPolicies.PdfExport, context =>
            {
                var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? UnknownIp;
                return RateLimitPartition.GetFixedWindowLimiter($"{RateLimitPolicies.PdfExport}:{userId}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromSeconds(60),
                        PermitLimit = 10,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.OnRejected = async (ctx, cancellationToken) =>
            {
                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    ctx.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await ctx.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.", cancellationToken);
            };
        });

        return services;
    }
}

internal static class RateLimitPolicies
{
    internal const string Global = "global";
    internal const string Auth = "auth";
    internal const string Refresh = "refresh";
    internal const string Writes = "writes";
    internal const string PdfExport = "pdfExport";
}

internal static class CachePolicies
{
    internal const string AuthUser = "AuthUser";
}

