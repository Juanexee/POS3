// Archivo: Controllers/RecetasController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NEGOCIO;
using ENTIDADES.RecetaDTO;
using System;
using System.Collections.Generic;

namespace RestauranteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Administrador")] // Solo Admins gestionan recetas
    public class RecetasController : ControllerBase
    {
        private readonly RecetaNegocio _recetaNegocio;

        public RecetasController(RecetaNegocio recetaNegocio)
        {
            _recetaNegocio = recetaNegocio;
        }

        /// <summary>
        /// Buscar una receta en especifico
        /// </summary>
        /// <param name="platilloID"></param>
        /// <returns></returns>
        /// <response code="200">La receta con el PlatilloID fue encontrada</response>
        /// <response code="404">. No se encontró ninguna receta asociada al platilloID especificado</response>
        /// <response code="500">Fallo en el servidor</response>






        // GET: api/Recetas/{platilloID}
        [HttpGet("{platilloID}")]
        public IActionResult ObtenerReceta(int platilloID)
        {
            try
            {
                var receta = _recetaNegocio.LeerPorPlatillo(platilloID);
                return Ok(receta);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la receta.", detail = ex.Message });
            }
        }
        /// <summary>
        /// Actualizar una receta insertada anteriormente
        /// </summary>
        /// <param name="platilloID"></param>
        /// <param name="detalles"></param>
        /// <returns></returns>
        /// <response code="201">La receta se actualizo con exito</response>
        /// <response code="404">La receta asociada con el ID especificado no existe</response>
        /// <response code="500">Fallo en la base de datos al guardar la receta</response>


        // PUT: api/Recetas/{platilloID} (Para guardar o actualizar la receta completa)
        [HttpPut("{platilloID}")]
        public IActionResult GuardarReceta(int platilloID, [FromBody] List<RecetaBaseDTO> detalles)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Si la lista de detalles es null o vacía, el negocio lo interpreta como un borrado.
                bool exito = _recetaNegocio.GuardarReceta(platilloID, detalles);

                if (exito)
                {
                    return Ok(new { message = $"Receta para Platillo ID {platilloID} guardada/actualizada exitosamente." });
                }
                else
                {
                    return StatusCode(500, new { message = "Fallo en la base de datos al guardar la receta." });
                }
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al gestionar la receta.", detail = ex.Message });
            }
        }
    }
}