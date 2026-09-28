using System;
using System.ComponentModel.DataAnnotations;

namespace ENTIDADES
{
    /// <summary>
    /// Parámetros de filtro y trazabilidad para consultar logs de auditoría.
    /// Cubre RF-MOV-AUD-02.
    /// </summary>
    public class FiltroLogsDTO
    {
        /// <summary>Fecha de inicio del rango de búsqueda.</summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>Fecha de fin del rango de búsqueda.</summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>Tipo de evento a filtrar (AnulacionComanda, CierreCaja, etc.).</summary>
        public string? TipoEvento { get; set; }

        /// <summary>ID del usuario responsable del evento.</summary>
        public int? UsuarioID { get; set; }

        /// <summary>Módulo del sistema (Ventas, Inventario, Caja, etc.).</summary>
        public string? Modulo { get; set; }

        /// <summary>Página solicitada (comienza en 1).</summary>
        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        /// <summary>Cantidad de registros por página.</summary>
        [Range(1, 100)]
        public int TamanoPagina { get; set; } = 50;
    }
}
