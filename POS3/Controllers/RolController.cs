using Microsoft.AspNetCore.Mvc;
using DATOS;
using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
namespace POS3.Controllers;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;


// Controlador para gestionar roles
[ApiController]
[Route("[controller]")]
public class RolController : ControllerBase
{

    //Declara las intancia de datos
    private readonly RolesDatos _rolesDatos;

    // Constructor que recibe la configuración para obtener la cadena de conexión
    public RolController(IConfiguration configuration)
    {
        var cadenaConexion = configuration.GetConnectionString("RestauranteDB");
        _rolesDatos = new RolesDatos(cadenaConexion);
    }

    /// <summary>
    /// Crear un nuevo rol
    /// </summary>
    /// <param name="rol"></param>
    /// <returns></returns>
    /// <response code="201">El nuevo rol se creo con exito</response>
    /// <response code="500">Fallo en el servidor</response>




    // [Authorize(Roles = "Administrador")]
    //EndPoint Para Insertar un nuevo rol
    [HttpPost("Crear")]
    [AllowAnonymous]
    public IActionResult Post([FromBody] Rol rol)
    {
        try
        {
            // Llamar al método de inserción con el objeto rol
            _rolesDatos.Insertar(rol);
            return Ok(new { mensaje = "Rol insertado correctamente" });
        }

        catch (Exception ex)
        {
            // Manejo de errores
            return BadRequest(new { error = ex.Message });
        }
    }
    /// <summary>
    /// Obtener la lista de los roles creados 
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Devuelve una lista de todos los roles existente</response>
    /// <response code="500">Fallo en el servidor </response>



    //EndPoint para leer todos los roles
    //[Authorize (Roles = "Administrador")] 
    [HttpGet("Leer")]
    [AllowAnonymous]
    public IActionResult Get()
    {
        try
        {
            // Llamar al método Leer de la capa de datos
            List<Rol> roles = _rolesDatos.Leer();
            return Ok(roles);
        }
        catch (Exception ex)
        {

            return BadRequest(new { error = ex.Message });

        }

    }

    /// <summary>
    /// Buscar un rol creado en especifico 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <response code="200">El rol con es ID fue encontrado </response> 
    /// <response code="404">El rol con el ID especificado no existe</response>
    /// <response code="500">Fallo en el servidor</response>




    //EndPoint para leer un rol por ID
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        try
        {
            Rol rol = _rolesDatos.LeerPorID(id);

            if (rol == null)
            {
                return NotFound(new { mensaje = $"El rol con ID {id} no fue encontrado." });
            }

            return Ok(rol);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Ocurrió un error al buscar el rol con ID {id}.", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar un rol creado anteriormente
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rol"></param>
    /// <returns></returns>
    /// <response code="200">El rol se actualizo con exito</response>
    /// <response code="404">El rol que se intento actualizar no existe</response>
    /// <response code="500">Fallo al intentar aplicar los cambios</response>





    [Authorize(Roles = "Administrador")]
    //EndPoint para actualizar un rol
    [HttpPut("Actualizar/{id}")]
    public IActionResult PUT(int id, [FromBody] Rol rol)
    {
        try 
        {
            // Verificar si el rol existe
            var rolExistente = _rolesDatos.LeerPorID(id);
            if (rolExistente == null)
            {
                return NotFound(new { mensaje = $"El rol con ID {id} no fue encontrado." });
            }
            // Asignamos el ID de la URL al objeto para asegurar que se actualice el correcto.
            rol.RolID = id;
            //  Llamamos al método de la capa de datos para actualizar.'
            _rolesDatos.Actualizar(rol);
            //  Devolver una respuesta exitosa
            return Ok(new { mensaje = "Rol actualizado correctamente" });

        }

        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Ocurrió un error al actualizar el rol con ID {id}.", detalle = ex.Message });
        }

    }

    /// <summary>
    /// Desactivar un rol en especifico
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <response code="200">El rol se desactivo con exito</response>
    /// <response code="404">El rol que se intenta desactivr no existe</response>
    /// <response code="500">Fallo al intentar cambiar el estado</response>



    //EndPoint para Desactivar un rol
    [Authorize]
    [HttpPut("{id}/Desactivar")]
    public IActionResult Put(int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Convertir el ID a entero (usado para auditoría)
        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int usuarioIDLogueado))
        {
            // ESTA ES LA LÍNEA CORREGIDA: Usamos StatusCode(403)
            return StatusCode(403, new
            {
                error = "No tienes los permisos o tu token no contiene la información de usuario válida (ID)."
            });
        }

        try
        {
            // ... el resto del código es correcto
            _rolesDatos.Eliminar(id, false, usuarioIDLogueado);

            return Ok(new { mensaje = "Rol desactivado correctamente." });
        }
        catch (Exception ex)
        {
            // Esta línea es correcta y funciona con el objeto anónimo.
            return BadRequest(new { error = ex.Message });
        }
    }





}
