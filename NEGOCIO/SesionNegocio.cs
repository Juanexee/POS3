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

        public RespuestaProceso ProcesarCambioMesa(int sesionId, int nuevaMesaId)
        {
            // 1. Validar que la sesión esté realmente activa para ser movida
            string estadoActual = _sesionDatos.ObtenerEstadoSesion(sesionId);

            if (estadoActual != "Activa")
            {
                return new RespuestaProceso
                {
                    Success = false,
                    Message = $"No se puede mover la mesa. La sesión está en estado: {estadoActual}."
                };
            }

            // 2. Validar disponibilidad de la mesa de destino
            if (!EsMesaDisponible(nuevaMesaId))
            {
                return new RespuestaProceso { Success = false, Message = "La mesa de destino no está disponible." };
            }

            // 3. Ejecutar el cambio
            bool resultado = EjecutarCambioEnBaseDeDatos(sesionId, nuevaMesaId);

            return resultado
                ? new RespuestaProceso { Success = true, Message = "Cambio realizado con éxito." }
                : new RespuestaProceso { Success = false, Message = "Error técnico al mover la mesa." };
        }

        private bool EsMesaDisponible(int mesaId)
        {
            // Aquí deberías implementar la lógica real para verificar si la mesa está disponible.
            // Por ejemplo, podrías consultar la base de datos usando _sesionDatos.

            return _sesionDatos.EsMesaDisponible(mesaId);
        }

        private bool EjecutarCambioEnBaseDeDatos(int sesionId, int nuevaMesaId)
        {
            // Implementa aquí la lógica real para cambiar la mesa en la base de datos.
            // Por ahora, devolvemos true como ejemplo.
            return _sesionDatos.EjecutarCambioMesa(sesionId, nuevaMesaId);
        }
    }
}