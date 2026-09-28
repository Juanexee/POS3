using System;

namespace ENTIDADES
{
    /// <summary>
    /// DTO para los datos de tendencias de ventas usados en gráficos comparativos.
    /// Cubre RF-MOV-DSH-02.
    /// </summary>
    public class TendenciaVentasDTO
    {
        /// <summary>Etiqueta del período (ej: "Lun 01/09", "Semana 35", "Agosto 2026").</summary>
        public string Etiqueta { get; set; } = string.Empty;

        /// <summary>Fecha de referencia del período.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>Total acumulado de ventas en el período.</summary>
        public decimal TotalVentas { get; set; }

        /// <summary>Cantidad de órdenes/facturas en el período.</summary>
        public int CantidadOrdenes { get; set; }

        /// <summary>Ticket promedio del período.</summary>
        public decimal TicketPromedio => CantidadOrdenes > 0 ? Math.Round(TotalVentas / CantidadOrdenes, 2) : 0;
    }
}
