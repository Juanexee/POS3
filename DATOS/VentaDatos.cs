using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENTIDADES;
using Microsoft.Data.SqlClient;

namespace DATOS
{
    public class VentaDatos : IVentaDatos
    {
        private readonly string _cadenaConexion;
        public VentaDatos(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        // Metodo Insertar // Dentro de VentaDatos.cs, en el método Insertar(Venta venta)

        // Dentro de VentaDatos.cs
        public int Insertar(Venta venta)
        {
            // 1. Preparamos el contenedor para los detalles (UDTT)
            DataTable tablaDetalles = new DataTable();
            tablaDetalles.Columns.Add("platilloID", typeof(int));
            tablaDetalles.Columns.Add("cantidad", typeof(int));
            tablaDetalles.Columns.Add("precio_unitario", typeof(decimal));
            tablaDetalles.Columns.Add("comentario", typeof(string));

            foreach (var item in venta.DetalleVenta)
            {
                tablaDetalles.Rows.Add(item.PlatilloID, item.Cantidad, item.PrecioUnitario, (object)item.Comentario ?? DBNull.Value);
            }

            using var con = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("sp_RegistrarVentaCompleta_QR", con);
            cmd.CommandType = CommandType.StoredProcedure;

            // 2. Parámetros de la cabecera
            cmd.Parameters.AddWithValue("@mesaID", venta.MesaID);
            cmd.Parameters.AddWithValue("@sesionID", (object)venta.SesionID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@usuarioID", (object)venta.UsuarioID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@clienteID", (object)venta.ClienteID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@total", venta.Total);

            // 3. El parámetro de tabla (UDTT) 🔑
            var parametroLista = cmd.Parameters.AddWithValue("@detalles", tablaDetalles);
            parametroLista.SqlDbType = SqlDbType.Structured;
            parametroLista.TypeName = "dbo.DetalleVentaType";

            try
            {
                con.Open();
                // ExecuteScalar devuelve el ID de la venta que genera el SP
                object result = cmd.ExecuteScalar();
                return (result != null) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                // Aquí capturamos errores de SQL (como el RAISERROR de stock insuficiente)
                throw new Exception("Error en la base de datos al registrar la venta completa.", ex);
            }
        }
        // Dentro de VentaDatos.cs

        public Venta SeleccionarVentaConDetalle(int idVenta)
        {
            Venta venta = null;

            // Asegúrate de usar el nuevo nombre del SP
            using SqlConnection con = new(_cadenaConexion);
            using SqlCommand cmd = new("sp_SeleccionarVentaConDetalle_Restaurante", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@VentaId", idVenta);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            // 1. Leer el primer conjunto de resultados (La Cabecera de la Venta)
            if (dr.Read())
            {
                venta = new Venta
                {
                    VentaID = dr.GetInt32(dr.GetOrdinal("ventaID")),
                    UsuarioID = dr.IsDBNull(dr.GetOrdinal("usuarioID")) ? (int?)null : dr.GetInt32(dr.GetOrdinal("usuarioID")),
                    MesaID = dr.GetInt32(dr.GetOrdinal("mesaID")),

                    // ClienteID puede ser nulo, se maneja de forma segura
                    ClienteID = dr.IsDBNull(dr.GetOrdinal("clienteID")) ? (int?)null : dr.GetInt32(dr.GetOrdinal("clienteID")),

                    Fecha = dr.GetDateTime(dr.GetOrdinal("fecha_venta")),
                    Total = dr.GetDecimal(dr.GetOrdinal("total")),
                    Estado = dr.GetString(dr.GetOrdinal("estado")),
                    
                    SesionID = dr.IsDBNull(dr.GetOrdinal("sesionID")) ? (int?)null : dr.GetInt32(dr.GetOrdinal("sesionID")),
                    TipoPedido = dr.IsDBNull(dr.GetOrdinal("tipoPedido")) ? null : dr.GetString(dr.GetOrdinal("tipoPedido")),

                    DetalleVenta = new List<DetalleVenta>()
                };
            }

            if (venta == null) return null;

            // 2. Moverse al segundo conjunto de resultados (Los Detalles)
            dr.NextResult();

            // 3. Leer los detalles del platillo
            while (dr.Read())
            {
                // Reemplaza DetalleID por DetalleVentaID para que coincida con la definición de la clase DetalleVenta
                venta.DetalleVenta.Add(new DetalleVenta
                {
                    DetalleVentaID = dr.GetInt32(dr.GetOrdinal("detalleID")), // Nuevo nombre
                    VentaID = idVenta,
                    PlatilloID = dr.GetInt32(dr.GetOrdinal("platilloID")), // Nuevo nombre
                    NombreProducto = dr.GetString(dr.GetOrdinal("NombrePlatillo")),
                    Cantidad = dr.IsDBNull(dr.GetOrdinal("cantidad")) ? 0 : dr.GetInt32(dr.GetOrdinal("cantidad")),
                    PrecioUnitario = dr.GetDecimal(dr.GetOrdinal("precio_unitario")),
                    Subtotal = dr.IsDBNull(dr.GetOrdinal("subtotal")) ? 0 : dr.GetDecimal(dr.GetOrdinal("subtotal")),
                    Comentario = dr.IsDBNull(dr.GetOrdinal("comentario")) ? null : dr.GetString(dr.GetOrdinal("comentario")),
                    EstadoCocinero = dr.IsDBNull(dr.GetOrdinal("estadoCocinero")) ? null : dr.GetString(dr.GetOrdinal("estadoCocinero"))
                });
            }

            return venta;
        }

        public List<VentaListaDTO> LeerTodas()
        {
            var ventas = new List<VentaListaDTO>();

            using (SqlConnection con = new(_cadenaConexion))
            {
                con.Open();
                using (SqlCommand cmd = new("sp_ReadVentas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            int fechaVentaOrdinal = -1;
                            try
                            {
                                fechaVentaOrdinal = dr.GetOrdinal("fechaVenta");
                            }
                            catch (IndexOutOfRangeException)
                            {
                                fechaVentaOrdinal = dr.GetOrdinal("fecha_venta");
                            }

                            int ventaIDOrdinal = dr.GetOrdinal("ventaID");
                            int usuarioIDOrdinal = dr.GetOrdinal("usuarioID");
                            int nombreCajeroOrdinal = dr.GetOrdinal("nombreCajero");
                            int totalOrdinal = dr.GetOrdinal("total");
                            int estadoOrdinal = dr.GetOrdinal("estado");
                            int mesaIDOrdinal = dr.GetOrdinal("mesaID");

                            while (dr.Read())
                            {
                                ventas.Add(new VentaListaDTO
                                {
                                    VentaID = dr.GetInt32(ventaIDOrdinal),
                                    UsuarioID = dr.IsDBNull(usuarioIDOrdinal) ? (int?)null : dr.GetInt32(usuarioIDOrdinal),
                                    NombreCajero = dr.IsDBNull(nombreCajeroOrdinal) ? null : dr.GetString(nombreCajeroOrdinal),
                                    FechaVenta = dr.GetDateTime(fechaVentaOrdinal),
                                    Total = dr.GetDecimal(totalOrdinal),
                                    Estado = dr.GetString(estadoOrdinal),
                                    MesaID = dr.GetInt32(mesaIDOrdinal)
                                });
                            }
                        }
                    }
                }
            }
            return ventas;
        }

        public int RegistrarVenta(VentaListaDTO venta)
        {
            
            throw new NotImplementedException("RegistrarVenta aún no está implementado.");
        }

        public decimal ObtenerTotalSesion(int sesionId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // Sumamos la columna 'total' de todas las ventas de esta sesión 💰
                string query = "SELECT ISNULL(SUM(total), 0) FROM Ventas WHERE sesionID = @sesionId";

                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@sesionId", sesionId);

                conexion.Open();
                // Usamos ExecuteScalar porque solo esperamos un único valor numérico
                return (decimal)comando.ExecuteScalar();
            }
        }

        public List<PedidoAgrupadoDTO> ListarPedidosAgrupados()
        {
            List<PedidoAgrupadoDTO> lista = new List<PedidoAgrupadoDTO>();

            // Usamos la conexión a tu base de datos RestauranteDB
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ConsultarPedidosCocinaAgrupados", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new PedidoAgrupadoDTO()
                            {
                                NombrePlatillo = dr["NombrePlatillo"].ToString(),
                                CantidadTotal = Convert.ToInt32(dr["CantidadTotal"]),
                                FechaPrimerPedido = Convert.ToDateTime(dr["FechaPrimerPedido"]),
                                IdsRelacionados = dr["IdsRelacionados"].ToString(),
                                NumerosMesas = dr["NumerosMesas"].ToString(),
                                Estado = dr["Estado"].ToString(),
                                Comentarios = dr["Comentarios"] == DBNull.Value ? string.Empty : dr["Comentarios"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Es importante manejar el error para la trazabilidad
                    lista = new List<PedidoAgrupadoDTO>();
                }
            }
            return lista;
        }

        public bool ActualizarEstadoMasivo(string ids, string nuevoEstado)
        {
            bool respuesta = false;

            using (SqlConnection oconexion = new SqlConnection(_cadenaConexion))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ActualizarEstadoPedidosAgrupados", oconexion);
                    // Pasamos los parámetros que espera el SP
                    cmd.Parameters.AddWithValue("IdsPedidos", ids);
                    cmd.Parameters.AddWithValue("NuevoEstado", nuevoEstado);
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    // ExecuteNonQuery devuelve el número de filas afectadas
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        respuesta = true;
                    }
                }
                catch (Exception ex)
                {
                    // Aquí podrías loguear el error para depuración
                    respuesta = false;
                    throw new Exception("Error al actualizar los estados en la base de datos.", ex);
                }
            }
            return respuesta;
        }
        public bool ActualizarEstadoVenta(int ventaId, string nuevoEstado)
        {
            using (SqlConnection con = new(_cadenaConexion))
            {
                string query = "UPDATE Ventas SET estado = @nuevoEstado WHERE ventaID = @ventaId";
                using (SqlCommand cmd = new(query, con))
                {
                    cmd.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
                    cmd.Parameters.AddWithValue("@ventaId", ventaId);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public int ObtenerVentaActivaPorMesa(int mesaId)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string query = "SELECT TOP 1 ventaID FROM Ventas WHERE mesaID = @mesaId AND estado = 'Pendiente' ORDER BY ventaID DESC";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@mesaId", mesaId);

                conexion.Open();
                var resultado = comando.ExecuteScalar();
                return resultado != null ? Convert.ToInt32(resultado) : 0;
            }
        }

        // =====================================================
        // Métodos analíticos para la App Móvil Gerencial
        // RF-MOV-DSH-01, RF-MOV-DSH-02, RF-MOV-BUS-02
        // =====================================================

        /// <summary>
        /// Calcula los KPIs del dashboard para la fecha indicada.
        /// Cubre RF-MOV-DSH-01.
        /// </summary>
        public DashboardKpiDTO ObtenerKPIs(DateTime fecha)
        {
            var kpi = new DashboardKpiDTO { FechaConsulta = DateTime.Now };
            DateTime inicioDia = fecha.Date;
            DateTime finDia = inicioDia.AddDays(1).AddSeconds(-1);
            DateTime inicioSemana = inicioDia.AddDays(-(int)inicioDia.DayOfWeek);
            DateTime inicioMes = new DateTime(fecha.Year, fecha.Month, 1);

            using var con = new SqlConnection(_cadenaConexion);
            con.Open();

            // KPIs del día
            string queryDia = @"
                SELECT 
                    ISNULL(SUM(total), 0)   AS TotalHoy,
                    COUNT(*)                AS CantidadOrdenes
                FROM Ventas
                WHERE estado = 'Pagada'
                  AND fecha_venta >= @inicioDia
                  AND fecha_venta <= @finDia";

            using (var cmd = new SqlCommand(queryDia, con))
            {
                cmd.Parameters.AddWithValue("@inicioDia", inicioDia);
                cmd.Parameters.AddWithValue("@finDia", finDia);
                using var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    kpi.TotalVentasHoy = dr.GetDecimal(0);
                    kpi.CantidadOrdenesHoy = dr.GetInt32(1);
                    kpi.TicketPromedio = kpi.CantidadOrdenesHoy > 0
                        ? Math.Round(kpi.TotalVentasHoy / kpi.CantidadOrdenesHoy, 2)
                        : 0;
                }
            }

            // Total semana
            string querySemana = @"
                SELECT ISNULL(SUM(total), 0)
                FROM Ventas
                WHERE estado = 'Pagada'
                  AND fecha_venta >= @inicioSemana
                  AND fecha_venta <= @finDia";
            using (var cmd = new SqlCommand(querySemana, con))
            {
                cmd.Parameters.AddWithValue("@inicioSemana", inicioSemana);
                cmd.Parameters.AddWithValue("@finDia", finDia);
                kpi.TotalVentasSemana = (decimal)(cmd.ExecuteScalar() ?? 0m);
            }

            // Total mes y margen
            string queryMes = @"
                SELECT ISNULL(SUM(total), 0)
                FROM Ventas
                WHERE estado = 'Pagada'
                  AND fecha_venta >= @inicioMes
                  AND fecha_venta <= @finDia";
            using (var cmd = new SqlCommand(queryMes, con))
            {
                cmd.Parameters.AddWithValue("@inicioMes", inicioMes);
                cmd.Parameters.AddWithValue("@finDia", finDia);
                kpi.TotalVentasMes = (decimal)(cmd.ExecuteScalar() ?? 0m);
            }

            // Margen estimado (30% sobre ventas del mes como aproximación, ajustar según lógica real)
            kpi.MargenGananciaAcumulado = Math.Round(kpi.TotalVentasMes * 0.30m, 2);

            return kpi;
        }

        /// <summary>
        /// Obtiene datos de tendencia de ventas agrupados por período.
        /// Cubre RF-MOV-DSH-02.
        /// </summary>
        public List<TendenciaVentasDTO> ObtenerTendenciaVentas(string periodo)
        {
            var lista = new List<TendenciaVentasDTO>();
            string query;

            switch (periodo?.ToLower())
            {
                case "semana":
                    // Últimas 8 semanas agrupadas
                    query = @"
                        SELECT 
                            DATEPART(YEAR, fecha_venta)  AS Anio,
                            DATEPART(WEEK, fecha_venta)  AS Semana,
                            MIN(CAST(fecha_venta AS DATE)) AS FechaInicio,
                            ISNULL(SUM(total), 0)        AS TotalVentas,
                            COUNT(*)                     AS CantidadOrdenes
                        FROM Ventas
                        WHERE estado = 'Pagada'
                          AND fecha_venta >= DATEADD(WEEK, -8, GETDATE())
                        GROUP BY DATEPART(YEAR, fecha_venta), DATEPART(WEEK, fecha_venta)
                        ORDER BY Anio, Semana";
                    break;
                case "mes":
                    // Últimos 12 meses agrupados
                    query = @"
                        SELECT 
                            DATEPART(YEAR, fecha_venta)  AS Anio,
                            DATEPART(MONTH, fecha_venta) AS Mes,
                            MIN(CAST(fecha_venta AS DATE)) AS FechaInicio,
                            ISNULL(SUM(total), 0)        AS TotalVentas,
                            COUNT(*)                     AS CantidadOrdenes
                        FROM Ventas
                        WHERE estado = 'Pagada'
                          AND fecha_venta >= DATEADD(MONTH, -12, GETDATE())
                        GROUP BY DATEPART(YEAR, fecha_venta), DATEPART(MONTH, fecha_venta)
                        ORDER BY Anio, Mes";
                    break;
                default: // "dia" o cualquier otro valor
                    // Últimos 7 días
                    query = @"
                        SELECT 
                            CAST(fecha_venta AS DATE)    AS FechaInicio,
                            CAST(fecha_venta AS DATE)    AS FechaRef,
                            ISNULL(SUM(total), 0)        AS TotalVentas,
                            COUNT(*)                     AS CantidadOrdenes
                        FROM Ventas
                        WHERE estado = 'Pagada'
                          AND fecha_venta >= DATEADD(DAY, -7, GETDATE())
                        GROUP BY CAST(fecha_venta AS DATE)
                        ORDER BY FechaInicio";
                    break;
            }

            using var con = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand(query, con);
            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var fecha = Convert.ToDateTime(dr["FechaInicio"]);
                var totalVentas = Convert.ToDecimal(dr["TotalVentas"]);
                var cantidad = Convert.ToInt32(dr["CantidadOrdenes"]);

                string etiqueta = periodo?.ToLower() switch
                {
                    "semana" => $"Sem {dr["Semana"]} ({fecha:dd/MM})",
                    "mes" => fecha.ToString("MMMM yyyy"),
                    _ => fecha.ToString("ddd dd/MM")
                };

                lista.Add(new TendenciaVentasDTO
                {
                    Etiqueta = etiqueta,
                    Fecha = fecha,
                    TotalVentas = totalVentas,
                    CantidadOrdenes = cantidad
                });
            }
            return lista;
        }

        /// <summary>
        /// Obtiene facturas filtradas con paginación.
        /// Cubre RF-MOV-BUS-02 y RF-MOV-MON-03.
        /// </summary>
        public FacturasPaginadasDTO ObtenerFacturasFiltradas(FiltroFacturasDTO filtro)
        {
            var resultado = new FacturasPaginadasDTO
            {
                PaginaActual = filtro.Pagina,
                TamanoPagina = filtro.TamanoPagina
            };

            // Construimos la query dinámicamente con los filtros opcionales
            var whereClausulas = new List<string> { "1=1" };

            if (filtro.FechaDesde.HasValue)
                whereClausulas.Add("v.fecha_venta >= @fechaDesde");
            if (filtro.FechaHasta.HasValue)
                whereClausulas.Add("v.fecha_venta <= @fechaHasta");
            if (filtro.MontoMinimo.HasValue)
                whereClausulas.Add("v.total >= @montoMinimo");
            if (filtro.MontoMaximo.HasValue)
                whereClausulas.Add("v.total <= @montoMaximo");
            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                whereClausulas.Add("v.estado = @estado");
            if (!string.IsNullOrWhiteSpace(filtro.MetodoPago))
                whereClausulas.Add("v.metodoPago = @metodoPago");

            string where = string.Join(" AND ", whereClausulas);
            int offset = (filtro.Pagina - 1) * filtro.TamanoPagina;

            string queryConteo = $@"
                SELECT COUNT(*)
                FROM Ventas v
                WHERE {where}";

            string queryData = $@"
                SELECT 
                    v.ventaID, v.fecha_venta AS fechaVenta, v.total, v.estado, 
                    v.usuarioID, u.nombre AS nombreCajero, v.mesaID
                FROM Ventas v
                LEFT JOIN Usuarios u ON v.usuarioID = u.usuarioID
                WHERE {where}
                ORDER BY v.fecha_venta DESC
                OFFSET @offset ROWS FETCH NEXT @tamanoPagina ROWS ONLY";

            using var con = new SqlConnection(_cadenaConexion);
            con.Open();

            // Conteo total
            using (var cmdCount = new SqlCommand(queryConteo, con))
            {
                AgregarParametrosFiltro(cmdCount, filtro);
                resultado.TotalRegistros = (int)cmdCount.ExecuteScalar();
            }

            // Datos paginados
            using var cmdData = new SqlCommand(queryData, con);
            AgregarParametrosFiltro(cmdData, filtro);
            cmdData.Parameters.AddWithValue("@offset", offset);
            cmdData.Parameters.AddWithValue("@tamanoPagina", filtro.TamanoPagina);

            using var dr = cmdData.ExecuteReader();
            while (dr.Read())
            {
                resultado.Facturas.Add(new VentaListaDTO
                {
                    VentaID = dr.GetInt32(dr.GetOrdinal("ventaID")),
                    FechaVenta = dr.GetDateTime(dr.GetOrdinal("fechaVenta")),
                    Total = dr.GetDecimal(dr.GetOrdinal("total")),
                    Estado = dr.GetString(dr.GetOrdinal("estado")),
                    UsuarioID = dr.IsDBNull(dr.GetOrdinal("usuarioID")) ? null : dr.GetInt32(dr.GetOrdinal("usuarioID")),
                    NombreCajero = dr.IsDBNull(dr.GetOrdinal("nombreCajero")) ? null : dr.GetString(dr.GetOrdinal("nombreCajero")),
                    MesaID = dr.GetInt32(dr.GetOrdinal("mesaID"))
                });
            }

            return resultado;
        }

        /// <summary>Agrega los parámetros de filtro a un SqlCommand.</summary>
        private static void AgregarParametrosFiltro(SqlCommand cmd, FiltroFacturasDTO filtro)
        {
            if (filtro.FechaDesde.HasValue)
                cmd.Parameters.AddWithValue("@fechaDesde", filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                cmd.Parameters.AddWithValue("@fechaHasta", filtro.FechaHasta.Value.Date.AddDays(1).AddSeconds(-1));
            if (filtro.MontoMinimo.HasValue)
                cmd.Parameters.AddWithValue("@montoMinimo", filtro.MontoMinimo.Value);
            if (filtro.MontoMaximo.HasValue)
                cmd.Parameters.AddWithValue("@montoMaximo", filtro.MontoMaximo.Value);
            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                cmd.Parameters.AddWithValue("@estado", filtro.Estado);
            if (!string.IsNullOrWhiteSpace(filtro.MetodoPago))
                cmd.Parameters.AddWithValue("@metodoPago", filtro.MetodoPago);
        }
    }
}