using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES.InsumosDTO
{
    public class CrearActualizarInsumoDTO
    {
        public int? InsumoID { get; set; } // Nullable para Creación
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int UnidadID { get; set; }
    }
}
