using ENTIDADES;
using NEGOCIO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Collections.Generic;

/// <summary>
/// Gestión de roles del sistema. Solo el Administrador puede crear, actualizar o desactivar roles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RolController : ControllerBase
{
    private readonly RolNegocio _rolNegocio;

    public RolController(RolNegocio rolNegocio)
    {
        _rolNegocio = rolNegocio;
    }

    /// <summary>Obtener todos los roles registrados en el sistema.</summary>
    /// <response code="200">Lista de roles</response>
    /// <response code="401">Token inválido o ausente</response>
    [HttpGet("Leer")]
    [Authorize(Roles = RolesApp.Todos)]
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

    /// <summary>Crear un nuevo rol. Solo Administrador.</summary>
    /// <response code="200">Rol creado con éxito</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="403">Sin permisos</response>
    [HttpPost("Crear")]
    [Authorize(Roles = RolesApp.Admin)]
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

    /// <summary>Actualizar un rol existente. Solo Administrador.</summary>
    /// <response code="200">Rol actualizado</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="403">Sin permisos o token sin ID</response>
    [HttpPut("Actualizar")]
    [Authorize(Roles = RolesApp.Admin)]
    public IActionResult Actualizar([FromBody] Rol rol)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int usuarioIDLogueado))
            return StatusCode(403, new { error = "El token no contiene un ID de usuario válido." });

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

    /// <summary>Desactivar (baja lógica) un rol. Solo Administrador.</summary>
    /// <response code="200">Rol desactivado</response>
    /// <response code="403">Sin permisos</response>
    [HttpPut("{id}/Desactivar")]
    [Authorize(Roles = RolesApp.Admin)]
    public IActionResult Desactivar(int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int usuarioIDLogueado))
            return StatusCode(403, new { error = "El token no contiene un ID de usuario válido." });

        try
        {
            var resultado = _rolNegocio.DesactivarRol(id, usuarioIDLogueado);
            if (resultado.Success) return Ok(resultado);

            return BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Fallo al cambiar el estado del rol.", detalle = ex.Message });
        }
    }
}