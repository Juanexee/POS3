// Archivo: Controllers/UnidadesMedidaController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NEGOCIO;
using ENTIDADES.UnidadMedidaDTO;
using System;

namespace RestauranteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnidadesMedidaController : ControllerBase
    {
        private readonly UnidadMedidaNegocio _unidadMedidaNegocio;

        public UnidadesMedidaController(UnidadMedidaNegocio unidadMedidaNegocio)
        {
            _unidadMedidaNegocio = unidadMedidaNegocio;
        }

        /// <summary>
        /// Ver las unidades de medidas creadas
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Devuelve una lista de todas las unidades de medida regristrada</response>
        /// <response code="500">Fallo al obtener las unidades de medida</response>

        // GET: api/UnidadesMedida (Lectura pública)
        [HttpGet("Leer")]
        [AllowAnonymous] // Permitir la lectura de unidades a cualquier usuario logueado o incluso anónimo si es necesario
        public IActionResult ObtenerUnidades()
        {
            try
            {
                var unidades = _unidadMedidaNegocio.LeerTodos();
                return Ok(unidades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las unidades de medida.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Crear una nueva unidad de medida
        /// </summary>
        /// <param name="unidad"></param>
        /// <returns></returns>
        /// <response code="201">La unidad de medidad se creo con exito</response>
        /// <response code="400">Datos invalidos o incompletos</response>
        /// <response code="500">Fallo en el servidor</response>

        // POST: api/UnidadesMedida (Creación restringida)
        [HttpPost("Insertar")]
        // [Authorize(Roles = "Administrador")] // Solo el administrador puede crear nuevas unidades
        [AllowAnonymous]
        public IActionResult CrearUnidad([FromBody] CrearUnidadMedidaDTO unidad)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int nuevoID = _unidadMedidaNegocio.Insertar(unidad);
                return CreatedAtAction(nameof(CrearUnidad), new { id = nuevoID }, new { message = $"Unidad '{unidad.Nombre}' creada exitosamente con ID {nuevoID}." });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al crear la unidad.", detail = ex.Message });
            }
        }
    }
}