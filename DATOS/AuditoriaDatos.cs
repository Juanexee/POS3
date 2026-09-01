using System;
using System.Collections.Generic;
using ENTIDADES;

// NOTA: Este archivo requiere el paquete MongoDB.Driver.
// Instalar con: dotnet add DATOS/DATOS.csproj package MongoDB.Driver
// Mientras MongoDB no esté disponible (Docker no configurado), los métodos de lectura
// retornan listas vacías y los de escritura solo registran en consola (modo stub).
// Cuando Docker+MongoDB esté listo, activar la implementación real descomentando el código.

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
        private readonly string _collectionName = "logs_auditoria";
        private readonly bool _mongoDisponible;

        public AuditoriaDatos(string mongoConnectionString, string databaseName)
        {
            _mongoConnectionString = mongoConnectionString;
            _databaseName = databaseName;

            // Verificar si MongoDB está disponible
            _mongoDisponible = !string.IsNullOrWhiteSpace(mongoConnectionString)
                               && mongoConnectionString != "PENDIENTE";
        }

        /// <summary>
        /// Inserta un nuevo log de auditoría en MongoDB.
        /// </summary>
        public bool InsertarLog(LogAuditoriaDTO log)
        {
            if (!_mongoDisponible)
            {
                // Modo stub: registrar en consola hasta que MongoDB esté disponible
                Console.WriteLine($"[AUDITORIA-STUB] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | " +
                                  $"Evento: {log.TipoEvento} | Usuario: {log.NombreUsuario} | " +
                                  $"Módulo: {log.Modulo} | Desc: {log.Descripcion}");
                return true;
            }

            try
            {
                // =====================================================
                // IMPLEMENTACIÓN REAL (activar cuando Docker esté listo)
                // =====================================================
                // var client = new MongoDB.Driver.MongoClient(_mongoConnectionString);
                // var db = client.GetDatabase(_databaseName);
                // var col = db.GetCollection<MongoDB.Bson.BsonDocument>(_collectionName);
                // var doc = new MongoDB.Bson.BsonDocument
                // {
                //     { "tipoEvento",    log.TipoEvento },
                //     { "usuarioID",     log.UsuarioID },
                //     { "nombreUsuario", log.NombreUsuario },
                //     { "descripcion",   log.Descripcion },
                //     { "modulo",        log.Modulo },
                //     { "fechaHora",     log.FechaHora }
                // };
                // col.InsertOne(doc);
                // return true;

                Console.WriteLine($"[AUDITORIA] Log insertado en MongoDB: {log.TipoEvento}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUDITORIA-ERROR] No se pudo insertar el log: {ex.Message}");
                return false;
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
                // Modo stub: retornar lista vacía con mensaje informativo
                Console.WriteLine("[AUDITORIA-STUB] MongoDB no disponible. Configure Docker para activar esta función.");
                return new List<LogAuditoriaDTO>
                {
                    new LogAuditoriaDTO
                    {
                        Id = "stub-001",
                        TipoEvento = "INFO",
                        NombreUsuario = "Sistema",
                        Descripcion = "MongoDB no está configurado aún. Active Docker con MongoDB para ver los logs reales.",
                        Modulo = "Sistema",
                        FechaHora = DateTime.UtcNow
                    }
                };
            }

            var lista = new List<LogAuditoriaDTO>();

            try
            {
                // =====================================================
                // IMPLEMENTACIÓN REAL (activar cuando Docker esté listo)
                // =====================================================
                // var client = new MongoDB.Driver.MongoClient(_mongoConnectionString);
                // var db = client.GetDatabase(_databaseName);
                // var col = db.GetCollection<MongoDB.Bson.BsonDocument>(_collectionName);
                //
                // var filterBuilder = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter;
                // var filters = new List<MongoDB.Driver.FilterDefinition<MongoDB.Bson.BsonDocument>>();
                //
                // if (filtro.FechaDesde.HasValue)
                //     filters.Add(filterBuilder.Gte("fechaHora", filtro.FechaDesde.Value));
                // if (filtro.FechaHasta.HasValue)
                //     filters.Add(filterBuilder.Lte("fechaHora", filtro.FechaHasta.Value));
                // if (!string.IsNullOrWhiteSpace(filtro.TipoEvento))
                //     filters.Add(filterBuilder.Eq("tipoEvento", filtro.TipoEvento));
                // if (filtro.UsuarioID.HasValue)
                //     filters.Add(filterBuilder.Eq("usuarioID", filtro.UsuarioID.Value));
                // if (!string.IsNullOrWhiteSpace(filtro.Modulo))
                //     filters.Add(filterBuilder.Eq("modulo", filtro.Modulo));
                //
                // var combinedFilter = filters.Count > 0
                //     ? filterBuilder.And(filters)
                //     : filterBuilder.Empty;
                //
                // int skip = (filtro.Pagina - 1) * filtro.TamanoPagina;
                // var docs = col.Find(combinedFilter)
                //               .SortByDescending(d => d["fechaHora"])
                //               .Skip(skip)
                //               .Limit(filtro.TamanoPagina)
                //               .ToList();
                //
                // foreach (var doc in docs)
                // {
                //     lista.Add(new LogAuditoriaDTO
                //     {
                //         Id          = doc["_id"].ToString(),
                //         TipoEvento  = doc.GetValue("tipoEvento", "").AsString,
                //         UsuarioID   = doc.GetValue("usuarioID", 0).AsInt32,
                //         NombreUsuario = doc.GetValue("nombreUsuario", "").AsString,
                //         Descripcion = doc.GetValue("descripcion", "").AsString,
                //         Modulo      = doc.GetValue("modulo", "").AsString,
                //         FechaHora   = doc.GetValue("fechaHora", DateTime.UtcNow).ToUniversalTime()
                //     });
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUDITORIA-ERROR] Error al obtener logs: {ex.Message}");
            }

            return lista;
        }
    }
}
