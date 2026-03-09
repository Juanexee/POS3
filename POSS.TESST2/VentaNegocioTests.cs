using Xunit;
using Moq;
using NEGOCIO;
using DATOS; // Para IVentaDatos
using System;
using System.Collections.Generic;
using ENTIDADES; // Cambiado según la ubicación real de tu DTO

namespace POSS.TESTS
{
    // Debe ser una clase, no una interfaz
    public class VentaNegocioTests
    {
        // Declaramos el Mock de la interfaz de datos
        private readonly Mock<IVentaDatos> _mockVentaDatos;

        // Declaramos la clase real de negocio que vamos a probar
        private readonly VentaNegocio _ventaNegocio;

        public VentaNegocioTests()
        {
            // Inicializar el Mock de la interfaz
            _mockVentaDatos = new Mock<IVentaDatos>();

            // Instanciar la clase de Negocio, inyectando el objeto simulado
            _ventaNegocio = new VentaNegocio(_mockVentaDatos.Object);
        }

        // --- Aquí vamos a empezar a escribir los [Fact] ---

        [Fact]
        // Archivo: POSS.TESTS/VentaNegocioTests.cs (ACTUALIZADO)        
        public void GuardarVenta_CantidadNegativa_LanzaArgumentException()
        {
            // 1. ARRANGE (Preparación) 
            int idVentaEsperado = 5;

            // 1. Crear el DetalleVenta con la cantidad INVÁLIDA (negativa)
            var detallesInvalidos = new List<DetalleVenta>
            {
                new DetalleVenta
                {
                    PlatilloID = 1,
                    Cantidad = -1, // VALOR NO VÁLIDO
                    PrecioUnitario = 10.00M
                }
            };

            // 2. Crear el objeto Maestro (Venta) que contiene los detalles inválidos
            var ventaInvalida = new Venta
            {
                UsuarioID = 1,
                MesaID = 1,
                Total = 10.00M,
                DetalleVenta = detallesInvalidos
            };

            // 3. Crear una venta válida para la última línea del test
            var detallesValidos = new List<DetalleVenta>
            {
                new DetalleVenta
                {
                    PlatilloID = 1,
                    Cantidad = 1, // VALOR VÁLIDO
                    PrecioUnitario = 10.00M
                }
            };

            var ventaValida = new Venta
            {
                UsuarioID = 1,
                MesaID = 1,
                Total = 10.00M,
                DetalleVenta = detallesValidos
            };

            // ACT & ASSERT

            // 1. Verificamos que se lance la ArgumentException
            Assert.Throws<ArgumentException>(() =>
         _ventaNegocio.RegistrarVenta(ventaInvalida)
           );



            // 2. Verificamos que la capa de datos NUNCA fue llamada.
            _mockVentaDatos.Verify(m =>
             m.Insertar(It.IsAny<Venta>()),
           Times.Never);


        }

        [Fact]
        public void GuardarVenta_DatosValidos_LlamaCapaDatos()
        {
            // 1. ARRANGE (Preparación)
            int idVentaEsperado = 5;

            var detallesValidos = new List<DetalleVenta>
    {
        new DetalleVenta
        {
            PlatilloID = 1,
            Cantidad = 2,
            PrecioUnitario = 12.50M
        }
    };

            var ventaValida = new Venta
            {
                UsuarioID = 1,
                MesaID = 1,
                Total = 25.00M, // 2 * 12.50
                DetalleVenta = detallesValidos
            };

            // Configurar el Mock (STUBBING): Insertar debe devolver 5
            _mockVentaDatos.Setup(m => m.Insertar(It.IsAny<Venta>()))
                           .Returns(idVentaEsperado);

            // 2. ACT (Actuación)
            int resultadoID = _ventaNegocio.RegistrarVenta(ventaValida);

            // 3. ASSERT (Verificación)

            // ✅ Verificar 1: Que el ID devuelto por Negocio sea el ID simulado por Datos (5)
            Assert.Equal(idVentaEsperado, resultadoID);

            // ✅ Verificar 2: Que el método de la Capa de Datos Insertar() fue llamado exactamente una vez.
            _mockVentaDatos.Verify(m =>
                m.Insertar(It.IsAny<Venta>()),
                Times.Once);
        }

        [Fact]
        public void ObtenerVentaConDetalles_IDValido_DevuelveVenta()
        {
            // 1. ARRANGE (Preparación de datos simulados)
            int idVentaBuscada = 10;

            // Objeto que SIMULA la Venta DEVUELTA por la capa de datos
            var ventaEsperada = new Venta
            {
                VentaID = idVentaBuscada,
                UsuarioID = 1,
                MesaID = 2,
                Total = 50.00M,
                DetalleVenta = new List<DetalleVenta>
        {
            new DetalleVenta { PlatilloID = 1, Cantidad = 4, PrecioUnitario = 12.50M }
        }
            };

            // 2. STUBBING (Configurar el Mock)
            // Le decimos al Mock que cuando se llame a SeleccionarVentaConDetalle(10),
            // debe devolver el objeto ventaEsperada que acabamos de crear.
            _mockVentaDatos.Setup(m => m.SeleccionarVentaConDetalle(idVentaBuscada))
                           .Returns(ventaEsperada);

            // ACT y ASSERT...
            // 2. ACT (Actuación) 
            // Llamar al método de negocio con el ID
            Venta resultado = _ventaNegocio.ObtenerVentaConDetalles(idVentaBuscada);

            // 3. ASSERT (Verificación)

            // Verificar 1: Que el resultado devuelto no sea nulo.
            Assert.NotNull(resultado);

            // Verificar 2: Que el ID de la venta devuelta coincida con el ID buscado (10).
            Assert.Equal(idVentaBuscada, resultado.VentaID);

            // Verificar 3: Que la capa de datos fue llamada exactamente una vez.
            _mockVentaDatos.Verify(m =>
                m.SeleccionarVentaConDetalle(idVentaBuscada),
                Times.Once);
        }

        [Fact]
        public void ObtenerVentaConDetalles_IDInvalido_LanzaArgumentException()
        {
            // 1. ARRANGE (Preparación)
            // Usamos 0 o un número negativo para forzar la excepción de validación.
            int idVentaInvalido = 0;

            // 2. ACT & ASSERT (Verificación de la Excepción)

            // Verificamos que al llamar al método con el ID inválido, se lance la excepción esperada.
            Assert.Throws<ArgumentException>(() =>
                _ventaNegocio.ObtenerVentaConDetalles(idVentaInvalido)
            );

            // 3. ASSERT (Verificación del Mock)

            // Verificamos que la capa de datos NUNCA fue llamada, ya que la validación falló en Negocio.
            _mockVentaDatos.Verify(m =>
                m.SeleccionarVentaConDetalle(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public void ObtenerVentaConDetalles_IDExistente_DevuelveNullSiNoEncuentra()
        {
            // 1. ARRANGE (Preparación de datos simulados)
            int idVentaNoEncontrada = 999;

            // Configurar el Mock (STUBBING): La capa de datos debe devolver NULL
            // Usamos (Venta)null para asegurar que Moq entienda el tipo de retorno.
            _mockVentaDatos.Setup(m => m.SeleccionarVentaConDetalle(idVentaNoEncontrada))
                           .Returns((Venta)null);

            // 2. ACT (Actuación)
            // Llamar al método de negocio con el ID
            Venta resultado = _ventaNegocio.ObtenerVentaConDetalles(idVentaNoEncontrada);

            // 3. ASSERT (Verificación)

            // ✅ Verificar 1: Que el resultado devuelto por Negocio es NULO.
            Assert.Null(resultado);

            // ✅ Verificar 2: Que la capa de datos fue llamada exactamente una vez.
            _mockVentaDatos.Verify(m =>
                m.SeleccionarVentaConDetalle(idVentaNoEncontrada),
                Times.Once);
        }
    }
}