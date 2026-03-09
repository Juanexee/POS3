// Archivo: NEGOCIO/CompraNegocio.cs
using System;
using System.Collections.Generic;
using System.Linq; // Necesario para la función Sum()
using DATOS;
using ENTIDADES.CompraDTO;

namespace NEGOCIO
{
    public class CompraNegocio
    {
        private readonly CompraDatos _compraDatos;

        // Asume que CompraDatos ya fue inyectado con la cadena de conexión
        public CompraNegocio(CompraDatos compraDatos)
        {
            _compraDatos = compraDatos;
        }

        public int RegistrarNuevaCompra(CompraMaestroDTO compra)
        {
            // 1. Validaciones de Negocio
            if (compra.ProveedorID <= 0)
            {
                throw new ArgumentException("El Proveedor es obligatorio.");
            }
            if (compra.Detalles == null || !compra.Detalles.Any())
            {
                throw new ArgumentException("Una compra debe tener al menos un insumo en el detalle.");
            }

            // 2. Calcular el Total de la Compra
            decimal totalCalculado = 0;
            foreach (var detalle in compra.Detalles)
            {
                if (detalle.Cantidad <= 0 || detalle.PrecioUnitario <= 0)
                {
                    throw new ArgumentException("La cantidad y el precio unitario deben ser mayores a cero en todos los detalles.");
                }

                // Cálculo del subtotal (Cantidad * Precio)
                totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;
            }

            // Asignar el total calculado al DTO Maestro antes de pasarlo a la BD
            compra.Total = totalCalculado;

            // 3. Llamar a la Capa de Datos (maneja la transacción SQL)
            return _compraDatos.InsertarCompra(compra);
        }
    }
}