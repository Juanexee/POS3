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
    public  class PlatillosDatos
    {
        //Campo Para la cadena de conexion
        private readonly string _cadenaConexion;
        //Contructor que resive la cadena de conexion
        public PlatillosDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        //Metodo Insertar Platillos
        public void Insertar(PlatilloDTO platillo)
        {
            //1. Abrimos la conexión a la base de datos
            using SqlConnection con = new(_cadenaConexion);
            con.Open();
            //2. Creamos el comando para ejecutar el procedimiento almacenado
            using SqlCommand cmd = new SqlCommand("sp_InsertarPlatillo")
            {
                CommandType = System.Data.CommandType.StoredProcedure,
                Connection = con
            };

            //3. Agregamos los parámetros al comando
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@nombre", platillo.Nombre);
            cmd.Parameters.AddWithValue("@descripcion", platillo.Descripcion ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", platillo.Precio);
            cmd.Parameters.AddWithValue("@categoriaID", platillo.CategoriaID);
            cmd.ExecuteNonQuery();
        }

        //Metodo Leer Platillos
        public List<Platillo> Leer()
        {
            //Creamos una lista llamada lista que actuará como un contenedor para guardar todos los platillo que leamos de la base de datos
            List<Platillo> lista = new();
            //1. Abrimos la conexión a la base de datos
            using SqlConnection con = new(_cadenaConexion);
            con.Open();
            //Llamamos al procedimiento almacenado sp_LeerPlatillos
            using SqlCommand cmd = new SqlCommand("sp_ReadPlatillos")
            {
                CommandType = System.Data.CommandType.StoredProcedure,
                Connection = con
            };

            // Ejecutamos el comando y leemos los datos
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Platillo
                    {
                        PlatilloID = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Precio = reader.GetDecimal(3),
                        Disponible = reader.GetBoolean(4),
                        CategoriaID = reader.GetInt32(5),
                        NombreCategoria = reader.GetString(6)
                    });
                }
            }
            return lista;
        }

        //Metodo para obtener un platillo por ID
        public Platillo LeerPorId(int id)
        {
            Platillo platillo = null;
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_LeerPlatilloPorId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@platilloID", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            platillo = new Platillo
                            {
                                PlatilloID = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Precio = reader.GetDecimal(3),
                                Disponible = reader.GetBoolean(4),
                                CategoriaID = reader.GetInt32(5),
                                NombreCategoria = reader.GetString(6)
                            };
                        }
                    }
                }
            }
            return platillo;
        }
        //Metodo Actualizar Platillos
        public void Actualizar(int id, PlatilloDTO platillo)
        {
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ActualizarPlatillo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@platilloID", id);
                    cmd.Parameters.AddWithValue("@nombre", platillo.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", platillo.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@precio", platillo.Precio);
                    cmd.Parameters.AddWithValue("@categoriaID", platillo.CategoriaID);
                    cmd.Parameters.AddWithValue("@disponible", platillo.Disponible);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Metodo Eliminar Platillos
        public void Eliminar(int id, bool disponible)
        {
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_EliminarPlatillo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@platilloID", id);
                    cmd.Parameters.AddWithValue("@disponible", disponible);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
