using System.Data;
using Microsoft.Data.SqlClient;
using ENTIDADES;

namespace DATOS
{
    public class SesionDatos
    {
        // Creamos la cadena de conexxion
        private readonly string _cadenaConexion;


        //Contructor que resive la cadena de conexion
        public SesionDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        //Metodo abrir seseion 

        public int AbrirSesion(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Usamos SQL directo para este ejemplo, o podrías crear un SP
                string query = @"INSERT INTO SesionMesa (mesaID, estado, fecha_inicio) 
                               VALUES (@mesaId, 'Activa', GETDATE());
                               SELECT SCOPE_IDENTITY();";

                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@mesaId", mesaId);

                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public bool ExisteSesionActiva(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string query = "SELECT COUNT(1) FROM SesionMesa WHERE mesaID = @mesaId AND estado = 'Activa'";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@mesaId", mesaId);

                conexion.Open();
                return (int)comando.ExecuteScalar() > 0;
            }
        }

        // este metodo sirve para buscar si existe una sesion con estado activopara la mesa
        public int? ObtenerSesionActiva(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string query = "SELECT sesionID FROM SesionMesa WHERE mesaID = @mesaId AND estado = 'Activa'";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@mesaId", mesaId);

                conexion.Open();
                var resultado = comando.ExecuteScalar();

                return resultado != null ? Convert.ToInt32(resultado) : (int?)null;
            }
        }

        public bool ValidarSesionActiva(int sesionId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Usamos COUNT para saber si existe al menos una fila que cumpla
                string query = "SELECT COUNT(1) FROM SesionMesa WHERE sesionID = @sesionId AND estado = 'Activa'";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@sesionId", sesionId);

                conexion.Open();
                int existe = (int)comando.ExecuteScalar();

                return existe > 0; // Si es mayor a 0, la sesión es válida y está abierta
            }
        }

        //METODO FINALIZAR SESION, ESTE METODO SERVIRA PARA QUE CUANDO EL CLIENTE PAGUE LA SESION SEA FINALIZADA 


        public bool FinalizarSesion(int sesionId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // 1. Cerramos la sesión 📱
                // 2. Buscamos el mesaID de esa sesión y ponemos la mesa como 'Disponible' 🟢
                string query = @"
            UPDATE SesionMesa 
            SET estado = 'Finalizada', fecha_fin = GETDATE() 
            WHERE sesionID = @sesionId;

            UPDATE Mesas 
            SET estado = 'Disponible' 
            WHERE mesaID = (SELECT mesaID FROM SesionMesa WHERE sesionID = @sesionId);";

                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@sesionId", sesionId);

                conexion.Open();
                int filasAfectadas = comando.ExecuteNonQuery();

                return filasAfectadas > 0;
            }
        }

        public bool EsMesaDisponible(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Queremos saber si existe una mesa con ese ID que esté 'Disponible'
                string sql = "SELECT COUNT(*) FROM Mesas WHERE MesaID = @id AND Estado = 'Disponible'";
                SqlCommand cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@id", mesaId);

                conexion.Open();
                int count = (int)cmd.ExecuteScalar();

                // Si el conteo es 1, significa que está libre ✅
                return count > 0;
            }
        }

        public bool EjecutarCambioMesa(int sesionId, int nuevaMesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Llamamos al procedimiento almacenado que creamos en SQL
                var comando = new SqlCommand("sp_CambiarMesaSesion", conexion);
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@SesionID", sesionId);
                comando.Parameters.AddWithValue("@NuevaMesaID", nuevaMesaId);

                conexion.Open();
                int filasAfectadas = comando.ExecuteNonQuery();

                // El SP hace 3 UPDATES, así que filasAfectadas debería ser > 0
                return filasAfectadas > 0;
            }
        }

        public string ObtenerEstadoSesion(int sesionId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string query = "SELECT estado FROM SesionMesa WHERE sesionID = @sesionId";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@sesionId", sesionId);

                conexion.Open();
                var resultado = comando.ExecuteScalar();

                return resultado?.ToString() ?? string.Empty;
            }
        }
    }
}