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
    public  class RolesDatos
    {
        //Campo Para la cadena de conexión
        private readonly string _cadenaConexion;
        //Constructor que recibe la cadena de conexión
        public RolesDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        //Metodo Insertar Roles
        public void Insertar (Rol rol)
        {
            //1. Abrimos la conexión a la base de datos
            using SqlConnection con = new (_cadenaConexion);
            con.Open();
            //2. Creamos el comando para ejecutar el procedimiento almacenado
            using SqlCommand cmd = new SqlCommand("sp_InsertarRoles")
            {
                CommandType = System.Data.CommandType.StoredProcedure,
                Connection = con
            };

            //3. Agregamos los parámetros al comando
            cmd.Parameters.AddWithValue("@nombreRol", rol.NombreRol);
            cmd.Parameters.AddWithValue("@descripcionRol", rol.DescripcionRol);
            cmd.ExecuteNonQuery();
            


        }

        //Metodo Leer Roles
        public List<Rol> Leer()
        {
            //Creamos una lista llamada lista que actuará como un contenedor para guardar todos los roles que leamos de la base de datos
            List<Rol> lista = new();
            //1. Abrimos la conexión a la base de datos
            using SqlConnection con = new(_cadenaConexion);
            con.Open();

            //Llamamos al procedimiento almacenado sp_LeerRoles
            using SqlCommand cmd = new SqlCommand("sp_ReadRoles")
            {
                CommandType = System.Data.CommandType.StoredProcedure,
                Connection = con
            };

            // Ejecutamos el comando y leemos los datos

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    //Mapear la fila actual a un objeto Rol.
                    Rol rol = new()
                    {
                        RolID = Convert.ToInt32(reader["RolID"]),
                        NombreRol = reader["NombreRol"].ToString()!,
                        DescripcionRol = reader["DescripcionRol"].ToString()!
                    };
                    //Agregamos el objeto rol a la lista

                    lista.Add(rol);

                }

                return lista;
            }
        }

        //Metodo Leer por id los roles
        public Rol LeerPorID(int rolID)
        {
            // 1. Inicializamos un objeto Rol a null.
            Rol rol = null;
            // 2. Abrimos la conexión a la base de datos.          
            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_LeerRolPorId", con))
                {
                    // 1. Especificamos que el comando es un procedimiento almacenado.
                    cmd.CommandType = CommandType.StoredProcedure;
                    // 2. Agregamos el parámetro @rolID al comando.
                    cmd.Parameters.AddWithValue("@rolID", rolID);
                    // 3. Ejecutamos el comando para leer los datos.
                    using (var reader = cmd.ExecuteReader())
                    {
                        // 4. Verificamos si se encontró un registro.
                        if (reader.Read())
                        {
                            // 5. Mapeamos la única fila de resultados a un objeto Rol.
                            rol = new Rol
                            {
                                RolID = Convert.ToInt32(reader["RolID"]),
                                NombreRol = reader["NombreRol"].ToString()!,
                                DescripcionRol = reader["DescripcionRol"].ToString()!

                            };
                        }

                    }

                }


            }
            // 6. Devolvemos el objeto Rol (o null si no se encontró).

            return rol;


        }

        //Metodo para Actualizar
        public void Actualizar(Rol rol)
        {
            // 1. Abrimos la conexión a la base de datos
            using SqlConnection con = new(_cadenaConexion);
            con.Open();

            // 2. Creamos el comando para ejecutar el procedimiento almacenado
            using SqlCommand cmd = new("sp_ActualizarRol", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            // 3. Agregamos los parámetros al comando
            cmd.Parameters.AddWithValue("@rolID", rol.RolID);
            cmd.Parameters.AddWithValue("@nombreRol", rol.NombreRol);
            cmd.Parameters.AddWithValue("@decripcionRol", rol.DescripcionRol);
            cmd.Parameters.AddWithValue("@activo", rol.Activo);
            cmd.ExecuteNonQuery();

            // 4. Ejecutamos el comando
            cmd.ExecuteNonQuery();
        }

        //Metodo para Eliminar
        public void Eliminar (int rolId, bool activo, int usuarioIDLogueado)
        {
            using SqlConnection con = new(_cadenaConexion);
            con.Open();
            using SqlCommand cmd = new("sp_EliminarRol", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@rolID", rolId);
            cmd.Parameters.AddWithValue("@activo", activo);

            // El parámetro requerido por tu SQL
            cmd.Parameters.AddWithValue("@usuarioID", usuarioIDLogueado);

            cmd.ExecuteNonQuery();
        }



    }
}   
