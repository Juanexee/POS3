using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENTIDADES;
using DATOS;
using static ENTIDADES.UsuarioDTO;

namespace NEGOCIO
{
    public class VentaNegocio : IVentaNegocio
    {
        private readonly IVentaDatos _ventaDatos; // Cambia el campo a la interfaz
        private readonly SesionDatos _sesionDatos; // Para validar sesiones en pedidos por QR
        private readonly PlatillosDatos _platilloDatos;
       
        public VentaNegocio(IVentaDatos ventaDatos, SesionDatos sesionDatos, PlatillosDatos platilloDatos) // Agrega el parámetro faltante
        {
            _ventaDatos = ventaDatos;
            _sesionDatos = sesionDatos;
            _platilloDatos = platilloDatos;
        }


        // Metodo para registrar venta
        public RespuestaRegistroVenta RegistrarVenta(Venta venta)
        {
            // --- 1. VALIDACIONES DE CABECERA --- (Se mantienen igual)
            if (venta.MesaID <= 0) throw new ArgumentException("El ID de la Mesa es obligatorio.");
            if (venta.DetalleVenta == null || !venta.DetalleVenta.Any()) throw new ArgumentException("Debe haber al menos un platillo.");

            if (venta.TipoPedido == "QR")
            {
                if (venta.SesionID == null || venta.SesionID <= 0)
                    throw new ArgumentException("Sesión inválida para pedido QR.");

                if (!_sesionDatos.ValidarSesionActiva(venta.SesionID.Value))
                    throw new Exception("La sesión no está activa.");
            }
            else
            {
                if (venta.UsuarioID <= 0)
                    throw new ArgumentException("El ID de Usuario es obligatorio para pedidos presenciales.");
            }

            // --- 2. VALIDACIÓN DE DETALLES Y PRECIOS --- (Se mantienen igual)
            decimal totalCalculado = 0;
            foreach (var detalle in venta.DetalleVenta)
            {
                var platilloInfo = _platilloDatos.LeerPorId(detalle.PlatilloID);
                if (platilloInfo == null)
                    throw new Exception($"El platillo ID {detalle.PlatilloID} no existe.");

                detalle.PrecioUnitario = platilloInfo.Precio;
                totalCalculado += (detalle.Cantidad * detalle.PrecioUnitario);
            }

            venta.Total = totalCalculado;

            // --- 3. PERSISTENCIA Y CIERRE ---
            try
            {
                // Insertamos la venta y obtenemos el ID
                int ventaID = _ventaDatos.Insertar(venta);
                decimal totalFinalSesion = 0;

                // Lógica de Cierre de Sesión
                if (venta.EsPagoFinal && venta.SesionID.HasValue && venta.SesionID > 0)
                {
                    totalFinalSesion = _ventaDatos.ObtenerTotalSesion(venta.SesionID.Value);
                    _sesionDatos.FinalizarSesion(venta.SesionID.Value);
                }

                // 2. RETORNO DEL OBJETO COMPLETO:
                return new RespuestaRegistroVenta
                {
                    VentaID = ventaID,
                    TotalAcumulado = totalFinalSesion,
                    Mensaje = venta.EsPagoFinal ? "¡Cuenta cerrada! Gracias por su visita." : "¡Pedido recibido! Ya está en cocina."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico en BD: {ex.Message}");
                throw;
            }
        }


        // Método para leer la venta (correctamente adaptado)
        public Venta ObtenerVentaConDetalles(int idVenta)
        {
            if (idVenta <= 0)
            {
                throw new ArgumentException("El ID de la venta debe ser un valor positivo.");
            }
            // Simplemente llama a la Capa de Datos
            return _ventaDatos.SeleccionarVentaConDetalle(idVenta);
        }

        public List<VentaListaDTO> LeerTodas()
        {
            // Lógica de negocio mínima, solo llamada a datos.
            return _ventaDatos.LeerTodas();
        }

        // Archivo: NEGOCIO/VentaNegocio.cs (FRAGMENTO)

        public int GuardarVenta(VentaListaDTO venta)
        {
            // ... otras validaciones del maestro

            // Si todo es válido, llama a la capa de datos
            return _ventaDatos.RegistrarVenta(venta);
        }
    }
}