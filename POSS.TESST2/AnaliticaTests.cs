using Xunit;
using Moq;
using NEGOCIO;
using DATOS;
using ENTIDADES;
using System;
using System.Collections.Generic;

namespace POSS.TESTS
{
    /// <summary>
    /// Tests unitarios para AnaliticaNegocio.
    /// Cubre RF-MOV-DSH-01 (KPIs) y RF-MOV-DSH-02 (Tendencias).
    /// </summary>
    public class AnaliticaNegocioTests
    {
        private readonly Mock<IVentaDatos> _mockVentaDatos;
        private readonly AnaliticaNegocio _analiticaNegocio;

        public AnaliticaNegocioTests()
        {
            _mockVentaDatos  = new Mock<IVentaDatos>();
            _analiticaNegocio = new AnaliticaNegocio(_mockVentaDatos.Object);
        }

        // ─────────────────────────────────────────────────────────────
        // RF-MOV-DSH-01: ObtenerKPIsDashboard
        // ─────────────────────────────────────────────────────────────

        [Fact]
        public void ObtenerKPIsDashboard_Exitoso_DevuelveDTO()
        {
            // ARRANGE
            var kpiEsperado = new DashboardKpiDTO
            {
                TotalVentasHoy         = 1500.00m,
                CantidadOrdenesHoy     = 10,
                TicketPromedio         = 150.00m,
                TotalVentasSemana      = 7500.00m,
                TotalVentasMes         = 30000.00m,
                MargenGananciaAcumulado = 9000.00m,
                FechaConsulta          = DateTime.Today
            };

            _mockVentaDatos
                .Setup(m => m.ObtenerKPIs(It.IsAny<DateTime>()))
                .Returns(kpiEsperado);

            // ACT
            var resultado = _analiticaNegocio.ObtenerKPIsDashboard();

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(1500.00m, resultado.TotalVentasHoy);
            Assert.Equal(10, resultado.CantidadOrdenesHoy);
            _mockVentaDatos.Verify(m => m.ObtenerKPIs(It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public void ObtenerKPIsDashboard_SinVentas_DevuelveCerosEnKPIs()
        {
            // ARRANGE — Día sin ventas
            var kpiVacio = new DashboardKpiDTO
            {
                TotalVentasHoy     = 0m,
                CantidadOrdenesHoy = 0,
                TicketPromedio     = 0m,
                FechaConsulta      = DateTime.Today
            };

            _mockVentaDatos
                .Setup(m => m.ObtenerKPIs(It.IsAny<DateTime>()))
                .Returns(kpiVacio);

            // ACT
            var resultado = _analiticaNegocio.ObtenerKPIsDashboard();

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(0m, resultado.TotalVentasHoy);
            Assert.Equal(0, resultado.CantidadOrdenesHoy);
        }

        // ─────────────────────────────────────────────────────────────
        // RF-MOV-DSH-02: ObtenerTendenciaVentas
        // ─────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("dia")]
        [InlineData("semana")]
        [InlineData("mes")]
        public void ObtenerTendenciaVentas_PeriodosValidos_LlamaCapaDatos(string periodo)
        {
            // ARRANGE
            var tendencias = new List<TendenciaVentasDTO>
            {
                new TendenciaVentasDTO { Etiqueta = "Lun 01/09", TotalVentas = 500m, CantidadOrdenes = 5 }
            };

            _mockVentaDatos
                .Setup(m => m.ObtenerTendenciaVentas(periodo))
                .Returns(tendencias);

            // ACT
            var resultado = _analiticaNegocio.ObtenerTendenciaVentas(periodo);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Single(resultado);
            Assert.Equal(500m, resultado[0].TotalVentas);
        }

        [Fact]
        public void ObtenerTendenciaVentas_PeriodoInvalido_UsaValorPorDefecto()
        {
            // ARRANGE — Un período que no existe debería ser corregido a "dia"
            var tendencias = new List<TendenciaVentasDTO>
            {
                new TendenciaVentasDTO { Etiqueta = "Lun 01/09", TotalVentas = 300m }
            };

            // El stub responde solo al período "dia" (el valor seguro por defecto)
            _mockVentaDatos
                .Setup(m => m.ObtenerTendenciaVentas("dia"))
                .Returns(tendencias);

            // ACT — Se pasa un período inválido
            var resultado = _analiticaNegocio.ObtenerTendenciaVentas("quincenal");

            // ASSERT — Se corrige a "dia" internamente y devuelve datos
            Assert.NotNull(resultado);
            _mockVentaDatos.Verify(m => m.ObtenerTendenciaVentas("dia"), Times.Once);
        }

        [Fact]
        public void ObtenerTendenciaVentas_PeriodoNulo_UsaValorPorDefecto()
        {
            // ARRANGE
            _mockVentaDatos
                .Setup(m => m.ObtenerTendenciaVentas("dia"))
                .Returns(new List<TendenciaVentasDTO>());

            // ACT — null debería ser tratado como período por defecto
            var resultado = _analiticaNegocio.ObtenerTendenciaVentas(null);

            // ASSERT — No debe lanzar excepción y llama con "dia"
            Assert.NotNull(resultado);
            _mockVentaDatos.Verify(m => m.ObtenerTendenciaVentas("dia"), Times.Once);
        }
    }

    /// <summary>
    /// Tests unitarios para VentaNegocio.ObtenerFacturasFiltradas.
    /// Cubre RF-MOV-BUS-02 y RF-MOV-MON-03.
    /// </summary>
    public class VentaNegocioFiltradoTests
    {
        private readonly Mock<IVentaDatos>   _mockVentaDatos;
        private readonly Mock<SesionDatos>   _mockSesionDatos;
        private readonly Mock<PlatillosDatos> _mockPlatillosDatos;
        private readonly VentaNegocio        _ventaNegocio;

        public VentaNegocioFiltradoTests()
        {
            _mockVentaDatos     = new Mock<IVentaDatos>();
            _mockSesionDatos    = new Mock<SesionDatos>("fake_string");
            _mockPlatillosDatos = new Mock<PlatillosDatos>("fake_string");
            _ventaNegocio = new VentaNegocio(
                _mockVentaDatos.Object,
                _mockSesionDatos.Object,
                _mockPlatillosDatos.Object);
        }

        // ─────────────────────────────────────────────────────────────
        // RF-MOV-BUS-02: ObtenerFacturasFiltradas
        // ─────────────────────────────────────────────────────────────

        [Fact]
        public void ObtenerFacturasFiltradas_FiltroNulo_LanzaArgumentNullException()
        {
            // ASSERT
            Assert.Throws<ArgumentNullException>(() =>
                _ventaNegocio.ObtenerFacturasFiltradas(null));
        }

        [Fact]
        public void ObtenerFacturasFiltradas_PaginaMenorA1_SeCorrigeA1()
        {
            // ARRANGE
            var filtro = new FiltroFacturasDTO { Pagina = 0, TamanoPagina = 10 };
            var paginadoEsperado = new FacturasPaginadasDTO
            {
                Facturas = new List<VentaListaDTO>(),
                TotalRegistros = 0,
                PaginaActual = 1,
                TamanoPagina = 10
            };

            _mockVentaDatos
                .Setup(m => m.ObtenerFacturasFiltradas(It.IsAny<FiltroFacturasDTO>()))
                .Returns(paginadoEsperado);

            // ACT
            var resultado = _ventaNegocio.ObtenerFacturasFiltradas(filtro);

            // ASSERT — La validación interna de VentaNegocio corrige Pagina a 1
            Assert.Equal(1, filtro.Pagina);
            Assert.NotNull(resultado);
        }

        [Fact]
        public void ObtenerFacturasFiltradas_TamanoPaginaMayor100_SeCorrigeA100()
        {
            // ARRANGE
            var filtro = new FiltroFacturasDTO { Pagina = 1, TamanoPagina = 200 };

            _mockVentaDatos
                .Setup(m => m.ObtenerFacturasFiltradas(It.IsAny<FiltroFacturasDTO>()))
                .Returns(new FacturasPaginadasDTO());

            // ACT
            _ventaNegocio.ObtenerFacturasFiltradas(filtro);

            // ASSERT — Máximo permitido es 100
            Assert.Equal(100, filtro.TamanoPagina);
        }

        [Fact]
        public void ObtenerFacturasFiltradas_FiltroValido_DevuelvePaginado()
        {
            // ARRANGE
            var filtro = new FiltroFacturasDTO
            {
                FechaDesde   = new DateTime(2026, 9, 1),
                FechaHasta   = new DateTime(2026, 9, 8),
                Estado       = "Pagada",
                Pagina       = 1,
                TamanoPagina = 20
            };

            var facturas = new List<VentaListaDTO>
            {
                new VentaListaDTO { VentaID = 1, Total = 250m, Estado = "Pagada" },
                new VentaListaDTO { VentaID = 2, Total = 180m, Estado = "Pagada" }
            };

            var paginadoEsperado = new FacturasPaginadasDTO
            {
                Facturas       = facturas,
                TotalRegistros = 2,
                PaginaActual   = 1,
                TamanoPagina   = 20
            };

            _mockVentaDatos
                .Setup(m => m.ObtenerFacturasFiltradas(It.IsAny<FiltroFacturasDTO>()))
                .Returns(paginadoEsperado);

            // ACT
            var resultado = _ventaNegocio.ObtenerFacturasFiltradas(filtro);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.TotalRegistros);
            Assert.Equal(2, resultado.Facturas.Count);
            Assert.Equal(1, resultado.TotalPaginas);
            _mockVentaDatos.Verify(
                m => m.ObtenerFacturasFiltradas(It.IsAny<FiltroFacturasDTO>()),
                Times.Once);
        }

        [Fact]
        public void ObtenerFacturasFiltradas_SinResultados_DevuelvePaginadoVacio()
        {
            // ARRANGE
            var filtro = new FiltroFacturasDTO
            {
                Estado       = "Anulada",
                Pagina       = 1,
                TamanoPagina = 20
            };

            _mockVentaDatos
                .Setup(m => m.ObtenerFacturasFiltradas(It.IsAny<FiltroFacturasDTO>()))
                .Returns(new FacturasPaginadasDTO
                {
                    Facturas       = new List<VentaListaDTO>(),
                    TotalRegistros = 0,
                    PaginaActual   = 1,
                    TamanoPagina   = 20
                });

            // ACT
            var resultado = _ventaNegocio.ObtenerFacturasFiltradas(filtro);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Empty(resultado.Facturas);
            Assert.Equal(0, resultado.TotalRegistros);
            Assert.Equal(0, resultado.TotalPaginas);
        }
    }
}
