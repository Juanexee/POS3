// Controlador de autenticación con JWT

using System.Data;
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
/// Permite a un usuario iniciar secion y obtener un Token  
/// </summary>
 

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuarioNegocio _usuarioNegocio;
    private readonly IConfiguration _config;
    private readonly UsuariosDatos _usuariosDatos;


    public AuthController(IConfiguration configuration)
    {
        _config = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var cadenaConexion = _config.GetConnectionString("RestauranteDB");
        if (string.IsNullOrWhiteSpace(cadenaConexion))
            throw new InvalidOperationException("Server=WINDOWS-TUTGG56\\KEVINLARA;Database=RestauranteDB;User Id=sa;Password=An1w0;Trusted_Connection=True;TrustServerCertificate=True");

        var usuariosDatos = new UsuariosDatos(cadenaConexion);
        _usuarioNegocio = new UsuarioNegocio(usuariosDatos);
    }
    /// <summary>
    /// Permite que un usuario inicie secion y reciba un token JWT si las credenciales son validas
    /// </summary>
    /// <param name="request"></param>
    /// <returns> JWT y datos basicos del usuario </returns>
    /// <response code="200">Inicio de sesion exitoso </response>
    /// <response code="401">Credenciales incorrectas </response>
    /// <response code="400">Faltan campos obligatorios como nombre usuario o password. </response>
    /// <response code="500">Error de servidor,problema de conexión con la base de datos </response>


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto request)
    {
        // Validar el modelo model state
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (request == null || string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Nombre de usuario y contraseña son requeridos.");
        
        var usuario = _usuarioNegocio.Login(request.NombreUsuario, request.Password);
        if (usuario == null) return Unauthorized(new { error = "Credenciales inválidas." });

        var token = GenerarToken(usuario);
        return Ok(new { token });
    }

    private string GenerarToken(Usuario usuario)
    {
        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario));

        // Validar que los campos no sean nulos
        string nombreUsuario = usuario.Nombre ?? "Usuario";
        string rolUsuario = usuario.RolNombre ?? "Usuario";

        // Crear claims
        var claims = new[]
        {
            // ¡CRÍTICO! Permite identificar al usuario en cualquier endpoint.
          new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()), 
    
          // Este se usa para mostrar el nombre en la interfaz, etc.
           new Claim(ClaimTypes.Name, nombreUsuario), 
    
           // Este se usa para la autorización ([Authorize(Roles=...)]).
           new Claim(ClaimTypes.Role, rolUsuario)
        };

        // Obtener clave secreta desde configuración
        var keyValue = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyValue))
            throw new InvalidOperationException("JWT Key no configurada en Jwt:Key");

        // Validar tamaño de clave (UTF8 bytes)
        var keyBytes = Encoding.UTF8.GetBytes(keyValue);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException($"La clave JWT es demasiado corta ({keyBytes.Length} bytes). Debe ser de al menos 32 bytes (256 bits) para el algoritmo HS256.");

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Crear token
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "juan",
            audience: _config["Jwt:Audience"] ?? "juanUsuarios",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),  // Usar UTC
            signingCredentials: creds
        );

        // Retornar token en formato string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
