// Controlador de autenticación con JWT

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DATOS;
using ENTIDADES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NEGOCIO;

/// <summary>
/// Gestiona la autenticación de usuarios y la emisión de tokens JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuarioNegocio _usuarioNegocio;
    private readonly IConfiguration _config;

    public AuthController(IConfiguration configuration)
    {
        _config = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var cadenaConexion = _config.GetConnectionString("RestauranteDB");
        if (string.IsNullOrWhiteSpace(cadenaConexion))
            throw new InvalidOperationException("La cadena de conexión 'RestauranteDB' no está configurada.");

        _usuarioNegocio = new UsuarioNegocio(new UsuariosDatos(cadenaConexion));
    }

    /// <summary>
    /// Inicia sesión y devuelve un token JWT junto con los datos básicos del usuario.
    /// </summary>
    /// <param name="request">Credenciales (NombreUsuario + Password)</param>
    /// <returns>Token JWT y datos del usuario autenticado</returns>
    /// <response code="200">Inicio de sesión exitoso</response>
    /// <response code="400">Faltan campos obligatorios (usuario o contraseña)</response>
    /// <response code="401">Credenciales incorrectas o usuario inactivo</response>
    /// <response code="500">Error de servidor o problema de conexión con la BD</response>
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request == null
            || string.IsNullOrWhiteSpace(request.NombreUsuario)
            || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Nombre de usuario y contraseña son requeridos.");

        var usuario = _usuarioNegocio.Login(request.NombreUsuario, request.Password);
        if (usuario == null)
            return Unauthorized(new { error = "Credenciales inválidas o usuario inactivo." });

        var token = GenerarToken(usuario);

        // Devolvemos el token junto con datos básicos para que Flutter los muestre
        // sin necesidad de llamar a otro endpoint inmediatamente.
        return Ok(new
        {
            token,
            usuarioID  = usuario.UsuarioID,
            nombre     = usuario.Nombre,
            nombreUsuario = usuario.NombreUsuario,
            rol        = usuario.RolNombre,
            expiraEn   = ObtenerExpiracionMinutos()
        });
    }

    /// <summary>
    /// Devuelve el perfil del usuario actualmente autenticado (extraído del token JWT).
    /// Útil para que la app móvil obtenga los datos del usuario sin guardarlos en local.
    /// </summary>
    /// <response code="200">Perfil del usuario autenticado</response>
    /// <response code="401">Token inválido o ausente</response>
    [HttpGet("me")]
    [Authorize]
    public IActionResult ObtenerPerfil()
    {
        var usuarioID  = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var nombre     = User.FindFirstValue(ClaimTypes.Name);
        var rol        = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(usuarioID))
            return Unauthorized(new { error = "Token inválido: no contiene ID de usuario." });

        return Ok(new
        {
            usuarioID  = int.Parse(usuarioID),
            nombre,
            rol
        });
    }

    // -----------------------------------------------------------------------
    // PRIVADO: Generación del token JWT
    // -----------------------------------------------------------------------

    private string GenerarToken(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        string nombreUsuario = usuario.Nombre     ?? "Usuario";
        string rolUsuario    = usuario.RolNombre  ?? RolesApp.Mesero;

        var claims = new[]
        {
            // ID único del usuario → usado en endpoints para saber quién actúa
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
            // Nombre legible → visible en cabeceras de la app
            new Claim(ClaimTypes.Name, nombreUsuario),
            // Rol → usado por [Authorize(Roles=...)] en todos los controllers
            new Claim(ClaimTypes.Role, rolUsuario)
        };

        var keyValue = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyValue))
            throw new InvalidOperationException("JWT Key no configurada en 'Jwt:Key'.");

        var keyBytes = Encoding.UTF8.GetBytes(keyValue);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException(
                $"La clave JWT es demasiado corta ({keyBytes.Length} bytes). Mínimo 32 bytes para HS256.");

        var key   = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Leemos la expiración desde appsettings.json (Jwt:ExpirationMinutes).
        // El TDR (RNF-SEG-03) exige máximo 8 horas (480 min). Si el valor de
        // configuración supera ese límite, lo forzamos a 480 min.
        int expirationMinutes = ObtenerExpiracionMinutos();

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"]   ?? "RanchoLaMimi",
            audience:           _config["Jwt:Audience"] ?? "RanchoLaMimiApp",
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Lee ExpirationMinutes de la configuración y lo limita al máximo
    /// permitido por el TDR (RNF-SEG-03): 8 horas = 480 minutos.
    /// </summary>
    private int ObtenerExpiracionMinutos()
    {
        const int maxMinutos = 480; // 8 horas — límite TDR RNF-SEG-03
        const int defMinutos = 480; // valor por defecto si no está en config

        if (int.TryParse(_config["Jwt:ExpirationMinutes"], out int configMinutos) && configMinutos > 0)
            return Math.Min(configMinutos, maxMinutos);

        return defMinutos;
    }
}
