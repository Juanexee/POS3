using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENTIDADES;
using Microsoft.Data.SqlClient;

namespace DATOS
{
    public class CategoriasDatos
    {
        //Campo Para la cadena de conexion
        private readonly string _cadenaConexion;
        //Contructor que resive la cadena de conexion
        public CategoriasDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        public void Insertar(CategoriaDTO categoria)
        {
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_InsertarCategoria", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Categoria> LeerTodos()
        {
            List<Categoria> lista = new();
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ReadCategorias", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Categoria
                            {
                                CategoriaID = reader.GetInt32(0),
                                Nombre = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // ---------- READ (Leer por ID) ----------
        public Categoria LeerPorId(int id)
        {
            Categoria categoria = null;
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_LeerCategoriaPorId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@categoriaID", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoria = new Categoria
                            {
                                CategoriaID = reader.GetInt32(0),
                                Nombre = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            return categoria;
        }

        // ---------- UPDATE (Actualizar) ----------
        public void Actualizar(int id, CategoriaDTO categoria)
        {
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ActualizarCategoria", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@categoriaID", id);
                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
