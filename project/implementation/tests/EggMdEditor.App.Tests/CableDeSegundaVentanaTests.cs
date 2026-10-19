using Avalonia.Controls;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Cableado de la contraparte de enfoque de la ventana única (T-006 → T-011):
// la solicitud de segunda ventana que llega al disparador de la ventana se
// atiende enfocando la SC-001 existente —exactamente una ventana por sesión,
// BR-003, AC-014—, sin crear otra y sin transición de estado.
public class CableDeSegundaVentanaTests
{
    [Fact]
    public void La_solicitud_de_segunda_ventana_se_atiende_enfocando_la_ventana_existente()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        ventana.Show();

        var resultado = ventana.DisparadorDeSegundaVentana.SolicitarSegundaVentana();

        // La contraparte cableada sobre esta ventana recibió la solicitud:
        // exactamente una invocación de enfoque, sin crear otra ventana
        // (BR-003, AC-014) y sin transición de estado en la sesión.
        Assert.Equal(EstadoSolicitudSegundaVentana.Enfocada, resultado.Estado);
        Assert.True(ventana.IsVisible);
        Assert.Equal(CatalogoIdiomas.Defecto, sesion.IdiomaActivo);
    }

    [Fact]
    public void El_arranque_por_defecto_expone_la_ventana_con_su_sesion_y_servicios_cableados()
    {
        // El constructor sin argumentos (arranque, T-012) crea la sesión en
        // su defecto —español, BR-007— y los canales sobre ella.
        var ventana = new MainWindow();

        Assert.NotNull(ventana.ServicioDeIdioma);
        Assert.NotNull(ventana.DespachadorDeAtajos);
        Assert.NotNull(ventana.DisparadorDeSegundaVentana);
        Assert.Equal(EstadoSolicitudSegundaVentana.Enfocada,
            ventana.DisparadorDeSegundaVentana.SolicitarSegundaVentana().Estado);
    }
}
