using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class Venta
    {
        public int VentaID { get; set; }
        public int? UsuarioID { get; set; }
        public int? SesionID { get; set; }
        public string TipoPedido { get; set; }  // 'QR', 'Mesero', 'Caja'
        public int MesaID { get; set; }
        public int? ClienteID { get; set; }
        public DateTime Fecha { get; set; } // <-- Agregar esta propiedad
        public decimal Total { get; set; }
        public string Estado { get; set; } 
        public List<DetalleVenta> DetalleVenta { get; set; }

        public bool EsPagoFinal { get; set; }

    }
}
