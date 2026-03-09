using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class VentaListaDTO
    {
        public int VentaID { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }

        // Información del cajero/usuario
        public int UsuarioID { get; set; }
        public string NombreCajero { get; set; }
    }
}
