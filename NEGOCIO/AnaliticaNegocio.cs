using System;
using System.Collections.Generic;
using DATOS;
using ENTIDADES;

namespace NEGOCIO
{
    /// <summary>
    /// Orquesta la lógica analítica del negocio para el Dashboard gerencial.
    /// Cubre RF-MOV-DSH-01 y RF-MOV-DSH-02.
    /// </summary>
    public class AnaliticaNegocio
    {
        private readonly IVentaDatos _ventaDatos;

        public AnaliticaNegocio(IVentaDatos ventaDatos)
        {
            _ventaDatos = ventaDatos ?? throw new ArgumentNullException(nameof(ventaDatos));
        }

        /// <summary>
        /// Obtiene los KPIs del dashboard para el día actual.
        /// Cubre RF-MOV-DSH-01.
        /// </summary>
        public DashboardKpiDTO ObtenerKPIsDashboard()
        {
            return _ventaDatos.ObtenerKPIs(DateTime.Today);
        }

        /// <summary>
        /// Obtiene los datos de tendencia de ventas agrupados por período.
        /// Cubre RF-MOV-DSH-02.
        /// </summary>
        /// <param name="periodo">"dia" (últimos 7 días), "semana" (últimas 8 semanas), "mes" (últimos 12 meses)</param>
        public List<TendenciaVentasDTO> ObtenerTendenciaVentas(string periodo = "dia")
        {
            // Validación: solo períodos válidos
            string[] periodosValidos = { "dia", "semana", "mes" };
            if (!Array.Exists(periodosValidos, p => p.Equals(periodo?.ToLower())))
                periodo = "dia"; // Valor seguro por defecto

            return _ventaDatos.ObtenerTendenciaVentas(periodo);
        }
    }
}
