using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Shutdown of SC-001 with release of the session (COM-004, T-011):
/// counterpart of the close order that ends the session releasing its data,
/// synchronous and in process.
///
/// <para>
/// The shutdown sequence is LOG-003's one (T-007): log the beginning of the
/// shutdown and of the release of the session resources, release and log the
/// end of the process with the total session duration. Releasing is letting
/// go of the session data: ephemeral state, with no external resources nor
/// writing to disk (C-001) — the language choice does not survive the session
/// (BR-F001-006, FR-013 → F-030). The release confirmation stays in
/// <see cref="SessionReleased"/>, which the window close exposes as the
/// confirmation of the COM-004 channel; with the session released, the
/// session has ended (ST-004) and all internal channels cease.
/// </para>
///
/// <para>
/// If an exception occurs while closing or releasing, ERR-005 applies: the
/// logger writes «Error al cerrar la aplicación: <c>exception type and
/// message</c>» to standard error and logs the failure (LOG-003, Error
/// level); the process is terminated anyway — no residual processes — with
/// the error code this component exposes in
/// <see cref="PendingExitCode"/> for the wiring to apply it to the process
/// (T-012). The language choice is not persisted on this path either
/// (BR-F001-006).
/// </para>
/// </summary>
public sealed class SessionShutdownManager
{
    private readonly LifecycleLogger? _lifecycleLogger;

    /// <summary>
    /// Creates the shutdown manager over the application session. The
    /// lifecycle logger is optional: with it, the shutdown logs LOG-003 and
    /// applies ERR-005 on failure; without it, the shutdown only releases and
    /// confirms.
    /// </summary>
    public SessionShutdownManager(ApplicationSession session, LifecycleLogger? lifecycleLogger = null)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        _lifecycleLogger = lifecycleLogger;
    }

    /// <summary>
    /// Application session this manager shuts down. After a confirmed release
    /// it remains exposed only as a reference to the ended session (ST-004):
    /// its data is not persisted and does not survive the session.
    /// </summary>
    public ApplicationSession Session { get; }

    /// <summary>
    /// Confirmation of the release of the session data (COM-004):
    /// <c>true</c> from the moment <see cref="Shutdown"/> completes the
    /// release. With the session released, the language choice is discarded —
    /// it is not persisted (FR-013 → F-030, BR-F001-006)—.
    /// </summary>
    public bool SessionReleased { get; private set; }

    /// <summary>
    /// Exit code with which the process must end if the shutdown failed
    /// (ERR-005): <c>0</c> while no failure has been applied.
    /// </summary>
    public int PendingExitCode { get; private set; }

    /// <summary>
    /// Executes the session shutdown (COM-004): logs the beginning of the
    /// shutdown (LOG-003), releases the session data —ephemeral, with no
    /// persistence (FR-013)—, confirms the release and logs the end of the
    /// process (LOG-003). Upon an exception in any of those steps, ERR-005
    /// applies — Spanish message through the error output and LOG-003 log
    /// with Error level, handled by the logger—, sets the pending exit code
    /// and rethrows, so the process is terminated anyway guaranteeing no
    /// residual processes remain.
    /// </summary>
    public void Shutdown()
    {
        try
        {
            _lifecycleLogger?.LogShutdownStart();

            // Release of the session data (COM-004): the state is ephemeral
            // and has no external resources; once the release is confirmed,
            // the data becomes ownerless and is not persisted — the language
            // choice dies with the session (FR-013 → F-030, BR-F001-006).
            SessionReleased = true;

            _lifecycleLogger?.LogProcessEnd();
        }
        catch (Exception exception)
        {
            // ERR-005: the process is terminated anyway, guaranteeing no
            // residual processes remain; the language choice is not
            // persisted (BR-F001-006, FR-013 → F-030).
            PendingExitCode = _lifecycleLogger is null
                ? LifecycleLogger.FailureExitCode
                : _lifecycleLogger.LogShutdownFailure(exception);

            throw;
        }
    }
}
