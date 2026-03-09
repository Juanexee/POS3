// Archivo: DATOS/UnidadMedidaDatos.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ENTIDADES;
using ENTIDADES.UnidadMedidaDTO;

namespace DATOS
{
    public class UnidadMedidaDatos
    {
        private readonly string _cadenaConexion;

        public UnidadMedidaDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        public List<UnidadMedida> LeerTodos()
        {
            var unidades = new List<UnidadMedida>();
            // ... (Lógica para ejecutar sp_ReadUnidadesMedida y mapear resultados)
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ReadUnidadesMedida", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            unidades.Add(new UnidadMedida
                            {
                                UnidadID = dr.GetInt32(dr.GetOrdinal("unidadID")),
                                Nombre = dr.GetString(dr.GetOrdinal("nombre")),
                                Abreviatura = dr.GetString(dr.GetOrdinal("abreviatura"))
                            });
                        }
                    }
                }
            }
            return unidades;
        }

        public int Insertar(CrearUnidadMedidaDTO unidad)
        {
            int nuevoID = 0;
            // ... (Lógica para ejecutar sp_InsertarUnidadMedida y obtener el ID)
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_InsertarUnidadMedida", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre", unidad.Nombre);
                    cmd.Parameters.AddWithValue("@abreviatura", unidad.Abreviatura);

                    var resultado = cmd.ExecuteScalar();
                    if (resultado != null)
                    {
                        nuevoID = Convert.ToInt32(resultado);
                    }
                }
            }
            return nuevoID;
        }
    }
}