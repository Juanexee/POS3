using ENTIDADES;
using NEGOCIO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Collections.Generic;

[ApiController]
[Route("[controller]")]
public class RolController : ControllerBase
{
    private readonly RolNegocio _rolNegocio;

    // Inyección de dependencias a través de la Capa de Negocio
    public RolController(RolNegocio rolNegocio)
    {
        _rolNegocio = rolNegocio;
    }

    /// <summary>
    /// Crear un nuevo rol
    /// </summary>
    [HttpPost("Crear")]
    [AllowAnonymous]
    public IActionResult Post([FromBody] Rol rol)
    {
        try
        {
            if (rol == null) return BadRequest("Datos inválidos.");

            var resultado = _rolNegocio.GuardarNuevoRol(rol);
            if (resultado.Success) return Ok(resultado);

            return BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todos los roles
    /// </summary>
    [HttpGet("Leer")]
    [AllowAnonymous]
    public IActionResult Leer()
    {
        try
        {
            var lista = _rolNegocio.ListarTodosLosRoles();
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar un rol existente
    /// </summary>
    [HttpPut("Actualizar")]
    [AllowAnonymous]
    public IActionResult Actualizar([FromBody] Rol rol)
    {
        // Obtener ID del usuario logueado desde el token
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int usuarioIDLogueado))
        {
            return StatusCode(403, new
            {
                error = "No tienes los permisos o tu token no contiene la información de usuario válida (ID)."
            });
        }

        try
        {
            if (rol == null) return BadRequest("Datos inválidos.");

            var resultado = _rolNegocio.ActualizarRol(rol, usuarioIDLogueado);
            if (resultado.Success) return Ok(resultado);

            return BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Desactivar un rol específico
    /// </summary>
     // Exige Token JWT para saber qué usuario está ejecutando la acción
    [HttpPut("{id}/Desactivar")]
    public IActionResult Desactivar(int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int usuarioIDLogueado))
        {
            return StatusCode(403, new
            {
                error = "No tienes los permisos o tu token no contiene la información de usuario válida (ID)."
            });
        }

        try
        {
            var resultado = _rolNegocio.DesactivarRol(id, usuarioIDLogueado);
            if (resultado.Success) return Ok(resultado);

            return BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Fallo al intentar cambiar el estado", detalle = ex.Message });
        }
    }
}