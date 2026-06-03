using DATOS;
using ENTIDADES;
using System;
using System.Collections.Generic;

namespace NEGOCIO
{
    public class RolNegocio
    {
        private readonly RolesDatos _rolesDatos;

        public RolNegocio(RolesDatos rolesDatos)
        {
            _rolesDatos = rolesDatos;
        }

        public List<Rol> ListarTodosLosRoles()
        {
            return _rolesDatos.Leer();
        }

        public RespuestaProceso GuardarNuevoRol(Rol nuevoRol)
        {
            if (string.IsNullOrWhiteSpace(nuevoRol.NombreRol))
            {
                return new RespuestaProceso { Success = false, Message = "El nombre del rol es obligatorio." };
            }

            try
            {
                _rolesDatos.Insertar(nuevoRol);
                return new RespuestaProceso { Success = true, Message = "Rol creado con éxito." };
            }
            catch (Exception ex)
            {
                return new RespuestaProceso { Success = false, Message = "Error al insertar el rol: " + ex.Message };
            }
        }

        // ---------- EDITAR / ACTUALIZAR CATEGORÍA ----------
        public RespuestaProceso ActualizarRol(Rol rolAEditar, int usuarioModificacionID)
        {
            if (rolAEditar.RolID <= 0)
            {
                return new RespuestaProceso { Success = false, Message = "El ID del rol no es válido." };
            }

            if (string.IsNullOrWhiteSpace(rolAEditar.NombreRol))
            {
                return new RespuestaProceso { Success = false, Message = "El nombre del rol no puede estar vacío." };
            }

            if (usuarioModificacionID <= 0)
            {
                return new RespuestaProceso { Success = false, Message = "Se requiere un ID de usuario válido para la auditoría de la modificación." };
            }

            try
            {
                // Enviamos el rol y el usuario que modifica a la capa de datos
                _rolesDatos.Actualizar(rolAEditar, usuarioModificacionID);
                return new RespuestaProceso { Success = true, Message = "Rol actualizado correctamente." };
            }
            catch (Exception ex)
            {
                return new RespuestaProceso { Success = false, Message = "Error al actualizar el rol: " + ex.Message };
            }
        }

        public RespuestaProceso DesactivarRol(int rolId, int usuarioIDLogueado)
        {
            if (rolId <= 0)
            {
                return new RespuestaProceso { Success = false, Message = "ID de rol inválido." };
            }

            try
            {
                // Pasamos el ID, el estado falso (desactivado) y el ID de auditoría exigido por tus Datos
                _rolesDatos.Eliminar(rolId, false, usuarioIDLogueado);
                return new RespuestaProceso { Success = true, Message = "Rol desactivado correctamente." };
            }
            catch (Exception ex)
            {
                return new RespuestaProceso { Success = false, Message = "Error al desactivar el rol: " + ex.Message };
            }
        }
    }
}