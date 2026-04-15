using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATOS;
using ENTIDADES;

namespace NEGOCIO
{
    public class PedidoNegocio

    {
        // Inyectamos el SesionDatos para validar sesiones en pedidos por QR
        private readonly SesionDatos _sesionDatos;
        // Constructor que recibe la dependencia de SesionDatos
        public PedidoNegocio(SesionDatos sesionDatos)
        {
            _sesionDatos = sesionDatos;
        }

        // Método para obtener pedidos agrupados por estado para la cocina

        public List<PedidoAgrupadoDTO> AceptarYRefrescarCocina(List<int> ids, string estado)
        {
            // 1. Mandamos a actualizar (Capa de Datos)
            bool exito = _sesionDatos.CambiarEstadoPedidos(ids, estado);

            // 2. Sin importar si hubo éxito total o parcial, 
            // refrescamos la vista de lo que queda por hacer
            return ObtenerPedidosParaCocina();
        }

        public List<PedidoAgrupadoDTO> ObtenerPedidosParaCocina()
        {
            // Aquí llamaríamos al SQL de "Agrupación Inteligente" 
            // que devuelve Hamburguesa x5, etc.
            return _sesionDatos.ObtenerPedidosAgrupados();
        }

        public List<PedidoAgrupadoDTO> ObtenerPedidosAgrupados()
        {
            // Le pedimos a la capa de datos que nos traiga la información real
            return _sesionDatos.ObtenerPedidosAgrupados();
        }

        public bool CambiarEstadoVariosPedidos(List<int> idsPedidos, string nuevoEstado)
        {
            // Una buena práctica es validar que la lista no venga vacía antes de ir a la base de datos
            if (idsPedidos == null || idsPedidos.Count == 0) return false;

            // Llamamos al método que ya tienes en SesionDatos
            return _sesionDatos.CambiarEstadoPedidos(idsPedidos, nuevoEstado);
        }
    }
}
