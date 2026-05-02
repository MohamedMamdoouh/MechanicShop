using System.Threading.RateLimiting;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Identity;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Testcontainers.MsSql;
using Xunit;
namespace MechanicShop.Api.IntegrationTests.Common;

public class WebFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer =
     new MsSqlBuilder()
         .WithPassword("Str0ng_password_123!")
         .Build();

    private readonly List<IServiceScope> _scopes = [];
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        foreach (var scope in _scopes)
        {
            scope.Dispose();
        }

        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<OverdueBookingCleanupService>();

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var interceptor = sp.GetService<ISaveChangesInterceptor>();
                if (interceptor != null)
                {
                    options.AddInterceptors(interceptor);
                }

                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            services.RemoveAll<AppSettings>();
            services.PostConfigureAll<AppSettings>(settings =>
            {
                settings.OpeningTime = new TimeOnly(9, 0);
                settings.ClosingTime = new TimeOnly(17, 0);
                settings.ShopTimeZone = "Africa/Cairo";
            });

            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.Configure<RateLimiterOptions>(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy("auth", _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy("refresh", _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy("writes", _ => RateLimitPartition.GetNoLimiter("no-limit"));
                options.AddPolicy("pdfExport", _ => RateLimitPartition.GetNoLimiter("no-limit"));
            });
        });
    }

    public AppHttpClient CreateAppHttpClient() => new(CreateClient());

    public IAppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        _scopes.Add(scope);
        return scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    }

    public async Task<(string Email, string Password)> SeedUserAsync(Role role = Role.Manager)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roleName = role.ToString();
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var email = $"test-{id}@example.com";
        const string Password = "Test@1234!";

        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, Password);
        await userManager.AddToRoleAsync(user, roleName);

        return (email, Password);
    }
}
