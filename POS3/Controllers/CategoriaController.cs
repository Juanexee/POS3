// Ubicación: API/Controllers/CategoriaController.cs

using DATOS;
using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

[Route("Categoria")]
[ApiController]
//[Authorize(Roles = "Administrador")] // Solo el Administrador debe manejar categorías

public class CategoriaController : ControllerBase
{
    private readonly CategoriasDatos _categoriasDatos;

    public CategoriaController(IConfiguration configuration)
    {
        var cadenaConexion = configuration.GetConnectionString("RestauranteDB");
        _categoriasDatos = new CategoriasDatos(cadenaConexion);
    }


    /// <summary>
    /// Insertar un nuevo producto 
    /// </summary>
    /// <param name="categoriaDto"></param>
    /// <returns></returns>
    /// <response code="201">El producto se inserto con exito</response>
    /// <response code="500">Fallo en el servidor error al conectarse en la base de datos.</response>



    //[Authorize(Roles = "Administrador")]
    [AllowAnonymous]
    [HttpPost("Insertar")]
    public IActionResult InsertarCategoria([FromBody] CategoriaDTO categoriaDto)
    {
        try
        {
            _categoriasDatos.Insertar(categoriaDto);
            return CreatedAtAction(nameof(LeerTodas), new { mensaje = "Categoría creada con éxito." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    /// <summary>
    /// Ver todos los productos que sean insertado
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Lista de producto</response>
    /// <response code="401">La consulta de producto requiere que el usuario este logueado</response>
    /// <response code="500">Fallo al leer los datos de la base de datos</response>

    //  [Authorize(Roles = "Administrador")]
    [AllowAnonymous]
    [HttpGet("Leer")]
     public IActionResult LeerTodas()
    {
        try
        {
            var categorias = _categoriasDatos.LeerTodos();
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al obtener categorías: " + ex.Message });
        }
    }
    /// <summary>
    /// Buscar un producto insertado en especifico
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <response code="200">Devuelve el objeto del producto solicitado</response>
    /// <response code="401">Token invalido</response>
    /// <response code="404">No se encontro ningun producto con el id proporcionado</response>
    /// <response code="500">Fallo en el servidor</response> 

    // ---------- GET: Leer una categoría por ID ----------
    [HttpGet("{id}/Leer")]
    [AllowAnonymous]
    public IActionResult LeerPorId(int id)
    {
        try
        {
            var categoria = _categoriasDatos.LeerPorId(id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
            }
            return Ok(categoria);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    /// <summary>
    /// Actualizar un producto que se a insertado anteriormente
    /// </summary>
    /// <param name="id"></param>
    /// <param name="categoriaDto"></param>
    /// <returns></returns>
    /// <response code="200">El producto se actuliazo con exito</response>
    /// <response code="404">El producto que se intento actualizar no existe</response>
    /// <response code="500">Fallo al intentar moificar regristro</response>
    //[Authorize(Roles = "Administrador")]
    [AllowAnonymous]
    // ---------- PUT: Actualizar una categoría por ID ----------
    [HttpPut("{id}/Actualizar")]
    public IActionResult ActualizarCategoria(int id, [FromBody] CategoriaDTO categoriaDto)
    {
        try
        {
            var categoriaExistente = _categoriasDatos.LeerPorId(id);
            if (categoriaExistente == null)
            {
                return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
            }

            _categoriasDatos.Actualizar(id, categoriaDto);
            return Ok(new { mensaje = "Categoría actualizada correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}