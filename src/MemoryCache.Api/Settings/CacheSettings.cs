namespace MemoryCache.Api.Settings;

public record CacheSettings
{
    public TimeSpan RefreshTimespan { get; init; }
}
