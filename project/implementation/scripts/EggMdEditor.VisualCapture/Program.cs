using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using EggMdEditor.App;
using EggMdEditor.App.I18n;

namespace EggMdEditor.VisualCapture;

/// <summary>
/// Herramienta de captura visual de las superficies de F-001 mediante
/// Avalonia headless + Skia. Escribe un PNG por invocación y usa los comandos
/// anclados en design-system.md.
/// </summary>
internal static class Program
{
    private const int Success = 0;
    private const int InvalidArguments = 2;
    private const int CaptureFailure = 3;

    public static int Main(string[] args)
    {
        if (!CaptureRequest.TryParse(args, out var request, out var error))
        {
            Console.Error.WriteLine(error);
            Console.Error.WriteLine(CaptureRequest.Usage);
            return InvalidArguments;
        }

        try
        {
            BuildAvaloniaApp().SetupWithoutStarting();
            Capture(request);
            return Success;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"No se pudo capturar {request.Scenario} ({request.SurfaceName}) " +
                $"en '{request.OutputPath}': {exception.Message}");
            return CaptureFailure;
        }
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<EggMdEditor.App.App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false,
                ShouldRenderOnUIThread = true,
            })
            .WithInterFont();

    private static void Capture(CaptureRequest request)
    {
        var application = Application.Current
            ?? throw new InvalidOperationException("La aplicación Avalonia no se inicializó.");

        // Estado base común: tema claro e idioma español, independientemente
        // del tema o del idioma del sistema anfitrión.
        var catalog = new I18nCatalog();
        if (!catalog.TrySetLanguage(I18nCatalog.Spanish))
        {
            throw new InvalidOperationException("No se pudo cargar el catálogo en español.");
        }

        using var session = new Session(catalog, new UiPreferences(I18nCatalog.Spanish, Theme.Light));
        using var themeApplier = new ApplicationThemeApplier(application);
        themeApplier.Attach(session);

        var viewModel = new MainWindowViewModel(session)
        {
            ThemeApplier = themeApplier,
        };

        switch (request.Surface)
        {
            case CaptureSurface.MainWindow:
                // SC-001 en estado base: menú cerrado y sin aviso visible.
                viewModel.CloseMenu();
                viewModel.HideNotice();
                CaptureWindow(new MainWindow(viewModel), request.OutputPath);
                break;

            case CaptureSurface.Dialog:
                // SC-002: overlay modal sobre la ventana principal.
                viewModel.Dialog.Open();
                CaptureWindow(new MainWindow(viewModel), request.OutputPath);
                break;

            case CaptureSurface.Help:
                // SC-003 en estado base: sección Acerca de.
                CaptureWindow(
                    new HelpWindow(new HelpWindowViewModel(
                        session,
                        catalog,
                        HelpSection.About,
                        () => viewModel.AvailableShortcuts)),
                    request.OutputPath);
                break;

            default:
                throw new InvalidOperationException($"Superficie no soportada: {request.Surface}.");
        }
    }

    private static void CaptureWindow(Window window, string outputPath)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.WindowState = WindowState.Normal;
            window.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            using var frame = window.CaptureRenderedFrame()
                ?? throw new InvalidOperationException("La superficie no generó ningún fotograma.");

            frame.Save(fullPath, new PngBitmapEncoderOptions());
        }
        finally
        {
            try
            {
                window.Close();
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            catch (Exception)
            {
                // El cierre de la superficie no debe ocultar el resultado de la
                // captura; el proceso termina a continuación.
            }
        }

        if (!File.Exists(fullPath) || new FileInfo(fullPath).Length == 0)
        {
            throw new InvalidOperationException($"La captura no se escribió en '{fullPath}'.");
        }
    }
}
