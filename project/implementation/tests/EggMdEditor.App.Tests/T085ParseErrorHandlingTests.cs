using EggMdEditor.App;
using Microsoft.Extensions.Logging;
using Xunit;
using System;
using System.Collections.Generic;

namespace EggMdEditor.App.Tests;

public class T085ParseErrorHandlingTests
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
    public void ForcedParseError_MarksInvalid_PreservesContent_Logs()
    {
        var logger = new CapLogger();
        MarkdownAst.Logger = logger;
        MarkdownAst.ForceParseError = true;
        try
        {
            var md = "# Hola\n\nTexto";
            var ast = MarkdownAst.Parse(md);
            Assert.False(ast.IsValid);
            Assert.True(ast.HasParseError);
            Assert.True(ast.IsParseError);
            Assert.False(ast.ParseSucceeded);
            Assert.Equal(md, ast.Source);
            Assert.Equal(md, ast.ToMarkdown());
            Assert.Equal(md, ast.Content);
            Assert.Contains("LOG-012", string.Join(" ", logger.Messages));
            Assert.Equal(MarkdownAst.ParseErrorText, ast.ErrorMessage);
        }
        finally
        {
            MarkdownAst.ForceParseError = false;
            MarkdownAst.Logger = null;
        }
    }

    [Fact]
    public void NormalParse_IsValid_NoError()
    {
        var ast = MarkdownAst.Parse("# Hola mundo");
        Assert.True(ast.IsValid);
        Assert.False(ast.HasParseError);
        Assert.True(ast.ParseSucceeded);
        Assert.Equal(string.Empty, ast.ErrorMessage);
    }

    [Fact]
    public void NullParse_DoesNotThrow()
    {
        var ast = MarkdownAst.Parse(null);
        Assert.True(ast.IsValid);
        Assert.Equal(string.Empty, ast.Source);
    }

    [Fact]
    public void HandleParseError_SetsInvalid_Logs()
    {
        var logger = new CapLogger();
        MarkdownAst.Logger = logger;
        var ast = new MarkdownAst("# x");
        ast.HandleParseError(new InvalidOperationException("boom"), "custom");
        Assert.False(ast.IsValid);
        Assert.True(ast.HasParseError);
        Assert.Equal("custom", ast.ErrorMessage);
        Assert.NotNull(ast.ParseException);
        Assert.Contains("LOG-012", string.Join(" ", logger.Messages));
        MarkdownAst.Logger = null;
    }

    [Fact]
    public void TryParse_ReturnsFalse_OnForcedError()
    {
        MarkdownAst.ForceParseError = true;
        try
        {
            var ok = MarkdownAst.TryParse("# x", out var ast);
            Assert.False(ok);
            Assert.NotNull(ast);
            Assert.False(ast!.IsValid);
        }
        finally
        {
            MarkdownAst.ForceParseError = false;
        }
    }

    [Fact]
    public void ParseErrorHandler_Handle_PreservesContent_Logs()
    {
        var logger = new CapLogger();
        ParseErrorHandler.Logger = logger;
        MarkdownAst.ForceParseError = true;
        try
        {
            var md = "texto";
            var result = ParseErrorHandler.Handle(md);
            Assert.Equal(md, result);
            Assert.Contains("LOG-012", string.Join(" ", logger.Messages));
        }
        finally
        {
            MarkdownAst.ForceParseError = false;
            ParseErrorHandler.Logger = null;
        }
    }

    [Fact]
    public void ParseErrorHandler_AliasClasses_Work()
    {
        var h = new MarkdownParseErrorHandler("# hi");
        Assert.NotNull(h.Ast);
        Assert.True(h.Ast!.IsValid);
        var p = new ParseException("# hi");
        Assert.NotNull(p.HandleAst());
    }

    [Fact]
    public void EditableProperties_True()
    {
        var ast = MarkdownAst.Parse("x");
        Assert.True(ast.IsEditable);
        Assert.True(ast.Editable);
        Assert.True(ast.StaysEditable);
        Assert.True(ast.DocumentEditable);
        Assert.True(ast.MarkdownEditable);
    }

    [Fact]
    public void ParseErrorText_Constant()
    {
        Assert.Equal("No se pudo analizar el documento como Markdown; revisa la sintaxis.", MarkdownAst.ParseErrorText);
        Assert.Equal(MarkdownAst.ParseErrorText, ParseErrorHandler.Message);
    }
}
