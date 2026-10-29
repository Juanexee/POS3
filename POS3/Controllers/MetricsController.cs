using ENTIDADES.Metrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS3.Data.Mongo;

namespace POS3.Controllers
{
    /// <summary>
    /// Endpoints para consultar las métricas de rendimiento de la API y el uso de memoria almacenadas en MongoDB.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Gerente,Administrador,Supervisor,admin,El super admin,Administrador3")]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="MetricsController"/>.
        /// </summary>
        public MetricsController(IMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        /// <summary>
        /// Obtiene los registros recientes de uso y tiempos de respuesta de la API desde MongoDB (`apiUsageLogs`).
        /// </summary>
        /// <param name="limit">Cantidad máxima de registros a devolver (1 a 500, por defecto 100).</param>
        /// <param name="endpoint">Filtro opcional por ruta exacta del endpoint (ej. `/api/Ventas`).</param>
        /// <param name="onlySlow">Si es `true`, devuelve únicamente las peticiones lentas (`>= 1000 ms`).</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de métricas de uso de la API.</returns>
        [HttpGet("api-usage")]
        [ProducesResponseType(typeof(List<ApiUsageMetric>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApiUsageMetrics(
            [FromQuery] int limit = 100,
            [FromQuery] string? endpoint = null,
            [FromQuery] bool onlySlow = false,
            CancellationToken cancellationToken = default)
        {
            limit = Math.Clamp(limit, 1, 500);
            var metrics = await _metricsService.GetRecentApiUsageAsync(limit, endpoint, onlySlow, cancellationToken);
            return Ok(metrics);
        }

        /// <summary>
        /// Obtiene las métricas recientes de memoria y CPU del servidor desde MongoDB (`memoryMetrics`).
        /// </summary>
        /// <param name="limit">Cantidad máxima de registros a devolver (1 a 500, por defecto 100).</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de métricas de memoria y CPU.</returns>
        [HttpGet("memory")]
        [ProducesResponseType(typeof(List<MemoryMetric>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMemoryMetrics(
            [FromQuery] int limit = 100,
            CancellationToken cancellationToken = default)
        {
            limit = Math.Clamp(limit, 1, 500);
            var metrics = await _metricsService.GetRecentMemoryMetricsAsync(limit, cancellationToken);
            return Ok(metrics);
        }

        /// <summary>
        /// Obtiene un resumen consolidado de rendimiento de la API y el último estado de memoria/CPU.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Resumen con promedios de latencia, conteo de peticiones lentas y estado actual de memoria.</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetMetricsSummary(CancellationToken cancellationToken = default)
        {
            var recentApi = await _metricsService.GetRecentApiUsageAsync(200, cancellationToken: cancellationToken);
            var recentMemory = await _metricsService.GetRecentMemoryMetricsAsync(1, cancellationToken);

            var totalSampled = recentApi.Count;
            var slowRequests = recentApi.Count(x => x.IsSlowRequest);
            var avgResponseMs = totalSampled > 0
                ? Math.Round(recentApi.Average(x => x.ResponseTimeMs), 2)
                : 0.0;

            return Ok(new
            {
                TotalSampledRequests = totalSampled,
                SlowRequestsCount = slowRequests,
                AverageResponseTimeMs = avgResponseMs,
                LatestMemoryMetric = recentMemory.FirstOrDefault()
            });
        }
    }
}

