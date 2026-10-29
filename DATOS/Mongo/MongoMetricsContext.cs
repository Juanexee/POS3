using System.Diagnostics.CodeAnalysis;
using ENTIDADES.Metrics; // Ajusta esto según el namespace donde creaste ApiUsageMetric y MemoryMetric
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using POS3.Data.Mongo;      // Ajusta esto según el namespace donde creaste MongoDbSettings

namespace POS3.Data.Mongo;

public class MongoMetricsContext
{
    private static readonly TimeSpan ServerSelectionTimeout = TimeSpan.FromSeconds(5);

    // null cuando no hay configuración utilizable.
    public IMongoDatabase? Database { get; }

    // false cuando no hay configuración utilizable: la analítica queda deshabilitada.
    [MemberNotNullWhen(true, nameof(Database))]
    public bool IsConfigured { get; }

    private readonly ILogger<MongoMetricsContext> _logger;
    private readonly Lazy<Task> _indexesReady;

    public MongoMetricsContext(IOptions<MongoDbSettings> settings, ILogger<MongoMetricsContext> logger)
    {
        _logger = logger;

        string connectionString = settings.Value.ConnectionString ?? string.Empty;
        string databaseName = settings.Value.DatabaseName ?? string.Empty;

        bool hasUsableSettings = !string.IsNullOrWhiteSpace(connectionString)
            && !string.IsNullOrWhiteSpace(databaseName)
            && !connectionString.Contains('<');

        MongoClientSettings? clientSettings = null;
        if (hasUsableSettings)
        {
            try
            {
                clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "La cadena de conexión de MongoDB no es válida.");
            }
        }

        if (clientSettings is null)
        {
            IsConfigured = false;

            _logger.LogWarning(
                "MongoDB no está configurado ('{Section}')",
                MongoDbSettings.SectionName); // <-- Asegúrate de que SectionName exista en MongoDbSettings
        }
        else
        {
            IsConfigured = true;
            clientSettings.ServerSelectionTimeout = ServerSelectionTimeout;
            Database = new MongoClient(clientSettings).GetDatabase(databaseName);
        }

        _indexesReady = new Lazy<Task>(CreateIndexesAsync);
    }

    public IMongoCollection<ApiUsageMetric> ApiUsageMetrics =>
        RequireDatabase().GetCollection<ApiUsageMetric>("apiUsageLogs");

    public IMongoCollection<MemoryMetric> MemoryMetrics =>
        RequireDatabase().GetCollection<MemoryMetric>("memoryMetrics");

    private IMongoDatabase RequireDatabase() =>
        Database ?? throw new InvalidOperationException("MongoDB no está configurado.");

    public Task EnsureIndexesAsync() => _indexesReady.Value;

    private async Task CreateIndexesAsync()
    {
        if (!IsConfigured)
        {
            return;
        }

        try
        {
            IndexKeysDefinition<ApiUsageMetric> timestampIndex =
                Builders<ApiUsageMetric>.IndexKeys.Descending(x => x.Timestamp);
            await ApiUsageMetrics.Indexes.CreateOneAsync(new CreateIndexModel<ApiUsageMetric>(timestampIndex));

            IndexKeysDefinition<ApiUsageMetric> endpointIndex = Builders<ApiUsageMetric>.IndexKeys
                .Ascending(x => x.Endpoint)
                .Descending(x => x.Timestamp);
            await ApiUsageMetrics.Indexes.CreateOneAsync(new CreateIndexModel<ApiUsageMetric>(endpointIndex));

            IndexKeysDefinition<MemoryMetric> memoryTimeIndex =
                Builders<MemoryMetric>.IndexKeys.Descending(x => x.Timestamp);
            await MemoryMetrics.Indexes.CreateOneAsync(new CreateIndexModel<MemoryMetric>(memoryTimeIndex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron crear los índices de MongoDB para las colecciones de métricas.");
        }
    }
}