using System.IO;
using EggMdEditor.App.Catalogs;
using EggMdEditor.App.Services;
using Xunit;

namespace EggMdEditor.App.Tests;

// Startup and shutdown errors and logs (T-007): LOG-001 for startup
// (beginning of the process and end of the initialization), LOG-003 for
// shutdown (beginning of the shutdown and end of the process), every log
// with ISO 8601 timestamp and shared session identifier, to standard output
// and with no writing to disk (C-001, NFR-003); ERR-001, ERR-002 and ERR-005
// with the Spanish message to the standard error output.
public class LifecycleLoggerTests
{
    private static readonly DateTimeOffset FixedInstant =
        new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void The_process_start_logs_LOG001_as_Info_with_version_runtime_and_language()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var standardError = new StringWriter();
        var logger = new LifecycleLogger(
            session, captured, standardError, clock: () => FixedInstant,
            applicationVersion: "1.2.3.0", runtimeVersion: ".NET 10.0.0");

        logger.LogProcessStart();

        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Info, log.Level);
        Assert.Contains("LOG-001", log.Message);
        Assert.Contains(FixedInstant.ToString("O"), log.Message);
        Assert.Contains(session.SessionId, log.Message);
        Assert.Contains("1.2.3.0", log.Message);
        Assert.Contains(".NET 10.0.0", log.Message);
        Assert.Contains("idioma activo es", log.Message);

        // Without failure there is no error output.
        Assert.Empty(standardError.ToString());
    }

    [Fact]
    public void The_initialization_end_logs_the_startup_time_and_the_active_language()
    {
        var session = new ApplicationSession();
        var clock = new TestClock(FixedInstant);
        var captured = new CapturedOutput();
        var logger = new LifecycleLogger(
            session, captured, new StringWriter(), clock.Clock,
            applicationVersion: "1.0.0.0", runtimeVersion: ".NET 10.0.0");

        clock.Advance(TimeSpan.FromMilliseconds(250));
        session.ActiveLanguage = InterfaceLanguage.English;
        logger.LogInitializationEnd();

        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Info, log.Level);
        Assert.Contains("LOG-001", log.Message);
        Assert.Contains("ventana SC-001 creada", log.Message);
        Assert.Contains("catálogo i18n cargado", log.Message);
        Assert.Contains("idioma activo en", log.Message);
        Assert.Contains("tiempo de arranque 250 ms", log.Message);
    }

    [Fact]
    public void The_startup_failure_goes_in_spanish_to_standard_error_and_logs_LOG001_Error_ERR001_ERR002()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var standardError = new StringWriter();
        var logger = new LifecycleLogger(
            session, captured, standardError, clock: () => FixedInstant,
            applicationVersion: "1.0.0.0", runtimeVersion: ".NET 10.0.0");
        var exception = new InvalidOperationException("el catálogo i18n inicial no carga");

        var code = logger.LogStartupFailure(exception);

        // The process ends with an error code (ERR-001, ERR-002).
        Assert.Equal(LifecycleLogger.FailureExitCode, code);
        Assert.Equal(1, code);

        // Message in Spanish, exactly the Spec's, on the standard error
        // output: «Error al iniciar la aplicación: <type and message of the
        // exception>».
        var lines = standardError.ToString().Split('\n', StringSplitOptions.TrimEntries);
        var line = Assert.Single(lines, l => l.Length > 0);
        Assert.Equal(
            "Error al iniciar la aplicación: " +
            "System.InvalidOperationException: el catálogo i18n inicial no carga",
            line);

        // Required log: LOG-001 as Error, with the exception's type and message.
        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Contains("LOG-001", log.Message);
        Assert.Contains("System.InvalidOperationException: el catálogo i18n inicial no carga", log.Message);
        Assert.Contains(session.SessionId, log.Message);
        Assert.Contains(FixedInstant.ToString("O"), log.Message);
    }

    [Fact]
    public void The_painting_failure_during_startup_uses_the_same_message_and_log_as_ERR001()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var standardError = new StringWriter();
        var logger = new LifecycleLogger(
            session, captured, standardError, clock: () => FixedInstant);

        var code = logger.LogStartupFailure(
            new TestException("fallo al pintar SC-001 durante el arranque"));

        Assert.Equal(LifecycleLogger.FailureExitCode, code);
        Assert.Contains(
            "Error al iniciar la aplicación: " +
            "EggMdEditor.App.Tests.LifecycleLoggerTests.TestException: " +
            "fallo al pintar SC-001 durante el arranque",
            standardError.ToString());
        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Contains("(ERR-001, ERR-002)", log.Message);
    }

    [Fact]
    public void The_shutdown_logs_start_and_end_with_the_total_session_duration_LOG003()
    {
        var session = new ApplicationSession();
        var clock = new TestClock(FixedInstant);
        var captured = new CapturedOutput();
        var standardError = new StringWriter();
        var logger = new LifecycleLogger(
            session, captured, standardError, clock.Clock);

        clock.Advance(TimeSpan.FromMilliseconds(1250));
        logger.LogShutdownStart();
        logger.LogProcessEnd();

        Assert.Equal(2, captured.Logs.Count);
        Assert.All(captured.Logs, l => Assert.Equal(LogLevel.Info, l.Level));
        Assert.Contains("LOG-003", captured.Logs[0].Message);
        Assert.Contains("inicio del cierre de SC-001", captured.Logs[0].Message);
        Assert.Contains("(COM-004)", captured.Logs[0].Message);
        Assert.Contains("fin del proceso", captured.Logs[1].Message);
        Assert.Contains("cierre limpio sin procesos residuales", captured.Logs[1].Message);
        Assert.Contains("duración total de la sesión 1250 ms", captured.Logs[1].Message);

        // Without failure there is no error output.
        Assert.Empty(standardError.ToString());
    }

    [Fact]
    public void The_close_failure_goes_in_spanish_to_standard_error_and_logs_LOG003_Error_ERR005()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var standardError = new StringWriter();
        var logger = new LifecycleLogger(
            session, captured, standardError, clock: () => FixedInstant);
        var exception = new InvalidOperationException("fallo al liberar los recursos de la sesión");

        var code = logger.LogShutdownFailure(exception);

        // The process is terminated anyway, with an error code (ERR-005).
        Assert.Equal(LifecycleLogger.FailureExitCode, code);

        // Message in Spanish, exactly the Spec's, on the standard error output.
        var lines = standardError.ToString().Split('\n', StringSplitOptions.TrimEntries);
        var line = Assert.Single(lines, l => l.Length > 0);
        Assert.Equal(
            "Error al cerrar la aplicación: " +
            "System.InvalidOperationException: fallo al liberar los recursos de la sesión",
            line);

        var log = Assert.Single(captured.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Contains("LOG-003", log.Message);
        Assert.Contains("System.InvalidOperationException: fallo al liberar los recursos de la sesión", log.Message);
        Assert.Contains("(ERR-005)", log.Message);
        Assert.Contains(session.SessionId, log.Message);
    }

    [Fact]
    public void The_whole_lifecycle_shares_session_identifier_and_ISO8601_timestamp()
    {
        var session = new ApplicationSession();
        var captured = new CapturedOutput();
        var logger = new LifecycleLogger(
            session, captured, new StringWriter(), clock: () => FixedInstant);

        logger.LogProcessStart();
        logger.LogInitializationEnd();
        logger.LogShutdownStart();
        logger.LogProcessEnd();

        Assert.Equal(4, captured.Logs.Count);
        Assert.All(captured.Logs, log =>
        {
            // Correlation: unique session and ISO 8601 time in every log.
            Assert.Contains(session.SessionId, log.Message);
            Assert.Contains(FixedInstant.ToString("O"), log.Message);
        });
    }

    [Fact]
    public void By_default_exposes_the_assembly_and_runtime_versions()
    {
        var logger = new LifecycleLogger(new ApplicationSession());

        Assert.False(string.IsNullOrWhiteSpace(logger.ApplicationVersion));
        Assert.False(string.IsNullOrWhiteSpace(logger.RuntimeVersion));
    }

    [Fact]
    public void A_null_session_is_a_programming_error()
    {
        Assert.Throws<ArgumentNullException>(
            () => new LifecycleLogger(null!));
    }

    [Fact]
    public void A_null_exception_is_a_programming_error()
    {
        var logger = new LifecycleLogger(
            new ApplicationSession(), new CapturedOutput(), new StringWriter());

        Assert.Throws<ArgumentNullException>(() => logger.LogStartupFailure(null!));
        Assert.Throws<ArgumentNullException>(() => logger.LogShutdownFailure(null!));
    }

    /// <summary>Log output that captures what is emitted for its verification.</summary>
    private sealed class CapturedOutput : ILogOutput
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = [];

        public void Write(LogLevel level, string message) =>
            Logs.Add((level, message));
    }

    /// <summary>Fixed clock with controlled advance, for deterministic times.</summary>
    private sealed class TestClock
    {
        private DateTimeOffset _now;

        public TestClock(DateTimeOffset initial) => _now = initial;

        public Func<DateTimeOffset> Clock => () => _now;

        public void Advance(TimeSpan interval) => _now = _now.Add(interval);
    }

    /// <summary>Own exception, to verify the type and message in the output.</summary>
    private sealed class TestException : Exception
    {
        public TestException(string message)
            : base(message)
        {
        }
    }
}
