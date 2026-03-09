// Archivo: NEGOCIO/RecetaNegocio.cs
using System;
using System.Collections.Generic;
using DATOS;
using ENTIDADES.RecetaDTO;
using System.Linq;

namespace NEGOCIO
{
    public class RecetaNegocio
    {
        //private readonly RecetaDatos _recetaDatos;

        //public RecetaNegocio(RecetaDatos recetaDatos)
        //{
        //  _recetaDatos = recetaDatos;
        // }

        private readonly IRecetaDatos _recetaDatos; // Cambia el campo a la interfaz

        public RecetaNegocio(IRecetaDatos recetaDatos) // Cambia el parámetro a la interfaz
        {
            _recetaDatos = recetaDatos;
        }

        public List<RecetaDetalle> LeerPorPlatillo(int platilloID)
        {
            if (platilloID <= 0)
            {
                throw new ArgumentException("ID de platillo inválido.");
            }
            return _recetaDatos.LeerPorPlatillo(platilloID);
        }

        public bool GuardarReceta(int platilloID, List<RecetaBaseDTO> detalles)
        {
            // 1. VALIDACIONES DE NEGOCIO
            if (platilloID <= 0)
            {
                throw new ArgumentException("ID de platillo inválido para guardar la receta.");
            }

            if (detalles == null)
            {
                // Si detalles es null, asumimos que se quiere borrar la receta (pasar lista vacía)
                detalles = new List<RecetaBaseDTO>();
            }

            // Validar cada detalle
            foreach (var detalle in detalles)
            {
                if (detalle.InsumoID <= 0)
                {
                    throw new ArgumentException("El ID de Insumo es obligatorio en cada detalle de la receta.");
                }
                if (detalle.Cantidad <= 0)
                {
                    throw new ArgumentException($"La cantidad requerida para el Insumo ID {detalle.InsumoID} debe ser mayor que cero.");
                }
            }

            // 2. LLAMADA A LA CAPA DE DATOS
            try
            {
                return _recetaDatos.Gestionar(platilloID, detalles);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la receta. La operación fue revertida.", ex);
            }
        }
    }
}