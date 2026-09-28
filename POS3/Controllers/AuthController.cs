// Controlador de autenticación con JWT
// RF-MOV-AUT-01: Login con credenciales
// RF-MOV-AUT-02: Respuesta con token + datos del usuario para RBAC
// RF-MOV-AUT-03: Rol incluido en el token y en la respuesta
// RNF-MOV-SEG-03: Token con expiración máxima de 8 horas

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ENTIDADES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NEGOCIO;

/// <summary>
/// Permite a un usuario iniciar sesión y obtener un Token JWT con sus datos de sesión.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuarioNegocio _usuarioNegocio;
    private readonly IConfiguration _config;

    /// <summary>
    /// Constructor con Inyección de Dependencias (DI).
    /// UsuarioNegocio y IConfiguration son resueltos automáticamente por el contenedor de DI.
    /// </summary>
    public AuthController(UsuarioNegocio usuarioNegocio, IConfiguration configuration)
    {
        _usuarioNegocio = usuarioNegocio ?? throw new ArgumentNullException(nameof(usuarioNegocio));
        _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Permite que un usuario inicie sesión y reciba un token JWT si las credenciales son válidas.
    /// </summary>
    /// <param name="request">Credenciales del usuario (nombreUsuario + password).</param>
    /// <returns>JWT y datos básicos del usuario para RBAC en Flutter.</returns>
    /// <response code="200">Inicio de sesión exitoso.</response>
    /// <response code="400">Faltan campos obligatorios (nombreUsuario o password).</response>
    /// <response code="401">Credenciales incorrectas o usuario inactivo.</response>
    /// <response code="500">Error de servidor o problema de conexión con la base de datos.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    public IActionResult Login([FromBody] LoginDto request)
    {
        // Validar el modelo
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request == null
            || string.IsNullOrWhiteSpace(request.NombreUsuario)
            || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Nombre de usuario y contraseña son requeridos." });

        try
        {
            var usuario = _usuarioNegocio.Login(request.NombreUsuario, request.Password);
            if (usuario == null)
                return Unauthorized(new { error = "Credenciales inválidas o usuario inactivo." });

            // Calcular fecha de expiración — máximo 8 horas (RNF-MOV-SEG-03)
            var expiresAt = DateTime.UtcNow.AddHours(8);
            var token = GenerarToken(usuario, expiresAt);

            // Respuesta completa para RBAC local en Flutter (RF-MOV-AUT-02, RF-MOV-AUT-03)
            var response = new LoginResponseDto
            {
                Token         = token,
                UsuarioID     = usuario.UsuarioID,
                NombreCompleto = usuario.Nombre,
                NombreUsuario = usuario.NombreUsuario,
                Rol           = usuario.RolNombre ?? "Usuario",
                ExpiresAt     = expiresAt
            };

            return Ok(response);
        }
        catch (InvalidOperationException opEx)
        {
            // Error de datos de seguridad incompletos en BD
            return StatusCode(500, new { error = "Error de configuración de seguridad.", detail = opEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno al procesar el login.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Genera un token JWT firmado con los datos del usuario.
    /// </summary>
    private string GenerarToken(Usuario usuario, DateTime expiresAt)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        string nombreUsuario = usuario.Nombre ?? "Usuario";
        string rolUsuario    = usuario.RolNombre ?? "Usuario";

        var claims = new[]
        {
            // ID del usuario — permite identificarlo en cualquier endpoint protegido
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
            // Nombre del usuario — para mostrar en la interfaz
            new Claim(ClaimTypes.Name, nombreUsuario),
            // Rol — usado por [Authorize(Roles = "...")] en los controladores
            new Claim(ClaimTypes.Role, rolUsuario)
        };

        // Obtener y validar la clave secreta JWT
        var keyValue = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyValue))
            throw new InvalidOperationException("JWT Key no configurada en Jwt:Key.");

        var keyBytes = Encoding.UTF8.GetBytes(keyValue);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException(
                $"La clave JWT es demasiado corta ({keyBytes.Length} bytes). " +
                "Debe ser de al menos 32 bytes (256 bits) para HS256.");

        var key   = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"]   ?? "POS3",
            audience:          _config["Jwt:Audience"] ?? "POS3Usuarios",
            claims:            claims,
            expires:           expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
