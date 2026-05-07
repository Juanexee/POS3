using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class Mesa
    {
        public int MesaID { get; set; }
        public int NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
