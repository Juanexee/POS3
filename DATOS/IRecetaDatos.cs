using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATOS
{
    public interface IRecetaDatos
    {
        // Firma para la lectura
        List<ENTIDADES.RecetaDTO.RecetaDetalle> LeerPorPlatillo(int platilloID);

        // Firma para la gestión (guardar/actualizar)
        bool Gestionar(int platilloID, List<ENTIDADES.RecetaDTO.RecetaBaseDTO> detalles);

        // ... Agrega las firmas de otros métodos de RecetaDatos
    }
}