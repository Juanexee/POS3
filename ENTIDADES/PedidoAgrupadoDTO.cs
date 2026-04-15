using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class PedidoAgrupadoDTO
    {
        public string NombrePlatillo { get; set; }
        public int CantidadTotal { get; set; }
        public string IdsRelacionados { get; set; } // Vital para "Aceptar Grupo"
        public DateTime FechaPrimerPedido { get; set; } // Para calcular el tiempo real

        // Propiedad calculada: No se guarda en BD, se genera al vuelo
        public int MinutosEspera => (int)(DateTime.Now - FechaPrimerPedido).TotalMinutes;

        // Lógica automatizada del semáforo
        public string AlertaPrioridad
        {
            get
            {
                if (MinutosEspera >= 20) return "CRITICAL";  // Rojo 🔴
                if (MinutosEspera >= 10) return "ATTENTION"; // Amarillo 🟡
                return "NORMAL";                             // Verde 🟢
            }
        }
    }
}
