using DATOS;
using ENTIDADES;
using System.Collections.Generic;

namespace NEGOCIO
{
    public class PlatilloNegocio
    {
        private readonly IPlatillosDatos _platillosDatos;

        // Inyectamos la interfaz de datos
        public PlatilloNegocio(IPlatillosDatos platillosDatos)
        {
            _platillosDatos = platillosDatos;
        }

        public List<Platillo> ListarTodosLosPlatillos()
        {
            return _platillosDatos.Leer();
        }

        public Platillo ObtenerPlatilloPorId(int id)
        {
            return _platillosDatos.LeerPorId(id);
        }

        public RespuestaProceso RegistrarPlatillo(PlatilloDTO nuevoPlatillo)
        {
            if (string.IsNullOrWhiteSpace(nuevoPlatillo.Nombre))
            {
                return new RespuestaProceso { Success = false, Message = "El nombre del platillo es obligatorio." };
            }

            if (nuevoPlatillo.Precio <= 0)
            {
                return new RespuestaProceso { Success = false, Message = "El precio del platillo debe ser mayor a 0." };
            }

            _platillosDatos.Insertar(nuevoPlatillo);
            return new RespuestaProceso { Success = true, Message = "Platillo insertado correctamente." };
        }

        public void CambiarDisponibilidad(int id, bool disponible)
        {
            _platillosDatos.Eliminar(id, disponible);
        }
    }
}