using System;

namespace ENTIDADES
{
    /// <summary>
    /// DTO con el estado completo del inventario de un insumo/materia prima.
    /// Cubre RF-MOV-MON-01 y RF-MOV-MON-02.
    /// </summary>
    public class InventarioResumenDTO
    {
        public int InsumoID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Existencia actual en stock.</summary>
        public decimal ExistenciaActual { get; set; }

        /// <summary>Umbral mínimo configurado (alerta si ExistenciaActual ≤ StockMinimo).</summary>
        public decimal StockMinimo { get; set; }

        /// <summary>Costo unitario del insumo para calcular valoración.</summary>
        public decimal CostoUnitario { get; set; }

        /// <summary>Valoración monetaria total del stock (ExistenciaActual × CostoUnitario).</summary>
        public decimal ValorTotal => Math.Round(ExistenciaActual * CostoUnitario, 2);

        /// <summary>true si la existencia está en nivel crítico (≤ StockMinimo).</summary>
        public bool EnAlerta => ExistenciaActual <= StockMinimo;

        /// <summary>Nombre de la unidad de medida (kg, litros, unidades, etc.).</summary>
        public string NombreUnidad { get; set; } = string.Empty;

        /// <summary>Abreviatura de la unidad.</summary>
        public string Abreviatura { get; set; } = string.Empty;
    }
}
