using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{

    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;

        // La contraseña original (solo para la entrada de datos)
        public string Password { get; set; } = string.Empty;

        // El hash y el salt para almacenar en la BD
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int RolID { get; set; }
        public string? RolNombre { get; set; }
        public bool Activo { get; set; }
    }
}
