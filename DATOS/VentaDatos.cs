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

        // Dentro de VentaDatos.cs
        public int Insertar(Venta venta)
        {
            // 1. Preparamos el contenedor para los detalles (UDTT)
            DataTable tablaDetalles = new DataTable();
            tablaDetalles.Columns.Add("platilloID", typeof(int));
            tablaDetalles.Columns.Add("cantidad", typeof(int));
            tablaDetalles.Columns.Add("precio_unitario", typeof(decimal));

            foreach (var item in venta.DetalleVenta)
            {
                tablaDetalles.Rows.Add(item.PlatilloID, item.Cantidad, item.PrecioUnitario);
            }

            using var con = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("sp_RegistrarVentaCompleta_QR", con);
            cmd.CommandType = CommandType.StoredProcedure;

            // 2. Parámetros de la cabecera
            cmd.Parameters.AddWithValue("@mesaID", venta.MesaID);
            cmd.Parameters.AddWithValue("@sesionID", (object)venta.SesionID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@usuarioID", (object)venta.UsuarioID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@clienteID", (object)venta.ClienteID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@total", venta.Total);

            // 3. El parámetro de tabla (UDTT) 🔑
            var parametroLista = cmd.Parameters.AddWithValue("@detalles", tablaDetalles);
            parametroLista.SqlDbType = SqlDbType.Structured;
            parametroLista.TypeName = "dbo.DetalleVentaType";

            try
            {
                con.Open();
                // ExecuteScalar devuelve el ID de la venta que genera el SP
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                // Aquí capturamos errores de SQL (como el RAISERROR de stock insuficiente)
                throw new Exception("Error en la base de datos al registrar la venta completa.", ex);
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
            
            throw new NotImplementedException("RegistrarVenta aún no está implementado.");
        }

        public decimal ObtenerTotalSesion(int sesionId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Sumamos la columna 'total' de todas las ventas de esta sesión 💰
                string query = "SELECT ISNULL(SUM(total), 0) FROM Ventas WHERE sesionID = @sesionId";

                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@sesionId", sesionId);

                conexion.Open();
                // Usamos ExecuteScalar porque solo esperamos un único valor numérico
                return (decimal)comando.ExecuteScalar();
            }
        }

        public List<PedidoAgrupadoDTO> ListarPedidosAgrupados()
        {
            List<PedidoAgrupadoDTO> lista = new List<PedidoAgrupadoDTO>();

            // Usamos la conexión a tu base de datos RestauranteDB
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ConsultarPedidosCocinaAgrupados", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new PedidoAgrupadoDTO()
                            {
                                NombrePlatillo = dr["NombrePlatillo"].ToString(),
                                CantidadTotal = Convert.ToInt32(dr["CantidadTotal"]),
                                FechaPrimerPedido = Convert.ToDateTime(dr["FechaPrimerPedido"]),
                                IdsRelacionados = dr["IdsRelacionados"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Es importante manejar el error para la trazabilidad
                    lista = new List<PedidoAgrupadoDTO>();
                }
            }
            return lista;
        }

        public bool ActualizarEstadoMasivo(string ids, string nuevoEstado)
        {
            bool respuesta = false;

            using (SqlConnection oconexion = new SqlConnection(_cadenaConexion))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ActualizarEstadoPedidosAgrupados", oconexion);
                    // Pasamos los parámetros que espera el SP
                    cmd.Parameters.AddWithValue("IdsPedidos", ids);
                    cmd.Parameters.AddWithValue("NuevoEstado", nuevoEstado);
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    // ExecuteNonQuery devuelve el número de filas afectadas
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        respuesta = true;
                    }
                }
                catch (Exception ex)
                {
                    // Aquí podrías loguear el error para depuración
                    respuesta = false;
                    throw new Exception("Error al actualizar los estados en la base de datos.", ex);
                }
            }
            return respuesta;
        }

    }
}