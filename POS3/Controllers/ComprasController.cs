// Archivo: Controllers/ComprasController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NEGOCIO;
using ENTIDADES.CompraDTO;
using System;

namespace RestauranteAPI.Controllers
{

    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")] // Solo el Administrador puede registrar compras
    public class ComprasController : ControllerBase
    {
        private readonly CompraNegocio _compraNegocio;

        // Inyección de dependencia (La registramos en Program.cs)
        public ComprasController(CompraNegocio compraNegocio)
        {
            _compraNegocio = compraNegocio;
        }
        /// <summary>
        /// Registrar compras
        /// </summary>
        /// <param name="compra"></param>
        /// <returns></returns>
        /// <response code="201">La compra se ha regristrado con exito</response>
        /// <response code="501">Error interno en el codigo</response>
        // POST: api/Compras (Registra una nueva compra a proveedor)
        [HttpPost]
        public IActionResult RegistrarCompra([FromBody] CompraMaestroDTO compra)
        {
            // 1. Validar el DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 2. Llamar a la Capa de Negocio
                int compraID = _compraNegocio.RegistrarNuevaCompra(compra);

                // 3. Devolver Respuesta de Éxito
                return Ok(new
                {
                    message = "Compra registrada y stock actualizado exitosamente.",
                    compraID = compraID
                });
            }
            catch (ArgumentException argEx)
            {
                // Captura errores de validación de negocio (ej. ProveedorID <= 0)
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                // Captura errores de Transacción/BD y los devuelve
                return StatusCode(500, new { message = "Error al procesar la compra. La transacción fue revertida.", detail = ex.Message });
            }
        }
    }
}