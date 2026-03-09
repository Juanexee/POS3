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
}
