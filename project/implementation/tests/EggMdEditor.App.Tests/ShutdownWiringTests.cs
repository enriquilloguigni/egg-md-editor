using Avalonia.Controls;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Wiring of COM-004 in SC-001 (T-011): the window close —by shortcut, by the
// system order or by the user— releases the session with its LOG-003 log
// (T-007), confirms the release and persists nothing (AC-011, FR-013 →
// F-030, BR-F001-006); on close or release failure, ERR-005 with Spanish
// message through the error output and the process being terminated anyway
// with an error code (AC-013).
public class ShutdownWiringTests
{
    [Fact]
    public void Shutdown_logs_log003_and_confirms_the_session_release()
    {
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var manager = new SessionShutdownManager(
            session,
            new LifecycleLogger(session, logs, clock: () => Instant));

        manager.Shutdown();

        Assert.True(manager.SessionReleased);
        Assert.Equal(0, manager.PendingExitCode);

        // LOG-003: beginning of the shutdown and end of the process,
        // correlated by the session identifier (NFR-003).
        Assert.Equal(2, logs.Lines.Count);
        Assert.Contains("LOG-003: inicio del cierre de SC-001 y de la liberación", logs.Lines[0]);
        Assert.Contains("LOG-003: fin del proceso; cierre limpio sin procesos residuales", logs.Lines[1]);
        Assert.Contains(session.SessionId, logs.Lines[0]);
        Assert.Contains(session.SessionId, logs.Lines[1]);
    }

    [Fact]
    public void Shutdown_without_logger_releases_and_confirms_with_no_logs()
    {
        var manager = new SessionShutdownManager(new ApplicationSession());

        manager.Shutdown();

        Assert.True(manager.SessionReleased);
        Assert.Equal(0, manager.PendingExitCode);
    }

    [Fact]
    public void A_close_failure_applies_err005_and_leaves_an_error_exit_code()
    {
        var session = new ApplicationSession();
        var errors = new StringWriter();
        var logs = new FailingFirstLogOutput();
        var manager = new SessionShutdownManager(
            session,
            new LifecycleLogger(session, logs, errors, clock: () => Instant));

        var failed = Assert.Throws<IOException>(manager.Shutdown);

        // AC-013 (ERR-005): Spanish message through the error output, failure
        // logged (LOG-003, Error level) and the process terminated anyway.
        Assert.Contains("Error al cerrar la aplicación:", errors.ToString());
        Assert.Contains(ExpectedExceptionTextOf(failed), errors.ToString());
        Assert.Contains("cierre fallido", logs.Lines[0]);

        Assert.False(manager.SessionReleased);
        Assert.Equal(LifecycleLogger.FailureExitCode, manager.PendingExitCode);
    }

    [Fact]
    public void Closing_the_window_releases_the_session_with_its_log003_log()
    {
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var window = new MainWindow(
            session,
            lifecycleLogger: new LifecycleLogger(session, logs, clock: () => Instant));
        window.Show();
        var closed = false;
        window.Closed += (_, _) => closed = true;

        window.Close();

        // COM-004: with the close confirmed, the session is released (ST-004).
        Assert.True(closed);
        Assert.False(window.IsVisible);
        Assert.Equal(2, logs.Lines.Count);
        Assert.Contains("LOG-003: inicio del cierre", logs.Lines[0]);
        Assert.Contains("LOG-003: fin del proceso", logs.Lines[1]);
    }

    [Fact]
    public void The_close_shortcut_closes_the_window_through_the_same_com004_path()
    {
        // Chain COM-002 → COM-004: Ctrl/Cmd+Q dispatches the close and the
        // window closes releasing the session with its log.
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var window = new MainWindow(
            session,
            lifecycleLogger: new LifecycleLogger(session, logs, clock: () => Instant));
        window.Show();
        var closed = false;
        window.Closed += (_, _) => closed = true;

        var result = window.ShortcutDispatcher.Dispatch(
            new KeyCombination(ShortcutModifiers.Primary, 'Q'));

        Assert.Equal(ShortcutDispatchState.Dispatched, result.State);
        Assert.Equal(ShortcutAction.CloseApplication, result.Action);
        Assert.True(closed);
        Assert.Contains("LOG-003", string.Join("\n", logs.Lines));
    }

    [Fact]
    public void The_released_session_does_not_persist_the_language_choice()
    {
        // AC-011: closing with English active writes nothing to disk and the
        // choice dies with the session (FR-013 → F-030, BR-F001-006). In the
        // process: nothing is written and a new session starts in Spanish.
        var session = new ApplicationSession();
        var logs = new InMemoryLogOutput();
        var window = new MainWindow(
            session,
            lifecycleLogger: new LifecycleLogger(session, logs, clock: () => Instant));
        window.Show();

        window.LanguageService.Switch(InterfaceLanguage.English);
        window.Close();

        Assert.False(window.IsVisible);
        Assert.All(logs.Lines, line => Assert.DoesNotContain("persist", line));
        Assert.Equal(InterfaceLanguage.Spanish, new ApplicationSession().ActiveLanguage);
    }

    // --- Helpers ---

    private static readonly DateTimeOffset Instant = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

    private static string ExpectedExceptionTextOf(Exception exception) =>
        $"{exception.GetType().FullName}: {exception.Message}";

    /// <summary>In-memory log output, to verify the emitted lines.</summary>
    private sealed class InMemoryLogOutput : ILogOutput
    {
        public List<string> Lines { get; } = [];

        public void Write(LogLevel level, string message) => Lines.Add($"[{level}] {message}");
    }

    /// <summary>
    /// Output whose first log fails —the log channel breaks right when
    /// logging the beginning of the shutdown— and that captures the following
    /// ones, to exercise the full ERR-005 path.
    /// </summary>
    private sealed class FailingFirstLogOutput : ILogOutput
    {
        private int _calls;

        public List<string> Lines { get; } = [];

        public void Write(LogLevel level, string message)
        {
            if (++_calls == 1)
            {
                throw new IOException("La salida de registros no está disponible.");
            }

            Lines.Add($"[{level}] {message}");
        }
    }
}
