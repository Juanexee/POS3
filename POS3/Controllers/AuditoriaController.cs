using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEGOCIO;

namespace POS3.Controllers
{
    /// <summary>
    /// Endpoints de Auditoría y Trazabilidad para la App Móvil Gerencial.
    /// Lee los logs almacenados en MongoDB (o modo stub hasta que Docker esté configurado).
    /// Cubre RF-MOV-AUD-01 y RF-MOV-AUD-02.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Gerente,Administrador,admin,El super admin,Administrador3")]
    public class AuditoriaController : ControllerBase
    {
        private readonly AuditoriaNegocio _auditoriaNegocio;

        public AuditoriaController(AuditoriaNegocio auditoriaNegocio)
        {
            _auditoriaNegocio = auditoriaNegocio;
        }

        /// <summary>
        /// Consulta el registro de actividad y eventos críticos del sistema.
        /// Soporta filtros por fecha, tipo de evento, módulo y usuario responsable.
        /// </summary>
        /// <param name="fechaDesde">Fecha de inicio del rango (opcional). Formato: YYYY-MM-DD</param>
        /// <param name="fechaHasta">Fecha de fin del rango (opcional). Formato: YYYY-MM-DD</param>
        /// <param name="tipoEvento">Tipo de evento: AnulacionComanda, AplicacionDescuento, CierreCaja, etc. (opcional)</param>
        /// <param name="usuarioId">ID del usuario responsable (opcional).</param>
        /// <param name="modulo">Módulo del sistema: Ventas, Inventario, Caja (opcional).</param>
        /// <param name="pagina">Número de página (comienza en 1).</param>
        /// <param name="tamanoPagina">Registros por página (máx. 100).</param>
        /// <returns>Lista de logs de auditoría filtrados.</returns>
        /// <response code="200">Lista de logs obtenida correctamente.</response>
        /// <response code="400">Parámetros de filtro inválidos.</response>
        /// <response code="401">Token JWT ausente o inválido.</response>
        /// <response code="403">El rol del usuario no tiene acceso a los logs de auditoría.</response>
        /// <response code="500">Error al consultar los logs de auditoría.</response>
        [HttpGet("logs")]
        [ProducesResponseType(typeof(List<LogAuditoriaDTO>), 200)]
        public IActionResult ObtenerLogs(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? tipoEvento = null,
            [FromQuery] int? usuarioId = null,
            [FromQuery] string? modulo = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 50)
        {
            if (pagina < 1) pagina = 1;
            if (tamanoPagina < 1 || tamanoPagina > 100) tamanoPagina = 50;

            var filtro = new FiltroLogsDTO
            {
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                TipoEvento = tipoEvento,
                UsuarioID = usuarioId,
                Modulo = modulo,
                Pagina = pagina,
                TamanoPagina = tamanoPagina
            };

            try
            {
                var logs = _auditoriaNegocio.ObtenerLogs(filtro);
                return Ok(new
                {
                    TotalRegistros = logs.Count,
                    PaginaActual = pagina,
                    Logs = logs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al consultar los logs de auditoría.",
                    detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Registra manualmente un evento de auditoría (para uso interno del sistema web).
        /// </summary>
        /// <response code="200">Evento registrado correctamente.</response>
        /// <response code="400">Datos del evento incompletos o inválidos.</response>
        /// <response code="401">Token JWT ausente o inválido.</response>
        [HttpPost("registrar")]
        public IActionResult RegistrarEvento([FromBody] LogAuditoriaDTO log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.TipoEvento))
                return BadRequest(new { success = false, message = "El tipo de evento es obligatorio." });

            try
            {
                // Enriquecer con los datos del usuario autenticado si no se especificaron
                if (log.UsuarioID == 0)
                {
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int uid))
                        log.UsuarioID = uid;
                }
                if (string.IsNullOrWhiteSpace(log.NombreUsuario))
                    log.NombreUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Sistema";

                log.FechaHora = DateTime.UtcNow;

                bool resultado = _auditoriaNegocio.RegistrarEvento(
                    log.TipoEvento, log.UsuarioID, log.NombreUsuario, log.Modulo, log.Descripcion);

                return resultado
                    ? Ok(new { success = true, message = "Evento de auditoría registrado." })
                    : StatusCode(500, new { success = false, message = "No se pudo registrar el evento." });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { success = false, message = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al registrar el evento.",
                    detail = ex.Message
                });
            }
        }
    }
}
