using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES.CompraDTO
{

    public class CompraMaestroDTO
    {

        // Corresponden a los parámetros del SP sp_InsertarCompra
        public int ProveedorID { get; set; }

        // Este campo será calculado en C#
        public decimal Total { get; set; }

        // La lista de todos los insumos comprados
        public List<DetalleCompraDTO> Detalles { get; set; } = new List<DetalleCompraDTO>();
    }
}