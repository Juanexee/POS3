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

        public int VentaID { get; set; }

        [Required(ErrorMessage = "El Id Producto es obligatorio")]
        public int  PlatilloID{ get; set; }
        public string NombreProducto { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        public string? Comentario { get; set; }
        
        // Property alias for mapping from frontend JSON which sends 'personalizacion'
        public string? Personalizacion
        {
            get => Comentario;
            set => Comentario = value;
        }

        public string? EstadoCocinero { get; set; }
    }
}
