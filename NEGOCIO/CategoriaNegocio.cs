using DATOS;
using ENTIDADES;
using System;
using System.Collections.Generic;

namespace NEGOCIO
{
    public class CategoriaNegocio
    {
        private readonly CategoriasDatos _categoriasDatos;

        public CategoriaNegocio(CategoriasDatos categoriasDatos)
        {
            _categoriasDatos = categoriasDatos;
        }

        public List<Categoria> ListarTodasLasCategorias()
        {
            return _categoriasDatos.LeerTodos();
        }

        public RespuestaProceso GuardarNuevaCategoria(CategoriaDTO nuevaCategoria)
        {
            if (string.IsNullOrWhiteSpace(nuevaCategoria.Nombre))
            {
                return new RespuestaProceso { Success = false, Message = "El nombre de la categoría es requerido." };
            }

            // Validación de nombres duplicados (RF1)
            bool existe = _categoriasDatos.LeerTodos().Any(c => c.Nombre.Trim().Equals(nuevaCategoria.Nombre.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existe)
            {
                return new RespuestaProceso { Success = false, Message = "Ya existe una categoría con este nombre." };
            }

            _categoriasDatos.Insertar(nuevaCategoria);
            return new RespuestaProceso { Success = true, Message = "Categoría creada con éxito." };
        }

        public RespuestaProceso EliminarCategoria(int id)
        {
            try
            {
                bool exito = _categoriasDatos.EliminarOIdesactivar(id);

                if (exito)
                {
                    return new RespuestaProceso { Success = true, Message = "La categoría ha sido procesada correctamente (eliminada o desactivada por seguridad)." };
                }

                return new RespuestaProceso { Success = false, Message = "La categoría solicitada no existe." };
            }
            catch (Exception ex)
            {
                return new RespuestaProceso { Success = false, Message = "Error en el procesamiento de negocio: " + ex.Message };
            }
        }

        public RespuestaProceso ActivarCategoria(int id)
        {
            try
            {
                bool exito = _categoriasDatos.Activar(id);

                if (exito)
                {
                    return new RespuestaProceso { Success = true, Message = "La categoría ha sido activada correctamente." };
                }

                return new RespuestaProceso { Success = false, Message = "La categoría solicitada no existe." };
            }
            catch (Exception ex)
            {
                return new RespuestaProceso { Success = false, Message = "Error en el procesamiento de negocio: " + ex.Message };
            }
        }

        // ---------- EDITAR / ACTUALIZAR CATEGORÍA ----------
        public RespuestaProceso ModificarCategoria(int id, CategoriaDTO categoriaDto)
        {
            try
            {
                if (id <= 0)
                {
                    return new RespuestaProceso { Success = false, Message = "El ID de la categoría no es válido." };
                }

                if (string.IsNullOrWhiteSpace(categoriaDto.Nombre))
                {
                    return new RespuestaProceso { Success = false, Message = "El nombre de la categoría no puede estar vacío." };
                }

                // Validación de nombres duplicados (RF1) - ignoramos el ID propio
                bool existe = _categoriasDatos.LeerTodos().Any(c => c.Nombre.Trim().Equals(categoriaDto.Nombre.Trim(), StringComparison.OrdinalIgnoreCase) && c.CategoriaID != id);
                if (existe)
                {
                    return new RespuestaProceso { Success = false, Message = "Ya existe otra categoría con este nombre." };
                }

                // Ejecutamos la actualización en la capa de datos
                _categoriasDatos.Actualizar(id, categoriaDto);

                return new RespuestaProceso { Success = true, Message = "Categoría actualizada correctamente." };
            }
            catch (Exception ex)
            {
                return new RespuestaProceso { Success = false, Message = "Error al actualizar la categoría: " + ex.Message };
            }
        }
    }
}