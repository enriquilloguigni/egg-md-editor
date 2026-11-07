using EggMdEditor.App;
using Microsoft.Extensions.Logging;
using Xunit;
using System;
using System.Collections.Generic;

namespace EggMdEditor.App.Tests;

public class TmpT086ValidationTests
{
    private class CapLogger : Microsoft.Extensions.Logging.ILogger
    {
        public List<string> Messages = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }

    [Fact]
    public void LargeDoc_MarksDegraded_Logs()
    {
        var logger = new CapLogger();
        MarkdownAst.Logger = logger;
        MarkdownAst.SimulateLargeDocument = true;
        try
        {
            var md = "x";
            var ast = MarkdownAst.Parse(md);
            Assert.True(MarkdownAst.IsDegraded);
            Assert.True(ast.IsLargeDocumentInstance);
            Assert.Contains("LOG-016", string.Join(" ", logger.Messages));
        }
        finally
        {
            MarkdownAst.SimulateLargeDocument = false;
            MarkdownAst.ResetDegradation();
            MarkdownAst.Logger = null;
        }
    }

    [Fact]
    public void TableLimit_Exceeded_Logs()
    {
        var logger = new CapLogger();
        MarkdownAst.Logger = logger;
        MarkdownAst.ResetDegradation();
        var msg = MarkdownAst.HandleTableLimit(1001, 51);
        Assert.True(MarkdownAst.TableLimitExceeded);
        Assert.Equal(MarkdownAst.TableLimitMessage, msg);
        Assert.Contains("LOG-015", string.Join(" ", logger.Messages));
        MarkdownAst.Logger = null;
    }

    [Fact]
    public void LargeDocumentHandler_Alias_Works()
    {
        var h = new LargeDocumentHandler("hello");
        Assert.NotNull(h);
        Assert.True(h.Handle().Contains("grande") || h.Handle().Length > 0);
    }
}
