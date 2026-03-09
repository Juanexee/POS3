// Archivo: DATOS/CompraDatos.cs (Nueva Clase)
using System.Data;
using ENTIDADES;
using ENTIDADES.CompraDTO;
using Microsoft.Data.SqlClient;

namespace DATOS
{
    public class CompraDatos
    {
        private readonly string _cadenaConexion;

        public CompraDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        public int InsertarCompra(CompraMaestroDTO compra)
        {
            // La transacción debe ser manejada en esta clase de datos
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                int compraID = -1;

                try
                {
                    // 1. Insertar el Encabezado de la Compra
                    using (SqlCommand cmdMaestro = new("sp_InsertarCompra", con, transaction))
                    {
                        cmdMaestro.CommandType = CommandType.StoredProcedure;
                        cmdMaestro.Parameters.AddWithValue("@proveedorID", compra.ProveedorID);
                        cmdMaestro.Parameters.AddWithValue("@total", compra.Total);

                        // Leer el ID generado
                        compraID = Convert.ToInt32(cmdMaestro.ExecuteScalar());
                    }

                    // 2. Insertar los Detalles y Actualizar Stock (Bucle)
                    foreach (var detalle in compra.Detalles)
                    {
                        using (SqlCommand cmdDetalle = new("sp_InsertarDetalleCompra_Transactional", con, transaction))
                        {
                            cmdDetalle.CommandType = CommandType.StoredProcedure;
                            cmdDetalle.Parameters.AddWithValue("@compraID", compraID);
                            cmdDetalle.Parameters.AddWithValue("@insumoID", detalle.InsumoID);
                            cmdDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                            cmdDetalle.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);

                            cmdDetalle.ExecuteNonQuery(); // Ejecutar Detalle e incrementar stock
                        }
                    }

                    // 3. Confirmar la Transacción (Si todo fue bien)
                    transaction.Commit();
                    return compraID;
                }
                catch (Exception ex)
                {
                    // 4. Revertir la Transacción (Si algo falló)
                    transaction.Rollback();
                    // Relanzar la excepción para que Negocio y el Controller la manejen
                    throw new Exception("Error al registrar la compra. Transacción revertida.", ex);
                }
            }
        }
    }
}