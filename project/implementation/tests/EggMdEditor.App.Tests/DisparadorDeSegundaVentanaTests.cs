using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Disparador de la solicitud de segunda ventana (AC-014): coordinador en
// proceso que, con SC-001 existente, la enfoca sin crear otra ventana
// (BR-003), sin transición de estado. Acotado a proceso único por la Spec
// (riesgo del Plan): sin canal interprocesal ni atención de segundos
// procesos. Sin ventana conectada, la solicitud no tiene respuesta.
public class DisparadorDeSegundaVentanaTests
{
    [Fact]
    public void La_solicitud_con_ventana_existente_la_enfoca_una_vez_sin_crear_otra()
    {
        var ventana = new VentanaUnicaDePrueba();
        var disparador = new DisparadorDeSegundaVentana(ventana);

        var resultado = disparador.SolicitarSegundaVentana();

        // AC-014: SC-001 recibe el foco; no se crea otra ventana.
        Assert.Equal(EstadoSolicitudSegundaVentana.Enfocada, resultado.Estado);
        Assert.Equal(1, ventana.EnfoquesRecibidos);
    }

    [Fact]
    public void Cada_solicitud_enfoca_la_misma_ventana_existente_y_unica()
    {
        var ventana = new VentanaUnicaDePrueba();
        var disparador = new DisparadorDeSegundaVentana(ventana);

        var resultados = Enumerable.Range(0, 3)
            .Select(_ => disparador.SolicitarSegundaVentana())
            .ToList();

        // BR-003: sigue habiendo exactamente una ventana por sesión; todas
        // las solicitudes se atienden enfocando la misma ventana existente
        // inyectada —el coordinador no crea otra ni la sustituye—.
        Assert.All(resultados, r => Assert.Equal(EstadoSolicitudSegundaVentana.Enfocada, r.Estado));
        Assert.Equal(3, ventana.EnfoquesRecibidos);
    }

    [Fact]
    public void La_solicitud_no_produce_transicion_de_estado_en_la_sesion()
    {
        var sesion = new SesionDeAplicacion
        {
            IdiomaActivo = IdiomaDeInterfaz.English,
            EstadoMenuPrincipal = EstadoMenuPrincipal.DesplegadoConEntradaActiva,
            AvisoFlotanteVisible = true,
        };
        var disparador = new DisparadorDeSegundaVentana(new VentanaUnicaDePrueba());

        disparador.SolicitarSegundaVentana();

        // states-transitions.md: solicitud de segunda ventana en ST-002/ST-003
        // sin transición; el disparador no toca la sesión.
        Assert.Equal(IdiomaDeInterfaz.English, sesion.IdiomaActivo);
        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);
        Assert.True(sesion.AvisoFlotanteVisible);
    }

    [Fact]
    public void La_solicitud_sin_ventana_conectada_no_tiene_respuesta_ni_crea_ventana()
    {
        var disparador = new DisparadorDeSegundaVentana();

        var resultado = disparador.SolicitarSegundaVentana();

        // Sin contraparte de enfoque (SC-001 no existe o su cableado llega en
        // T-011): sin respuesta y jamás se crea una ventana para atenderla —
        // la única ventana se crea en el arranque (T-008).
        Assert.Equal(EstadoSolicitudSegundaVentana.SinVentanaConectada, resultado.Estado);
    }

    [Fact]
    public void La_contraparte_de_enfoque_es_opcional_en_el_constructor()
    {
        Assert.Null(Record.Exception(() => new DisparadorDeSegundaVentana(null)));
        Assert.Null(Record.Exception(() => new DisparadorDeSegundaVentana()));
    }

    /// <summary>
    /// SC-001 de prueba: cuenta los enfoques recibidos sin levantar UI real.
    /// </summary>
    private sealed class VentanaUnicaDePrueba : IEnfocadorDeVentana
    {
        public int EnfoquesRecibidos { get; private set; }

        public void Enfocar() => EnfoquesRecibidos++;
    }
}
