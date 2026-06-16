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
                    
                    sb.AppendLine("TABLE TYPES:");
                    sb.AppendLine("--------------------------------------------------------------------------------");
                    string query = @"
                        SELECT TABLE_NAME, TABLE_TYPE 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME IN ('Venta', 'Ventas')";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sb.AppendLine($"{reader["TABLE_NAME"]}: {reader["TABLE_TYPE"]}");
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