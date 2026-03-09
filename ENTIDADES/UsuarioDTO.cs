using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class UsuarioDTO
    {

        public class CrearUsuarioDTO
        {
            // Propiedad que causaba el error de compilación
            public int UsuarioID { get; set; }

            [Required]
            public string NombreUsuario { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty; // Quitamos [Required] si es para Actualizar

            [Required]
            public string Nombre { get; set; } = string.Empty;

            public string Telefono { get; set; } = string.Empty;

            [Required]
            public int RolID { get; set; }

            public bool Activo { get; set; }

        }
    }
}
