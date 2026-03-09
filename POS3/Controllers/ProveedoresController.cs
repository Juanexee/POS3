// Archivo: Controllers/ProveedoresController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NEGOCIO;
using ENTIDADES.ProveedorDTO;
using System;

namespace RestauranteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedoresController : ControllerBase
    {
        private readonly ProveedorNegocio _proveedorNegocio;

        public ProveedoresController(ProveedorNegocio proveedorNegocio)
        {
            _proveedorNegocio = proveedorNegocio;
        }


        /// <summary>
        /// Ver los proveedores insertados
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Devuelve una lista de todo los proveedores regristrado</response>
        /// <response code="500">Fallo en el servidor </response>



        // GET: api/Proveedores (Lectura pública o para usuarios logueados)
        [HttpGet]
        [AllowAnonymous] // Permitir a cualquier usuario autenticado ver la lista de proveedores
        public IActionResult ObtenerProveedores()
        {
            try
            {
                var proveedores = _proveedorNegocio.LeerTodos();
                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la lista de proveedores.", detail = ex.Message });
            }
        }


        /// <summary>
        /// Insertar un nuevo proveedor     
        /// </summary>
        /// <param name="proveedor"></param>
        /// <returns></returns>
        /// <response code="201"> El nuevo proveedor se creo con exito</response>
        /// <response code="500">Fallo en el servidor</response>




        // POST: api/Proveedores (Creación restringida)
        [HttpPost]
        //[Authorize(Roles = "Administrador")] // Solo el administrador puede registrar nuevos proveedores
        [AllowAnonymous]
        public IActionResult CrearProveedor([FromBody] CrearProveedorDTO proveedor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int nuevoID = _proveedorNegocio.Insertar(proveedor);
                return CreatedAtAction(nameof(CrearProveedor), new { id = nuevoID }, new { message = $"Proveedor '{proveedor.Nombre}' creado exitosamente con ID {nuevoID}." });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor al crear el proveedor.", detail = ex.Message });
            }
        }
    }
}