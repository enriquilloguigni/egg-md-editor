using System;
using System.Collections.Generic;
using System.IO;

namespace EggMdEditor.VisualCapture;

// egg-md-editor — headless visual capture utility. It is the real command
// anchored in project/design/design-system.md for the sdd visual capture
// checks: it renders the surfaces implemented by the application under
// Avalonia's headless platform —no display, no network and no machine state;
// the only file it touches is the image file it is told to write (C-001)—
// and writes its pixels as PNG bytes.
//
// CLI contract: exactly three arguments <surface> <screen> <output-path>.
// On success the PNG bytes land EXACTLY on <output-path> —no extension
// rewriting: the sdd tool hashes the file that arrives there, whatever its
// extension is—, a one-line confirmation goes to standard output and the
// process ends with exit code 0 (see ExitCode). Every documented failure
// reports its reason through standard error and ends with a non-zero exit
// code, with no partial changes and no residual state.
internal static class Program
{
    // Surfaces of the design system (project/design/design-system.md). The
    // names are fixed: the sdd tool substitutes them into the anchored
    // command exactly as written there.
    internal const string MainWindowSurface = "ventana-principal";
    internal const string DialogSurface = "diálogo";
    internal const string HelpSurface = "ayuda";

    internal static readonly IReadOnlyList<string> KnownSurfaces = new[]
    {
        MainWindowSurface,
        DialogSurface,
        HelpSurface,
    };

    public static int Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine(
                "Visual capture of egg-md-editor: exactly three arguments are required "
                + "(<surface> <screen> <output-path>). Surfaces: "
                + $"{string.Join(", ", KnownSurfaces)} — implemented in F-001: '"
                + $"{MainWindowSurface}' only, with screen '{MainWindowScreen}'.");
            return ExitCode.Usage;
        }

        var surface = args[0];
        var screen = args[1];
        var outputPath = args[2];

        switch (surface)
        {
            case MainWindowSurface:
                return CaptureMainWindow(screen, outputPath);

            case DialogSurface:
            case HelpSurface:
                Console.Error.WriteLine(
                    $"Surface '{surface}' is declared in the design system but is not implemented in F-001: "
                    + $"it has no screens to capture (only '{MainWindowSurface}' and its screen '{MainWindowScreen}' exist in F-001).");
                return ExitCode.SurfaceNotImplemented;

            default:
                Console.Error.WriteLine(
                    $"Unknown surface '{surface}'. Surfaces of the design system: "
                    + $"{string.Join(", ", KnownSurfaces)}.");
                return ExitCode.UnknownSurface;
        }
    }

    // ventana-principal — SC-001: the single window at its startup state
    // (startup defaults of the session, always Spanish, BR-F001-004,
    // BR-007), rendered and written as PNG to the output path.
    private static int CaptureMainWindow(string screen, string outputPath)
    {
        if (screen != MainWindowScreen)
        {
            Console.Error.WriteLine(
                $"Unknown screen '{screen}' for surface '{MainWindowSurface}': "
                + $"the only screen implemented in F-001 is '{MainWindowScreen}'.");
            return ExitCode.UnknownScreen;
        }

        try
        {
            HeadlessCapture.CaptureMainWindow(outputPath);
        }
        catch (Exception exception) when (IsOutputFailure(exception))
        {
            Console.Error.WriteLine($"Failed to write the capture file '{outputPath}': {exception.Message}");
            return ExitCode.OutputFailed;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Capture failed: {exception.Message}");
            return ExitCode.CaptureFailed;
        }

        Console.WriteLine($"SC-001 ({MainWindowSurface}) captured to {outputPath}.");
        return ExitCode.Success;
    }

    // Failures of the write itself: unrepresentable or unwritable output
    // path. They are reported apart from the render failures (exit code 5
    // instead of 6) so the capture check distinguishes a bad output from a
    // bad render.
    private static bool IsOutputFailure(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException;

    internal const string MainWindowScreen = "SC-001";
}

// Exit codes of the CLI contract (recorded by the sdd visual capture
// check): 0 success; 1 wrong argument count; 2 unknown surface; 3 surface
// declared but not implemented in F-001; 4 unknown screen for the surface;
// 5 the output path could not be written; 6 unexpected capture failure.
internal static class ExitCode
{
    public const int Success = 0;
    public const int Usage = 1;
    public const int UnknownSurface = 2;
    public const int SurfaceNotImplemented = 3;
    public const int UnknownScreen = 4;
    public const int OutputFailed = 5;
    public const int CaptureFailed = 6;
}
