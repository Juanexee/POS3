using ENTIDADES.Metrics;

namespace POS3.Data.Mongo;

public interface IMetricsService
{
    Task LogApiUsageAsync(ApiUsageMetric metric, CancellationToken cancellationToken = default);
    Task LogMemoryMetricAsync(MemoryMetric metric, CancellationToken cancellationToken = default);
    Task<List<ApiUsageMetric>> GetRecentApiUsageAsync(
        int limit = 100,
        string? endpoint = null,
        bool onlySlow = false,
        CancellationToken cancellationToken = default);
    Task<List<MemoryMetric>> GetRecentMemoryMetricsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);
}
