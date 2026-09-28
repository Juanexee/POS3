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

        List<VentaListaDTO> LeerTodas();

        int GuardarVenta(VentaListaDTO venta);

        List<PedidoAgrupadoDTO> ObtenerPedidosParaCocina();

        bool ActualizarEstadoMasivo(string ids, string nuevoEstado);

        bool ActualizarEstadoVenta(int ventaId, string nuevoEstado);

        int ObtenerVentaActivaPorMesa(int mesaId);

        /// <summary>
        /// Obtiene facturas filtradas con paginación.
        /// Cubre RF-MOV-BUS-02 (filtros avanzados) y RF-MOV-MON-03 (listado histórico).
        /// </summary>
        FacturasPaginadasDTO ObtenerFacturasFiltradas(FiltroFacturasDTO filtro);
    }
}
