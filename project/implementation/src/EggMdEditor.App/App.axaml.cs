using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;

namespace EggMdEditor.App;

public partial class App : Application
{
    private readonly SesionDeAplicacion? _sesionDelProceso;
    private readonly RegistradorDeCicloDeVida? _registradorDeCicloDeVida;

    /// <summary>
    /// Constructor sin argumentos para el diseñador de XAML y las pruebas
    /// headless: la ventana que cree creará su propia sesión con los defectos
    /// de la Spec (idioma español, BR-007), sin registrador del proceso.
    /// </summary>
    public App() : this(null, null)
    {
    }

    /// <summary>
    /// Crea la aplicación del proceso (T-012) sobre la sesión nueva del
    /// arranque — siempre en español, sin persistencia (BR-F001-006) — y su
    /// registrador del ciclo de vida (T-007); con cualquiera de los dos
    /// ausentes (diseñador, pruebas), la ventana opera con los suyos por
    /// defecto.
    /// </summary>
    public App(SesionDeAplicacion? sesionDelProceso, RegistradorDeCicloDeVida? registradorDeCicloDeVida)
    {
        _sesionDelProceso = sesionDelProceso;
        _registradorDeCicloDeVida = registradorDeCicloDeVida;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // SC-001 (BR-003): ventana única que comparte la sesión del
            // proceso — una sesión por ejecución, correlación de registros
            // (NFR-003) — y su registrador del ciclo de vida, para que su
            // cierre emita LOG-003 y aplique ERR-005 (COM-004, T-011). Si la
            // creación falla, la excepción sube al punto de entrada: ERR-001,
            // sin ventana presentada (T-012).
            var ventana = new MainWindow(
                _sesionDelProceso ?? new SesionDeAplicacion(),
                registradorDeCicloDeVida: _registradorDeCicloDeVida);
            desktop.MainWindow = ventana;

            if (_registradorDeCicloDeVida is not null)
            {
                ventana.Opened += AlPresentarseLaVentana;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// LOG-001 (T-012): la ventana única se presentó completa — creada y
    /// abierta, sin que el pintado del arranque fallara (ERR-002) —; se
    /// registra el fin de la inicialización una sola vez, con el catálogo
    /// i18n cargado y el idioma activo de la sesión.
    /// </summary>
    private void AlPresentarseLaVentana(object? sender, EventArgs e)
    {
        if (sender is Window ventana)
        {
            ventana.Opened -= AlPresentarseLaVentana;
        }

        _registradorDeCicloDeVida?.RegistrarFinDeInicializacion();
    }
}
