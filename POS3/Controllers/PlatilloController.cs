using DATOS;
using ENTIDADES;
using NEGOCIO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace POS3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlatilloController : ControllerBase
    {
        private readonly PlatilloNegocio _platilloNegocio;

        // Inyección de dependencias profesional a través de la arquitectura de capas
        public PlatilloController(PlatilloNegocio platilloNegocio)
        {
            _platilloNegocio = platilloNegocio;
        }

        /// <summary>
        /// Ver todos los platillos que se han insertado
        /// </summary>
        [HttpGet("Leer")]
        [AllowAnonymous] // <--- SOLUCIÓN AL 401: Permite leer el menú sin necesidad de Token JWT
        public IActionResult LeerPlatillos()
        {
            try
            {
                var lista = _platilloNegocio.ListarTodosLosPlatillos();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Fallo al leer los datos en la base de datos: " + ex.Message });
            }
        }

        /// <summary>
        /// Obtener un platillo específico por su ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous] // También abierto para que el cliente escanee y busque por ID
        public IActionResult LeerPorId(int id)
        {
            try
            {
                var platillo = _platilloNegocio.ObtenerPlatilloPorId(id);
                if (platillo == null)
                {
                    return NotFound(new { mensaje = $"Platillo con ID {id} no encontrado." });
                }
                return Ok(platillo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener el platillo: " + ex.Message });
            }
        }

        /// <summary>
        /// Insertar un nuevo platillo
        /// </summary>
        [HttpPost("Insertar")]
        // Si quieres que solo los administradores inserten platillos, quitas el AllowAnonymous y dejas el Authorize general de la API
        public IActionResult InsertarPlatillo([FromBody] PlatilloDTO platilloDto)
        {
            try
            {
                if (platilloDto == null) return BadRequest("Datos inválidos.");

                var resultado = _platilloNegocio.RegistrarPlatillo(platilloDto);

                if (resultado.Success)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cambiar la disponibilidad del platillo
        /// </summary>
        [HttpPut("{id}/Disponibilidad")]
        public IActionResult DisponibilidadPlatillo(int id, [FromQuery] bool disponible)
        {
            try
            {
                var platillo = _platilloNegocio.ObtenerPlatilloPorId(id);
                if (platillo == null)
                {
                    return NotFound(new { mensaje = $"Platillo con ID {id} no encontrado." });
                }

                _platilloNegocio.CambiarDisponibilidad(id, disponible);
                string estado = disponible ? "disponible" : "agotado";
                return Ok(new { mensaje = $"Platillo ID {id} marcado como {estado} correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}