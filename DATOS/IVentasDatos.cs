using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENTIDADES;

namespace DATOS
{
    public interface IVentaDatos
    {
        int RegistrarVenta(VentaListaDTO venta);


        List<VentaListaDTO> LeerTodas();

        int Insertar(Venta venta);

        public Venta SeleccionarVentaConDetalle(int idVenta);

        decimal ObtenerTotalSesion(int sesionId);
    }
}