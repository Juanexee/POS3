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

        
        List<PedidoAgrupadoDTO> ListarPedidosAgrupados();

        bool ActualizarEstadoMasivo(string ids, string nuevoEstado);

        bool ActualizarEstadoVenta(int ventaId, string nuevoEstado);
        int ObtenerVentaActivaPorMesa(int mesaId);

        // =====================================================
        // Métodos analíticos para la App Móvil Gerencial
        // RF-MOV-DSH-01, RF-MOV-DSH-02, RF-MOV-BUS-02
        // =====================================================

        /// <summary>Obtiene los KPIs del dashboard para la fecha indicada.</summary>
        DashboardKpiDTO ObtenerKPIs(DateTime fecha);

        /// <summary>
        /// Obtiene los datos de tendencia de ventas agrupados por período.
        /// </summary>
        /// <param name="periodo">"dia" (últimos 7 días), "semana" (últimas 8 semanas), "mes" (últimos 12 meses)</param>
        List<TendenciaVentasDTO> ObtenerTendenciaVentas(string periodo);

        /// <summary>Obtiene facturas filtradas con paginación para RF-MOV-BUS-02.</summary>
        FacturasPaginadasDTO ObtenerFacturasFiltradas(FiltroFacturasDTO filtro);
    }
}