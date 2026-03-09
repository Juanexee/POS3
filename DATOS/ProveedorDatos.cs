// Archivo: DATOS/ProveedorDatos.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ENTIDADES;
using ENTIDADES.ProveedorDTO;

namespace DATOS
{
    public class ProveedorDatos
    {
        private readonly string _cadenaConexion;

        public ProveedorDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        public List<Proveedor> LeerTodos()
        {
            var proveedores = new List<Proveedor>();
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ReadProveedores", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            proveedores.Add(new Proveedor
                            {
                                ProveedorID = dr.GetInt32(dr.GetOrdinal("proveedorID")),
                                Nombre = dr.GetString(dr.GetOrdinal("nombre")),
                                Contacto = dr.IsDBNull(dr.GetOrdinal("contacto")) ? string.Empty : dr.GetString(dr.GetOrdinal("contacto")),
                                Telefono = dr.IsDBNull(dr.GetOrdinal("telefono")) ? string.Empty : dr.GetString(dr.GetOrdinal("telefono")),
                                Direccion = dr.IsDBNull(dr.GetOrdinal("direccion")) ? string.Empty : dr.GetString(dr.GetOrdinal("direccion"))
                            });
                        }
                    }
                }
            }
            return proveedores;
        }

        public int Insertar(CrearProveedorDTO proveedor)
        {
            int nuevoID = 0;
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_InsertarProveedor", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nombre", proveedor.Nombre);
                    cmd.Parameters.AddWithValue("@contacto", proveedor.Contacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@telefono", proveedor.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion", proveedor.Direccion ?? (object)DBNull.Value);

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