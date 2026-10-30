using EggMdEditor.App;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T053ManualVerificationTests
{
    [Fact]
    public void Retry_OnFailure()
    {
        Synchronizer.Reset();
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        Synchronizer.FailuresRemaining = 1;
        var logger = new RecordingLogger();
        Synchronizer.Logger = logger;
        bool ok = Synchronizer.SyncEdit(left, "new content");
        Assert.True(ok);
        Assert.Equal(1, Synchronizer.RetryCount);
        Assert.False(Synchronizer.NoticeVisible);
        Assert.Contains(logger.Entries, e => e.Message.Contains("LOG-009"));
    }

    [Fact]
    public void PersistentFailure_NonModalNotice()
    {
        Synchronizer.Reset();
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        Synchronizer.SimulateFailure = true;
        bool ok = Synchronizer.SyncEdit(left, "x");
        Assert.False(ok);
        Assert.True(Synchronizer.NoticeVisible);
        Assert.False(Synchronizer.IsModal);
        Assert.Equal("No se pudo sincronizar las vistas. Sigue editando; se reintentará automáticamente.", Synchronizer.NoticeMessage);
        Synchronizer.SimulateFailure = false;
    }

    [Fact]
    public void Degradation_Deferred_NoBlock()
    {
        Synchronizer.Reset();
        var doc = new OpenDocument("old");
        var (left, right) = EditorPanel.CreatePair(doc);
        Synchronizer.ThresholdMs = 0;
        bool ok = Synchronizer.SyncEdit(left, "x");
        Assert.True(ok);
        Assert.True(Synchronizer.IsDegraded);
        Assert.True(Synchronizer.Deferred);
        Synchronizer.ThresholdMs = 200;
    }

    private sealed class RecordingLogger : ILogger
    {
        public System.Collections.Generic.List<(Microsoft.Extensions.Logging.LogLevel Level, string Message)> Entries { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }
}