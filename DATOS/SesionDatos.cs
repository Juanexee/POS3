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
                // Actualizamos el estado y la fecha de finalización
                string query = @"UPDATE SesionMesa 
                         SET estado = 'Finalizada', 
                             fecha_fin = GETDATE() 
                         WHERE sesionID = @sesionId";

                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@sesionId", sesionId);

                conexion.Open();
                int filasAfectadas = comando.ExecuteNonQuery();

                // Si filasAfectadas > 0, significa que se encontró y actualizó la sesión
                return filasAfectadas > 0;
            }
        }
    }
}