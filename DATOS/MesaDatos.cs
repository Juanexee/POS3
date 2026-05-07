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
                    // Obtener índices de columna una sola vez mejora fiabilidad y rendimiento
                    int idxMesaID = reader.GetOrdinal("mesaID");
                    int idxNumero = reader.GetOrdinal("numero_mesa");
                    int idxCapacidad = reader.GetOrdinal("Capacidad");
                    int idxUbicacion = reader.GetOrdinal("Ubicacion");
                    int idxEstado = reader.GetOrdinal("Estado");

                    while (reader.Read())
                    {
                        lista.Add(new Mesa
                        {
                            MesaID = !reader.IsDBNull(idxMesaID) ? reader.GetInt32(idxMesaID) : 0,
                            // Cambiamos a "numero_mesa" porque así lo pusiste en el SELECT del SP
                            NumeroMesa = !reader.IsDBNull(idxNumero) ? reader.GetInt32(idxNumero) : 0,
                            Capacidad = !reader.IsDBNull(idxCapacidad) ? reader.GetInt32(idxCapacidad) : 0,
                            Ubicacion = !reader.IsDBNull(idxUbicacion) ? reader.GetString(idxUbicacion) : string.Empty,
                            Estado = !reader.IsDBNull(idxEstado) ? reader.GetString(idxEstado) : string.Empty
                        });
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