using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class ActualizarPedidoRequest
    {

        public List<int> IdsPedidos { get; set; } // La lista de IDs que agrupamos
        public string NuevoEstado { get; set; }   // Ejemplo: "En Preparación"
    }
}
