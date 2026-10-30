using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Headless.XUnit;
using EggMdEditor.App.I18n;
using EggMdEditor.App.Startup;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EggMdEditor.App.Tests;

// T-013 (ERR-005, LOG-003): una excepción al cerrar SC-001 o al liberar la
// sesión no impide que el proceso termine; deja el mensaje en español por la
// salida de error y el registro técnico del cierre.
public class CloseErrorHandlingTests
{
    [AvaloniaFact]
    public void ClosingWithAFailingSessionReportsShutdownErrorAndStillCloses()
    {
        var logger = new RecordingLogger();
        using var errorOutput = new StringWriter();
        using var lifecycle = new ApplicationLifecycle(logger, errorOutput);

        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel, lifecycle);

        window.Show();

        var exception = new InvalidOperationException("fallo al liberar la sesión");
        session.PropertyChanged += (_, _) => throw exception;

        var thrown = Record.Exception(window.Close);

        Assert.Null(thrown);
        Assert.False(window.IsVisible);
        Assert.Equal(
            "Error al cerrar la aplicación: InvalidOperationException: fallo al liberar la sesión"
            + Environment.NewLine,
            errorOutput.ToString());
        Assert.Contains(
            logger.Entries,
            entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Error
                && entry.Message.Contains("Error al cerrar la aplicación", StringComparison.Ordinal));
        Assert.False(lifecycle.IsStartupCompleted);
    }

    [AvaloniaFact]
    public void ClosingWithoutFailuresDoesNotWriteToTheErrorOutput()
    {
        var logger = new RecordingLogger();
        using var errorOutput = new StringWriter();
        using var lifecycle = new ApplicationLifecycle(logger, errorOutput);

        var session = new Session(new I18nCatalog());
        var viewModel = new MainWindowViewModel(session);
        var window = new MainWindow(viewModel, lifecycle);

        window.Show();
        window.Close();

        Assert.Equal(string.Empty, errorOutput.ToString());
        Assert.False(lifecycle.HasShutdownFailure);
        Assert.Single(logger.Entries, entry =>
            entry.Message.Contains("Inicio del cierre", StringComparison.Ordinal));
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
}
