using Microsoft.AspNetCore.Mvc;
using NEGOCIO;

[ApiController]
[Route("api/[controller]")]
public class SesionController : ControllerBase
{
    private readonly SesionNegocio _sesionNegocio;

    public SesionController(SesionNegocio sesionNegocio)
    {
        _sesionNegocio = sesionNegocio;
    }

    [HttpPost("Abrir/{mesaId}")]
    public IActionResult AbrirSesion(int mesaId)
    {
        try
        {
            // Llamamos al negocio para obtener o crear la sesión
            int sesionId = _sesionNegocio.ObtenerOAbrirSesion(mesaId);

            return Ok(new
            {
                mensaje = "Sesión de mesa establecida",
                sesionID = sesionId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}