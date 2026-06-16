using DATOS;
using ENTIDADES;
using System;
using System.Collections.Generic;

namespace NEGOCIO
{
    public class MesaNegocio
    {
        private readonly MesaDatos _mesaDatos;

        // Inyectamos la capa de datos
        public MesaNegocio(MesaDatos mesaDatos)
        {
            _mesaDatos = mesaDatos;
        }

        public List<Mesa> ListarTodasLasMesas()
        {
            return _mesaDatos.ListarMesas();
        }

        public RespuestaProceso GuardarNuevaMesa(Mesa nuevaMesa)
        {
            // 1. Validaciones de Negocio
            if (nuevaMesa.NumeroMesa <= 0)
            {
                return new RespuestaProceso { Success = false, Message = "El número de mesa debe ser mayor a 0." };
            }

            if (nuevaMesa.Capacidad <= 0)
            {
                return new RespuestaProceso { Success = false, Message = "La capacidad debe ser al menos para 1 persona." };
            }

            // 2. Verificar si el número de mesa ya existe para evitar duplicados
            var mesasExistentes = _mesaDatos.ListarMesas();
            if (mesasExistentes.Exists(m => m.NumeroMesa == nuevaMesa.NumeroMesa))
            {
                return new RespuestaProceso { Success = false, Message = $"La mesa número {nuevaMesa.NumeroMesa} ya está registrada." };
            }

            // 3. Si todo está bien, mandamos a insertar
            bool inserto = _mesaDatos.InsertarMesa(nuevaMesa.NumeroMesa, nuevaMesa.Capacidad, nuevaMesa.Ubicacion);

            return inserto
                ? new RespuestaProceso { Success = true, Message = "Mesa creada exitosamente." }
                : new RespuestaProceso { Success = false, Message = "Error técnico al intentar registrar la mesa." };
        }

        public RespuestaProceso EditarMesa(Mesa mesaEditada)
        {
            // Validamos que la mesa exista
            if (mesaEditada.MesaID <= 0)
            {
                return new RespuestaProceso { Success = false, Message = "ID de mesa no válido para actualizar." };
            }

            bool actualizo = _mesaDatos.ActualizarMesa(
                mesaEditada.MesaID,
                mesaEditada.NumeroMesa,
                mesaEditada.Capacidad,
                mesaEditada.Ubicacion
            );

            return actualizo
                ? new RespuestaProceso { Success = true, Message = "Mesa actualizada correctamente." }
                : new RespuestaProceso { Success = false, Message = "No se realizaron cambios o la mesa no existe." };
        }

        public RespuestaProceso DarDeBajaMesa(int mesaId)
        {
            // Regla de negocio: No se puede eliminar una mesa si está ocupada
            var mesas = _mesaDatos.ListarMesas();
            var mesa = mesas.Find(m => m.MesaID == mesaId);

            if (mesa == null)
            {
                return new RespuestaProceso { Success = false, Message = "La mesa no existe o ya está inactiva." };
            }

            if (mesa.SesionID.HasValue)
            {
                return new RespuestaProceso { Success = false, Message = "No se puede dar de baja una mesa con una sesión activa o que esté ocupada." };
            }

            bool elimino = _mesaDatos.EliminarMesa(mesaId);

            return elimino
                ? new RespuestaProceso { Success = true, Message = "Mesa eliminada del sistema." }
                : new RespuestaProceso { Success = false, Message = "No se pudo eliminar la mesa." };
        }
    }
}