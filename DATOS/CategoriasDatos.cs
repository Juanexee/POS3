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
                                Nombre = reader.GetString(1),
                                Activo = reader.FieldCount > 2 ? (reader.IsDBNull(2) ? true : reader.GetBoolean(2)) : true
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

        // ---------- ACTIVAR CATEGORÍA ----------
        public bool Activar(int id)
        {
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                string queryUpdate = "UPDATE Categorias SET activo = 1 WHERE categoriaID = @id";
                using (SqlCommand cmd = new(queryUpdate, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }


        // ---------- DELETE O DESACTIVAR CATEGORÍA (Borrado Seguro) ----------
        public bool EliminarOIdesactivar(int id)
        {
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                try
                {
                    // 1. Intentamos borrado físico directo
                    string queryDelete = "DELETE FROM Categorias WHERE categoriaID = @id";
                    using (SqlCommand cmd = new(queryDelete, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int filas = cmd.ExecuteNonQuery();
                        if (filas > 0) return true; // Se eliminó físicamente con éxito
                    }
                }
                catch (SqlException ex)
                {
                    // Error 547 = Conflicto de FK (Tiene platillos asociados)
                    if (ex.Number == 547)
                    {
                        // 2. Aplicamos Borrado Lógico (Desactivación) para proteger la integridad
                        string queryUpdate = "UPDATE Categorias SET activo = 0 WHERE categoriaID = @id";
                        using (SqlCommand cmd = new(queryUpdate, con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            return cmd.ExecuteNonQuery() > 0;
                        }
                    }
                    throw; // Si es otro tipo de error SQL, lo elevamos
                }
            }
            return false;
        }
    }
}
