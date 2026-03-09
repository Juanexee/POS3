using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class Platillo
    {

        public int PlatilloID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int CategoriaID { get; set; }
        public bool Disponible { get; set; }      
        public string NombreCategoria { get; set; } = string.Empty;
    }

}
