using System;
using System.Collections.Generic;

namespace ENTIDADES
{
    /// <summary>
    /// Representa un registro de auditoría/evento del sistema.
    /// Almacenado en MongoDB (colección: logs_auditoria).
    /// Cubre RF-MOV-AUD-01.
    /// </summary>
    public class LogAuditoriaDTO
    {
        /// <summary>ID del documento en MongoDB (ObjectId como string).</summary>
        public string? Id { get; set; }

        /// <summary>
        /// Tipo de evento registrado. Valores posibles:
        /// "AnulacionComanda", "AplicacionDescuento", "CierreCaja",
        /// "ModificacionInventario", "LoginUsuario", "LogoutUsuario", "OtroEvento"
        /// </summary>
        public string TipoEvento { get; set; } = string.Empty;

        /// <summary>ID del usuario que generó el evento.</summary>
        public int UsuarioID { get; set; }

        /// <summary>Nombre del usuario responsable.</summary>
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>Descripción legible del evento ocurrido.</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Fecha y hora exacta del evento (UTC).</summary>
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        /// <summary>Datos adicionales del evento en formato clave-valor.</summary>
        public Dictionary<string, object>? DatosAdicionales { get; set; }

        /// <summary>Módulo del sistema donde ocurrió el evento (Ventas, Inventario, Caja, etc.).</summary>
        public string Modulo { get; set; } = string.Empty;
    }
}
