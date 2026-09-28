using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEGOCIO;

namespace POS3.Controllers
{
    /// <summary>
    /// Endpoints analíticos para el Dashboard Gerencial de la App Móvil.
    /// Requiere autenticación JWT y rol Gerente, Administrador o Supervisor.
    /// Cubre RF-MOV-DSH-01 y RF-MOV-DSH-02.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Gerente,Administrador,Supervisor,admin,El super admin,Administrador3")]
    public class DashboardController : ControllerBase
    {
        private readonly AnaliticaNegocio _analiticaNegocio;

        public DashboardController(AnaliticaNegocio analiticaNegocio)
        {
            _analiticaNegocio = analiticaNegocio;
        }

        /// <summary>
        /// Obtiene los KPIs del negocio: ventas del día, ticket promedio y margen acumulado.
        /// </summary>
        /// <returns>Objeto DashboardKpiDTO con los indicadores clave de rendimiento.</returns>
        /// <response code="200">KPIs calculados correctamente.</response>
        /// <response code="401">Token JWT ausente o inválido.</response>
        /// <response code="403">El rol del usuario no tiene acceso a este endpoint.</response>
        /// <response code="500">Error interno al calcular los KPIs.</response>
        [HttpGet("kpis")]
        [ProducesResponseType(typeof(DashboardKpiDTO), 200)]
        public IActionResult ObtenerKPIs()
        {
            try
            {
                var kpis = _analiticaNegocio.ObtenerKPIsDashboard();
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al calcular los KPIs del dashboard.",
                    detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene los datos de tendencia de ventas para gráficos comparativos.
        /// </summary>
        /// <param name="periodo">Agrupación: "dia" (últimos 7 días), "semana" (últimas 8 semanas), "mes" (últimos 12 meses).</param>
        /// <returns>Lista de puntos de datos para renderizar el gráfico de tendencias.</returns>
        /// <response code="200">Lista de tendencias calculada correctamente.</response>
        /// <response code="400">Parámetro 'periodo' inválido.</response>
        /// <response code="401">Token JWT ausente o inválido.</response>
        /// <response code="500">Error interno al calcular tendencias.</response>
        [HttpGet("tendencias")]
        [ProducesResponseType(typeof(List<TendenciaVentasDTO>), 200)]
        public IActionResult ObtenerTendencias([FromQuery] string periodo = "dia")
        {
            string[] periodosValidos = { "dia", "semana", "mes" };
            if (!Array.Exists(periodosValidos, p => p.Equals(periodo?.ToLower())))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El parámetro 'periodo' debe ser: 'dia', 'semana' o 'mes'."
                });
            }

            try
            {
                var tendencias = _analiticaNegocio.ObtenerTendenciaVentas(periodo);
                return Ok(tendencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las tendencias de ventas.",
                    detail = ex.Message
                });
            }
        }
    }
}
