using System;
using Avalonia;
using EggMdEditor.App.Startup;

namespace EggMdEditor.App;

internal static class Program
{
    public static int Main(string[] args)
    {
        // COM-005/BR-001: una única ventana por usuario. La primera instancia
        // conserva el cerrojo y escucha solicitudes de foco; una segunda
        // instancia se limita a pedir el foco de SC-001 y termina sin crear
        // ventana (A-002, AC-005).
        using var singleInstance = SingleInstanceCoordinator.Acquire();

        if (!singleInstance.IsPrimary)
        {
            singleInstance.TryRequestActivation();
            return 0;
        }

        singleInstance.ActivationRequested += OnActivationRequested;
        try
        {
            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            singleInstance.ActivationRequested -= OnActivationRequested;
        }
    }

    private static void OnActivationRequested(object? sender, EventArgs e) =>
        App.RequestActivation();

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
