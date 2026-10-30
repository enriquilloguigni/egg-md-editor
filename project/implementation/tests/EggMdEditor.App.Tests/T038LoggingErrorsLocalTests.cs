using EggMdEditor.App;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T038LoggingErrorsLocalTests
{
    private sealed class RecordingLogger : ILogger
    {
        public System.Collections.Generic.List<(Microsoft.Extensions.Logging.LogLevel Level, string Message)> Entries { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }

    [Fact]
    public void Abrir_Valid_LogsLog004Info()
    {
        var path = Path.Combine(Path.GetTempPath(), "t038_abrir.md");
        File.WriteAllText(path, "hola", new UTF8Encoding(false));
        var logger = new RecordingLogger();
        AbrirOperation.Logger = logger;
        try
        {
            AbrirOperation.Execute(path);
            Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Information && e.Message.Contains("LOG-004"));
        }
        finally
        {
            AbrirOperation.Logger = null;
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Guardar_Valid_LogsLog005Info()
    {
        var path = Path.Combine(Path.GetTempPath(), "t038_guardar.md");
        try
        {
            var doc = new OpenDocument("content");
            var logger = new RecordingLogger();
            GuardarOperation.Logger = logger;
            try
            {
                GuardarOperation.Execute(doc, path);
                Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Information && e.Message.Contains("LOG-005"));
            }
            finally { GuardarOperation.Logger = null; }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Autoguardado_Valid_LogsLog006Debug()
    {
        var path = Path.Combine(Path.GetTempPath(), "t038_auto.md");
        try
        {
            var doc = new OpenDocument("content", path);
            doc.MarkChanged();
            var logger = new RecordingLogger();
            AutoguardadoOperation.Logger = logger;
            try
            {
                AutoguardadoOperation.Execute(doc, path);
                Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Debug && e.Message.Contains("LOG-006"));
            }
            finally { AutoguardadoOperation.Logger = null; }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void Recuperacion_NoCopy_LogsLog007Debug()
    {
        var path = Path.Combine(Path.GetTempPath(), "t038_nocopy.recovery");
        if (File.Exists(path)) File.Delete(path);
        var logger = new RecordingLogger();
        RecuperacionOperation.Logger = logger;
        try
        {
            RecuperacionOperation.Execute(path);
            Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Debug && e.Message.Contains("LOG-007"));
        }
        finally { RecuperacionOperation.Logger = null; }
    }

    [Fact]
    public void ConfirmDiscard_DoNotSave_LogsLog008Info()
    {
        var recovery = new RecoveryCopy(new OpenDocument("x"));
        var logger = new RecordingLogger();
        ConfirmDiscardOperation.Logger = logger;
        try
        {
            ConfirmDiscardOperation.Execute(new OpenDocument("x"), ConfirmDiscardOption.DoNotSave, recovery);
            Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Information && e.Message.Contains("LOG-008"));
        }
        finally { ConfirmDiscardOperation.Logger = null; }
    }

    [Fact]
    public void RecentFiles_LoadCorrupt_LogsWarning()
    {
        var path = Path.Combine(Path.GetTempPath(), "t038_corrupt.json");
        File.WriteAllText(path, "{ not json", new UTF8Encoding(false));
        var logger = new RecordingLogger();
        RecentFiles.Logger = logger;
        try
        {
            RecentFiles.Load(path);
            Assert.Contains(logger.Entries, e => e.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
        }
        finally
        {
            RecentFiles.Logger = null;
            if (File.Exists(path)) File.Delete(path);
        }
    }
}