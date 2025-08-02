using Microsoft.Extensions.Logging;
using TickerQ.Utilities.Base;

namespace ExampleApp.Scheduler;

public static class CronExpressions
{
    // cron expressions -> https://cronevery.day/
    public const string EveryMinute    = "* * * * *";
    public const string Every10Minutes = "*/10 * * * *";
    public const string EveryHour      = "0 * * * *";
}

public class ScheduledTasks(ILogger<ScheduledTasks> logger)
{
    
    [TickerFunction(nameof(ShortRunningTask), CronExpressions.EveryMinute)]
    public async Task ShortRunningTask()
    {
        logger.LogInformation("Started cronjob {cronjob}", nameof(ShortRunningTask));
        await Task.Delay(TimeSpan.FromSeconds(5));
        logger.LogInformation("Finished cronjob {cronjob}", nameof(ShortRunningTask));
    }

    [TickerFunction(nameof(MediumRunningTask), CronExpressions.Every10Minutes)]
    public async Task MediumRunningTask()
    {
        logger.LogInformation("Started cronjob {cronjob}", nameof(MediumRunningTask));
        await Task.Delay(TimeSpan.FromMinutes(5));
        logger.LogInformation("Finished cronjob {cronjob}", nameof(MediumRunningTask));
    }
    
    [TickerFunction(nameof(LongRunningTask), CronExpressions.EveryHour)]
    public async Task LongRunningTask()
    {
        logger.LogInformation("Started cronjob {cronjob}", nameof(LongRunningTask));
        await Task.Delay(TimeSpan.FromMinutes(15));
        logger.LogInformation("Finished cronjob {cronjob}", nameof(LongRunningTask));
    }
}