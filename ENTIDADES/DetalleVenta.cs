using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class DetalleVenta
    {
        public int DetalleVentaID { get; set; }

        [Required(ErrorMessage = "El Id de la venta es requierido ")]
        public int VentaID { get; set; }

        [Required(ErrorMessage = "El Id Producto es obligatorio")]
        public int  PlatilloID{ get; set; }
        public string NombreProducto { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
