using ENTIDADES.Metrics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace POS3.Data.Mongo;

public class MongoMetricsService : IMetricsService
{
    private readonly MongoMetricsContext _context;
    private readonly ILogger<MongoMetricsService> _logger;

    public MongoMetricsService(MongoMetricsContext context, ILogger<MongoMetricsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogApiUsageAsync(ApiUsageMetric metric, CancellationToken cancellationToken = default)
    {
        if (!_context.IsConfigured)
        {
            return;
        }

        try
        {
            await _context.EnsureIndexesAsync();
            await _context.ApiUsageMetrics.InsertOneAsync(metric, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar métrica de uso de API en MongoDB para {Endpoint}", metric.Endpoint);
        }
    }

    public async Task LogMemoryMetricAsync(MemoryMetric metric, CancellationToken cancellationToken = default)
    {
        if (!_context.IsConfigured)
        {
            return;
        }

        try
        {
            await _context.EnsureIndexesAsync();
            await _context.MemoryMetrics.InsertOneAsync(metric, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar métrica de memoria en MongoDB.");
        }
    }

    public async Task<List<ApiUsageMetric>> GetRecentApiUsageAsync(
        int limit = 100,
        string? endpoint = null,
        bool onlySlow = false,
        CancellationToken cancellationToken = default)
    {
        if (!_context.IsConfigured)
        {
            return new List<ApiUsageMetric>();
        }

        try
        {
            await _context.EnsureIndexesAsync();

            var filterBuilder = Builders<ApiUsageMetric>.Filter;
            var filters = new List<FilterDefinition<ApiUsageMetric>>();

            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                filters.Add(filterBuilder.Eq(x => x.Endpoint, endpoint));
            }

            if (onlySlow)
            {
                filters.Add(filterBuilder.Eq(x => x.IsSlowRequest, true));
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _context.ApiUsageMetrics
                .Find(combinedFilter)
                .SortByDescending(x => x.Timestamp)
                .Limit(limit)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar métricas de uso de API en MongoDB.");
            return new List<ApiUsageMetric>();
        }
    }

    public async Task<List<MemoryMetric>> GetRecentMemoryMetricsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (!_context.IsConfigured)
        {
            return new List<MemoryMetric>();
        }

        try
        {
            await _context.EnsureIndexesAsync();
            return await _context.MemoryMetrics
                .Find(Builders<MemoryMetric>.Filter.Empty)
                .SortByDescending(x => x.Timestamp)
                .Limit(limit)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar métricas de memoria en MongoDB.");
            return new List<MemoryMetric>();
        }
    }
}
