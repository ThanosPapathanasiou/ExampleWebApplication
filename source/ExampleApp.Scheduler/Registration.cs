using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;

namespace ExampleApp.Scheduler;

[UsedImplicitly]
public static class SchedulerExtensions
{
    [UsedImplicitly]
    public static IServiceCollection RegisterScheduler(this IServiceCollection services)
    {
        services.AddDbContext<SchedulerDbContext>((provider, builder) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("schedulerDb");
            builder.UseSqlite(connectionString);
        });
        
        services.AddTickerQ(options =>
        {
            options.AddOperationalStore<SchedulerDbContext>(efOptions =>
            {
                efOptions.UseModelCustomizerForMigrations();
                efOptions.CancelMissedTickersOnApplicationRestart();
            });
            options.AddDashboard("/tickerq");
            
            #if !DEBUG
            options.AddDashboardBasicAuth();
            #endif
        });

        return services;
    }

    [UsedImplicitly]
    public static void PerformSchedulerMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<SchedulerDbContext>();
        dbContext.Database.Migrate();
        dbContext.SaveChanges();
    }
    
    [UsedImplicitly]
    public static void UseScheduler(this WebApplication app)
    {
        app.UseTickerQ();
    }
}