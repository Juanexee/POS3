using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NEGOCIO;
using POS3.Hubs;

namespace POS4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {

        private readonly VentaNegocio _ventaNegocio;
        private readonly IHubContext<CocinaHub> _hubContext;

        // Constructor con Inyección de Dependencias (DI)
        public VentasController(VentaNegocio ventaNegocio, IHubContext<CocinaHub> hubContext)
        {
            _ventaNegocio = ventaNegocio;
            _hubContext = hubContext;
        }
        /// <summary> Insertar venta con sus detalles (transaccion).</summary>
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarVenta([FromBody] Venta venta)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // 1. Ejecutamos la lógica de negocio para guardar en la BD
                var respuesta = _ventaNegocio.RegistrarVenta(venta);

                // 2. Si la venta fue exitosa, notificamos a la cocina 🔔
                if (respuesta.Success)
                {
                    // Enviamos el mismo mensaje para que la cocina refresque su lista
                    await _hubContext.Clients.All.SendAsync("PedidoActualizado");
                }

                return Ok(respuesta);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { success = false, message = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo procesar el pedido.",
                    detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Buscar una venta insertada en especifico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Devuelve la lista de ventas</response>
        /// <response code="401">Token invalido o ausente</response>
        /// <response code="500">Fallo al acceder a la base de datos de ventas</response>

        [HttpGet("{id}")] // Endpoint: GET /api/Ventas/5
        public IActionResult ObtenerVenta(int id)
        {
            try
            {
                var venta = _ventaNegocio.ObtenerVentaConDetalles(id);

                if (venta == null)
                {
                    return NotFound($"No se encontró la venta con ID: {id}");
                }

                return Ok(venta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al consultar la venta.", detail = ex.Message });
            }
        }


        /// <summary>
        ///  Ver la venta insertada
        /// </summary>
        /// <returns></returns>
        /// <response code="200">La venta con ese ID fue encontrada con exito</response>
        /// <response code="400">El valor del ID no es un formato valido</response>
        /// <response code="401">Token invalido o ausente</response>
        /// <response code="404">La venta con el ID especificado no existe</response>
        /// <response code="500">Fallo en el servidor</response>
        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult ObtenerListadoVentas()
        {
            try
            {
                var listadoVentas = _ventaNegocio.LeerTodas();
                return Ok(listadoVentas); // Retorna 200 OK con la lista de facturas
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el listado de ventas.", detail = ex.Message });
            }
        }

        [HttpGet("pedidos")]
        public IActionResult ObtenerPedidosCocina()
        {
            try
            {
                // Llamamos a la lógica de negocio 🧠
                var pedidos = _ventaNegocio.ObtenerPedidosParaCocina();

                if (pedidos == null || !pedidos.Any())
                {
                    return NotFound("No hay pedidos pendientes en cocina.");
                }

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Actualiza el estado de un grupo de pedidos (Ej: de Pendiente a En Preparación o a Listo).
        /// </summary>
        /// <param name="ids">Cadena de IDs separados por coma (ej: "10,11,12")</param>
        /// <param name="nuevoEstado">El estado destino ('EnPreparacion' o 'Listo')</param>
        [HttpPatch("actualizar-estado-grupo")]
        public async Task<IActionResult> ActualizarEstadoPedidos([FromQuery] string ids, [FromQuery] string nuevoEstado)
        {
            if (string.IsNullOrEmpty(ids)) return BadRequest("Debe proporcionar al menos un ID.");

            try
            {
                // Llamamos a la capa de negocio para procesar la actualización masiva
                // Nota: Asegúrate de implementar este método en VentaNegocio
                bool resultado = _ventaNegocio.ActualizarEstadoMasivo(ids, nuevoEstado);

                if (resultado)
                {
                    // Enviamos un mensaje llamado "PedidoActualizado" a todos los clientes conectados
                    await _hubContext.Clients.All.SendAsync("PedidoActualizado");
                    return Ok(new { success = true, message = $"Pedidos actualizados a: {nuevoEstado}" });
                }



                return BadRequest("No se pudieron actualizar los pedidos.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar estados.", detail = ex.Message });
            }
        }

    }
}
