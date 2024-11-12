using MemoryCache.Api.Constants;
using MemoryCache.Api.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace MemoryCache.Api.BackgroundServices;

public class CacheBackgroundService(
    ILogger<CacheBackgroundService> logger,
    IMemoryCache memoryCache,
    IOptions<CacheSettings> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Start cache update");

            try
            {
                var model = new CacheModel("Current time", TimeOnly.FromDateTime(DateTime.UtcNow));

                memoryCache.Set(key: CacheKeys.MyKey, value: model);

                logger.LogInformation("Cache update succeeded");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Cache update error");
            }

            var timespan = options.Value.RefreshTimespan;

            logger.LogInformation(
                "Next cache update will happen in {Timespan} at {NextRunTime} UTC",
                timespan, DateTime.UtcNow.Add(timespan).ToString("HH:mm:ss"));

            await Task.Delay(timespan, cancellationToken);
        }
    }
}
