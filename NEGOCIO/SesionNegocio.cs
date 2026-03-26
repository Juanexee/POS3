using DATOS;
using ENTIDADES;

namespace NEGOCIO
{
    public class SesionNegocio
    {
        private readonly SesionDatos _sesionDatos;

        public SesionNegocio(SesionDatos sesionDatos)
        {
            _sesionDatos = sesionDatos;
        }

        public int ObtenerOAbrirSesion(int mesaId)
        {
            // Regla: Si la mesa ya tiene una sesión activa, no creamos otra, devolvemos la misma.
            // (Esto evita duplicados si el cliente refresca el navegador del QR)

            // 1. Primero preguntamos: ¿Ya hay una sesión para esta mesa?
            int? sesionExistente = _sesionDatos.ObtenerSesionActiva(mesaId);

            if (sesionExistente.HasValue)
            {
                // 2. Si existe, la devolvemos tal cual
                return sesionExistente.Value;
            }

            // 3. Si no existe (es null), entonces procedemos a crear una nueva
            return _sesionDatos.AbrirSesion(mesaId);

          
        }
    }
}