using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENTIDADES;
using Microsoft.Data.SqlClient;

namespace DATOS
{
    public class VentaDatos : IVentaDatos
    {
        private readonly string _cadenaConexion;
        public VentaDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        // Metodo Insertar // Dentro de VentaDatos.cs, en el método Insertar(Venta venta)

        public int Insertar(Venta venta)
        {
            using SqlConnection con = new(_cadenaConexion);
            con.Open();
            using SqlTransaction transaction = con.BeginTransaction();

            try
            {
                // 1. Insertar Cabecera y obtener el VentaID
                int ventaID = 0;
                using (SqlCommand cmd = new("sp_InsertarVenta", con, transaction)) // Nuevo SP para cabecera
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Añadir nuevos parámetros de entrada (Mesa, Cliente)
                    cmd.Parameters.AddWithValue("@clienteID", (object)venta.ClienteID ?? DBNull.Value); // Manejar nulos
                    cmd.Parameters.AddWithValue("@mesaID", venta.MesaID);
                    cmd.Parameters.AddWithValue("@usuarioID", venta.UsuarioID);
                    cmd.Parameters.AddWithValue("@total", venta.Total);

                    ventaID = Convert.ToInt32(cmd.ExecuteScalar()); // SCOPE_IDENTITY es decimal/numeric, se convierte
                }
                venta.VentaID = ventaID;

                // 2. Insertar Detalle de Venta (en un bucle) y Consumir Insumos
                foreach (var detalle in venta.DetalleVenta)
                {
                    // Usamos el nuevo SP transaccional
                    using (SqlCommand cmdDetalle = new("sp_InsertarDetalleVenta_Transactional", con, transaction))
                    {
                        cmdDetalle.CommandType = System.Data.CommandType.StoredProcedure;

                        cmdDetalle.Parameters.AddWithValue("@ventaID", ventaID);
                        cmdDetalle.Parameters.AddWithValue("@platilloID", detalle.PlatilloID); // Nuevo nombre
                        cmdDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);

                        cmdDetalle.ExecuteNonQuery(); // Aquí se verifica el stock de Insumos
                    }
                }

                // 3. Si todo va bien: transaction.Commit();
                transaction.Commit();
                return ventaID;
            }
            catch (Exception ex)
            {
                // Si falla la cabecera, un detalle o el stock de insumos, revertir todo.
                transaction.Rollback();
                // Propaga la excepción (ej. "Stock insuficiente...")
                throw new Exception("Error al registrar la venta (RestauranteDB). Transacción revertida.", ex);
            }
        }

        // Dentro de VentaDatos.cs

        public Venta SeleccionarVentaConDetalle(int idVenta)
        {
            Venta venta = null;

            // Asegúrate de usar el nuevo nombre del SP
            using SqlConnection con = new(_cadenaConexion);
            using SqlCommand cmd = new("sp_SeleccionarVentaConDetalle_Restaurante", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@VentaId", idVenta);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            // 1. Leer el primer conjunto de resultados (La Cabecera de la Venta)
            if (dr.Read())
            {
                venta = new Venta
                {
                    VentaID = dr.GetInt32(dr.GetOrdinal("ventaID")),
                    UsuarioID = dr.GetInt32(dr.GetOrdinal("usuarioID")),
                    MesaID = dr.GetInt32(dr.GetOrdinal("mesaID")),

                    // ClienteID puede ser nulo, se maneja de forma segura
                    ClienteID = dr.IsDBNull(dr.GetOrdinal("clienteID")) ? (int?)null : dr.GetInt32(dr.GetOrdinal("clienteID")),

                    Fecha = dr.GetDateTime(dr.GetOrdinal("fecha_venta")),
                    Total = dr.GetDecimal(dr.GetOrdinal("total")),
                    Estado = dr.GetString(dr.GetOrdinal("estado")),

                    // Podrías mapear NombreMesero y NombreCliente aquí si los agregaste a la Entidad Venta.

                    DetalleVenta = new List<DetalleVenta>()
                };
            }

            if (venta == null) return null;

            // 2. Moverse al segundo conjunto de resultados (Los Detalles)
            dr.NextResult();

            // 3. Leer los detalles del platillo
            while (dr.Read())
            {
                // Reemplaza DetalleID por DetalleVentaID para que coincida con la definición de la clase DetalleVenta
                venta.DetalleVenta.Add(new DetalleVenta
                {
                    DetalleVentaID = dr.GetInt32(dr.GetOrdinal("detalleID")), // Nuevo nombre
                    VentaID = idVenta,
                    PlatilloID = dr.GetInt32(dr.GetOrdinal("platilloID")), // Nuevo nombre
                    NombreProducto = dr.GetString(dr.GetOrdinal("NombrePlatillo")),
                    Cantidad = dr.GetInt32(dr.GetOrdinal("cantidad")),
                    PrecioUnitario = dr.GetDecimal(dr.GetOrdinal("precio_unitario")),
                    Subtotal = dr.GetDecimal(dr.GetOrdinal("subtotal"))
                });
            }

            return venta;
        }

        public List<VentaListaDTO> LeerTodas()
        {
            var ventas = new List<VentaListaDTO>();

            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ReadVentas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            ventas.Add(new VentaListaDTO
                            {
                                VentaID = dr.GetInt32(dr.GetOrdinal("ventaID")),
                                UsuarioID = dr.GetInt32(dr.GetOrdinal("usuarioID")),
                                NombreCajero = dr.GetString(dr.GetOrdinal("nombreCajero")),
                                FechaVenta = dr.GetDateTime(dr.GetOrdinal("fechaVenta")),
                                Total = dr.GetDecimal(dr.GetOrdinal("total")),
                                Estado = dr.GetString(dr.GetOrdinal("estado"))
                            });
                        }
                    }
                }
            }
            return ventas;
        }

        public int RegistrarVenta(VentaListaDTO venta)
        {
            // Implementación mínima para cumplir con la interfaz.
            // Puedes ajustar la lógica según tus necesidades reales.
            throw new NotImplementedException("RegistrarVenta aún no está implementado.");
        }

    }
}