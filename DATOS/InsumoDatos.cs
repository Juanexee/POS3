using System.Collections.Generic;
using System.Data;
using ENTIDADES;

using ENTIDADES.InsumosDTO;
using Microsoft.Data.SqlClient;
namespace DATOS
{
    public class InsumoDatos
    {

        private readonly string _cadenaConexion;

        public InsumoDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        public List<Insumo> LeerTodos()
        {
            var insumos = new List<Insumo>();

            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ReadInsumos", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            insumos.Add(new Insumo
                            {
                                InsumoID = dr.GetInt32(dr.GetOrdinal("insumoID")),
                                Nombre = dr.GetString(dr.GetOrdinal("nombre")),
                                Descripcion = dr.IsDBNull(dr.GetOrdinal("descripcion")) ? string.Empty : dr.GetString(dr.GetOrdinal("descripcion")),
                                Existencia = dr.GetDecimal(dr.GetOrdinal("existencia")),
                                UnidadID = dr.GetInt32(dr.GetOrdinal("unidadID")),
                                // Campos del JOIN
                                NombreUnidad = dr.GetString(dr.GetOrdinal("nombreUnidad")),
                                Abreviatura = dr.GetString(dr.GetOrdinal("abreviatura"))
                            });
                        }
                    }
                }
            }
            return insumos;
        }

        // 2. Insertar (Implementa sp_InsertarInsumo)
        public int Insertar(CrearActualizarInsumoDTO insumo)
        {
            int nuevoInsumoID = 0;
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_InsertarInsumo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nombre", insumo.Nombre);
                    // Manejar la descripción nullable
                    cmd.Parameters.AddWithValue("@descripcion", insumo.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@unidadID", insumo.UnidadID);

                    var resultado = cmd.ExecuteScalar(); // Ejecuta y obtiene el ID
                    if (resultado != null)
                    {
                        nuevoInsumoID = Convert.ToInt32(resultado);
                    }
                }
            }
            return nuevoInsumoID;
        }

        // =====================================================
        // Métodos para la App Móvil Gerencial - Inventario
        // RF-MOV-MON-01, RF-MOV-MON-02
        // =====================================================

        /// <summary>
        /// Obtiene el resumen completo del inventario: existencias, valoración y alertas de stock.
        /// Cubre RF-MOV-MON-01.
        /// </summary>
        /// <param name="stockMinimoDefault">Umbral mínimo global si el insumo no tiene uno configurado.</param>
        public List<InventarioResumenDTO> ObtenerResumenInventario(decimal stockMinimoDefault = 5)
        {
            var lista = new List<InventarioResumenDTO>();

            // Obtenemos el costo unitario promedio más reciente desde las compras
            string query = @"
                SELECT 
                    i.insumoID,
                    i.nombre,
                    ISNULL(i.descripcion, '') AS descripcion,
                    i.existencia,
                    ISNULL(i.stockMinimo, @stockMinimoDefault) AS stockMinimo,
                    ISNULL(uc.costoUnitario, 0)               AS costoUnitario,
                    um.nombreUnidad,
                    um.abreviatura
                FROM Insumos i
                INNER JOIN UnidadesMedida um ON i.unidadID = um.unidadID
                OUTER APPLY (
                    SELECT TOP 1 dc.precioUnitario AS costoUnitario
                    FROM DetallesCompra dc
                    WHERE dc.insumoID = i.insumoID
                    ORDER BY dc.detalleCompraID DESC
                ) uc
                ORDER BY i.nombre";

            using var con = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@stockMinimoDefault", stockMinimoDefault);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new InventarioResumenDTO
                {
                    InsumoID = dr.GetInt32(dr.GetOrdinal("insumoID")),
                    Nombre = dr.GetString(dr.GetOrdinal("nombre")),
                    Descripcion = dr.GetString(dr.GetOrdinal("descripcion")),
                    ExistenciaActual = dr.GetDecimal(dr.GetOrdinal("existencia")),
                    StockMinimo = dr.GetDecimal(dr.GetOrdinal("stockMinimo")),
                    CostoUnitario = dr.GetDecimal(dr.GetOrdinal("costoUnitario")),
                    NombreUnidad = dr.GetString(dr.GetOrdinal("nombreUnidad")),
                    Abreviatura = dr.GetString(dr.GetOrdinal("abreviatura"))
                });
            }
            return lista;
        }

        /// <summary>
        /// Obtiene únicamente los insumos que han alcanzado el nivel crítico de stock.
        /// Cubre RF-MOV-MON-02.
        /// </summary>
        /// <param name="stockMinimoDefault">Umbral mínimo global si el insumo no tiene uno configurado.</param>
        public List<InventarioResumenDTO> ObtenerInsumosEnAlerta(decimal stockMinimoDefault = 5)
        {
            // Reutilizamos el método completo y filtramos los que están en alerta
            return ObtenerResumenInventario(stockMinimoDefault)
                .Where(i => i.EnAlerta)
                .ToList();
        }
    }

}
