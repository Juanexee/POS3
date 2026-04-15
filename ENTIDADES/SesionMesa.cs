using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class SesionMesa
    {
        public int SesionID { get; set; }
        public int MesaID { get; set; }
        public string Estado { get; set; } = "Activa";
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
