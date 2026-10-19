using System;
using Avalonia;
using Avalonia.Logging;
using EggMdEditor.App.Catalogos;
using EggMdEditor.App.Servicios;

namespace EggMdEditor.App;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things
    // aren't initialized yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        // Configuración de ejecución fijada (T-012): la aplicación arranca
        // siempre en español, con una sesión nueva y efímera (BR-F001-004,
        // BR-F001-006, AC-001) — no se lee ninguna preferencia del disco ni
        // de variables de entorno, no hay escritura a disco y no hay acceso
        // a red (C-001); los registros del proceso salen por la salida
        // estándar (T-007, NFR-003). La verificación es `dotnet test` en la
        // raíz del árbol de implementación.
        var sesion = new SesionDeAplicacion();
        var registrador = new RegistradorDeCicloDeVida(sesion);
        var arranque = new ArranqueDeLaAplicacion(sesion, registrador);

        return arranque.Ejecutar(
            () => BuildAvaloniaApp(sesion, registrador)
                .StartWithClassicDesktopLifetime(args));
    }

    // Avalonia configuration: don't edit; used by design tooling.
    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(null, null);

    /// <summary>
    /// Construye la aplicación del proceso compartiendo la sesión nueva del
    /// arranque — siempre en español (BR-F001-006) — y su registrador del
    /// ciclo de vida (T-007): la ventana única SC-001 que la App crea y
    /// presenta emite los registros del proceso — LOG-001 al arrancar y
    /// LOG-003/ERR-005 al cerrar (COM-004, T-011) — y cualquier fallo del
    /// arranque vuelve al punto de entrada como ERR-001/ERR-002 (T-012).
    /// </summary>
    public static AppBuilder BuildAvaloniaApp(
        SesionDeAplicacion? sesionDelProceso,
        RegistradorDeCicloDeVida? registradorDeCicloDeVida) =>
        AppBuilder.Configure(() => new App(sesionDelProceso, registradorDeCicloDeVida))
            .UsePlatformDetect()
            .LogToTrace();
}
