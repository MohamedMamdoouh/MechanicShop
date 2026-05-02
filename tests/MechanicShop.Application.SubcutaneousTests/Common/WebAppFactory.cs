using MechanicShop.Api;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using Xunit;
namespace MechanicShop.Application.SubcutaneousTests.Common;

public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer =
        new MsSqlBuilder()
            .WithPassword("Str0ng_password_123!")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .Build();

    private readonly List<IServiceScope> _scopes = [];
    public IMediator CreateMediator()
    {
        var scope = Services.CreateScope();
        _scopes.Add(scope);
        return scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public IAppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        _scopes.Add(scope);
        return scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    }

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
        });
    }

}
