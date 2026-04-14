using Microsoft.AspNetCore.Mvc;
using NEGOCIO;
using ENTIDADES;

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

    [HttpPost("cambiar-mesa")]
    public IActionResult CambiarMesa([FromBody] CambioMesaRequest request)
    {
        // Llamamos a la capa de negocio
        var resultado = _sesionNegocio.ProcesarCambioMesa(request.SesionId, request.NuevaMesaId);

        if (resultado.Success)
        {
            return Ok(resultado); // Retorna 200 con el mensaje de éxito
        }

        return BadRequest(resultado); // Retorna 400 con el mensaje "Mesa no disponible"
    }
}