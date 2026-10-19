namespace EggMdEditor.App.Services;

/// <summary>
/// Levels of the technical logs (logs.md): <c>Debug</c> for full success,
/// <c>Warning</c> for success with degradation and <c>Error</c> for failure.
/// The names are the literal tokens that logs.md and error-handling.md use
/// when declaring the level of each LOG-xxx.
/// </summary>
public enum LogLevel
{
    Debug = 1,
    Info = 2,
    Warning = 3,
    Error = 4,
}

/// <summary>
/// Output of the process's technical logs (NFR-003): every log shares the
/// session identifier, carries an ISO 8601 timestamp and goes to standard
/// output — nothing is written to disk or leaves the machine (C-001). The
/// implementation with formatting and destination on standard output is wired
/// with the startup and shutdown logs (LOG-001, LOG-003); services emit the
/// full log through this interface.
/// </summary>
public interface ILogOutput
{
    /// <summary>Writes an already composed log, with its level.</summary>
    void Write(LogLevel level, string message);
}

/// <summary>
/// Null log output: default object of the services while the real output is
/// not wired. It writes nowhere; the output formatted to the process's
/// standard output is <see cref="ConsoleLogOutput"/> (T-007) and its wiring
/// in SC-001 arrives with T-011.
/// </summary>
public sealed class NullLogOutput : ILogOutput
{
    /// <summary>Single stateless instance.</summary>
    public static NullLogOutput Instance { get; } = new();

    private NullLogOutput()
    {
    }

    /// <summary>Does nothing: the output is not wired.</summary>
    public void Write(LogLevel level, string message)
    {
    }
}

/// <summary>
/// Log output formatted to the process's standard output (T-007): the
/// implementation of the contract that the startup and shutdown logs
/// (LOG-001, LOG-003) and the rest of the services wire to the process. The
/// message arrives already composed with its ISO 8601 timestamp and session
/// identifier (correlation, NFR-003); this output adds the literal token of
/// the level (<c>Debug</c>, <c>Info</c>, <c>Warning</c>, <c>Error</c>, the
/// tokens of logs.md) and writes one line per log. Nothing is written to disk
/// or leaves the machine (C-001). The writer is injectable for tests; by
/// default it is standard output.
/// </summary>
public sealed class ConsoleLogOutput : ILogOutput
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Creates the output over the given writer; without a writer, over the
    /// process's standard output (<see cref="Console.Out"/>).
    /// </summary>
    public ConsoleLogOutput(TextWriter? writer = null) =>
        _writer = writer ?? Console.Out;

    /// <summary>Writes the log as one line with its level in front.</summary>
    public void Write(LogLevel level, string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _writer.WriteLine($"[{level}] {message}");
    }
}
