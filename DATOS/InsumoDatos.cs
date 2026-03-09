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
    }

}
