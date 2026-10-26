using System;
using Avalonia;
using Avalonia.Headless;
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
        StartupTestMode.Record("process-start", $"pid={Environment.ProcessId}");

        var exitCode = ExitCodes.Success;
        try
        {
            exitCode = Run(args, lifecycle);
        }
        catch (Exception exception)
        {
            // ERR-001/ERR-002: el fallo puede ocurrir durante la
            // inicialización o al pintar SC-001 antes de presentarla; ambos
            // comparten mensaje y comportamiento.
            exitCode = lifecycle.IsStartupPresented
                ? lifecycle.ReportShutdownFailure(exception)
                : lifecycle.ReportStartupFailure(exception);
        }
        finally
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            ApplicationLifecycle.Current = null;
            StartupTestMode.Record("process-exit", $"code={exitCode}");
            lifecycle.Dispose();
        }

        return exitCode;
    }

    private static int Run(string[] args, ApplicationLifecycle lifecycle)
    {
        // COM-005/BR-001: una única ventana por usuario. La primera instancia
        // conserva el cerrojo y escucha solicitudes de foco; una segunda
        // instancia se limita a pedir el foco de SC-001 y termina sin crear
        // ventana (A-002, AC-005). El arnés E2E aísla el cerrojo por prueba.
        using var singleInstance = SingleInstanceCoordinator.Acquire(
            StartupTestMode.IsEnabled ? StartupTestMode.ApplicationId : null);

        if (!singleInstance.IsPrimary)
        {
            // AC-005: la instancia secundaria no construye la aplicación ni
            // crea ventana; solo entrega la solicitud de activación.
            StartupTestMode.Record("secondary", "windowCount=0");
            var sent = singleInstance.TryRequestActivation();
            StartupTestMode.Record("activation-sent", $"sent={sent}");
            return ExitCodes.Success;
        }

        StartupTestMode.Record("primary", "windowCount=1");
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

    private static void OnActivationRequested(object? sender, EventArgs e)
    {
        StartupTestMode.Record("activation-received");
        App.RequestActivation();
    }

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

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>();

        // T-018: el arnés E2E usa la plataforma headless para ejecutar SC-001
        // sin servidor gráfico. No hay ningún cambio en la ruta normal.
        if (StartupTestMode.IsEnabled)
        {
            return builder
                .UseHeadless(new AvaloniaHeadlessPlatformOptions
                {
                    UseHeadlessDrawing = true,
                    ShouldRenderOnUIThread = true,
                })
                .WithInterFont();
        }

        return builder
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
