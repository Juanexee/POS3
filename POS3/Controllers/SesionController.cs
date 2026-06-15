using Microsoft.AspNetCore.Mvc;
using NEGOCIO;
using ENTIDADES;
using Microsoft.AspNetCore.SignalR;
using POS3.Hubs;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class SesionController : ControllerBase
{
    private readonly SesionNegocio _sesionNegocio;
    private readonly PedidoNegocio _pedidoNegocio; // 1. Agregamos el campo para pedidos
    private readonly IHubContext<CocinaHub> _hubContext;

    public SesionController(SesionNegocio sesionNegocio, PedidoNegocio pedidoNegocio, IHubContext<CocinaHub> hubContext)
    {
        _sesionNegocio = sesionNegocio;
        _pedidoNegocio = pedidoNegocio; // 3. Asignamos la referencia
        _hubContext = hubContext;
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
    public async Task<IActionResult> CambiarMesa([FromBody] CambioMesaRequest request)
    {
        // Llamamos a la capa de negocio
        var resultado = _sesionNegocio.ProcesarCambioMesa(request.SesionId, request.NuevaMesaId);

        if (resultado.Success)
        {
            // Notificamos a la cocina para que actualice los números de mesa en tiempo real 🔔
            await _hubContext.Clients.All.SendAsync("PedidoActualizado");
            return Ok(resultado); // Retorna 200 con el mensaje de éxito
        }

        return BadRequest(resultado); // Retorna 400 con el mensaje "Mesa no disponible"
    }

    [HttpPost("aceptar-lote")]
    public async Task<IActionResult> AceptarLotePedidos([FromBody] ActualizarPedidoRequest request)
    {
        // 1. Mandamos a actualizar todos los IDs de un solo golpe
        bool exito = _pedidoNegocio.CambiarEstadoVariosPedidos(request.IdsPedidos, request.NuevoEstado);

        if (exito)
        {
            // Notificamos a los demás visores conectados (cocina/mesero) 🔔
            await _hubContext.Clients.All.SendAsync("PedidoActualizado");

            // 2. En lugar de solo decir "OK", devolvemos los pedidos que AÚN están pendientes
            // Esto mantiene la pantalla del chef sincronizada al instante.
            var pendientesActualizados = _pedidoNegocio.ObtenerPedidosAgrupados();
            return Ok(pendientesActualizados);
        }

        return BadRequest("No se pudieron actualizar los pedidos.");
    }

    /// <summary>
    /// Confirmación de entrega de platillos a la mesa por parte del mesero
    /// </summary>
    [HttpPost("entregar-pedidos")]
    public async Task<IActionResult> EntregarPedidos([FromBody] ActualizarPedidoRequest request)
    {
        try
        {
            // Invocamos las reglas de negocio de la capa intermedia
            var resultado = _sesionNegocio.MarcarPedidosComoEntregados(request.IdsPedidos);

            if (resultado.Success)
            {
                // Notificamos a los visores de cocina/mesas que el pedido fue despachado de la bandeja de salida 🔔
                await _hubContext.Clients.All.SendAsync("PedidoActualizado");
                return Ok(resultado); // Retorna 200 con el mensaje exitoso
            }

            return BadRequest(resultado); // Retorna 400 mapeando el error de la regla rota
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Fallo crítico en el servidor: " + ex.Message });
        }
    }
}