using System;
using System.Collections.Generic;
using DATOS;
using ENTIDADES;

using ENTIDADES.InsumosDTO;

namespace NEGOCIO
{
    public class InsumoNegocio
    {
        private readonly InsumoDatos _insumoDatos;
        // Opcional: Podrías inyectar UnidadMedidaDatos si necesitaras validar si la UnidadID existe realmente

        public InsumoNegocio(InsumoDatos insumoDatos)
        {
            _insumoDatos = insumoDatos;
        }

        // --- MÉTODOS DE LECTURA ---

        public List<Insumo> LeerTodos()
        {
            // La lógica de negocio aquí es mínima, solo llama a la capa de datos.
            return _insumoDatos.LeerTodos();
        }

        // --- MÉTODOS DE ESCRITURA ---

        public int Insertar(CrearActualizarInsumoDTO insumo)
        {
            // 1. VALIDACIONES DE NEGOCIO

            // a) Validar que el nombre sea obligatorio
            if (string.IsNullOrWhiteSpace(insumo.Nombre))
            {
                throw new ArgumentException("El nombre del insumo es obligatorio.");
            }

            // b) Validar la longitud máxima del nombre (según tu BD, NVARCHAR(100))
            if (insumo.Nombre.Length > 100)
            {
                throw new ArgumentException("El nombre del insumo no puede exceder los 100 caracteres.");
            }

            // c) Validar la unidad de medida (debe ser un ID válido)
            if (insumo.UnidadID <= 0)
            {
                throw new ArgumentException("Debe seleccionar una Unidad de Medida válida para el insumo.");
            }

            // d) Validación de descripción (opcional, si es null, la BD lo acepta)
            if (insumo.Descripcion != null && insumo.Descripcion.Length > 250)
            {
                throw new ArgumentException("La descripción del insumo no puede exceder los 250 caracteres.");
            }

            // 2. LLAMADA A LA CAPA DE DATOS
            // Si todas las validaciones pasan, se llama a la Capa de Datos
            int nuevoID = _insumoDatos.Insertar(insumo);
            return nuevoID;
        }

        // Aquí se agregarían los métodos de Actualizar, Desactivar, etc.
    }
}