using Microsoft.AspNetCore.Mvc;
using NEGOCIO;
using ENTIDADES;

[ApiController]
[Route("api/[controller]")]
public class SesionController : ControllerBase
{
    private readonly SesionNegocio _sesionNegocio;
    private readonly PedidoNegocio _pedidoNegocio; // 1. Agregamos el campo para pedidos

    public SesionController(SesionNegocio sesionNegocio, PedidoNegocio pedidoNegocio)
    {
        _sesionNegocio = sesionNegocio;
        _pedidoNegocio = pedidoNegocio; // 3. Asignamos la referencia
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


    [HttpPost("aceptar-lote")]
    public IActionResult AceptarLotePedidos([FromBody] ActualizarPedidoRequest request)
    {
        // 1. Mandamos a actualizar todos los IDs de un solo golpe
        bool exito = _pedidoNegocio.CambiarEstadoVariosPedidos(request.IdsPedidos, request.NuevoEstado);

        if (exito)
        {
            // 2. En lugar de solo decir "OK", devolvemos los pedidos que AÚN están pendientes
            // Esto mantiene la pantalla del chef sincronizada al instante.
            var pendientesActualizados = _pedidoNegocio.ObtenerPedidosAgrupados();
            return Ok(pendientesActualizados);
        }

        return BadRequest("No se pudieron actualizar los pedidos.");
    }
}