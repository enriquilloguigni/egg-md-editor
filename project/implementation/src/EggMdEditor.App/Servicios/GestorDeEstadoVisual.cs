using Avalonia.Controls;
using Avalonia.Interactivity;
using EggMdEditor.App.Catalogos;

namespace EggMdEditor.App.Servicios;

/// <summary>
/// Estado visual de menú y barra de formato de SC-001 (COM-003, T-011):
/// espeja sobre la sesión —la fuente observable que pinta la ventana— el
/// estado transitorio de la superficie {<see cref="EstadoMenuPrincipal"/>:
/// Cerrado | Desplegado con entrada activa} y el aviso flotante
/// {visible | oculto}, a partir de los eventos de puntero sobre la barra de
/// menú y la barra de formato.
///
/// <para>
/// Espejo sin semántica funcional: ningún evento de estos genera emisión
/// hacia una acción ni altera el documento (ADR-001) y no es estado
/// funcional de la sesión (states-transitions.md) — solo el reflejo que
/// COM-003 declara como destino. La apariencia (avisos flotantes
/// VR-002 sobre la barra de formato, menú desplegable) la pinta Avalonia;
/// este gestor mantiene el estado de la sesión al corriente de ella. El
/// estado del menú pasa a «Desplegado con entrada activa» cuando el puntero
/// entra en una entrada de menú y vuelve a «Cerrado» cuando la deja; el
/// aviso flotante queda visible mientras el puntero permanece sobre un
/// control de la barra de formato y oculto al retirarlo.
/// </para>
/// </summary>
public sealed class GestorDeEstadoVisual
{
    private readonly SesionDeAplicacion _sesion;

    /// <summary>
    /// Crea el gestor sobre la sesión de la aplicación, cuyos campos de
    /// estado visual actualiza.
    /// </summary>
    public GestorDeEstadoVisual(SesionDeAplicacion sesion) =>
        _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));

    /// <summary>
    /// Conecta los eventos de puntero de la barra de menú y de la barra de
    /// formato de SC-001 al estado de la sesión: cada entrada de menú
    /// reporta su entrada y salida del puntero y cada control de la barra,
    /// el suyo. Los eventos llegan en el hilo de la interfaz, síncronos y en
    /// proceso (COM-003).
    /// </summary>
    public void Conectar(Menu barraMenu, StackPanel barraFormato)
    {
        ArgumentNullException.ThrowIfNull(barraMenu);
        ArgumentNullException.ThrowIfNull(barraFormato);

        foreach (var item in barraMenu.Items.OfType<MenuItem>())
        {
            ConectarEntradaDeMenu(item);
        }

        foreach (var boton in barraFormato.Children.OfType<Button>())
        {
            boton.PointerEntered += AlEntrarElPunteroEnUnControlDeBarra;
            boton.PointerExited += AlSalirElPunteroDeUnControlDeBarra;
        }
    }

    /// <summary>
    /// El puntero entra en una entrada de menú: la entrada queda activa y el
    /// menú, desplegado con entrada activa.
    /// </summary>
    public void AlEntrarEnEntradaDeMenu(MenuItem entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        _sesion.EstadoMenuPrincipal = EstadoMenuPrincipal.DesplegadoConEntradaActiva;
    }

    /// <summary>
    /// El puntero deja una entrada de menú: sin entrada activa, el menú
    /// vuelve a estar cerrado.
    /// </summary>
    public void AlSalirDeEntradaDeMenu(MenuItem entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        _sesion.EstadoMenuPrincipal = EstadoMenuPrincipal.Cerrado;
    }

    /// <summary>
    /// El puntero entra en un control de la barra de formato: su aviso
    /// flotante queda visible (VR-002).
    /// </summary>
    public void AlEntrarEnControlDeBarra(Button control)
    {
        ArgumentNullException.ThrowIfNull(control);

        _sesion.AvisoFlotanteVisible = true;
    }

    /// <summary>
    /// El puntero deja un control de la barra de formato: el aviso flotante
    /// queda oculto (VR-002).
    /// </summary>
    public void AlSalirDeControlDeBarra(Button control)
    {
        ArgumentNullException.ThrowIfNull(control);

        _sesion.AvisoFlotanteVisible = false;
    }

    private void ConectarEntradaDeMenu(MenuItem item)
    {
        item.PointerEnteredItem += AlEntrarElPunteroEnUnaEntradaDeMenu;
        item.PointerExitedItem += AlSalirElPunteroDeUnaEntradaDeMenu;

        foreach (var hijo in item.Items.OfType<MenuItem>())
        {
            ConectarEntradaDeMenu(hijo);
        }
    }

    private void AlEntrarElPunteroEnUnaEntradaDeMenu(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem entrada)
        {
            AlEntrarEnEntradaDeMenu(entrada);
        }
    }

    private void AlSalirElPunteroDeUnaEntradaDeMenu(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem entrada)
        {
            AlSalirDeEntradaDeMenu(entrada);
        }
    }

    private void AlEntrarElPunteroEnUnControlDeBarra(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (sender is Button control)
        {
            AlEntrarEnControlDeBarra(control);
        }
    }

    private void AlSalirElPunteroDeUnControlDeBarra(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (sender is Button control)
        {
            AlSalirDeControlDeBarra(control);
        }
    }
}
