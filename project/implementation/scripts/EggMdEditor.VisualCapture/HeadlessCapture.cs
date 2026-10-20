using System;
using System.IO;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using EggMdEditor.App;
using EggMdEditor.App.Catalogs;

// Alias for the application type: the simple name 'App' would resolve to the
// enclosing namespace 'EggMdEditor.App' instead of the class inside it.
using AppType = EggMdEditor.App.App;

namespace EggMdEditor.VisualCapture;

// Render of the application's implemented surfaces under Avalonia's headless
// platform —no display, no network and no machine state; the only file it
// touches is the image file it is told to write (C-001)— so the visual
// checks can capture SC-001 at its startup state: the single window in
// Spanish (BR-F001-004, BR-007) with the persistent menu bar at 100 % of
// FR-011 (T-008, T-010), the format bar at 100 % of FR-012 with its icons
// and tooltips declared (T-009, VR-001/VR-002) and the empty document area
// of AC-015.
//
// The visual conformity against the approved mockup is judged by the human
// reviewer in the verification's Result (§12.5): this utility only
// materializes the surface as the application itself paints it (real window
// code of SC-001, not an ad-hoc drawing).
internal static class HeadlessCapture
{
    private static bool _headlessReady;

    /// <summary>
    /// Renders the SC-001 window — built exactly as the startup builds it,
    /// over a new ephemeral session at its startup state — under the headless
    /// platform and writes the rendered frame as PNG bytes to the given
    /// output path, exactly as received: the sdd tool hashes the file that
    /// lands there, whatever its extension is. The window is not closed
    /// (COM-004): the capture is not an application run — no LOG-001 startup
    /// logs and no LOG-003 close logs are emitted — and the process exits
    /// right after the file is written.
    /// </summary>
    public static void CaptureMainWindow(string outputPath)
    {
        EnsureHeadlessPlatform();

        // The SC-001 window over a new, ephemeral session in its Startup
        // state (Spanish language, closed menu, hidden tooltip, empty
        // document area — mockup state A).
        var window = new MainWindow(new ApplicationSession());

        window.Show();

        // The checkpoint is the frame the headless platform actually painted
        // for the startup state (the capture extension triggers one render
        // timer tick and returns the last frame's pixels).
        var frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("Avalonia rendered no frame of the window.");

        WriteAsPng(frame, outputPath);
    }

    // One-time bootstrap of the headless platform, sharing the application's
    // own App (SC-001 rides on the FluentTheme styles of App.axaml): Skia as
    // the real drawing backend (UseHeadlessDrawing = false, per the platform
    // option's own documentation) so the capture holds painted pixels, and a
    // deterministic single-threaded render (ShouldRenderOnUIThread = true),
    // with no lifetime started — the tool builds the window itself.
    private static void EnsureHeadlessPlatform()
    {
        if (_headlessReady)
        {
            return;
        }

        AppBuilder
            .Configure<AppType>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false,
                ShouldRenderOnUIThread = true,
            })
            .UseSkia()
            .SetupWithoutStarting();

        _headlessReady = true;
    }

    // The PNG bytes go exactly to the requested path — the parent directory
    // is created if missing (the verification evidence location), the file is
    // overwritten if present, and no other file is touched (C-001).
    private static void WriteAsPng(WriteableBitmap frame, string outputPath)
    {
        var fullOutputPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullOutputPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(fullOutputPath);
        frame.Save(stream, new PngBitmapEncoderOptions());
    }
}
