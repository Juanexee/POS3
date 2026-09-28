using System;
using System.ComponentModel.DataAnnotations;

namespace ENTIDADES
{
    /// <summary>
    /// Parámetros de filtro avanzado para consultar facturas/ventas.
    /// Cubre RF-MOV-BUS-02 y RF-MOV-MON-03.
    /// </summary>
    public class FiltroFacturasDTO
    {
        /// <summary>Fecha de inicio del rango de búsqueda.</summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>Fecha de fin del rango de búsqueda.</summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>Monto mínimo de la factura.</summary>
        public decimal? MontoMinimo { get; set; }

        /// <summary>Monto máximo de la factura.</summary>
        public decimal? MontoMaximo { get; set; }

        /// <summary>Método de pago (Efectivo, Tarjeta, etc.).</summary>
        public string? MetodoPago { get; set; }

        /// <summary>Estado de la factura (Pendiente, Pagada, Anulada).</summary>
        public string? Estado { get; set; }

        /// <summary>Página solicitada para paginación (comienza en 1).</summary>
        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        /// <summary>Cantidad de registros por página.</summary>
        [Range(1, 100)]
        public int TamanoPagina { get; set; } = 20;
    }

    /// <summary>
    /// Respuesta paginada de facturas con metadatos de paginación.
    /// </summary>
    public class FacturasPaginadasDTO
    {
        public List<VentaListaDTO> Facturas { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
    }
}
