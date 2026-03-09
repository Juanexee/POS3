using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class Insumo
    {
        public int InsumoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Existencia { get; set; }

        // Relación con Unidad de Medida
        public int UnidadID { get; set; }
        public string NombreUnidad { get; set; } = string.Empty; // Propiedad para el nombre de la unidad (viene del JOIN)
        public string Abreviatura { get; set; } = string.Empty;// Propiedad para la abreviatura (viene del JOIN)
    
     }
}
