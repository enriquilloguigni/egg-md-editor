using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Process execution configuration (T-012): application startup sequence,
/// fixed at the entry point — <c>Program.Main</c> — with these invariable
/// properties:
///
/// <list type="bullet">
/// <item>The process always starts in Spanish: the session is created new and
/// ephemeral with its Spec defaults (BR-F001-004, BR-007) and no preference is
/// read from anywhere — neither from disk nor from environment variables —, so
/// a previous session's choice never conditions a new startup (BR-F001-006,
/// AC-001, AC-011).</item>
/// <item>Nothing in the startup writes to disk or accesses the network
/// (C-001): the lifecycle logs go to the process's standard output (T-007,
/// NFR-003).</item>
/// <item>The verification command is <c>dotnet test</c> at the root of the
/// implementation tree (<c>egg-md-editor.slnx</c> solution).</item>
/// </list>
///
/// <para>
/// The sequence is LOG-001's one (T-007): log the beginning of the process
/// and, when SC-001 is created and presented complete, the end of the
/// initialization — that second log is emitted by the window wiring
/// (<see cref="App"/>, T-012)—. If the interface fails at startup, whether in
/// process initialization or SC-001 creation (ERR-001), or in its painting
/// after creation (ERR-002), the failure is logged and the error code with
/// which the process must end is returned, without presenting any window and
/// with no residual processes; the exception is not rethrown. With a correct
/// startup, the exit code is the one the window close (COM-004, T-011) left
/// in the process: 0 on clean close or ERR-005's one on close failure.
/// </para>
///
/// <para>
/// The lifecycle logger is optional — as in the rest of the services, it is
/// created over the given session if it does not arrive — and the process
/// exit code reader is injectable for deterministic tests.
/// </para>
/// </summary>
public sealed class ApplicationStartup
{
    private readonly LifecycleLogger _lifecycleLogger;
    private readonly Func<int> _exitCodeReader;

    /// <summary>
    /// Creates the execution configuration over the process's new session.
    /// Without a logger, a default one is created over the session; without an
    /// exit code reader, the process's one is read
    /// (<see cref="Environment.ExitCode"/>, set by T-011's close).
    /// </summary>
    public ApplicationStartup(
        ApplicationSession session,
        LifecycleLogger? lifecycleLogger = null,
        Func<int>? exitCodeReader = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        Session = session;
        _lifecycleLogger = lifecycleLogger ?? new LifecycleLogger(session);
        _exitCodeReader = exitCodeReader ?? (() => Environment.ExitCode);
    }

    /// <summary>New, ephemeral startup session: always in Spanish (BR-F001-006).</summary>
    public ApplicationSession Session { get; }

    /// <summary>
    /// Lifecycle logger of the process (T-007): emits LOG-001 at startup and
    /// shares the session with the single window, so its close emits LOG-003
    /// and applies ERR-005 (COM-004, T-011).
    /// </summary>
    public LifecycleLogger LifecycleLogger => _lifecycleLogger;

    /// <summary>
    /// Runs the process startup: logs the beginning (LOG-001) and launches the
    /// interface — the delegate that builds and runs Avalonia —. If the
    /// interface fails, applies ERR-001/ERR-002 with the logger and returns
    /// the error code; if it starts, returns the exit code the close (COM-004)
    /// left in the process. In both cases the process ends with no residual
    /// processes.
    /// </summary>
    public int Run(Action startUi)
    {
        ArgumentNullException.ThrowIfNull(startUi);

        // LOG-001: beginning of the process — application and runtime
        // versions, and Spanish active language as session default (BR-007).
        _lifecycleLogger.LogProcessStart();

        try
        {
            startUi();
        }
        catch (Exception exception)
        {
            // ERR-001 / ERR-002: no window is presented —none usable—, the
            // Spanish message goes to the error output, the failure is logged
            // (LOG-001, Error level) and the process ends with an error code,
            // with no residual processes.
            return _lifecycleLogger.LogStartupFailure(exception);
        }

        // Correct startup (LOG-001 end of initialization emitted by the window
        // when presented complete): the exit code was left by the close
        // (COM-004, T-011) — 0 on clean close or ERR-005's one.
        return _exitCodeReader();
    }
}
