using System;

namespace ENTIDADES
{
    /// <summary>
    /// DTO con los indicadores clave de rendimiento (KPIs) para el Dashboard gerencial.
    /// Cubre RF-MOV-DSH-01.
    /// </summary>
    public class DashboardKpiDTO
    {
        /// <summary>Total de ventas (pagadas) del día actual.</summary>
        public decimal TotalVentasHoy { get; set; }

        /// <summary>Ticket promedio del día (TotalVentasHoy / CantidadOrdenes).</summary>
        public decimal TicketPromedio { get; set; }

        /// <summary>Margen de ganancia acumulado del mes en curso.</summary>
        public decimal MargenGananciaAcumulado { get; set; }

        /// <summary>Cantidad de órdenes cerradas/pagadas hoy.</summary>
        public int CantidadOrdenesHoy { get; set; }

        /// <summary>Total de ventas de la semana actual.</summary>
        public decimal TotalVentasSemana { get; set; }

        /// <summary>Total de ventas del mes actual.</summary>
        public decimal TotalVentasMes { get; set; }

        /// <summary>Fecha y hora en que se generaron los KPIs.</summary>
        public DateTime FechaConsulta { get; set; } = DateTime.Now;
    }
}
