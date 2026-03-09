using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class PlatilloDTO
    {
        [Required]        
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        [Required]      
        public decimal Precio { get; set; }

        [Required]
        public int CategoriaID { get; set; }

        // El campo Disponible se omite en POST, pero se usa en PUT
        public bool Disponible { get; set; } = true;
    }
}
