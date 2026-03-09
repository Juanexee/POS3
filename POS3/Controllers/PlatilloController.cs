using DATOS;
using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace POS3.Controllers

{
    [ApiController]
    [Route("[controller]")]
    public class PlatilloController : ControllerBase
    {
        //Declara las intancia de datos
        private readonly PlatillosDatos _platillosDatos;
        // Constructor que recibe la configuración para obtener la cadena de conexión
        public PlatilloController(IConfiguration configuration)
        {
            var cadenaConexion = configuration.GetConnectionString("RestauranteDB");
            _platillosDatos = new PlatillosDatos(cadenaConexion);
        }

        /// <summary>
        /// Insertar un nuevo platillo
        /// </summary>
        /// <param name="platilloDto"></param>
        /// <returns></returns>
        /// <response code="201"> El nuevo platillo se creo con exito</response>
        /// <response code="400">Datos invalidos o incompletos</response>
        /// <response code="500">Fallo en el servidor</response>




        //EndPoint Para Insertar un nuevo platillo
        [HttpPost("Insertar")]
        public IActionResult InsertarPlatillo([FromBody] PlatilloDTO platilloDto)
        {
            try
            {
                _platillosDatos.Insertar(platilloDto);
                return Ok(new { mensaje = "Rol insertado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Ver todo los platillos que se han insertado 
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Devuelve una lista de los platillos regristrado</response>
        /// <response code="500">Fallo al leer los datos en la base de datos</response>




        [Authorize(Roles = "Administrador")]
        //EndPoint para leer todos los platillos
        [HttpGet("Leer")]        
        public IActionResult LeerTodos()
        {
            try
            {
                var platillos = _platillosDatos.Leer();
                return Ok(platillos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener platillos: " + ex.Message });
            }
        }

        /// <summary>
        /// Actualiazar un platillo que se a insertado anteriormente 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="platilloDto"></param>
        /// <returns></returns>
        /// <response code="200"> El platillo se actualizo correctamente</response>
        /// <response code="404"> El paltillo con el id especificado nose encontro</response>
        /// <response code="500"> Fallo al intenta aplicar los cambios</response>






        [HttpPut("{id}/Actualizar")] // Ruta más simple
        public IActionResult ActualizarPlatillo(int id, [FromBody] PlatilloDTO platilloDto)
        {
            try
            {
                var platilloExistente = _platillosDatos.LeerPorId(id);
                if (platilloExistente == null)
                {
                    return NotFound(new { mensaje = $"Platillo con ID {id} no encontrado." });
                }

                _platillosDatos.Actualizar(id, platilloDto);
                return Ok(new { mensaje = "Platillo actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verificar la disponibilidad del platillo 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="disponible"></param>
        /// <returns></returns>
        /// <response code="200">El estado de disponibilidad se ha cambiado con exito</response>



        [HttpPut("{id}/Disponibilidad")]
        public IActionResult CambiarDisponibilidad(int id, [FromQuery] bool disponible)
        {
            try
            {
                var platilloExistente = _platillosDatos.LeerPorId(id);
                if (platilloExistente == null)
                {
                    return NotFound(new { mensaje = $"Platillo con ID {id} no encontrado." });
                }

                _platillosDatos.Eliminar(id, disponible);
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

