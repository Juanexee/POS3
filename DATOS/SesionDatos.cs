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

        public virtual int AbrirSesion(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_AbrirSesionMesa", conexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@mesaID", mesaId);


                conexion.Open();
                // El SP devuelve una fila con el sesionID
                var resultado = comando.ExecuteScalar();
                return Convert.ToInt32(resultado);
            }
        }

        public virtual bool ExisteSesionActiva(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_ExisteSesionActiva", conexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@mesaId", mesaId);

                conexion.Open();
                return (int)comando.ExecuteScalar() > 0;
            }
        }

        // este metodo sirve para buscar si existe una sesion con estado activopara la mesa
        public virtual int? ObtenerSesionActiva(int mesaId)
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

        public virtual bool ValidarSesionActiva(int sesionId)
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


        public virtual bool FinalizarSesion(int sesionId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_FinalizarSesionYLibre", conexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@sesionID", sesionId);

                conexion.Open();
                int filasAfectadas = comando.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
        }

        public virtual bool EsMesaDisponible(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_EsMesaDisponible", conexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@mesaId", mesaId);

                conexion.Open();
                return (int)comando.ExecuteScalar() > 0;
            }
        }

        public virtual bool EjecutarCambioMesa(int sesionId, int nuevaMesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_CambiarMesaSesion", conexion);
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@sesionID", sesionId);
                comando.Parameters.AddWithValue("@nuevaMesaID", nuevaMesaId);

                conexion.Open();

                // Ejecutamos y validamos si el SP devolvió un 1 (Éxito)
                var resultado = comando.ExecuteScalar();
                return resultado != null && Convert.ToInt32(resultado) == 1;
            }
        }

        public virtual string ObtenerEstadoSesion(int sesionId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_ObtenerEstadoSesion", conexion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@sesionId", sesionId);

                conexion.Open();
                var resultado = comando.ExecuteScalar();
                return resultado?.ToString() ?? string.Empty;
            }
        }

        public virtual bool CambiarEstadoPedidos(List<int> ids, string nuevoEstado)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Convertimos la lista [1,2,3] en una cadena "1,2,3" para el SQL
                string idsFormateados = string.Join(",", ids);

                string query = $"UPDATE DetalleVenta SET estadoCocinero = @estado WHERE detalleID IN ({idsFormateados})";

                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@estado", nuevoEstado);

                conexion.Open();
                int filasAfectadas = comando.ExecuteNonQuery();

                // Si se actualizaron filas, devolvemos true
                return filasAfectadas > 0;
            }
        }

        public List<PedidoAgrupadoDTO> ObtenerPedidosAgrupados()
        {
            var lista = new List<PedidoAgrupadoDTO>();
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                var comando = new SqlCommand("sp_ConsultarPedidosCocinaAgrupados", conexion);
                comando.CommandType = CommandType.StoredProcedure;

                conexion.Open();
                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new PedidoAgrupadoDTO
                        {
                            NombrePlatillo = reader["NombrePlatillo"].ToString(),
                            CantidadTotal = Convert.ToInt32(reader["CantidadTotal"]),
                            IdsRelacionados = reader["IdsRelacionados"].ToString(),
                            FechaPrimerPedido = Convert.ToDateTime(reader["FechaPrimerPedido"]),
                            NumerosMesas = reader["NumerosMesas"].ToString(),
                            Estado = reader["Estado"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public virtual List<string> ObtenerEstadosDePedidos(List<int> ids)
        {
            var estados = new List<string>();
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string idsFormateados = string.Join(",", ids);
                // Consulta limpia para traer los estados actuales en base a los IDs del lote
                string query = $"SELECT estadoCocinero FROM DetalleVenta WHERE detalleID IN ({idsFormateados})";

                var comando = new SqlCommand(query, conexion);
                conexion.Open();

                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        estados.Add(reader["estadoCocinero"].ToString());
                    }
                }
            }
            return estados;
        }

    }




}