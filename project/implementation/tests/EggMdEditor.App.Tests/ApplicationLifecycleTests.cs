using System;
using System.Collections.Generic;
using System.IO;
using EggMdEditor.App.Startup;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EggMdEditor.App.Tests;

// T-013 (ERR-001/ERR-002, LOG-001): diagnóstico técnico del arranque y del
// cierre, con mensajes en español por la salida de error en los fallos.
public class ApplicationLifecycleTests
{
    [Fact]
    public void StartupAndInitializationLogTheRequiredLog001Data()
    {
        var logger = new RecordingLogger();
        using var errorOutput = new StringWriter();
        using var lifecycle = new ApplicationLifecycle(logger, errorOutput);

        lifecycle.BeginStartup();
        lifecycle.MarkInitialized("es");

        Assert.True(lifecycle.IsStartupStarted);
        Assert.True(lifecycle.IsStartupCompleted);
        Assert.True(lifecycle.IsStartupPresented);
        Assert.False(lifecycle.HasStartupFailure);
        Assert.Equal(ExitCodes.Success, lifecycle.ExitCode);
        Assert.Equal(string.Empty, errorOutput.ToString());

        var start = Assert.Single(logger.Entries, entry =>
            entry.Message.Contains("Inicio del proceso", StringComparison.Ordinal));
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Information, start.Level);
        Assert.Contains(lifecycle.SessionId, start.Message, StringComparison.Ordinal);
        Assert.Contains(lifecycle.ApplicationVersion, start.Message, StringComparison.Ordinal);
        Assert.Contains(lifecycle.RuntimeVersion, start.Message, StringComparison.Ordinal);

        var end = Assert.Single(logger.Entries, entry =>
            entry.Message.Contains("Fin de la inicialización", StringComparison.Ordinal));
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Information, end.Level);
        Assert.Contains(lifecycle.SessionId, end.Message, StringComparison.Ordinal);
        Assert.Contains("Idioma activo: es", end.Message, StringComparison.Ordinal);
        Assert.Contains("ms", end.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StartupFailureWritesSpanishMessageToErrorAndLogsError()
    {
        var logger = new RecordingLogger();
        using var errorOutput = new StringWriter();
        using var lifecycle = new ApplicationLifecycle(logger, errorOutput);

        var exception = new InvalidOperationException("el catálogo i18n inicial no carga");
        var exitCode = lifecycle.ReportStartupFailure(exception);

        Assert.Equal(ExitCodes.StartupFailure, exitCode);
        Assert.True(lifecycle.HasStartupFailure);
        Assert.Same(exception, lifecycle.StartupFailure);
        Assert.Equal(
            "Error al iniciar la aplicación: InvalidOperationException: el catálogo i18n inicial no carga"
            + Environment.NewLine,
            errorOutput.ToString());

        Assert.Contains(
            logger.Entries,
            entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Information
                && entry.Message.Contains("Inicio del proceso", StringComparison.Ordinal));
        Assert.Contains(
            logger.Entries,
            entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Error
                && entry.Message.Contains("InvalidOperationException", StringComparison.Ordinal));

        // ERR-001: repetir el diagnóstico no duplica el mensaje ni el registro.
        Assert.Equal(exitCode, lifecycle.ReportStartupFailure(exception));
        Assert.Single(logger.Entries, entry =>
            entry.Level == Microsoft.Extensions.Logging.LogLevel.Error
            && entry.Message.Contains("Error al iniciar la aplicación", StringComparison.Ordinal));
        Assert.Single(
            errorOutput.ToString()
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void CleanShutdownLogsStartAndEndWithSessionDuration()
    {
        var logger = new RecordingLogger();
        using var errorOutput = new StringWriter();
        using var lifecycle = new ApplicationLifecycle(logger, errorOutput);

        lifecycle.BeginStartup();
        lifecycle.MarkInitialized("es");
        lifecycle.BeginShutdown();
        var exitCode = lifecycle.CompleteShutdown();

        Assert.True(lifecycle.IsShutdownCompleted);
        Assert.False(lifecycle.HasShutdownFailure);
        Assert.Equal(ExitCodes.Success, exitCode);
        Assert.Equal(string.Empty, errorOutput.ToString());

        Assert.Single(logger.Entries, entry =>
            entry.Level == Microsoft.Extensions.Logging.LogLevel.Information
            && entry.Message.Contains("Inicio del cierre", StringComparison.Ordinal));

        var end = Assert.Single(logger.Entries, entry =>
            entry.Message.Contains("Fin del proceso", StringComparison.Ordinal));
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Information, end.Level);
        Assert.Contains(lifecycle.SessionId, end.Message, StringComparison.Ordinal);
        Assert.Contains("Duración total de la sesión:", end.Message, StringComparison.Ordinal);
        Assert.Contains("ms", end.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ShutdownFailureWritesSpanishMessageLogsErrorAndCompletesShutdown()
    {
        var logger = new RecordingLogger();
        using var errorOutput = new StringWriter();
        using var lifecycle = new ApplicationLifecycle(logger, errorOutput);

        lifecycle.BeginStartup();
        lifecycle.MarkInitialized("es");
        lifecycle.BeginShutdown();

        var exception = new IOException("no se pudo liberar la sesión");
        var exitCode = lifecycle.CompleteShutdown(exception);

        Assert.Equal(ExitCodes.ShutdownFailure, exitCode);
        Assert.True(lifecycle.HasShutdownFailure);
        Assert.True(lifecycle.IsShutdownCompleted);
        Assert.Same(exception, lifecycle.ShutdownFailure);
        Assert.Equal(
            "Error al cerrar la aplicación: IOException: no se pudo liberar la sesión"
            + Environment.NewLine,
            errorOutput.ToString());

        var error = Assert.Single(logger.Entries, entry =>
            entry.Level == Microsoft.Extensions.Logging.LogLevel.Error
            && entry.Message.Contains("Error al cerrar la aplicación", StringComparison.Ordinal));
        Assert.Contains(lifecycle.SessionId, error.Message, StringComparison.Ordinal);
        Assert.Contains("Duración total de la sesión:", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(logger.Entries, entry =>
            entry.Message.Contains("Fin del proceso", StringComparison.Ordinal));
    }

    [Fact]
    public void StartupFailureDoesNotMixAShutdownDiagnosis()
    {
        var logger = new RecordingLogger();
        using var errorOutput = new StringWriter();
        using var lifecycle = new ApplicationLifecycle(logger, errorOutput);

        lifecycle.ReportStartupFailure(new InvalidOperationException("arranque roto"));
        errorOutput.GetStringBuilder().Clear();

        var exitCode = lifecycle.CompleteShutdown(new IOException("cierre irrelevante"));

        Assert.Equal(ExitCodes.StartupFailure, exitCode);
        Assert.Equal(string.Empty, errorOutput.ToString());
        Assert.DoesNotContain(logger.Entries, entry =>
            entry.Message.Contains("Inicio del cierre", StringComparison.Ordinal));
        Assert.DoesNotContain(logger.Entries, entry =>
            entry.Message.Contains("Error al cerrar la aplicación", StringComparison.Ordinal));
    }

    [Fact]
    public void SessionLoggerAddsTheSessionIdScope()
    {
        var inner = new ScopeRecordingLogger();
        var logger = ApplicationLogging.CreateSessionLogger(inner, "sesion-tecnica");

        using var scope = logger.BeginScope("operacion");

        var values = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(
            Assert.Single(inner.Scopes));
        Assert.Equal("sesion-tecnica", values["SessionId"]);
    }

    [Fact]
    public void EverySessionLogCarriesTheSessionIdEvenWithoutAnExplicitScope()
    {
        var inner = new ScopeRecordingLogger();
        var logger = ApplicationLogging.CreateSessionLogger(inner, "sesion-tecnica");

        logger.LogInformation("mensaje");

        var values = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(
            Assert.Single(inner.Scopes));
        Assert.Equal("sesion-tecnica", values["SessionId"]);
        Assert.Single(inner.Messages);
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(Microsoft.Extensions.Logging.LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception), exception));
        }
    }

    private sealed class ScopeRecordingLogger : ILogger
    {
        public List<object?> Scopes { get; } = new();

        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            Scopes.Add(state);
            return null;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
