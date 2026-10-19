using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless;

namespace EggMdEditor.App.Tests;

// Arranque de la plataforma headless de Avalonia para las pruebas (una vez
// por ensamblado): permite construir controles y ventanas de SC-001 —como la
// comprobación del área de documento vacía de AC-015 (T-010)— sin ventana
// real, sin pantalla y sin red (C-001). Las pruebas que no la necesitan no
// se ven afectadas.
internal static class ArranqueDeAvaloniaParaPruebas
{
    [ModuleInitializer]
    internal static void Inicializar() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();
}
