//y en la que queda de recetas pones este código using System.Collections.Generic;

namespace ENTIDADES.RecetaDTO
{
    // DTO para enviar la lista completa de ingredientes a guardar/actualizar
    public class RecetaBaseDTO
    {
        public int InsumoID { get; set; }
        public decimal Cantidad { get; set; } // Cantidad requerida de este insumo por platillo
    }

    // Entidad para la lectura (contiene información de insumo y unidad)
    public class RecetaDetalle : RecetaBaseDTO
    {
        public int RecetaID { get; set; }
        public int PlatilloID { get; set; }
        public string NombreInsumo { get; set; }
        public string NombreUnidad { get; set; }
        public string Abreviatura { get; set; }
    }

    // domingo 16-11-25
}