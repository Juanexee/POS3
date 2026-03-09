using Xunit;
using Moq;
using NEGOCIO;
using DATOS; // Requerido para el tipo que vamos a mockear
using System;
using ENTIDADES.RecetaDTO;
using System.Collections.Generic;

namespace POSS.TESTS
{
    public class RecetaNegocioTests
    {
        // 1. Declaramos el objeto Mock (la simulación de la Capa de Datos)
        //private readonly Mock<RecetaDatos> _mockRecetaDatos;

        // 2. Declaramos la clase real que vamos a probar

        private readonly Mock<IRecetaDatos> _mockRecetaDatos; // <-- Usa la interfaz
        private readonly RecetaNegocio _recetaNegocio;

        public RecetaNegocioTests()
        {
            // Inicializar el Mock. 
            // Si RecetaDatos no tiene una interfaz, Moq intentará mockearla.
            // Le pasamos un argumento (null) si su constructor lo requiere (como tu cadena de conexión).
            _mockRecetaDatos = new Mock<IRecetaDatos>();

            // Instanciamos la clase de Negocio inyectando el objeto Mock
            _recetaNegocio = new RecetaNegocio(_mockRecetaDatos.Object);
        }

        // --- TEST CASE: ID de Platillo Inválido ---
        [Fact] // Atributo de xUnit
        public void LeerPorPlatillo_IDInvalido_LanzaArgumentException()
        {
            // ARRANGE (Preparación)
            int platilloIDInvalido = 0; // Un ID que el negocio debe rechazar

            // ACT (Actuación)

            // ASSERT (Verificación)
            // Queremos verificar que al ejecutar el método, se lanza una excepción.
            Assert.Throws<ArgumentException>(() =>
          _recetaNegocio.LeerPorPlatillo(platilloIDInvalido)
              );
        }

        [Fact]
        public void LeerPorPlatillo_IDValido_DevuelveReceta()
        {
            // ARRANGE (Preparación)
            int platilloIDValido = 1;

            // 1. Creación de los datos simulados que DEBERÍA devolver la capa de datos
            var recetaSimulada = new List<RecetaDetalle>
    {
        new RecetaDetalle { InsumoID = 7, Cantidad = 0.5M, NombreInsumo = "Pollo", PlatilloID = 1, NombreUnidad = "Kg" }
    };

            // 2. CONFIGURACIÓN DEL MOCK (Moq)
            // Cuando LLAMEN al método LeerPorPlatillo con cualquier entero (It.IsAny<int>()),
            // DEVUELVE (Returns) la lista de recetas simuladas.
            _mockRecetaDatos
                .Setup(r => r.LeerPorPlatillo(It.IsAny<int>()))
                .Returns(recetaSimulada);


            // ACT (Actuación)
            var resultado = _recetaNegocio.LeerPorPlatillo(platilloIDValido);

            // ASSERT (Verificación)
            // 1. Aseguramos que la lista devuelta no sea nula.
            Assert.NotNull(resultado);
            // 2. Aseguramos que la lista tenga la cantidad de elementos esperados (1).
            Assert.Single(resultado);
            // 3. Aseguramos que los datos devueltos son los correctos (ej. la Cantidad).
            Assert.Equal(0.5M, resultado[0].Cantidad);
        }

        [Fact]
        public void GuardarReceta_CantidadNegativa_LanzaArgumentException()
        {
            // ARRANGE (Preparación)
            int platilloID = 1;

            // 1. Crear una lista de detalles con una cantidad INVÁLIDA (negativa)
            var detallesInvalidos = new List<RecetaBaseDTO>
    {
        new RecetaBaseDTO { InsumoID = 7, Cantidad = -1.0M }
    };

            // ACT & ASSERT (Actuación y Verificación)
            // 2. Esperamos que la llamada a GuardarReceta lance la ArgumentException
            Assert.Throws<ArgumentException>(() =>
                _recetaNegocio.GuardarReceta(platilloID, detallesInvalidos)
            );

            // 3. Verificamos con Moq que la capa de datos NUNCA fue llamada 
            //    porque la lógica de negocio atrapó el error.
            _mockRecetaDatos.Verify(m =>
                m.Gestionar(It.IsAny<int>(), It.IsAny<List<RecetaBaseDTO>>()),
                Times.Never
            );
        }

        [Fact]
        public void GuardarReceta_DatosValidos_LlamaACapaDeDatosUnaVez()
        {
            // ARRANGE (Preparación)
            int platilloID = 1;
            var detallesValidos = new List<RecetaBaseDTO>
    {
        new RecetaBaseDTO { InsumoID = 7, Cantidad = 0.5M }
    };

            // 1. CONFIGURACIÓN DEL MOCK: 
            // Siempre que se llame al método Gestionar, devuelve 'true' (simulando éxito en la BD)
            _mockRecetaDatos
                .Setup(m => m.Gestionar(platilloID, detallesValidos))
                .Returns(true);

            // ACT (Actuación)
            bool exito = _recetaNegocio.GuardarReceta(platilloID, detallesValidos);

            // ASSERT (Verificación)
            // 1. Aseguramos que el método de negocio devolvió 'true' (éxito)
            Assert.True(exito);

            // 2. 💡 VERIFICACIÓN CRÍTICA DEL MOCK (Moq)
            // Confirmamos que el método Gestionar fue llamado EXACTAMENTE una vez (Times.Once).
            _mockRecetaDatos.Verify(m =>
                m.Gestionar(It.IsAny<int>(), It.IsAny<List<RecetaBaseDTO>>()),
                Times.Once
            );
        }

    }
}