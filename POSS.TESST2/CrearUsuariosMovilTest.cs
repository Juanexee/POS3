using System;
using DATOS;
using ENTIDADES;
using NEGOCIO;
using Xunit;

namespace POSS.TESST2
{
    public class CrearUsuariosMovilTest
    {
        private const string ConnectionString = "Server=DESKTOP-3AL47F6;Database=RestauranteDB;User Id=sa;Password=An1w0;Trusted_Connection=True;TrustServerCertificate=True";

        [Fact]
        public void RegistrarYVerificarUsuariosMovil()
        {
            var usuariosDatos = new UsuariosDatos(ConnectionString);
            var usuarioNegocio = new UsuarioNegocio(usuariosDatos);

            // Lista de usuarios a crear para la app móvil
            var usuariosACrear = new[]
            {
                new { Username = "admin_movil", Pass = "Admin123*", Nombre = "Administrador Móvil", Telefono = "88880001", RolID = 1 },
                new { Username = "gerente_movil", Pass = "Gerente123*", Nombre = "Gerente Analítica", Telefono = "88880002", RolID = 1004 },
                new { Username = "supervisor_movil", Pass = "Super123*", Nombre = "Supervisor Operativo", Telefono = "88880003", RolID = 1005 },
                new { Username = "mesero_movil", Pass = "Mesero123*", Nombre = "Mesero Salón", Telefono = "88880004", RolID = 3 }
            };

            foreach (var u in usuariosACrear)
            {
                var existente = usuariosDatos.LeerPorNombreUsuario(u.Username);
                if (existente == null)
                {
                    var nuevoUsuario = new Usuario
                    {
                        NombreUsuario = u.Username,
                        Password = u.Pass,
                        Nombre = u.Nombre,
                        Telefono = u.Telefono,
                        RolID = u.RolID
                    };

                    usuarioNegocio.CrearUsuario(nuevoUsuario, "Administrador");
                }

                // Verificar que el login funcione perfectamente
                var usuarioLogueado = usuarioNegocio.Login(u.Username, u.Pass);
                Assert.NotNull(usuarioLogueado);
                Assert.True(usuarioLogueado.Activo);
                Assert.Equal(u.Username, usuarioLogueado.NombreUsuario);
            }
        }
    }
}
