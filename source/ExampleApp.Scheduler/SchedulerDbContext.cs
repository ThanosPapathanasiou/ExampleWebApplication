using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TickerQ.EntityFrameworkCore.Configurations;

namespace ExampleApp.Scheduler;

public class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new TimeTickerConfigurations());
        builder.ApplyConfiguration(new CronTickerConfigurations());
        builder.ApplyConfiguration(new CronTickerOccurrenceConfigurations());
        
        // Override table mappings for all entities to remove schemas
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            builder.Entity(entityType.ClrType).ToTable(entityType.GetTableName());
        }
    }
}