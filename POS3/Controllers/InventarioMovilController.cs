using DATOS;
using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace POS3.Controllers
{
    /// <summary>
    /// Endpoints de monitoreo de inventario para la App Móvil Gerencial.
    /// Cubre RF-MOV-MON-01 (estado global del inventario) y RF-MOV-MON-02 (alertas de stock crítico).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Gerente,Administrador,Supervisor,admin,El super admin,Administrador3")]
    public class InventarioMovilController : ControllerBase
    {
        private readonly InsumoDatos _insumoDatos;
        private readonly IConfiguration _config;

        public InventarioMovilController(InsumoDatos insumoDatos, IConfiguration config)
        {
            _insumoDatos = insumoDatos;
            _config = config;
        }

        /// <summary>
        /// Consulta el estado global del inventario: existencias, valoración monetaria y tasa de alerta.
        /// </summary>
        /// <returns>Lista completa de insumos con su existencia actual, valoración y estado de alerta.</returns>
        /// <response code="200">Resumen de inventario obtenido correctamente.</response>
        /// <response code="401">Token JWT ausente o inválido.</response>
        /// <response code="500">Error al consultar el inventario.</response>
        [HttpGet("resumen")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult ObtenerResumenInventario()
        {
            try
            {
                decimal stockMinimo = _config.GetValue<decimal>("StockMinimoDefault", 5);
                var inventario = _insumoDatos.ObtenerResumenInventario(stockMinimo);

                // Calcular métricas globales para el dashboard
                var resumenGlobal = new
                {
                    TotalInsumos = inventario.Count,
                    TotalEnAlerta = inventario.Count(i => i.EnAlerta),
                    ValorTotalInventario = inventario.Sum(i => i.ValorTotal),
                    Insumos = inventario
                };

                return Ok(resumenGlobal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener el resumen del inventario.",
                    detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene los insumos que han alcanzado o superado el nivel crítico de stock mínimo.
        /// Ideal para notificaciones push y alertas en tiempo real (RF-MOV-MON-02).
        /// </summary>
        /// <returns>Lista de insumos en estado de alerta crítica.</returns>
        /// <response code="200">Lista de alertas obtenida (puede ser vacía si no hay stock crítico).</response>
        /// <response code="401">Token JWT ausente o inválido.</response>
        /// <response code="500">Error al consultar las alertas de inventario.</response>
        [HttpGet("alertas")]
        [ProducesResponseType(typeof(List<InventarioResumenDTO>), 200)]
        public IActionResult ObtenerAlertasStock()
        {
            try
            {
                decimal stockMinimo = _config.GetValue<decimal>("StockMinimoDefault", 5);
                var alertas = _insumoDatos.ObtenerInsumosEnAlerta(stockMinimo);

                return Ok(new
                {
                    TotalAlertas = alertas.Count,
                    HayAlertas = alertas.Count > 0,
                    Alertas = alertas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener las alertas de stock.",
                    detail = ex.Message
                });
            }
        }
    }
}
