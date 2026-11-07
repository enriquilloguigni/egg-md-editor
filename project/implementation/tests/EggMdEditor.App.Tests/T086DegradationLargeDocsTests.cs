using EggMdEditor.App;
using Microsoft.Extensions.Logging;
using Xunit;
using System;
using System.Collections.Generic;

namespace EggMdEditor.App.Tests;

public class T086DegradationLargeDocsTests
{
    private class CapLogger : Microsoft.Extensions.Logging.ILogger
    {
        public List<string> Messages = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    [Fact]
    public void LargeDocument_MarksDegraded_PreservesContent_Logs()
    {
        var logger = new CapLogger();
        MarkdownAst.Logger = logger;
        MarkdownAst.SimulateLargeDocument = true;
        try
        {
            var md = "texto";
            var ast = MarkdownAst.Parse(md);
            // content small but degraded state true due to simulation
            Assert.True(MarkdownAst.IsDegraded);
            Assert.Equal(md, ast.Source);
            Assert.Equal(md, ast.ToMarkdown());
            Assert.Contains("LOG-016", string.Join(" ", logger.Messages));
        }
        finally
        {
            MarkdownAst.SimulateLargeDocument = false;
            MarkdownAst.IsDegraded = false;
            MarkdownAst.TableLimitExceeded = false;
            MarkdownAst.Logger = null;
        }
    }

    [Fact]
    public void NormalDocument_NotDegraded()
    {
        MarkdownAst.IsDegraded = false;
        MarkdownAst.SimulateLargeDocument = false;
        var ast = MarkdownAst.Parse("hola");
        Assert.False(MarkdownAst.IsDegraded);
    }

    [Fact]
    public void TableLimit_Exceeded_Logs()
    {
        var logger = new CapLogger();
        MarkdownAst.Logger = logger;
        MarkdownAst.TableLimitExceeded = false;
        var msg = MarkdownAst.HandleTableLimitStatic(1001, 51);
        Assert.True(MarkdownAst.TableLimitExceeded);
        Assert.Equal(MarkdownAst.TableLimitMessage, msg);
        Assert.Contains("LOG-015", string.Join(" ", logger.Messages));
        MarkdownAst.Logger = null;
    }

    [Fact]
    public void TableWithinLimits_Works()
    {
        Assert.True(MarkdownAst.IsTableWithinLimits(10, 5));
        Assert.False(MarkdownAst.IsTableWithinLimits(1001, 5));
        Assert.False(MarkdownAst.IsTableWithinLimits(10, 51));
    }

    [Fact]
    public void LargeDocumentHandler_AliasClasses_Work()
    {
        var h = new LargeDocumentHandler("# hi");
        Assert.NotNull(h);
        var p = new TableLimitHandler("# hi");
        Assert.NotNull(p);
    }

    [Fact]
    public void LargeDocumentMessage_Constant()
    {
        Assert.Equal("El documento es muy grande; el renderizado puede ser más lento.", MarkdownAst.LargeDocumentMessage);
        Assert.Equal(MarkdownAst.LargeDocumentMessage, MarkdownAst.Err017Message);
        Assert.Equal("La tabla supera el límite (1 000 filas × 50 columnas); se ha limitado la inserción.", MarkdownAst.TableLimitMessage);
    }
}
