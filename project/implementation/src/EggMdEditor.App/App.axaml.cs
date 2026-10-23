using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EggMdEditor.App.I18n;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public partial class App : Application
{
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
            desktop.Exit += (_, _) =>
            {
                _themeApplier?.Dispose();
                _loggerFactory?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
