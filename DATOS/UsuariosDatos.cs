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
    public class UsuariosDatos
    {
        //Campo para la cadena de conexión
        private readonly string _cadenaConexion;

        //Constructor que recibe la cadena de conexión
        public UsuariosDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        //Metodo de insertar usuario
        public void Insertar(Usuario usuario)
        {
            // 1. Abrimos la conexión a la base de datos.
            using SqlConnection con = new(_cadenaConexion);
            con.Open();

            // 2. Creamos el comando para ejecutar el procedimiento almacenado.
            using SqlCommand cmd = new("sp_InsertarUsuario", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            // 3. Agregamos los parámetros al comando.
            cmd.Parameters.AddWithValue("@nombreUsuario", usuario.NombreUsuario);
            cmd.Parameters.AddWithValue("@passwordHash", usuario.PasswordHash); // aqui esta el error
            cmd.Parameters.AddWithValue("@passwordSalt", usuario.PasswordSalt);
            cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
            cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(usuario.Telefono) ? (object)DBNull.Value : usuario.Telefono);
            cmd.Parameters.AddWithValue("@rolID", usuario.RolID);

            cmd.ExecuteNonQuery();
        }


        //Metodo de leer usuarios 

        public List<Usuario> Leer ()
        {
          
            //Creamos una lista llamada lista que actuará como un contenedor para guardar todos los usuarios que leamos de la base de datos
            List<Usuario> lista = new();
            // 1. Abrimos la conexión a la base de datos.
            using SqlConnection con = new(_cadenaConexion);
            con.Open();

            using SqlCommand cmd = new("sp_ReadUsuario", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            // 2. Ejecutar el comando y leer los datos.

            using (var reader = cmd.ExecuteReader())
            {
                // 3. Recorrer cada fila de resultados.
                while (reader.Read())
                {
                    // 4. Mapear la fila actual a un objeto Usuario.
                    Usuario usuario = new()
                    {
                        UsuarioID = Convert.ToInt32(reader["usuarioID"]),
                        NombreUsuario = reader["nombreUsuario"].ToString(),
                        Nombre = reader["nombre"].ToString(),
                        Telefono = reader["telefono"].ToString(),
                        RolID = Convert.ToInt32(reader["rolID"]),
                        Activo = Convert.ToBoolean(reader["activo"])
                    };

                    // 5. Agregar el objeto a la lista.
                    lista.Add(usuario);
                }
            }

            return lista;
        }

        //Metodo Leer por ID 
        public Usuario LeerPorId(int usuarioId)
        {
            // 1. Inicializamos un objeto Usuario a null.
            Usuario usuario = null;

            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_LeerUsuarioPorId", con))
                {
                    
                    cmd.CommandType = CommandType.StoredProcedure;
                    // 2. Agregamos el parámetro @usuarioID al comando.
                    cmd.Parameters.AddWithValue("@usuarioID", usuarioId);

                    // 3. Ejecutamos el comando para leer los datos.
                    using (var reader = cmd.ExecuteReader())
                    {
                        // 4. Verificamos si se encontró un registro.
                        if (reader.Read())
                        {
                            // 5. Mapeamos la única fila de resultados a un objeto Usuario.
                            usuario = new()
                            {
                                UsuarioID = Convert.ToInt32(reader["usuarioID"]),
                                NombreUsuario = reader["nombreUsuario"].ToString(),
                                Nombre = reader["nombre"].ToString(),
                                Telefono = reader["telefono"].ToString(),
                                RolID = Convert.ToInt32(reader["rolID"]),
                                Activo = Convert.ToBoolean(reader["activo"])
                            };
                        }
                    }
                }
            }

            // 6. Devolvemos el objeto Usuario (que será null si no se encontró).
            return usuario;
        }

        //Metodo de actualizar

        public void Actualizar(ENTIDADES.UsuarioDTO.CrearUsuarioDTO usuario, int usuarioModificacionID)
        {
            using SqlConnection con = new(_cadenaConexion);
            con.Open();
            using SqlCommand cmd = new("sp_ActualizarUsuario", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@usuarioID", usuario.UsuarioID);
            cmd.Parameters.AddWithValue("@nombreUsuario", usuario.NombreUsuario);
            cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
            cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(usuario.Telefono) ? (object)DBNull.Value : usuario.Telefono);
            cmd.Parameters.AddWithValue("@rolID", usuario.RolID);
            cmd.Parameters.AddWithValue("@activo", usuario.Activo);

            // **¡CAMBIO CRÍTICO DE AUDITORÍA!**
            cmd.Parameters.AddWithValue("@usuarioModificacionID", usuarioModificacionID);

            cmd.ExecuteNonQuery();
        }

        //Metodo Eliminar (Desactivar)
        public void Eliminar(int usuarioId, bool activo, int usuarioModificacionID)
        {
            using SqlConnection con = new(_cadenaConexion);
            con.Open();
            using SqlCommand cmd = new("sp_EliminarUsuario", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Agregamos los parámetros al comando
            cmd.Parameters.AddWithValue("@usuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@activo", activo);

            // **¡CAMBIO CRÍTICO DE AUDITORÍA!**
            cmd.Parameters.AddWithValue("@usuarioModificacionID", usuarioModificacionID);

            // Ejecutamos el comando
            cmd.ExecuteNonQuery();
        }




        // Metodo para leer por Nombre de Usuario
        public Usuario? LeerPorNombreUsuario(string nombreUsuario)
        {
            Usuario? usuario = null;

            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                
                using (SqlCommand cmd = new("sp_LeerUsuarioPorNombre", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new()
                            {
                                UsuarioID = Convert.ToInt32(reader["usuarioID"]),
                                NombreUsuario = reader["nombreUsuario"].ToString()!,
                                PasswordHash = (byte[]?)reader["passwordHash"],
                                PasswordSalt = (byte[]?)reader["passwordSalt"],
                                RolID = Convert.ToInt32(reader["rolID"]),
                                RolNombre = reader["NombreRol"].ToString()!, // Asumimos que el SP devuelve NombreRol
                                Activo = Convert.ToBoolean(reader["activo"]) // Otros campos si los necesitas (Nombre, Telefono, Activo)
                            };
                        }
                    }
                }
            }
            return usuario;
        }



    }
}
