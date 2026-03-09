// Archivo: NEGOCIO/ProveedorNegocio.cs
using System;
using System.Collections.Generic;
using DATOS;
using ENTIDADES;
using ENTIDADES.ProveedorDTO;

namespace NEGOCIO
{
    public class ProveedorNegocio
    {
        private readonly ProveedorDatos _proveedorDatos;

        public ProveedorNegocio(ProveedorDatos proveedorDatos)
        {
            _proveedorDatos = proveedorDatos;
        }

        public List<Proveedor> LeerTodos()
        {
            return _proveedorDatos.LeerTodos();
        }

        public int Insertar(CrearProveedorDTO proveedor)
        {
            // 1. VALIDACIONES DE NEGOCIO
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
            {
                throw new ArgumentException("El nombre del proveedor es obligatorio.");
            }

            if (proveedor.Nombre.Length > 100)
            {
                throw new ArgumentException("El nombre del proveedor no puede exceder los 100 caracteres.");
            }

            // 2. LLAMADA A LA CAPA DE DATOS
            return _proveedorDatos.Insertar(proveedor);
        }
    }
}