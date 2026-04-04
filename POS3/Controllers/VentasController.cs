using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NEGOCIO;

namespace POS4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {

        private readonly VentaNegocio _ventaNegocio;

        // Constructor con Inyección de Dependencias (DI)
        public VentasController(VentaNegocio ventaNegocio)
        {
            _ventaNegocio = ventaNegocio;
        }
        /// <summary> Insertar venta con sus detalles (transaccion).</summary>
        [HttpPost("registrar")]
        public IActionResult RegistrarVenta([FromBody] Venta venta)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // 1. Ejecutamos la lógica de negocio
                // 'respuesta' es de tipo RespuestaRegistroVenta
                var respuesta = _ventaNegocio.RegistrarVenta(venta);

                // 2. Retornamos el objeto completo que generó la capa de Negocio
                // Esto ya incluye success, el ID, el total y el mensaje correcto.
                return Ok(respuesta);

            }
            catch (ArgumentException argEx)
            {
                // Errores de validación (Ej: "Mesa no válida" o "Sesión inactiva")
                return BadRequest(new { success = false, message = argEx.Message });
            }
            catch (Exception ex)
            {
                // Errores críticos o de base de datos (Ej: "Stock insuficiente")
                // Es vital enviar el mensaje para que el usuario sepa qué falló
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

    }
}
