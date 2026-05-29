using System.Data;
using Microsoft.Data.SqlClient;
using ENTIDADES; // Asegúrate de tener una entidad 'Mesa' en este namespace

namespace DATOS
{
    public class MesaDatos
    {
        private readonly string _cadenaConexion;

        public MesaDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        // 1. INSERTAR MESA
        // 1. INSERTAR MESA
        public bool InsertarMesa(int numeroMesa, int capacidad, string ubicacion)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_InsertarMesa", conexion);
                comando.CommandType = CommandType.StoredProcedure;

                // Cambiamos @NumeroMesa por @numero_mesa para que coincida con tu SQL
                comando.Parameters.AddWithValue("@numero_mesa", numeroMesa);
                comando.Parameters.AddWithValue("@Capacidad", capacidad);
                comando.Parameters.AddWithValue("@Ubicacion", ubicacion);
                comando.Parameters.AddWithValue("@activo", true); // Agregamos el parámetro faltante

                conexion.Open();
                return comando.ExecuteNonQuery() > 0;
            }
        }

        // 2. LEER TODAS LAS MESAS
        // 2. LEER TODAS LAS MESAS
        public List<Mesa> ListarMesas()
        {
            var lista = new List<Mesa>();
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_ListarMesas", conexion);
                comando.CommandType = CommandType.StoredProcedure;

                conexion.Open();
                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var mesa = new Mesa
                        {
                            MesaID = Convert.ToInt32(reader["mesaID"]),
                            NumeroMesa = Convert.ToInt32(reader["numero_mesa"]),
                            Capacidad = Convert.ToInt32(reader["Capacidad"]),
                            Ubicacion = reader["Ubicacion"].ToString(),
                            Estado = reader["Estado"].ToString(),

                            // Lógica para mapear el entero nulo de la sesión
                            SesionID = reader["sesionID"] != DBNull.Value ? Convert.ToInt32(reader["sesionID"]) : (int?)null
                        };
                        lista.Add(mesa);
                    }
                }
            }
            return lista;
        }

        // 3. ACTUALIZAR DATOS DE LA MESA
        public bool ActualizarMesa(int mesaId, int numeroMesa, int capacidad, string ubicacion, bool activo = true)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_ActualizarMesa", conexion);
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@mesaID", mesaId);
                comando.Parameters.AddWithValue("@numero_mesa", numeroMesa); // Ajustado nombre
                comando.Parameters.AddWithValue("@Capacidad", capacidad);
                comando.Parameters.AddWithValue("@Ubicacion", ubicacion);
                comando.Parameters.AddWithValue("@activo", activo); // Agregamos el parámetro activo

                conexion.Open();
                return comando.ExecuteNonQuery() > 0;
            }
        }
        // 4. ELIMINAR O DESACTIVAR MESA (Opcional pero recomendado)
        public bool EliminarMesa(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Podrías crear un SP llamado sp_EliminarMesa
                string query = "DELETE FROM Mesas WHERE mesaID = @id";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@id", mesaId);

                conexion.Open();
                return comando.ExecuteNonQuery() > 0;
            }
        }
    }
}