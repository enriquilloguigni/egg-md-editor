using System;
using Avalonia;
using EggMdEditor.App.Startup;

namespace EggMdEditor.App;

internal static class Program
{
    public static int Main(string[] args)
    {
        // LOG-001/LOG-003 y ERR-001/ERR-002/ERR-005: el ciclo de vida posee
        // el registro técnico y la salida de error del proceso. Se crea antes
        // que cualquier otro componente para poder diagnosticar el arranque.
        var lifecycle = new ApplicationLifecycle();
        ApplicationLifecycle.Current = lifecycle;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        try
        {
            return Run(args, lifecycle);
        }
        catch (Exception exception)
        {
            // ERR-001/ERR-002: el fallo puede ocurrir durante la
            // inicialización o al pintar SC-001 antes de presentarla; ambos
            // comparten mensaje y comportamiento.
            return lifecycle.IsStartupPresented
                ? lifecycle.ReportShutdownFailure(exception)
                : lifecycle.ReportStartupFailure(exception);
        }
        finally
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            ApplicationLifecycle.Current = null;
            lifecycle.Dispose();
        }
    }

    private static int Run(string[] args, ApplicationLifecycle lifecycle)
    {
        // COM-005/BR-001: una única ventana por usuario. La primera instancia
        // conserva el cerrojo y escucha solicitudes de foco; una segunda
        // instancia se limita a pedir el foco de SC-001 y termina sin crear
        // ventana (A-002, AC-005).
        using var singleInstance = SingleInstanceCoordinator.Acquire();

        if (!singleInstance.IsPrimary)
        {
            singleInstance.TryRequestActivation();
            return ExitCodes.Success;
        }

        singleInstance.ActivationRequested += OnActivationRequested;
        try
        {
            lifecycle.BeginStartup();

            var exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

            if (lifecycle.HasStartupFailure)
            {
                // ERR-001/ERR-002: el proceso finaliza con código de error.
                return lifecycle.ExitCode;
            }

            if (!lifecycle.IsShutdownCompleted)
            {
                exitCode = lifecycle.CompleteShutdown();
            }
            else
            {
                exitCode = Math.Max(exitCode, lifecycle.ExitCode);
            }

            return exitCode;
        }
        finally
        {
            singleInstance.ActivationRequested -= OnActivationRequested;
        }
    }

    private static void OnActivationRequested(object? sender, EventArgs e) =>
        App.RequestActivation();

    /// <summary>
    /// ERR-001/ERR-002: última red de diagnóstico para una excepción que no
    /// pasa por el bucle de Avalonia (por ejemplo, un fallo de pintado en otro
    /// hilo). El proceso termina igualmente; solo se garantiza el mensaje en
    /// español y el registro LOG-001/LOG-003.
    /// </summary>
    private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception exception
            || ApplicationLifecycle.Current is not { } lifecycle)
        {
            return;
        }

        if (!lifecycle.IsStartupPresented)
        {
            lifecycle.ReportStartupFailure(exception);
        }
        else if (lifecycle.IsShutdownStarted)
        {
            lifecycle.ReportShutdownFailure(exception);
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
