using DATOS;
using ENTIDADES;
using NEGOCIO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

[Route("Categoria")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly CategoriaNegocio _categoriaNegocio;

    // Inyección de dependencias a través de la Capa de Negocio
    public CategoriaController(CategoriaNegocio categoriaNegocio)
    {
        _categoriaNegocio = categoriaNegocio;
    }

    [HttpGet("Leer")]
    [AllowAnonymous]
    public IActionResult LeerTodas()
    {
        try
        {
            var lista = _categoriaNegocio.ListarTodasLasCategorias();
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Fallo al conectar con la base de datos: " + ex.Message });
        }
    }

    [HttpPost("Insertar")]
    [Authorize(Roles = "Administrador,Administrador3,El super admin,admin")]
    public IActionResult InsertarCategoria([FromBody] CategoriaDTO categoriaDto)
    {
        try
        {
            if (categoriaDto == null) return BadRequest("Datos inválidos.");

            var resultado = _categoriaNegocio.GuardarNuevaCategoria(categoriaDto);

            if (resultado.Success)
                return Ok(resultado);

            return BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina una categoría o la desactiva si contiene platillos asignados
    /// </summary>
    [HttpDelete("Eliminar/{id}")]
    [Authorize(Roles = "Administrador,Administrador3,El super admin,admin")]
    public IActionResult Eliminar(int id)
    {
        try
        {
            var resultado = _categoriaNegocio.EliminarCategoria(id);

            if (resultado.Success)
            {
                return Ok(resultado); // Retorna 200 OK con detalles de la operación
            }

            return BadRequest(resultado); // Retorna 400 en caso de fallas de negocio
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Fallo crítico en el servidor: " + ex.Message });
        }


    }

    /// <summary>
    /// Actualizar una categoría existente por su ID
    /// </summary>
    [HttpPut("Actualizar/{id}")]
    [Authorize(Roles = "Administrador,Administrador3,El super admin,admin")]
    public IActionResult Actualizar(int id, [FromBody] CategoriaDTO categoriaDto)
    {
        try
        {
            if (categoriaDto == null) return BadRequest("Datos inválidos.");

            var resultado = _categoriaNegocio.ModificarCategoria(id, categoriaDto);

            if (resultado.Success)
            {
                return Ok(resultado); // Retorna 200 OK con el DTO de respuesta exitosa
            }

            return BadRequest(resultado); // Retorna 400 en caso de fallas de validación
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Fallo crítico en el servidor: " + ex.Message });
        }
    }
}