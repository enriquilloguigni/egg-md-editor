using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EggMdEditor.App.I18n;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public partial class App : Application
{
    private static int _activationPending;

    private ILoggerFactory? _loggerFactory;
    private ApplicationThemeApplier? _themeApplier;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // LOG-002 (y base de LOG-001/LOG-003 en T-013): registro técnico
            // por la salida estándar; nada se escribe a disco (NFR-010).
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
                    options.UseUtcTimestamp = true;
                });
            });

            var catalog = new I18nCatalog(_loggerFactory.CreateLogger<I18nCatalog>());

            // ENT-003: en F-001 el tema no se persiste (FR-013 → F-010), así
            // que cada arranque hereda el tema del SO. Se fija el tema
            // efectivo en la sesión y en la aplicación para que ST-009/ST-010
            // y las superficies de SC-001 queden alineados desde el primer
            // pintado; OP-002 los conmuta después en caliente (COM-003).
            var initialTheme = ApplicationThemeApplier.ResolveSystemTheme(this);
            var session = new Session(catalog, new UiPreferences("es", initialTheme));
            _themeApplier = new ApplicationThemeApplier(this);
            _themeApplier.Attach(session);

            var viewModel = new MainWindowViewModel(session)
            {
                Logger = _loggerFactory.CreateLogger<MainWindowViewModel>(),
                ThemeApplier = _themeApplier,
            };

            desktop.MainWindow = new MainWindow(viewModel);
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // COM-005: una solicitud de foco llegada mientras se construía
            // SC-001 se atiende en cuanto la ventana existe.
            if (Volatile.Read(ref _activationPending) == 1)
            {
                ConsumePendingActivation();
            }

            desktop.Exit += (_, _) =>
            {
                // OP-006: al terminar la sesión se liberan sus datos en
                // memoria; no queda ningún estado que persistir (FR-013).
                session.Dispose();
                _themeApplier?.Dispose();
                _loggerFactory?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
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
