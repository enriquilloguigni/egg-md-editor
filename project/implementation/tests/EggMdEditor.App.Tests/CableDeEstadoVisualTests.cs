using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Headless;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;
using Xunit;

namespace EggMdEditor.App.Tests;

// Cableado de COM-003 en SC-001 (T-011): el estado transitorio de la
// superficie se espeja sobre la sesión que pinta la ventana — menú
// {Cerrado | Desplegado con entrada activa} y aviso flotante {visible |
// oculto} — sin emisión alguna hacia acciones ni efecto sobre el documento
// (ADR-001): estados de superficie, no funcionales (states-transitions.md).
public class CableDeEstadoVisualTests
{
    // --- Contrato del gestor (espejo del estado de la sesión) ---

    [Fact]
    public void Entrar_en_una_entrada_de_menu_la_marca_desplegada_y_salir_cierra()
    {
        var sesion = new SesionDeAplicacion();
        var gestor = new GestorDeEstadoVisual(sesion);

        gestor.AlEntrarEnEntradaDeMenu(new MenuItem());

        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);

        gestor.AlSalirDeEntradaDeMenu(new MenuItem());

        Assert.Equal(EstadoMenuPrincipal.Cerrado, sesion.EstadoMenuPrincipal);
    }

    [Fact]
    public void Entrar_en_un_control_de_barra_marca_el_aviso_visible_y_salir_lo_oculta()
    {
        var sesion = new SesionDeAplicacion();
        var gestor = new GestorDeEstadoVisual(sesion);

        gestor.AlEntrarEnControlDeBarra(new Button());

        Assert.True(sesion.AvisoFlotanteVisible);

        gestor.AlSalirDeControlDeBarra(new Button());

        Assert.False(sesion.AvisoFlotanteVisible);
    }

    // --- Cableado de los eventos de puntero de SC-001 (COM-003) ---

    [Fact]
    public void El_puntero_sobre_una_entrada_de_menu_marca_el_estado_de_la_sesion()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        ventana.Show();

        var archivo = ventana.FindControl<Menu>("BarraMenu")!.Items.OfType<MenuItem>().First();
        ventana.MouseMove(PuntoEn(ventana, archivo), RawInputModifiers.None);

        Assert.Equal(EstadoMenuPrincipal.DesplegadoConEntradaActiva, sesion.EstadoMenuPrincipal);

        // Al retirar el puntero de la barra, el menú vuelve a estar cerrado.
        ventana.MouseMove(new Point(600, 400), RawInputModifiers.None);

        Assert.Equal(EstadoMenuPrincipal.Cerrado, sesion.EstadoMenuPrincipal);
    }

    [Fact]
    public void El_puntero_sobre_un_control_de_barra_marca_el_aviso_en_la_sesion()
    {
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        ventana.Show();

        var boton = ventana.FindControl<StackPanel>("BarraFormato")!.Children.OfType<Button>().First();
        ventana.MouseMove(PuntoEn(ventana, boton), RawInputModifiers.None);

        Assert.True(sesion.AvisoFlotanteVisible);

        ventana.MouseMove(new Point(600, 400), RawInputModifiers.None);

        Assert.False(sesion.AvisoFlotanteVisible);
    }

    [Fact]
    public void La_interaccion_visual_no_altera_el_idioma_ni_publica_eventos_de_conmutacion()
    {
        // COM-003: ningún evento de menú o barra genera emisión hacia una
        // acción (ADR-001); el único efecto es el estado de superficie.
        var sesion = new SesionDeAplicacion();
        var ventana = new MainWindow(sesion);
        ventana.Show();
        var conmutaciones = 0;
        ventana.ServicioDeIdioma.IdiomaConmutado += (_, _) => conmutaciones++;

        ventana.MouseMove(
            PuntoEn(ventana, ventana.FindControl<StackPanel>("BarraFormato")!.Children.OfType<Button>().First()),
            RawInputModifiers.None);

        Assert.Equal(0, conmutaciones);
        Assert.Equal(IdiomaDeInterfaz.Español, sesion.IdiomaActivo);
    }

    // --- Ayudantes ---

    private static Point PuntoEn(MainWindow ventana, Control control)
    {
        var centroDelControl = new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);
        return control.TranslatePoint(centroDelControl, ventana)
            ?? throw new InvalidOperationException("El control no se puede situar sobre la ventana.");
    }
}
