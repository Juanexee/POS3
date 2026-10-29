using System.Diagnostics;
using ENTIDADES.Metrics;
using POS3.Data.Mongo;

namespace POS3.Services;

/// <summary>
/// Servicio en segundo plano que recolecta periódicamente métricas de memoria y CPU del proceso y las almacena en MongoDB.
/// </summary>
public class MemoryMetricsBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MemoryMetricsBackgroundService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="MemoryMetricsBackgroundService"/>.
    /// </summary>
    public MemoryMetricsBackgroundService(
        IMetricsService metricsService,
        ILogger<MemoryMetricsBackgroundService> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var process = Process.GetCurrentProcess();
        var lastCpuTime = process.TotalProcessorTime;
        var lastCheckTime = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                process.Refresh();

                var now = DateTime.UtcNow;
                var currentCpuTime = process.TotalProcessorTime;

                var cpuUsedMs = (currentCpuTime - lastCpuTime).TotalMilliseconds;
                var totalMsPassed = (now - lastCheckTime).TotalMilliseconds;
                double cpuUsagePercent = 0;

                if (totalMsPassed > 0)
                {
                    cpuUsagePercent = Math.Round(
                        (cpuUsedMs / (Environment.ProcessorCount * totalMsPassed)) * 100.0,
                        2);
                }

                lastCpuTime = currentCpuTime;
                lastCheckTime = now;

                double processMemoryMb = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2);
                double gcMemoryMb = Math.Round(GC.GetTotalMemory(false) / (1024.0 * 1024.0), 2);

                string status = processMemoryMb switch
                {
                    >= 1000 => "Critical",
                    >= 500 => "Warning",
                    _ => "Normal"
                };

                var metric = new MemoryMetric
                {
                    ProcessMemoryMB = processMemoryMb,
                    GCMemoryMB = gcMemoryMb,
                    CpuUsage = Math.Clamp(cpuUsagePercent, 0, 100),
                    Status = status,
                    Timestamp = now,
                    ApplicationName = "POS3"
                };

                await _metricsService.LogMemoryMetricAsync(metric, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recolectar métricas de memoria en segundo plano.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}

