using Microsoft.AspNetCore.Mvc;
using NEGOCIO;
using ENTIDADES;
using System.Collections.Generic;

namespace API_REST_V3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MesasController : ControllerBase
    {
        private readonly MesaNegocio _mesaNegocio;

        // Inyección de la capa de negocio
        public MesasController(MesaNegocio mesaNegocio)
        {
            _mesaNegocio = mesaNegocio;
        }

        // GET: api/Mesas/Leer
        [HttpGet("Leer")]
        public IActionResult GetMesas()
        {
            var mesas = _mesaNegocio.ListarTodasLasMesas();
            return Ok(mesas);
        }

        // POST: api/Mesas/Insertar
        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] Mesa nuevaMesa)
        {
            if (nuevaMesa == null) return BadRequest("Datos de mesa no válidos.");

            var resultado = _mesaNegocio.GuardarNuevaMesa(nuevaMesa);

            if (resultado.Success)
                return Ok(resultado);
            else
                return BadRequest(resultado);
        }

        // PUT: api/Mesas/Actualizar/{id}
        [HttpPut("Actualizar/{id}")]
        public IActionResult Actualizar(int id, [FromBody] Mesa mesaDatos)
        {
            mesaDatos.MesaID = id; // Aseguramos que el ID sea el de la URL
            var resultado = _mesaNegocio.EditarMesa(mesaDatos);

            if (resultado.Success)
                return Ok(resultado);
            else
                return BadRequest(resultado);
        }

        // DELETE: api/Mesas/Eliminar/{id}
        [HttpDelete("Eliminar/{id}")]
        public IActionResult Eliminar(int id)
        {
            var resultado = _mesaNegocio.DarDeBajaMesa(id);

            if (resultado.Success)
                return Ok(resultado);
            else
                return BadRequest(resultado);
        }
    }
}