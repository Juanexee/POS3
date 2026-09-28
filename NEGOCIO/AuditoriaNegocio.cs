using System;
using System.Collections.Generic;
using DATOS;
using ENTIDADES;

namespace NEGOCIO
{
    /// <summary>
    /// Orquesta la lógica de auditoría y trazabilidad del sistema.
    /// Cubre RF-MOV-AUD-01 y RF-MOV-AUD-02.
    /// </summary>
    public class AuditoriaNegocio
    {
        private readonly AuditoriaDatos _auditoriaDatos;

        public AuditoriaNegocio(AuditoriaDatos auditoriaDatos)
        {
            _auditoriaDatos = auditoriaDatos ?? throw new ArgumentNullException(nameof(auditoriaDatos));
        }

        /// <summary>
        /// Registra un evento crítico en el sistema de auditoría (MongoDB).
        /// </summary>
        /// <param name="tipoEvento">Tipo de evento (AnulacionComanda, CierreCaja, AplicacionDescuento, etc.)</param>
        /// <param name="usuarioId">ID del usuario responsable.</param>
        /// <param name="nombreUsuario">Nombre del usuario responsable.</param>
        /// <param name="modulo">Módulo del sistema (Ventas, Inventario, Caja, etc.)</param>
        /// <param name="descripcion">Descripción legible del evento.</param>
        public bool RegistrarEvento(string tipoEvento, int usuarioId, string nombreUsuario,
                                    string modulo, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(tipoEvento))
                throw new ArgumentException("El tipo de evento es obligatorio.");
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción del evento es obligatoria.");

            var log = new LogAuditoriaDTO
            {
                TipoEvento = tipoEvento,
                UsuarioID = usuarioId,
                NombreUsuario = nombreUsuario ?? "Sistema",
                Modulo = modulo ?? "General",
                Descripcion = descripcion,
                FechaHora = DateTime.UtcNow
            };

            return _auditoriaDatos.InsertarLog(log);
        }

        /// <summary>
        /// Obtiene los logs de auditoría con filtros de trazabilidad.
        /// Cubre RF-MOV-AUD-02.
        /// </summary>
        public List<LogAuditoriaDTO> ObtenerLogs(FiltroLogsDTO filtro)
        {
            if (filtro == null)
                filtro = new FiltroLogsDTO();

            // Validación: si las fechas están invertidas, corregirlas
            if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue
                && filtro.FechaDesde > filtro.FechaHasta)
            {
                (filtro.FechaDesde, filtro.FechaHasta) = (filtro.FechaHasta, filtro.FechaDesde);
            }

            return _auditoriaDatos.ObtenerLogs(filtro);
        }
    }
}
