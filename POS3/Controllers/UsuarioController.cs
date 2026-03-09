using System.Security.Claims;
using DATOS;
using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NEGOCIO; 
using static ENTIDADES.UsuarioDTO;


namespace POS3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase //se  Cambia de 'Controller' a 'ControllerBase'
    {
       
        // Declara las intancia de datos y negocio
        private readonly UsuariosDatos _usuariosDatos; 
        private readonly UsuarioNegocio _usuarioNegocio;



        // Constructor que recibe la configuración para obtener la cadena de conexión
        public UsuarioController(IConfiguration configuration)
        {
            var cadenaConexion = configuration.GetConnectionString("RestauranteDB");
            _usuariosDatos = new UsuariosDatos(cadenaConexion);

            // Ahora le pasas la instancia de _usuariosDatos al constructor de UsuarioNegocio
            _usuarioNegocio = new UsuarioNegocio(_usuariosDatos);
        }
        // Método auxiliar para obtener el ID del usuario logueado
        private int ObtenerUsuarioIDModificador()
        {
            // Busca el Claim estándar para el ID de usuario (NameIdentifier)
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || !int.TryParse(claim.Value, out int usuarioID))
            {
                // Si falla la autenticación o el token es inválido, lanza una excepción
                throw new UnauthorizedAccessException("El token de usuario es inválido o no contiene el ID del usuario.");
            }
            return usuarioID;
        }
        /// <summary>
        /// Crear un nuevo usuario
        /// </summary>
        /// <param name="crearUsuarioDTO"></param>
        /// <returns></returns>
        /// <response code="201">El nuevo usuario se creo con exito</response>
        /// <response code="400">Datos invalidos o incompletos</response>
        /// <response code="500">Fallo al regristrar el nuevo usuario</response>

        // Endpoint para crear un nuevo usuario          
        [HttpPost("Crear")]
        [AllowAnonymous]
        public IActionResult Post([FromBody] CrearUsuarioDTO crearUsuarioDTO)
        {

            try
            {
                // 1. Mapea los datos del DTO a la entidad Usuario
                var usuario = new Usuario
                {
                    NombreUsuario = crearUsuarioDTO.NombreUsuario,
                    Password = crearUsuarioDTO.Password,
                    Nombre = crearUsuarioDTO.Nombre,
                    Telefono = crearUsuarioDTO.Telefono,
                    RolID = crearUsuarioDTO.RolID
                };

                // 2. Llama a la capa de negocio, que ahora se encarga de todo lo demás
                // Necesitarás una manera de pasar el rol. Por ahora, lo pasaremos como "Administrador"
                // hasta que implementemos la lógica de autenticación real.
                _usuarioNegocio.CrearUsuario(usuario, "Administrador");

                return Ok(new { mensaje = "Usuario insertado correctamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Ver la lista completa de los usuario creados
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Develve una lista completa de todos los usuarios regristrado</response>
        /// <response code="401">Token invalido o ausente</response>
        /// <response code="500">Fallo al leer los datos de la base de datos</response>

        //endpoint para leer todos los usuarios
        [HttpGet("Leer")]
        [AllowAnonymous]
        public IActionResult Get() 
        {
            try
            {
                // Llamar al método Leer de la capa de datos
                List<Usuario> usuarios = _usuariosDatos.Leer();
                // Devolver un Ok con la lista de usuarios
                return Ok(usuarios);
            }

            catch (Exception ex)
            {
                // Manejo de errores
                return BadRequest(new { error = ex.Message });
            }
        

        
        
        }

        /// <summary>
        /// Actualizar los datos de un usuario 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        /// <response code="200">Los datos del usuario se actualizaron con exito</response>
        /// <response code="400">Los datos enviados para la actualización son inválidos.</response>
        /// <response code="401">Token invalido o ausente</response>
        /// <response code="404">El usuario con el ID especificado no existe</response>
        /// <response code="500">Fallo al intentar los cambios</response>




        [HttpPut("{id}/Actualizar usuario")]
        public IActionResult ActualizarUsuario(int id, [FromBody] ENTIDADES.UsuarioDTO.CrearUsuarioDTO usuario)
        {
            try
            {
                // 1. Verificar si el usuario existe antes de actualizar
                Usuario usuarioExistente = _usuariosDatos.LeerPorId(id);

                if (usuarioExistente == null)
                {
                    return NotFound(new { mensaje = $"El usuario con ID {id} no fue encontrado." });
                }

                // 2. ASIGNACIÓN CORREGIDA
                usuario.UsuarioID = id; // <-- ¡Esto ya funcionará!

                // 3. Llamamos al método de la capa de datos para actualizar.
                // Se agrega el parámetro usuarioModificacionID (por ejemplo, el mismo id del usuario que modifica)
                _usuariosDatos.Actualizar(usuario, id);

                // 4. Devolvemos una respuesta exitosa.
                return Ok(new { mensaje = "Usuario actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar un usuario por su ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">El usuario fue eliminado (o desactivado) con exito</response>
        /// <response code="401">Token invalido o ausente</response>
        /// <response code="404">El usuario que se intenta eliminar no existe</response>
        /// <response code="500">Fallo al intentar cambiar el estado del usuario</response>

        // Endpoint para Eliminar un usuario
        [HttpPut("{id}/Eliminar")]
        public IActionResult Put(int id )
        {
            try 
            {
                // Aquí se debe pasar el parámetro usuarioModificacionID requerido por la firma de Eliminar.
                // Puedes usar el mismo id del usuario que realiza la modificación, o ajustarlo según tu lógica.
                int usuarioModificacionID = id; // O el ID del usuario autenticado si lo tienes disponible.

                _usuariosDatos.Eliminar(id, false, usuarioModificacionID);
                // 2. Devolver una respuesta exitosa.
                return Ok(new { mensaje = "Usuario desactivado correctamente." });

            }
            catch (Exception ex)
            {
                // 3. Manejo de errores.
                return StatusCode(500, new { error = "Ocurrió un error al desactivar el usuario.", detalle = ex.Message });
            }

        }
    }
}