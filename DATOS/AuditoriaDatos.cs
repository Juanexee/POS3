using System;
using System.Collections.Generic;
using ENTIDADES;
using MongoDB.Bson;
using MongoDB.Driver;

// RF-MOV-AUD-01, RF-MOV-AUD-02 — Auditoría y trazabilidad (MongoDB)
// RNF-MOV-BD-02: Almacena eventos semiestructurados en colecciones MongoDB.
//
// Configuración en appsettings.json:
//   "MongoDB": {
//     "ConnectionString": "mongodb://localhost:27017",
//     "DatabaseName": "RestauranteAuditoria"
//   }
// Si ConnectionString es "PENDIENTE" o vacío, opera en modo stub (log en consola).

namespace DATOS
{
    /// <summary>
    /// Capa de datos para logs de auditoría almacenados en MongoDB.
    /// Cubre RF-MOV-AUD-01 y RF-MOV-AUD-02.
    /// RNF-MOV-BD-02: Almacena eventos semiestructurados en colecciones MongoDB.
    /// </summary>
    public class AuditoriaDatos
    {
        private readonly string _mongoConnectionString;
        private readonly string _databaseName;
        private const string CollectionName = "logs_auditoria";
        private readonly bool _mongoDisponible;

        public AuditoriaDatos(string mongoConnectionString, string databaseName)
        {
            _mongoConnectionString = mongoConnectionString;
            _databaseName = databaseName;

            // Solo intentar MongoDB si la cadena no es el placeholder
            _mongoDisponible = !string.IsNullOrWhiteSpace(mongoConnectionString)
                               && mongoConnectionString != "PENDIENTE";
        }

        /// <summary>
        /// Inserta un nuevo log de auditoría en MongoDB.
        /// Si MongoDB no está disponible, registra en consola (modo stub).
        /// </summary>
        public bool InsertarLog(LogAuditoriaDTO log)
        {
            if (!_mongoDisponible)
            {
                Console.WriteLine($"[AUDITORIA-STUB] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | " +
                                  $"Evento: {log.TipoEvento} | Usuario: {log.NombreUsuario} | " +
                                  $"Módulo: {log.Modulo} | Desc: {log.Descripcion}");
                return true;
            }

            try
            {
                var client = new MongoClient(_mongoConnectionString);
                var db = client.GetDatabase(_databaseName);
                var col = db.GetCollection<BsonDocument>(CollectionName);

                var doc = new BsonDocument
                {
                    { "tipoEvento",    log.TipoEvento    ?? string.Empty },
                    { "usuarioID",     log.UsuarioID },
                    { "nombreUsuario", log.NombreUsuario  ?? "Sistema" },
                    { "descripcion",   log.Descripcion   ?? string.Empty },
                    { "modulo",        log.Modulo        ?? "General" },
                    { "fechaHora",     log.FechaHora.ToUniversalTime() }
                };

                col.InsertOne(doc);
                Console.WriteLine($"[AUDITORIA] Log insertado en MongoDB: {log.TipoEvento}");
                return true;
            }
            catch (Exception ex)
            {
                // Fallback a stub si falla la conexión
                Console.WriteLine($"[AUDITORIA-WARN] MongoDB no disponible, usando stub: {ex.Message}");
                Console.WriteLine($"[AUDITORIA-STUB] Evento: {log.TipoEvento} | Usuario: {log.NombreUsuario}");
                return true;
            }
        }

        /// <summary>
        /// Obtiene los logs de auditoría aplicando los filtros especificados.
        /// Cubre RF-MOV-AUD-02 (trazabilidad por fecha, tipo de evento y usuario).
        /// </summary>
        public List<LogAuditoriaDTO> ObtenerLogs(FiltroLogsDTO filtro)
        {
            if (!_mongoDisponible)
            {
                Console.WriteLine("[AUDITORIA-STUB] MongoDB no disponible. Configure la conexión en appsettings.json.");
                return new List<LogAuditoriaDTO>
                {
                    new LogAuditoriaDTO
                    {
                        Id = "stub-001",
                        TipoEvento = "INFO",
                        NombreUsuario = "Sistema",
                        Descripcion = "MongoDB no está configurado aún. " +
                                      "Actualice MongoDB:ConnectionString en appsettings.json para ver los logs reales.",
                        Modulo = "Sistema",
                        FechaHora = DateTime.UtcNow
                    }
                };
            }

            var lista = new List<LogAuditoriaDTO>();

            try
            {
                var client = new MongoClient(_mongoConnectionString);
                var db = client.GetDatabase(_databaseName);
                var col = db.GetCollection<BsonDocument>(CollectionName);

                // Construir filtro dinámico
                var filterBuilder = Builders<BsonDocument>.Filter;
                var filters = new List<FilterDefinition<BsonDocument>>();

                if (filtro.FechaDesde.HasValue)
                    filters.Add(filterBuilder.Gte("fechaHora", filtro.FechaDesde.Value.ToUniversalTime()));
                if (filtro.FechaHasta.HasValue)
                    filters.Add(filterBuilder.Lte("fechaHora", filtro.FechaHasta.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime()));
                if (!string.IsNullOrWhiteSpace(filtro.TipoEvento))
                    filters.Add(filterBuilder.Eq("tipoEvento", filtro.TipoEvento));
                if (filtro.UsuarioID.HasValue)
                    filters.Add(filterBuilder.Eq("usuarioID", filtro.UsuarioID.Value));
                if (!string.IsNullOrWhiteSpace(filtro.Modulo))
                    filters.Add(filterBuilder.Eq("modulo", filtro.Modulo));

                var combinedFilter = filters.Count > 0
                    ? filterBuilder.And(filters)
                    : filterBuilder.Empty;

                int skip = (filtro.Pagina - 1) * filtro.TamanoPagina;

                var docs = col.Find(combinedFilter)
                              .SortByDescending(d => d["fechaHora"])
                              .Skip(skip)
                              .Limit(filtro.TamanoPagina)
                              .ToList();

                foreach (var doc in docs)
                {
                    lista.Add(new LogAuditoriaDTO
                    {
                        Id            = doc["_id"].ToString(),
                        TipoEvento    = doc.GetValue("tipoEvento",    BsonNull.Value).AsString,
                        UsuarioID     = doc.GetValue("usuarioID",     0).AsInt32,
                        NombreUsuario = doc.GetValue("nombreUsuario", BsonNull.Value).AsString,
                        Descripcion   = doc.GetValue("descripcion",   BsonNull.Value).AsString,
                        Modulo        = doc.GetValue("modulo",        BsonNull.Value).AsString,
                        FechaHora     = doc.GetValue("fechaHora",     DateTime.UtcNow).ToUniversalTime()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUDITORIA-ERROR] Error al obtener logs de MongoDB: {ex.Message}");
                // Retornar lista vacía en lugar de propagar la excepción
            }

            return lista;
        }
    }
}
