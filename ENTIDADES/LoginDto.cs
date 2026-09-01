using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class LoginDto
    {
        
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string NombreUsuario { get; set; } // ¡CORREGIDO!

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; }

       
    }

    /// <summary>
    /// Respuesta del endpoint de login con el token JWT y datos del usuario.
    /// Permite a la app Flutter aplicar RBAC localmente (RF-MOV-AUT-02, RF-MOV-AUT-03).
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>Token JWT firmado para autorizar peticiones a la API.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>ID único del usuario autenticado.</summary>
        public int UsuarioID { get; set; }

        /// <summary>Nombre completo del usuario para mostrar en la UI.</summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>Nombre de usuario (login).</summary>
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>Rol del usuario para el control de acceso (Gerente, Administrador, Supervisor, etc.).</summary>
        public string Rol { get; set; } = string.Empty;

        /// <summary>Fecha y hora de expiración del token (máx. 8 horas, RNF-MOV-SEG-03).</summary>
        public DateTime ExpiresAt { get; set; }
    }
}
