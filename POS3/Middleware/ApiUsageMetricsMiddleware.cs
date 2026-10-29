using System.Diagnostics;
using System.Security.Claims;
using ENTIDADES.Metrics;
using POS3.Data.Mongo;

namespace POS3.Middleware;

/// <summary>
/// Middleware que intercepta las peticiones HTTP para registrar métricas de uso y rendimiento en MongoDB.
/// </summary>
public class ApiUsageMetricsMiddleware
{
    private const double SlowRequestThresholdMs = 1000.0;
    private readonly RequestDelegate _next;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<ApiUsageMetricsMiddleware> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ApiUsageMetricsMiddleware"/>.
    /// </summary>
    public ApiUsageMetricsMiddleware(
        RequestDelegate next,
        IMetricsService metricsService,
        ILogger<ApiUsageMetricsMiddleware> logger)
    {
        _next = next;
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Procesa la petición HTTP y registra la métrica de uso en MongoDB.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Omitir recursos de Swagger para no contaminar las métricas de la API
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            try
            {
                int? userId = null;
                var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? context.User?.FindFirst("id")?.Value;
                if (int.TryParse(userIdClaim, out int parsedId))
                {
                    userId = parsedId;
                }

                var elapsedMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);

                var metric = new ApiUsageMetric
                {
                    UserId = userId,
                    Endpoint = path,
                    HttpMethod = context.Request.Method,
                    StatusCode = context.Response.StatusCode,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    ResponseTimeMs = elapsedMs,
                    IsSlowRequest = elapsedMs >= SlowRequestThresholdMs,
                    Timestamp = DateTime.UtcNow,
                    ApplicationName = "POS3"
                };

                _ = _metricsService.LogApiUsageAsync(metric);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo capturar la métrica de uso de la API.");
            }
        }
    }
}

