using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENTIDADES;

namespace DATOS
{
    public interface IPlatillosDatos
    {
        void Insertar(PlatilloDTO platillo);
        List<Platillo> Leer();
        Platillo LeerPorId(int id);
        void Eliminar(int id, bool disponible);


    }
}
