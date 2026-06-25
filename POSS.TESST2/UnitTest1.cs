using System;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Xunit;

namespace POSS.TESST2
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string connStr = "Server=DESKTOP-3AL47F6;Database=RestauranteDB;User Id=sa;Password=An1w0;Trusted_Connection=True;TrustServerCertificate=True";
            var sb = new StringBuilder();

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    
                    sb.AppendLine("sp_InsertarDetalleVenta_Transactional DEFINITION:");
                    sb.AppendLine("--------------------------------------------------------------------------------");
                    using (var cmd = new SqlCommand("SELECT definition FROM sys.sql_modules WHERE object_id = OBJECT_ID('sp_InsertarDetalleVenta_Transactional')", conn))
                    {
                        sb.AppendLine(cmd.ExecuteScalar()?.ToString() ?? "Not found");
                    }
                    sb.AppendLine("--------------------------------------------------------------------------------\n");

                    sb.AppendLine("DetalleVentaType COLUMNS:");
                    sb.AppendLine("--------------------------------------------------------------------------------");
                    string query = @"
                        SELECT c.name, t.name AS type 
                        FROM sys.table_types tt
                        INNER JOIN sys.columns c ON c.object_id = tt.type_table_object_id
                        INNER JOIN sys.types t ON t.system_type_id = c.system_type_id
                        WHERE tt.name = 'DetalleVentaType'";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sb.AppendLine($"{reader["name"]}: {reader["type"]}");
                        }
                    }
                    sb.AppendLine("--------------------------------------------------------------------------------\n");
                }
                Assert.Fail(sb.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail("Error: " + ex.Message + "\n" + ex.StackTrace + "\n" + sb.ToString());
            }
        }
    }
}