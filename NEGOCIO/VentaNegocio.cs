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
    public class VentaNegocio
    {
        private readonly IVentaDatos _ventaDatos; // Cambia el campo a la interfaz

        public VentaNegocio(IVentaDatos ventaDatos) // Cambia el parámetro a la interfaz
        {
            _ventaDatos = ventaDatos;
        }


        // Metodo para registrar venta
        public int RegistrarVenta(Venta venta)
        {
            // --- 1. VALIDACIONES DE LA CABECERA (REQUERIMIENTOS BÁSICOS) ---

            // Validar Mesero/Cajero (UsuarioID)
            if (venta.UsuarioID <= 0)
            {
                throw new ArgumentException("El ID de Usuario (Mesero/Cajero) es obligatorio.");
            }

            // Validar Mesa
            if (venta.MesaID <= 0)
            {
                throw new ArgumentException("El ID de la Mesa es obligatorio.");
            }

            // Validar que la venta contenga al menos un platillo
            if (venta.DetalleVenta == null || !venta.DetalleVenta.Any())
            {
                throw new ArgumentException("La venta debe contener al menos un platillo.");
            }

            // --- 2. VALIDACIONES DEL DETALLE (POR CADA PLATILLO) ---
            foreach (var detalle in venta.DetalleVenta)
            {
                // Validar Platillo ID
                if (detalle.PlatilloID <= 0)
                {
                    // Adaptado de ProductoID a PlatilloID
                    throw new ArgumentException("El ID del platillo es obligatorio y debe ser un valor positivo.");
                }

                // Validar Cantidad
                if (detalle.Cantidad <= 0)
                {
                    throw new ArgumentException($"La cantidad para el Platillo ID {detalle.PlatilloID} debe ser mayor que cero.");
                }

                // Validar Precio Unitario (Precio > 0)
                if (detalle.PrecioUnitario <= 0)
                {
                    throw new ArgumentException($"El Precio Unitario para el Platillo ID {detalle.PlatilloID} debe ser mayor que cero.");
                }
            }

            // --- 3. VALIDACIÓN DE COHERENCIA (TOTAL CALCULADO) ---

            // Calcular el total de la venta basado en los detalles (Cálculo del Servidor)
            decimal totalCalculado = venta.DetalleVenta.Sum(d => d.Cantidad * d.PrecioUnitario);

            // Validar que el total reportado por el cliente (venta.Total) sea correcto.
            // Usamos una pequeña tolerancia (0.01m) para evitar problemas de redondeo de decimales.
            if (Math.Abs(venta.Total - totalCalculado) > 0.01m)
            {
                throw new ArgumentException($"El campo 'Total' ({venta.Total:C}) de la venta no coincide con el total calculado de los detalles ({totalCalculado:C}).");
            }

            // --- 4. VALIDACIÓN DE EXISTENCIAS (LOGICA DE STOCK/INSUMOS) ---
            // NOTA: La validación de STOCK de INSUMOS (existencia) NO SE HACE AQUÍ.
            // Se maneja de manera más segura y atómica en el Procedimiento Almacenado 
            // `sp_InsertarDetalleVenta_Transactional` dentro de la Capa de Datos (con ROLLBACK).

            // --- 5. ORQUESTACIÓN Y LLAMADA A DATOS (Transacción) ---
            try
            {
                // Si todas las validaciones de negocio pasan, llamamos a la capa de datos
                int ventaID = _ventaDatos.Insertar(venta);
                return ventaID;
            }
            catch (Exception ex)
            {
                // Captura el error transaccional de la BD (ej. 'Stock insuficiente para un insumo')
                // y relanza la excepción para que el Controlador la maneje.
                Console.WriteLine($"Error de Negocio al registrar venta: {ex.Message}");
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