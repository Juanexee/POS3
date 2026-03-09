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
        [HttpPost]
        public IActionResult RegistrarVenta([FromBody] Venta venta)
        {
            if (!ModelState.IsValid)
            {
                // Validación básica de los DTOs antes de tocar la BLL
                return BadRequest(ModelState);
            }

            try
            {
                // Llamada a la Capa de Negocio para iniciar la transacción en la BD
                int ventaID = _ventaNegocio.RegistrarVenta(venta);

                // Si es exitoso, devuelve el ID de la nueva venta con un código 201 Created
                return CreatedAtAction(nameof(RegistrarVenta), new { id = ventaID }, venta);
            }
            catch (ArgumentException argEx)
            {
                // Maneja errores de validación de negocio (ej. "La venta debe contener al menos un producto.")
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                // Este bloque captura cualquier error no controlado,
                // incluyendo el error de "Stock Insuficiente" que se lanzó desde la Capa de Datos.

                // El error 500 Internal Server Error es apropiado para un fallo transaccional.
                return StatusCode(500, new { message = "Error interno del servidor al registrar la venta. La transacción fue revertida.", detail = ex.Message });
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
