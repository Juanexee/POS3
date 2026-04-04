using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENTIDADES;

namespace NEGOCIO
{
    public interface IVentaNegocio
    {

        RespuestaRegistroVenta RegistrarVenta(Venta venta);

        Venta ObtenerVentaConDetalles(int idVenta);
        public List<VentaListaDTO> LeerTodas();


        int GuardarVenta(VentaListaDTO venta);
    }

}
