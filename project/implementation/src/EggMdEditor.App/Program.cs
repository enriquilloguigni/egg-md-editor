using System;
using Avalonia;
using Avalonia.Logging;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;

namespace EggMdEditor.App;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things
    // aren't initialized yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        // Fixed execution configuration (T-012): the application always starts
        // in Spanish, with a new, ephemeral session (BR-F001-004,
        // BR-F001-006, AC-001) — no preference is read from disk or from
        // environment variables, there is no writing to disk and no network
        // access (C-001); the process logs go to standard output (T-007,
        // NFR-003). Verification is `dotnet test` at the root of the
        // implementation tree.
        var session = new ApplicationSession();
        var logger = new LifecycleLogger(session);
        var startup = new ApplicationStartup(session, logger);

        return startup.Run(
            () => BuildAvaloniaApp(session, logger)
                .StartWithClassicDesktopLifetime(args));
    }

    // Avalonia configuration: don't edit; used by design tooling.
    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(null, null);

    /// <summary>
    /// Builds the process's application sharing the startup's new session —
    /// always in Spanish (BR-F001-006) — and its lifecycle logger (T-007): the
    /// single SC-001 window the App creates and presents emits the process
    /// logs — LOG-001 at startup and LOG-003/ERR-005 at close (COM-004,
    /// T-011) — and any startup failure returns to the entry point as
    /// ERR-001/ERR-002 (T-012).
    /// </summary>
    public static AppBuilder BuildAvaloniaApp(
        ApplicationSession? processSession,
        LifecycleLogger? lifecycleLogger) =>
        AppBuilder.Configure(() => new App(processSession, lifecycleLogger))
            .UsePlatformDetect()
            .LogToTrace();
}
