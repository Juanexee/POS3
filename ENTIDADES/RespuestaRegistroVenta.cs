using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class RespuestaRegistroVenta
    {
        public bool Success { get; set; } = true;
        public int VentaID { get; set; }
        public decimal TotalAcumulado { get; set; }
        public string Mensaje { get; set; }
    }
}
