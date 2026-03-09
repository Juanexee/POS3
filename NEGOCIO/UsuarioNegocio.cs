using System.Linq; // Necesario para SequenceEqual
using System.Security.Cryptography;
using System.Text;
using DATOS;
using ENTIDADES;
using Konscious.Security.Cryptography;
using static ENTIDADES.UsuarioDTO;





namespace NEGOCIO
{
    public class UsuarioNegocio
    {
        private readonly UsuariosDatos _usuariosDatos;

        public UsuarioNegocio(UsuariosDatos usuariosDatos)
        {
            _usuariosDatos = usuariosDatos;
        }

        // --- MÉTODOS DE SEGURIDAD (PASSWORD HASHING) ---

        // Función para generar el Salt
        public byte[] GenerarSalt(int tamaño = 16)
        {
            byte[] salt = new byte[tamaño];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // Genera la cadena combinada de Salt y Hash (usado en Creación)
        public string HashearPassword(string passwordPlana, byte[] salt) // Renombrado a HashearPassword
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passwordPlana)) // Usa 'passwordPlana'
            {
                Salt = salt,
                DegreeOfParallelism = 2,
                Iterations = 4,
                MemorySize = 1024
            };

            // Asegurar que el hash sea de 64 bytes para tu VARBINARY(64)
            var hashBytes = argon2.GetBytes(64);

            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hashBytes);
        }

        // Verifica si la Password plana coincide con el Hash almacenado (usado en Login)
        private bool VerificarPassword(string passwordPlana, byte[] saltAlmacenado, byte[] hashAlmacenado)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passwordPlana))
            {
                Salt = saltAlmacenado, // Usar el salt que viene de la BD
                DegreeOfParallelism = 2,
                Iterations = 4,
                MemorySize = 1024
            };

            // Generar el hash de la Password ingresada (de 64 bytes)
            var hashNuevo = argon2.GetBytes(64);

            // Comparar los dos arrays de bytes
            return hashNuevo.SequenceEqual(hashAlmacenado);
        }

        // --- MÉTODOS DE NEGOCIO ---

        // Metodo para Crear un nuevo usuario
        public void CrearUsuario(Usuario nuevoUsuario, string rolDelUsuario)
        {
            if (rolDelUsuario != "Administrador")
            {
                throw new UnauthorizedAccessException("No tienes permiso para crear usuarios.");
            }

            // Aquí asumimos que nuevoUsuario.Password contiene la contraseña plana
            // En tu código original, este campo se llamaba 'Clave'. 
            // ¡DEBES CORREGIR EL MODELO ENTIDADES.USUARIO!

            var passwordPlana = nuevoUsuario.Password; // Se debe llamar Password en ENTIDADES.Usuario
            var saltBytes = GenerarSalt();

            // Usamos la función de hasheo corregida
            var hashCompleto = HashearPassword(passwordPlana, saltBytes);

            // Dividir y asignar los bytes
            var partes = hashCompleto.Split('.');
            nuevoUsuario.PasswordSalt = Convert.FromBase64String(partes[0]);
            nuevoUsuario.PasswordHash = Convert.FromBase64String(partes[1]);

            _usuariosDatos.Insertar(nuevoUsuario);
        }
        // Metodo para Actualizar un usuario (NUEVO MÉTODO/ADAPTACIÓN)
        public void ActualizarUsuario(CrearUsuarioDTO usuarioAfectado, int usuarioModificacionID)
        {
            // 1. Validaciones de Negocio (Ejemplos)
            if (usuarioAfectado.UsuarioID <= 0)
            {
                throw new ArgumentException("El ID del usuario a modificar es inválido.");
            }
            if (usuarioModificacionID <= 0)
            {
                // Esto es una validación de seguridad crítica.
                throw new UnauthorizedAccessException("Se requiere el ID del usuario actual para la auditoría.");
            }

            // 2. Llamada a la Capa de Datos, pasando el ID del modificador.
            _usuariosDatos.Actualizar(usuarioAfectado, usuarioModificacionID);
        }

        public void DesactivarUsuario(int usuarioID, int usuarioModificacionID)
        {
            // 1. Validaciones de Negocio
            if (usuarioID <= 0)
            {
                throw new ArgumentException("El ID del usuario a desactivar es inválido.");
            }
            if (usuarioModificacionID <= 0)
            {
                throw new UnauthorizedAccessException("Se requiere el ID del usuario actual para la auditoría.");
            }

            // 2. Llamada a la Capa de Datos para la desactivación (activo = false)
            _usuariosDatos.Eliminar(usuarioID, false, usuarioModificacionID);
        }

        // Método para la Autenticación de usuarios
        public Usuario? Login(string nombreUsuario, string passwordPlana) // Renombrado a passwordPlana
        {
            // 1. Buscar al usuario
            var usuario = _usuariosDatos.LeerPorNombreUsuario(nombreUsuario);

            // 2. Si el usuario no existe o está inactivo
            if (usuario == null || !usuario.Activo) return null;

            // 3. Verificación de seguridad
            if (usuario.PasswordHash == null || usuario.PasswordSalt == null)
            {
                throw new InvalidOperationException("Datos de seguridad incompletos para el usuario.");
            }

            // 4. Utilizar la lógica de verificación de Password
            bool claveEsValida = VerificarPassword(
                passwordPlana,
                usuario.PasswordSalt,
                usuario.PasswordHash
            );

            // 5. Retornar el usuario si es válido, o null si no lo es
            return claveEsValida ? usuario : null;
        }
    }
}