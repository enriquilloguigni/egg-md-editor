using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Process execution configuration (T-012): the entry point fixes that the
// application always starts in Spanish without writing anything to disk
// (BR-F001-006) and without network access (C-001) — the session is born new
// with its Spec defaults and no preference is read —, and wires the startup
// T-011 deferred: LOG-001 with the beginning of the process and the end of
// the initialization when SC-001 is presented complete (AC-001),
// ERR-001/ERR-002 with error exit code and no window presented (AC-012), and
// the exit code the close (COM-004, T-011) leaves in the process.
public class ExecutionConfigurationTests
{
    [Fact]
    public void The_startup_logs_log001_with_start_and_end_of_initialization()
    {
        // LOG-001 (T-007, wiring T-012): the process logs the beginning and,
        // with SC-001 presented complete by the App wiring, the end of the
        // initialization — both with the shared session identifier.
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var errors = new StringWriter();
        var logger = new LifecycleLogger(session, logs, errors, clock: () => Instant);
        var startup = new ApplicationStartup(session, logger, exitCodeReader: () => 0);

        var code = startup.Run(() => WireAndShowWindow(session, logger));

        Assert.Equal(0, code);
        Assert.Equal(2, logs.Lines.Count);
        Assert.Contains("[Info]", logs.Lines[0]);
        Assert.Contains("LOG-001: inicio del proceso", logs.Lines[0]);
        Assert.Contains("idioma activo es (defecto de sesión", logs.Lines[0]);
        Assert.Contains(session.SessionId, logs.Lines[0]);
        Assert.Contains("[Info]", logs.Lines[1]);
        Assert.Contains("LOG-001: fin de la inicialización", logs.Lines[1]);
        Assert.Contains("catálogo i18n cargado", logs.Lines[1]);
        Assert.Contains("tiempo de arranque", logs.Lines[1]);
        Assert.Contains(session.SessionId, logs.Lines[1]);

        // C-001: nothing goes to the error output on a correct startup; every
        // log goes to standard output, with no writing to disk.
        Assert.Equal(string.Empty, errors.ToString());
    }

    [Fact]
    public void Every_startup_starts_from_the_spanish_default_reading_nothing_from_disk()
    {
        // BR-F001-006 (AC-001, AC-011): a session that ended in English does
        // not condition the next startup — there is nothing written to read —
        // and the new window is presented in Spanish with Español checked.
        var previousSession = new ApplicationSession();
        var previousStartup = CreateStartup(previousSession, new InMemoryLogOutput());
        previousStartup.Run(() =>
        {
            var (_, previousWindow) = WireAndShowWindow(
                previousSession, previousStartup.LifecycleLogger);
            previousWindow.LanguageService.Switch(InterfaceLanguage.English);
        });
        Assert.Equal(InterfaceLanguage.English, previousSession.ActiveLanguage);

        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var startup = CreateStartup(session, logs);

        startup.Run(() =>
        {
            var (_, window) = WireAndShowWindow(session, startup.LifecycleLogger);
            window.Show();

            var (spanish, english) = LanguageValuesOf(window);

            // AC-001: interface in Spanish, a single active language with
            // Tools → Interface language checked at Español.
            Assert.True(spanish.IsChecked);
            Assert.False(english.IsChecked);
            Assert.Equal(InterfaceLanguage.Spanish, session.ActiveLanguage);
        });

        // The new LOG-001 is born from the session default, not from the previous session.
        Assert.Contains(
            $"idioma activo es (defecto de sesión",
            logs.Lines[0]);
    }

    [Fact]
    public void The_process_window_shares_the_session_and_emits_log003_on_close()
    {
        // The process wiring (T-012) passes the startup session and its logger
        // to the single window: switching is seen in the session and the close
        // emits the correlated LOG-003 (COM-004, T-011).
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var startup = CreateStartup(session, logs);

        startup.Run(() =>
        {
            var (_, window) = WireAndShowWindow(session, startup.LifecycleLogger);
            window.Show();

            window.LanguageService.Switch(InterfaceLanguage.English);
            Assert.Equal(InterfaceLanguage.English, session.ActiveLanguage);

            window.Close();
        });

        Assert.Contains(logs.Lines, line => line.Contains("LOG-003: inicio del cierre"));
        Assert.Contains(logs.Lines, line => line.Contains("LOG-003: fin del proceso"));
        Assert.All(logs.Lines, line => Assert.Contains(session.SessionId, line));
    }

    [Fact]
    public void The_initialization_failure_applies_err001_and_returns_an_error_code()
    {
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var errors = new StringWriter();
        var logger = new LifecycleLogger(session, logs, errors, clock: () => Instant);
        var startup = new ApplicationStartup(session, logger, exitCodeReader: () => 0);
        var exception = new InvalidOperationException("el catálogo i18n inicial no carga");

        var code = startup.Run(() => throw exception);

        // AC-012 (ERR-001): the process ends with an error code, with the
        // Spanish message in the error output and the failure logged (LOG-001,
        // Error level); no window is presented.
        Assert.Equal(LifecycleLogger.FailureExitCode, code);
        Assert.Contains("Error al iniciar la aplicación:", errors.ToString());
        Assert.Contains(ExpectedExceptionTextOf(exception), errors.ToString());
        Assert.Equal(2, logs.Lines.Count);
        Assert.Contains("LOG-001: inicio del proceso", logs.Lines[0]);
        Assert.Contains("[Error]", logs.Lines[1]);
        Assert.Contains("LOG-001: arranque fallido", logs.Lines[1]);
        Assert.DoesNotContain("fin de la inicialización", logs.Lines[1]);
    }

    [Fact]
    public void The_failure_painting_the_window_after_its_creation_applies_err002()
    {
        // ERR-002: created window that never becomes fully presented — the
        // exception appears when presenting it, after its creation —. The
        // handling is the same LOG-001 result: error code and no usable
        // window.
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var errors = new StringWriter();
        var logger = new LifecycleLogger(session, logs, errors, clock: () => Instant);
        var startup = new ApplicationStartup(session, logger, exitCodeReader: () => 0);

        var code = startup.Run(() =>
        {
            var window = new MainWindow(session, lifecycleLogger: logger);
            window.Opened += (_, _) =>
                throw new IOException("fallo al pintar SC-001 al presentarse");
            window.Show();
        });

        Assert.Equal(LifecycleLogger.FailureExitCode, code);
        Assert.Contains("Error al iniciar la aplicación:", errors.ToString());
        Assert.Contains(typeof(IOException).FullName + ": fallo al pintar SC-001 al presentarse", errors.ToString());
        Assert.Equal(2, logs.Lines.Count);
        Assert.Contains("LOG-001: inicio del proceso", logs.Lines[0]);
        Assert.Contains("[Error]", logs.Lines[1]);
        Assert.Contains("LOG-001: arranque fallido", logs.Lines[1]);

        // The window never became fully presented: there is no end of the
        // initialization logged.
        Assert.DoesNotContain(
            logs.Lines,
            line => line.Contains("fin de la inicialización"));
    }

    [Fact]
    public void The_correct_startup_returns_the_exit_code_left_by_the_close()
    {
        // The close (COM-004, T-011) sets the process exit code: 0 on clean
        // close or ERR-005's one if the close or the release failed; the entry
        // point returns it as is.
        var session = new ApplicationSession();
        var cleanStartup = CreateStartup(
            session, new InMemoryLogOutput(), exitCodeReader: () => 0);
        var startupWithCloseFailure = CreateStartup(
            session,
            new InMemoryLogOutput(),
            exitCodeReader: () => LifecycleLogger.FailureExitCode);

        Assert.Equal(0, cleanStartup.Run(() => { }));
        Assert.Equal(
            LifecycleLogger.FailureExitCode,
            startupWithCloseFailure.Run(() => { }));
    }

    // --- Helpers ---

    private static readonly DateTimeOffset Instant = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

    private static ApplicationStartup CreateStartup(
        ApplicationSession session,
        InMemoryLogOutput logs,
        Func<int>? exitCodeReader = null) =>
        new(
            session,
            new LifecycleLogger(session, logs, clock: () => Instant),
            exitCodeReader);

    /// <summary>
    /// Wires the process's App over a desktop lifetime on the headless
    /// platform and presents the single window, as Avalonia's real startup
    /// does (T-012).
    /// </summary>
    private static (ClassicDesktopStyleApplicationLifetime Lifetime, MainWindow Window)
        WireAndShowWindow(ApplicationSession session, LifecycleLogger logger)
    {
        var app = new App(session, logger);
        app.Initialize();

        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        app.ApplicationLifetime = lifetime;
        app.OnFrameworkInitializationCompleted();

        var window = Assert.IsType<MainWindow>(lifetime.MainWindow);
        window.Show();

        return (lifetime, window);
    }

    private static (MenuItem Spanish, MenuItem English) LanguageValuesOf(MainWindow window)
    {
        var bar = window.FindControl<Menu>("MenuBar")
            ?? throw new InvalidOperationException("SC-001 without menu bar.");

        var values = bar.Items
            .Cast<MenuItem>()
            .SelectMany(menu => menu.Items.Cast<MenuItem>())
            .Single(entry => (string?)entry.Tag == MainMenuBuilder.LanguageEntryKey)
            .Items
            .Cast<MenuItem>()
            .ToList();

        return (values[0], values[1]);
    }

    private static string ExpectedExceptionTextOf(Exception exception) =>
        $"{exception.GetType().FullName}: {exception.Message}";

    /// <summary>In-memory log output, to verify the emitted lines.</summary>
    private sealed class InMemoryLogOutput : ILogOutput
    {
        public List<string> Lines { get; } = [];

        public void Write(LogLevel level, string message) => Lines.Add($"[{level}] {message}");
    }
}
