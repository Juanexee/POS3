// Archivo: NEGOCIO/UnidadMedidaNegocio.cs
using System;
using System.Collections.Generic;
using DATOS;
using ENTIDADES;
using ENTIDADES.UnidadMedidaDTO;

namespace NEGOCIO
{
    public class UnidadMedidaNegocio
    {
        private readonly UnidadMedidaDatos _unidadMedidaDatos;

        public UnidadMedidaNegocio(UnidadMedidaDatos unidadMedidaDatos)
        {
            _unidadMedidaDatos = unidadMedidaDatos;
        }

        public List<UnidadMedida> LeerTodos()
        {
            return _unidadMedidaDatos.LeerTodos();
        }

        public int Insertar(CrearUnidadMedidaDTO unidad)
        {
            // 1. VALIDACIONES DE NEGOCIO
            if (string.IsNullOrWhiteSpace(unidad.Nombre) || string.IsNullOrWhiteSpace(unidad.Abreviatura))
            {
                throw new ArgumentException("El nombre y la abreviatura de la unidad de medida son obligatorios.");
            }

            if (unidad.Nombre.Length > 50)
            {
                throw new ArgumentException("El nombre no puede exceder los 50 caracteres.");
            }

            if (unidad.Abreviatura.Length > 10)
            {
                throw new ArgumentException("La abreviatura no puede exceder los 10 caracteres.");
            }

            // 2. LLAMADA A LA CAPA DE DATOS
            return _unidadMedidaDatos.Insertar(unidad);
        }
    }
}