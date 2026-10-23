using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EggMdEditor.App.I18n;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public partial class App : Application
{
    private ILoggerFactory? _loggerFactory;

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
            var session = new Session(catalog);
            var viewModel = new MainWindowViewModel(session)
            {
                Logger = _loggerFactory.CreateLogger<MainWindowViewModel>(),
            };

            desktop.MainWindow = new MainWindow(viewModel);
            desktop.Exit += (_, _) => _loggerFactory?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
