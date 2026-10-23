using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using EggMdEditor.App.I18n;
using EggMdEditor.App.Startup;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public partial class App : Application
{
    private static int _activationPending;

    private ApplicationLifecycle? _lifecycle;
    private ILoggerFactory? _loggerFactory;
    private bool _ownsLifecycle;
    private bool _ownsLoggerFactory;
    private ApplicationThemeApplier? _themeApplier;
    private Session? _session;
    private Window? _mainWindow;
    private bool _startupFailed;
    private bool _exitHandled;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                InitializeDesktop(desktop);
            }
            catch (Exception exception)
            {
                // ERR-001/ERR-002: sin ventana usable, mensaje en español por
                // la salida de error y código de error. Program completa el
                // mismo diagnóstico de forma idempotente si la excepción
                // escapa del bucle de arranque.
                _startupFailed = true;
                _lifecycle?.ReportStartupFailure(exception);
                TryCloseMainWindow(desktop);
                throw;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        // LOG-001/LOG-003: Program crea el ciclo de vida y comparte su sesión
        // y su registro. En un arranque aislado (p. ej. pruebas de la ventana)
        // la aplicación crea uno propio.
        var currentLifecycle = ApplicationLifecycle.Current;
        _lifecycle = currentLifecycle ?? new ApplicationLifecycle();
        _ownsLifecycle = currentLifecycle is null;

        if (_lifecycle.LoggerFactory is null)
        {
            _loggerFactory = ApplicationLogging.CreateLoggerFactory();
            _ownsLoggerFactory = true;
        }
        else
        {
            _loggerFactory = _lifecycle.LoggerFactory;
        }

        _lifecycle.BeginStartup();

        var catalog = new I18nCatalog(
            ApplicationLogging.CreateSessionLogger(
                _loggerFactory,
                nameof(I18nCatalog),
                _lifecycle.SessionId))
        {
            SessionId = _lifecycle.SessionId,
        };

        // ENT-003: en F-001 el tema no se persiste (FR-013 → F-010), así
        // que cada arranque hereda el tema del SO. Se fija el tema efectivo
        // en la sesión y en la aplicación para que ST-009/ST-010 y las
        // superficies de SC-001 queden alineados desde el primer pintado;
        // OP-002 los conmuta después en caliente (COM-003).
        var initialTheme = ApplicationThemeApplier.ResolveSystemTheme(this);
        _session = new Session(catalog, new UiPreferences("es", initialTheme));
        _themeApplier = new ApplicationThemeApplier(this);
        _themeApplier.Attach(_session);

        var viewModel = new MainWindowViewModel(_session)
        {
            Logger = ApplicationLogging.CreateSessionLogger(
                _loggerFactory,
                nameof(MainWindowViewModel),
                _lifecycle.SessionId),
            ThemeApplier = _themeApplier,
            SessionId = _lifecycle.SessionId,
        };

        _mainWindow = new MainWindow(viewModel, _lifecycle);
        desktop.MainWindow = _mainWindow;
        desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

        desktop.ShutdownRequested += OnShutdownRequested;
        desktop.Exit += OnExit;

        // ERR-002: mientras SC-001 no se haya presentado por completo, una
        // excepción no controlada de la UI es un fallo de arranque. El
        // manejador se retira al completar la primera presentación.
        _mainWindow.Opened += OnMainWindowOpened;
        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += OnDispatcherUnhandledException;

        // COM-005: una solicitud de foco llegada mientras se construía
        // SC-001 se atiende en cuanto la ventana existe.
        if (Volatile.Read(ref _activationPending) == 1)
        {
            ConsumePendingActivation();
        }
    }

    private void OnMainWindowOpened(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            window.Opened -= OnMainWindowOpened;
        }

        // LOG-001 se cierra en el primer ciclo de presentación: una excepción
        // de pintado anterior a ese punto sigue siendo ERR-002.
        Avalonia.Threading.Dispatcher.UIThread.Post(
            () =>
            {
                if (_startupFailed || _exitHandled)
                {
                    return;
                }

                _lifecycle?.MarkInitialized(_session?.Language ?? I18nCatalog.Spanish);
                Avalonia.Threading.Dispatcher.UIThread.UnhandledException -= OnDispatcherUnhandledException;
            },
            DispatcherPriority.Loaded);
    }

    private void OnDispatcherUnhandledException(
        object? sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        if (_startupFailed || _lifecycle is null || _lifecycle.IsStartupPresented)
        {
            // Los fallos de pintado posteriores a la inicialización no tienen
            // ERR en F-001.
            return;
        }

        // ERR-002: ventana creada que no llega a presentarse completa. No se
        // ofrece usable y el proceso termina con código de error.
        _startupFailed = true;
        _lifecycle.ReportStartupFailure(e.Exception);
        e.Handled = true;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                if (!desktop.TryShutdown(ExitCodes.StartupFailure))
                {
                    Environment.Exit(ExitCodes.StartupFailure);
                }

                return;
            }
            catch (Exception)
            {
                // Se recurre a la finalización directa si el bucle ya no
                // admite el apagado ordenado.
            }
        }

        Environment.Exit(ExitCodes.StartupFailure);
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        // LOG-003 (inicio del cierre): se registra antes de liberar la
        // sesión. En un arranque fallido no hay SC-001 usable que cerrar.
        if (!_startupFailed)
        {
            _lifecycle?.BeginShutdown();
        }
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_exitHandled)
        {
            return;
        }

        _exitHandled = true;
        DetachEvents();

        if (_startupFailed)
        {
            // ERR-001/ERR-002: el proceso termina igualmente y sin recursos
            // vivos; el diagnóstico ya quedó en LOG-001.
            ReleaseSessionResources();
            DisposeOwnedDiagnostics();
            return;
        }

        _lifecycle?.BeginShutdown();
        var failure = ReleaseSessionResources();
        var exitCode = _lifecycle?.CompleteShutdown(failure) ?? e.ApplicationExitCode;
        if (exitCode != ExitCodes.Success)
        {
            e.ApplicationExitCode = exitCode;
        }

        DisposeOwnedDiagnostics();
    }

    private void DisposeOwnedDiagnostics()
    {
        if (_ownsLoggerFactory)
        {
            try
            {
                _loggerFactory?.Dispose();
            }
            catch (Exception)
            {
                // El cierre del registro no impide la finalización.
            }
        }

        if (_ownsLifecycle)
        {
            _lifecycle?.Dispose();
        }
    }

    private void DetachEvents()
    {
        if (_mainWindow is not null)
        {
            _mainWindow.Opened -= OnMainWindowOpened;
        }

        Avalonia.Threading.Dispatcher.UIThread.UnhandledException -= OnDispatcherUnhandledException;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested -= OnShutdownRequested;
            desktop.Exit -= OnExit;
        }
    }

    /// <summary>
    /// OP-006/ERR-005: libera los recursos de la sesión en el orden en que se
    /// crearon. Si alguno falla, se continúa con el resto y se devuelve la
    /// primera excepción para que el proceso termine igualmente registrando
    /// LOG-003.
    /// </summary>
    private Exception? ReleaseSessionResources()
    {
        Exception? firstFailure = null;

        try
        {
            _session?.CloseWindow();
        }
        catch (Exception exception)
        {
            firstFailure ??= exception;
        }

        try
        {
            _session?.Dispose();
        }
        catch (Exception exception)
        {
            firstFailure ??= exception;
        }

        try
        {
            _themeApplier?.Dispose();
        }
        catch (Exception exception)
        {
            firstFailure ??= exception;
        }

        _session = null;
        _themeApplier = null;
        return firstFailure;
    }

    /// <summary>
    /// ERR-001/ERR-002: no se debe ofrecer una ventana a medio construir.
    /// </summary>
    private static void TryCloseMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            desktop.MainWindow?.Close();
        }
        catch (Exception)
        {
            // La ventana nunca llegó a presentarse; el proceso finaliza igual.
        }
    }

    /// <summary>
    /// COM-005: solicitud de foco enviada por una segunda instancia. Puede
    /// llegar antes de que exista la ventana; en ese caso queda pendiente y
    /// se atiende al terminar el arranque de SC-001.
    /// </summary>
    public static void RequestActivation()
    {
        Interlocked.Exchange(ref _activationPending, 1);
        if (Application.Current is App app)
        {
            app.PostPendingActivation();
        }
    }

    private void PostPendingActivation()
    {
        try
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(ConsumePendingActivation);
        }
        catch (Exception)
        {
            // El dispatcher aún no está disponible (arranque en curso): la
            // solicitud permanece pendiente y se atiende al completar la
            // inicialización del framework.
        }
    }

    private void ConsumePendingActivation()
    {
        if (Interlocked.Exchange(ref _activationPending, 0) == 0)
        {
            return;
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is { } window)
        {
            ShowAndActivate(window);
            return;
        }

        // La ventana todavía no existe: la solicitud sigue pendiente.
        Interlocked.Exchange(ref _activationPending, 1);
    }

    /// <summary>
    /// COM-005: restaura SC-001 si está minimizada y la trae al frente sin
    /// crear otra ventana (BR-001, A-002).
    /// </summary>
    private static void ShowAndActivate(Window window)
    {
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        if (!window.IsVisible)
        {
            window.Show();
        }

        window.Activate();
    }
}
