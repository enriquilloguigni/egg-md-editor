using System.IO;
using System.Runtime.InteropServices;
using EggMdEditor.App.Catalogs;

namespace EggMdEditor.App.Services;

/// <summary>
/// Startup and shutdown errors and logs of the process (T-007): core
/// component that materializes LOG-001 and LOG-003 and the handling of
/// ERR-001, ERR-002 and ERR-005, without touching the interface — the real
/// wiring of SC-001's startup and shutdown corresponds to T-011 and T-012.
///
/// <para>
/// Startup (LOG-001): logs the beginning of the process and the end of the
/// initialization (SC-001 window created, i18n catalog loaded, active
/// language), with the application and .NET runtime versions, the active
/// language and the startup time in ms. Shutdown (LOG-003): logs the
/// beginning of the shutdown and the end of the process, with the total
/// session duration in ms. Every log shares the session identifier, carries
/// an ISO 8601 timestamp and goes to standard output — nothing is written to
/// disk or leaves the machine (C-001, NFR-003).
/// </para>
///
/// <para>
/// Errors (ERR-001, ERR-002, ERR-005): upon a failure during process
/// initialization or SC-001 creation (ERR-001), while painting it during
/// startup (ERR-002) or while closing it and releasing the session (ERR-005),
/// writes the Spec's Spanish message to standard error and logs the failure
/// with level <see cref="LogLevel.Error"/>, with the exception's type and
/// message. The process must end with the returned error code
/// (<see cref="FailureExitCode"/>), with no residual processes and, at
/// startup, without presenting any window: applying that code and guaranteeing
/// the termination belongs to the wiring (T-011, T-012), not to this
/// component, which does not terminate the process on its own.
/// </para>
/// </summary>
public sealed class LifecycleLogger
{
    /// <summary>
    /// Process exit code upon a startup or shutdown failure (ERR-001, ERR-002,
    /// ERR-005): the user relaunches the application from the source code
    /// (A-001); there is no state to undo.
    /// </summary>
    public const int FailureExitCode = 1;

    private readonly ApplicationSession _session;
    private readonly ILogOutput _logOutput;
    private readonly TextWriter _errorOutput;
    private readonly Func<DateTimeOffset> _clock;
    private readonly string _applicationVersion;
    private readonly string _runtimeVersion;

    /// <summary>
    /// Reference instant for the startup time (LOG-001) and the total session
    /// duration (LOG-003): that of the creation of this component, which
    /// accompanies the creation of the session.
    /// </summary>
    private readonly DateTimeOffset _startupInstant;

    /// <summary>
    /// Creates the logger over the application session. The default log output
    /// is the one formatted to the process's standard output
    /// (<see cref="ConsoleLogOutput"/>) and the error one, standard error; the
    /// clock and the application and runtime versions are injectable for a
    /// deterministic log in tests.
    /// </summary>
    public LifecycleLogger(
        ApplicationSession session,
        ILogOutput? logOutput = null,
        TextWriter? errorOutput = null,
        Func<DateTimeOffset>? clock = null,
        string? applicationVersion = null,
        string? runtimeVersion = null)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logOutput = logOutput ?? new ConsoleLogOutput();
        _errorOutput = errorOutput ?? Console.Error;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _applicationVersion = string.IsNullOrWhiteSpace(applicationVersion)
            ? DefaultApplicationVersion()
            : applicationVersion;
        _runtimeVersion = string.IsNullOrWhiteSpace(runtimeVersion)
            ? RuntimeInformation.FrameworkDescription
            : runtimeVersion;
        _startupInstant = _clock();
    }

    /// <summary>Application version included in LOG-001.</summary>
    public string ApplicationVersion => _applicationVersion;

    /// <summary>.NET runtime version included in LOG-001.</summary>
    public string RuntimeVersion => _runtimeVersion;

    /// <summary>
    /// Logs the beginning of the process (LOG-001, <see cref="LogLevel.Info"/>):
    /// time, session, application and .NET runtime versions and active
    /// language, Spanish as session default (BR-007).
    /// </summary>
    public void LogProcessStart() =>
        Log(
            LogLevel.Info,
            "LOG-001: inicio del proceso; versión de la aplicación " +
            $"{_applicationVersion}; runtime {_runtimeVersion}; " +
            $"idioma activo {ActiveLanguageCode} (defecto de sesión, BR-007).");

    /// <summary>
    /// Logs the end of the initialization (LOG-001,
    /// <see cref="LogLevel.Info"/>): SC-001 window created, i18n catalog
    /// loaded and active language, with the startup time in ms. The wiring
    /// invokes it only if the window was created and painted completely; a
    /// failure on that path is logged with <see cref="LogStartupFailure"/>.
    /// </summary>
    public void LogInitializationEnd() =>
        Log(
            LogLevel.Info,
            "LOG-001: fin de la inicialización: ventana SC-001 creada, catálogo " +
            $"i18n cargado e idioma activo {ActiveLanguageCode}; " +
            $"tiempo de arranque {MillisecondsSince(_startupInstant)} ms.");

    /// <summary>
    /// Logs a startup failure (ERR-001: process initialization or SC-001
    /// creation; ERR-002: exception painting it, after its creation): writes
    /// to standard error, in Spanish, «Error al iniciar la aplicación:
    /// <c>exception type and message</c>», logs LOG-001 with level
    /// <see cref="LogLevel.Error"/> and returns the error code with which the
    /// process must end, with no residual processes and without presenting
    /// any window.
    /// </summary>
    public int LogStartupFailure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var typeAndMessage = TypeAndMessageOf(exception);
        _errorOutput.WriteLine($"Error al iniciar la aplicación: {typeAndMessage}");
        Log(
            LogLevel.Error,
            $"LOG-001: arranque fallido: {typeAndMessage}; no se presenta ninguna " +
            $"ventana y el proceso finaliza con código de error {FailureExitCode}, " +
            "sin procesos residuales (ERR-001, ERR-002).");

        return FailureExitCode;
    }

    /// <summary>
    /// Logs the beginning of SC-001's shutdown and of the release of the
    /// session resources (LOG-003, <see cref="LogLevel.Info"/>, COM-004).
    /// </summary>
    public void LogShutdownStart() =>
        Log(
            LogLevel.Info,
            "LOG-003: inicio del cierre de SC-001 y de la liberación de los " +
            "recursos de la sesión (COM-004).");

    /// <summary>
    /// Logs the end of the process (LOG-003, <see cref="LogLevel.Info"/>):
    /// clean shutdown, no residual processes, with the total session duration
    /// in ms. The language choice is not logged as a preference: it is not
    /// persisted in F-001 (FR-013 → F-030).
    /// </summary>
    public void LogProcessEnd() =>
        Log(
            LogLevel.Info,
            "LOG-003: fin del proceso; cierre limpio sin procesos residuales; " +
            $"duración total de la sesión {MillisecondsSince(_startupInstant)} ms.");

    /// <summary>
    /// Logs a failure closing SC-001 or releasing the session resources
    /// (ERR-005): writes to standard error, in Spanish, «Error al cerrar la
    /// aplicación: <c>exception type and message</c>», logs LOG-003 with level
    /// <see cref="LogLevel.Error"/> and returns the error code with which the
    /// process is terminated anyway, guaranteeing no residual processes remain;
    /// the language choice is not persisted (BR-F001-006, FR-013 → F-030).
    /// </summary>
    public int LogShutdownFailure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var typeAndMessage = TypeAndMessageOf(exception);
        _errorOutput.WriteLine($"Error al cerrar la aplicación: {typeAndMessage}");
        Log(
            LogLevel.Error,
            $"LOG-003: cierre fallido: {typeAndMessage}; el proceso se termina " +
            "igualmente, garantizando que no queden procesos residuales (ERR-005).");

        return FailureExitCode;
    }

    private string ActiveLanguageCode => I18nStringCatalog.CodeOf(_session.ActiveLanguage);

    /// <summary>
    /// Exception type and message, as the Spec's error message requires. The
    /// reflection separator of nested types is normalized to a dot, for a
    /// user-readable output.
    /// </summary>
    private static string TypeAndMessageOf(Exception exception) =>
        $"{exception.GetType().ToString().Replace('+', '.')}: {exception.Message}";

    /// <summary>
    /// Whole milliseconds elapsed since the given instant, with the injected
    /// clock; never negative against a clock that goes backwards.
    /// </summary>
    private long MillisecondsSince(DateTimeOffset instant) =>
        Math.Max(0, (long)Math.Round((_clock() - instant).TotalMilliseconds, MidpointRounding.AwayFromZero));

    /// <summary>
    /// Default application version: the application assembly's one.
    /// </summary>
    private static string DefaultApplicationVersion() =>
        typeof(LifecycleLogger).Assembly.GetName().Version?.ToString()
            ?? "desconocida";

    /// <summary>
    /// Logs through the log output: ISO 8601 timestamp and session identifier
    /// shared by every log (logs.md, NFR-003), with no writing to disk and no
    /// leaving the machine (C-001).
    /// </summary>
    private void Log(LogLevel level, string message) =>
        _logOutput.Write(
            level,
            $"{_clock().ToString("O")} [sesión {_session.SessionId}] {message}");
}
