// Archivo: DATOS/RecetaDatos.cs
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ENTIDADES.RecetaDTO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace DATOS
{
    public class RecetaDatos : IRecetaDatos
    {
        private readonly string _cadenaConexion;

        public RecetaDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        // 1. Leer Receta por Platillo ID
        public List<RecetaDetalle> LeerPorPlatillo(int platilloID)
        {
            var detalles = new List<RecetaDetalle>();

            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ReadRecetaPorPlatillo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@platilloID", platilloID);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            detalles.Add(new RecetaDetalle
                            {
                                RecetaID = dr.GetInt32(dr.GetOrdinal("recetaID")),
                                PlatilloID = dr.GetInt32(dr.GetOrdinal("platilloID")),
                                InsumoID = dr.GetInt32(dr.GetOrdinal("insumoID")),
                                Cantidad = dr.GetDecimal(dr.GetOrdinal("cantidadRequerida")),
                                NombreInsumo = dr.GetString(dr.GetOrdinal("nombreInsumo")),
                                NombreUnidad = dr.GetString(dr.GetOrdinal("nombreUnidad")),
                                Abreviatura = dr.GetString(dr.GetOrdinal("abreviatura"))
                            });
                        }
                    }
                }
            }
            return detalles;
        }

        // 2. Gestionar (Upsert) Receta
        public bool Gestionar(int platilloID, List<RecetaBaseDTO> detalles)
        {
            // Serializar la lista de detalles a JSON
            string detallesJson = JsonSerializer.Serialize(detalles);

            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_GestionarReceta_Transactional", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@platilloID", platilloID);
                    cmd.Parameters.AddWithValue("@detallesReceta", detallesJson);

                    // ExecuteScalar para obtener el resultado de éxito (1) o error (excepción)
                    var resultado = cmd.ExecuteScalar();

                    // Si resultado es 1, fue exitoso. Si falló, la BD lanzó una excepción que C# captura.
                    return resultado != null && Convert.ToInt32(resultado) == 1;
                }
            }
        }
    }
}