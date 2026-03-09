using System;

using ENTIDADES.InsumosDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEGOCIO;

namespace RestauranteAPI.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    
    //[Authorize(Roles = "Administrador")]
    public class InsumosController : ControllerBase
    {
        private readonly InsumoNegocio _insumoNegocio;

        public InsumosController(InsumoNegocio insumoNegocio)
        {
            _insumoNegocio = insumoNegocio;
        }

        /// <summary>
        /// Leer todos los insumos que estan regristrado
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Lista de todos los insumos regristrado</response>
        /// <response code="500">Fallo inesperado en el servidor </response>



        // GET: api/Insumos (Lectura de todos los insumos)
        [HttpGet("Leer")]
        [AllowAnonymous]
        public IActionResult ObtenerInsumos()
        {
            try
            {
                var insumos = _insumoNegocio.LeerTodos();
                return Ok(insumos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la lista de insumos.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Insertar un nuevo insumo 
        /// </summary>
        /// <param name="insumo"></param>
        /// <returns></returns>
        /// <response code="201"> El insumo se creo con exito</response>
        /// <response code="500">Fallo en el servidor</response>


        // POST: api/Insumos (Para crear un nuevo insumo)
        [HttpPost("Insertar")]
        [AllowAnonymous]
        public IActionResult CrearInsumo([FromBody] CrearActualizarInsumoDTO insumo)
        {
            // 1. Validar el DTO (ModelState)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna 400 Bad Request
            }

            try
            {
                // 2. Llamar a la Capa de Negocio
                int nuevoID = _insumoNegocio.Insertar(insumo);

                // 3. Devolver Respuesta de Éxito
                return CreatedAtAction(nameof(CrearInsumo), new { id = nuevoID }, new { message = $"Insumo '{insumo.Nombre}' creado exitosamente con ID {nuevoID}." }); // Retorna 201 Created
            }
            catch (ArgumentException argEx)
            {
                // Captura y responde a errores de Negocio (ej. Nombre vacío, UnidadID inválida)
                return BadRequest(new { message = argEx.Message }); // Retorna 400 Bad Request
            }
            catch (Exception ex)
            {
                // Captura errores inesperados o de BD
                return StatusCode(500, new { message = "Error interno del servidor al crear el insumo.", detail = ex.Message }); // Retorna 500 Internal Server Error
            }
        }
    }
}