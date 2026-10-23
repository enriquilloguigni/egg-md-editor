using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App.Startup;

/// <summary>
/// Códigos de salida del proceso. ERR-001/ERR-002 exigen finalizar con un
/// código de error; ERR-005 termina igualmente el proceso aunque falle la
/// liberación de recursos.
/// </summary>
public static class ExitCodes
{
    public const int Success = 0;
    public const int Error = 1;

    public const int StartupFailure = Error;
    public const int ShutdownFailure = Error;
}

/// <summary>
/// Ciclo de vida técnico del proceso (LOG-001 y LOG-003): registra el inicio
/// del proceso y el fin de la inicialización de SC-001, y el inicio y fin del
/// cierre. En los fallos de arranque o cierre (ERR-001, ERR-002, ERR-005)
/// escribe el mensaje en español por la salida estándar de error y asegura un
/// código de salida que no deja el proceso en un estado ambiguo.
/// Ningún registro se escribe a disco ni sale del equipo (NFR-010).
/// </summary>
public sealed class ApplicationLifecycle : IDisposable
{
    /// <summary>Prefijo literal del mensaje de error de arranque (ERR-001/ERR-002).</summary>
    public const string StartupErrorMessagePrefix = "Error al iniciar la aplicación: ";

    /// <summary>Prefijo literal del mensaje de error de cierre (ERR-005).</summary>
    public const string ShutdownErrorMessagePrefix = "Error al cerrar la aplicación: ";

    private static readonly object CurrentSync = new();
    private static ApplicationLifecycle? _current;

    private readonly object _sync = new();
    private readonly ILogger _logger;
    private readonly ILoggerFactory? _ownedLoggerFactory;
    private readonly TextWriter _errorOutput;
    private readonly Stopwatch _startupWatch = new();
    private readonly Stopwatch _sessionWatch = new();

    private Exception? _startupFailure;
    private Exception? _shutdownFailure;
    private bool _startupStarted;
    private bool _startupCompleted;
    private bool _startupPresented;
    private bool _shutdownStarted;
    private bool _shutdownCompleted;
    private bool _disposed;
    private int _exitCode;

    /// <summary>
    /// Crea el ciclo de vida con su propio registro por consola. Es la
    /// composición usada por <c>Program</c>.
    /// </summary>
    public ApplicationLifecycle()
        : this(
            ApplicationLogging.CreateLoggerFactory(),
            Console.Error,
            sessionId: null,
            ownsLoggerFactory: true)
    {
    }

    /// <summary>Crea el ciclo de vida reutilizando una factoría de registro.</summary>
    public ApplicationLifecycle(ILoggerFactory loggerFactory)
        : this(loggerFactory, Console.Error, sessionId: null, ownsLoggerFactory: false)
    {
    }

    /// <summary>
    /// Crea el ciclo de vida con la salida de error indicada (útil para
    /// pruebas y para redirigir el mensaje sin tocar la consola global).
    /// </summary>
    public ApplicationLifecycle(ILoggerFactory loggerFactory, TextWriter errorOutput)
        : this(loggerFactory, errorOutput, sessionId: null, ownsLoggerFactory: false)
    {
    }

    /// <summary>Crea el ciclo de vida con una sesión explícita.</summary>
    public ApplicationLifecycle(
        ILoggerFactory loggerFactory,
        TextWriter errorOutput,
        string? sessionId)
        : this(loggerFactory, errorOutput, sessionId, ownsLoggerFactory: false)
    {
    }

    /// <summary>Crea el ciclo de vida sobre un logger ya compuesto.</summary>
    public ApplicationLifecycle(ILogger logger)
        : this(logger, Console.Error, sessionId: null)
    {
    }

    /// <summary>
    /// Crea el ciclo de vida sobre un logger ya compuesto y la salida de error
    /// indicada.
    /// </summary>
    public ApplicationLifecycle(ILogger logger, TextWriter errorOutput, string? sessionId = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(errorOutput);

        SessionId = NormalizeSessionId(sessionId);
        _logger = ApplicationLogging.CreateSessionLogger(logger, SessionId);
        _errorOutput = errorOutput;
        ApplicationVersion = ResolveApplicationVersion();
        RuntimeVersion = RuntimeInformation.FrameworkDescription;
    }

    private ApplicationLifecycle(
        ILoggerFactory loggerFactory,
        TextWriter errorOutput,
        string? sessionId,
        bool ownsLoggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(errorOutput);

        LoggerFactory = loggerFactory;
        _ownedLoggerFactory = ownsLoggerFactory ? loggerFactory : null;
        SessionId = NormalizeSessionId(sessionId);
        _logger = ApplicationLogging.CreateSessionLogger(
            loggerFactory,
            ApplicationLogging.LifecycleCategory,
            SessionId);
        _errorOutput = errorOutput;
        ApplicationVersion = ResolveApplicationVersion();
        RuntimeVersion = RuntimeInformation.FrameworkDescription;
    }

    /// <summary>
    /// Ciclo de vida de la ejecución actual. Lo fija la composición de
    /// <c>Program</c> para que <see cref="App"/> use la misma sesión y el mismo
    /// registro de LOG-001/LOG-003.
    /// </summary>
    public static ApplicationLifecycle? Current
    {
        get
        {
            lock (CurrentSync)
            {
                return _current;
            }
        }

        set
        {
            lock (CurrentSync)
            {
                _current = value;
            }
        }
    }

    /// <summary>Identificador de sesión compartido por todos los registros.</summary>
    public string SessionId { get; }

    /// <summary>Versión de la aplicación (LOG-001).</summary>
    public string ApplicationVersion { get; }

    /// <summary>Runtime .NET en ejecución (LOG-001).</summary>
    public string RuntimeVersion { get; }

    /// <summary>Factoría de registro asociada; <c>null</c> si solo se inyectó un logger.</summary>
    public ILoggerFactory? LoggerFactory { get; }

    public bool IsStartupStarted
    {
        get
        {
            lock (_sync)
            {
                return _startupStarted;
            }
        }
    }

    /// <summary>LOG-001: la inicialización terminó con éxito.</summary>
    public bool IsStartupCompleted
    {
        get
        {
            lock (_sync)
            {
                return _startupCompleted;
            }
        }
    }

    /// <summary>ERR-002: SC-001 llegó a presentarse por primera vez.</summary>
    public bool IsStartupPresented
    {
        get
        {
            lock (_sync)
            {
                return _startupPresented;
            }
        }
    }

    /// <summary>ERR-001/ERR-002: hubo un fallo de arranque.</summary>
    public bool HasStartupFailure
    {
        get
        {
            lock (_sync)
            {
                return _startupFailure is not null;
            }
        }
    }

    /// <summary>ERR-005: hubo un fallo al cerrar o liberar la sesión.</summary>
    public bool HasShutdownFailure
    {
        get
        {
            lock (_sync)
            {
                return _shutdownFailure is not null;
            }
        }
    }

    /// <summary>LOG-003: el cierre de SC-001 ya comenzó.</summary>
    public bool IsShutdownStarted
    {
        get
        {
            lock (_sync)
            {
                return _shutdownStarted;
            }
        }
    }

    public bool IsShutdownCompleted
    {
        get
        {
            lock (_sync)
            {
                return _shutdownCompleted;
            }
        }
    }

    /// <summary>Código de salida que corresponde al resultado registrado.</summary>
    public int ExitCode
    {
        get
        {
            lock (_sync)
            {
                return _exitCode;
            }
        }
    }

    /// <summary>Tiempo de arranque en ms (LOG-001).</summary>
    public long StartupElapsedMilliseconds
    {
        get
        {
            lock (_sync)
            {
                return _startupWatch.ElapsedMilliseconds;
            }
        }
    }

    /// <summary>Duración total de la sesión en ms (LOG-003).</summary>
    public long SessionElapsedMilliseconds
    {
        get
        {
            lock (_sync)
            {
                return _sessionWatch.ElapsedMilliseconds;
            }
        }
    }

    public Exception? StartupFailure
    {
        get
        {
            lock (_sync)
            {
                return _startupFailure;
            }
        }
    }

    public Exception? ShutdownFailure
    {
        get
        {
            lock (_sync)
            {
                return _shutdownFailure;
            }
        }
    }

    /// <summary>
    /// LOG-001: registra el inicio del proceso. Idempotente; si el arranque
    /// todavía no se había registrado, lo registra antes de un fallo.
    /// </summary>
    public void BeginStartup()
    {
        bool logStart = false;
        lock (_sync)
        {
            if (_disposed || _startupStarted)
            {
                return;
            }

            _startupStarted = true;
            _startupWatch.Start();
            _sessionWatch.Start();
            logStart = true;
        }

        if (logStart)
        {
            LogInformation(
                "Inicio del proceso. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
                "Versión de la aplicación: {ApplicationVersion}. Runtime: {RuntimeVersion}.",
                DateTimeOffset.UtcNow,
                SessionId,
                ApplicationVersion,
                RuntimeVersion);
        }
    }

    /// <summary>
    /// LOG-001: registra el fin correcto de la inicialización de SC-001 con el
    /// idioma activo y el tiempo de arranque. Se invoca cuando la ventana ya
    /// se ha presentado por primera vez.
    /// </summary>
    public void MarkInitialized(string? activeLanguage)
    {
        BeginStartup();

        long elapsedMilliseconds;
        lock (_sync)
        {
            if (_disposed
                || _startupFailure is not null
                || _startupCompleted
                || _shutdownStarted
                || _shutdownCompleted)
            {
                return;
            }

            _startupCompleted = true;
            _startupPresented = true;
            _startupWatch.Stop();
            elapsedMilliseconds = _startupWatch.ElapsedMilliseconds;
        }

        LogInformation(
            "Fin de la inicialización. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
            "Idioma activo: {ActiveLanguage}. Tiempo de arranque: {StartupMilliseconds} ms.",
            DateTimeOffset.UtcNow,
            SessionId,
            string.IsNullOrWhiteSpace(activeLanguage) ? "es" : activeLanguage,
            elapsedMilliseconds);
    }

    /// <summary>
    /// ERR-002: SC-001 se presentó por primera vez. A partir de aquí un fallo
    /// de pintado ya no es de arranque (no tiene ERR en F-001).
    /// </summary>
    public void MarkPresented()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _startupPresented = true;
        }
    }

    /// <summary>
    /// ERR-001/ERR-002: escribe el mensaje en español por la salida de error,
    /// registra el fallo como LOG-001 y devuelve el código de salida. Es
    /// idempotente: el mismo fallo no produce mensajes ni registros duplicados.
    /// </summary>
    public int ReportStartupFailure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        BeginStartup();

        long elapsedMilliseconds;
        lock (_sync)
        {
            if (_startupFailure is not null)
            {
                return _exitCode;
            }

            _startupFailure = exception;
            _startupCompleted = false;
            _exitCode = ExitCodes.StartupFailure;
            _startupWatch.Stop();
            elapsedMilliseconds = _startupWatch.ElapsedMilliseconds;
        }

        WriteError(StartupErrorMessagePrefix, exception);
        LogError(
            exception,
            "Error al iniciar la aplicación. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
            "Versión de la aplicación: {ApplicationVersion}. Runtime: {RuntimeVersion}. " +
            "Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}. " +
            "Tiempo de arranque: {StartupMilliseconds} ms.",
            DateTimeOffset.UtcNow,
            SessionId,
            ApplicationVersion,
            RuntimeVersion,
            exception.GetType().Name,
            exception.Message,
            elapsedMilliseconds);

        return ExitCodes.StartupFailure;
    }

    /// <summary>
    /// LOG-003: registra el inicio del cierre de SC-001. No se registra si el
    /// proceso nunca llegó a arrancar correctamente: en ese caso el fallo de
    /// arranque (LOG-001) es el diagnóstico relevante.
    /// </summary>
    public void BeginShutdown()
    {
        bool logStart = false;
        lock (_sync)
        {
            if (_disposed || _shutdownStarted || _startupFailure is not null)
            {
                return;
            }

            _shutdownStarted = true;
            if (!_sessionWatch.IsRunning)
            {
                _sessionWatch.Start();
            }

            logStart = true;
        }

        if (logStart)
        {
            LogInformation(
                "Inicio del cierre. Hora: {Timestamp:O}. Sesión: {SessionId}.",
                DateTimeOffset.UtcNow,
                SessionId);
        }
    }

    /// <summary>
    /// ERR-005: escribe el mensaje en español por la salida de error y
    /// registra el fallo como LOG-003. El proceso debe terminar igualmente; no
    /// hay rollback ni persistencia. Idempotente.
    /// </summary>
    public int ReportShutdownFailure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (HasStartupFailure)
        {
            // No hubo una SC-001 usable que cerrar; el diagnóstico es LOG-001.
            return ExitCode;
        }

        BeginShutdown();

        long elapsedMilliseconds;
        lock (_sync)
        {
            if (_startupFailure is not null)
            {
                return _exitCode;
            }

            if (_shutdownFailure is not null)
            {
                return _exitCode;
            }

            _shutdownFailure = exception;
            _exitCode = ExitCodes.ShutdownFailure;
            _sessionWatch.Stop();
            elapsedMilliseconds = _sessionWatch.ElapsedMilliseconds;
        }

        WriteError(ShutdownErrorMessagePrefix, exception);
        LogError(
            exception,
            "Error al cerrar la aplicación. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
            "Tipo: {ExceptionType}. Mensaje: {ExceptionMessage}. " +
            "Duración total de la sesión: {SessionMilliseconds} ms.",
            DateTimeOffset.UtcNow,
            SessionId,
            exception.GetType().Name,
            exception.Message,
            elapsedMilliseconds);

        return ExitCodes.ShutdownFailure;
    }

    /// <summary>
    /// LOG-003: registra el fin del proceso con la duración total de la
    /// sesión. Si se produce una excepción de liberación y aún no se había
    /// notificado, se delega en <see cref="ReportShutdownFailure"/>. Idempotente.
    /// </summary>
    public int CompleteShutdown(Exception? exception = null)
    {
        if (exception is not null)
        {
            ReportShutdownFailure(exception);
        }

        if (HasStartupFailure)
        {
            lock (_sync)
            {
                _shutdownCompleted = true;
                return _exitCode;
            }
        }

        BeginShutdown();

        bool clean = false;
        long elapsedMilliseconds;
        int exitCode;
        lock (_sync)
        {
            if (_shutdownCompleted)
            {
                return _exitCode;
            }

            _shutdownCompleted = true;
            _sessionWatch.Stop();
            elapsedMilliseconds = _sessionWatch.ElapsedMilliseconds;
            clean = _shutdownFailure is null;
            exitCode = _exitCode;
        }

        if (clean)
        {
            LogInformation(
                "Fin del proceso. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
                "Duración total de la sesión: {SessionMilliseconds} ms.",
                DateTimeOffset.UtcNow,
                SessionId,
                elapsedMilliseconds);
        }

        return exitCode;
    }

    public void Dispose()
    {
        ILoggerFactory? ownedFactory;
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ownedFactory = _ownedLoggerFactory;
        }

        try
        {
            ownedFactory?.Dispose();
        }
        catch (Exception)
        {
            // El cierre del registro no debe impedir la finalización.
        }
    }

    /// <summary>
    /// Formatea la parte «tipo y mensaje» del mensaje de error exigido por
    /// ERR-001/ERR-002/ERR-005.
    /// </summary>
    public static string FormatExceptionDetails(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return $"{exception.GetType().Name}: {exception.Message}";
    }

    private static string NormalizeSessionId(string? sessionId) =>
        string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId.Trim();

    private static string ResolveApplicationVersion()
    {
        var version = typeof(ApplicationLifecycle).Assembly.GetName().Version;
        return version?.ToString() ?? "desconocida";
    }

    private void WriteError(string prefix, Exception exception)
    {
        try
        {
            _errorOutput.WriteLine(prefix + FormatExceptionDetails(exception));
            _errorOutput.Flush();
        }
        catch (Exception)
        {
            // Un fallo del flujo de error no debe ocultar el fallo original.
        }
    }

    private void LogInformation(string message, params object?[] args)
    {
        try
        {
            _logger.LogInformation(message, args);
        }
        catch (Exception)
        {
            // El diagnóstico nunca debe sustituir a la operación.
        }
    }

    private void LogError(Exception exception, string message, params object?[] args)
    {
        try
        {
            _logger.LogError(exception, message, args);
        }
        catch (Exception)
        {
            // El diagnóstico nunca debe sustituir a la operación.
        }
    }
}
