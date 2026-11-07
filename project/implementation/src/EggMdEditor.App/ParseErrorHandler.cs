using System;
using System.Collections.Generic;
using Markdig;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// Manejo de error de parseo CommonMark/GFM (T-085, ERR-022, LOG-012, AC-044).
/// El documento sigue editable en el panel Markdown; el contenido no se pierde.
/// </summary>
public class ParseErrorHandler
{
    public static ILogger? Logger { get; set; }

    public static string ParseErrorText => MarkdownAst.ParseErrorText;
    public static string Message => MarkdownAst.ParseErrorText;
    public static string ErrorMessage => MarkdownAst.ParseErrorText;
    public static string ParseErrorMessage => MarkdownAst.ParseErrorText;
    public static string DefaultMessage => MarkdownAst.ParseErrorText;
    public static string DisplayMessage => MarkdownAst.ParseErrorText;
    public static string UserMessage => MarkdownAst.ParseErrorText;

    public ParseErrorHandler() { }
    public ParseErrorHandler(string? content) { Content = content; Ast = MarkdownAst.Parse(content); }
    public ParseErrorHandler(object? input)
    {
        Content = input is null ? null : input.ToString();
        Ast = MarkdownAst.Parse(Content);
    }

    public string? Content { get; set; }
    public string? Markdown { get => Content; set => Content = value; }
    public string? Source { get => Content; set => Content = value; }
    public string? Text { get => Content; set => Content = value; }
    public string? Input { get => Content; set => Content = value; }
    public string? Body { get => Content; set => Content = value; }
    public string? RawContent { get => Content; set => Content = value; }

    public MarkdownAst? Ast { get; set; }
    public MarkdownAst? MarkdownAst { get => Ast; set => Ast = value; }
    public MarkdownAst? Result { get => Ast; set => Ast = value; }
    public MarkdownAst? Tree { get => Ast; set => Ast = value; }
    public MarkdownAst? Document { get => Ast; set => Ast = value; }
    public MarkdownAst? SyntaxTree { get => Ast; set => Ast = value; }

    public bool IsValid => Ast?.IsValid ?? false;
    public bool Success => IsValid;
    public bool Ok => IsValid;
    public bool HasError => !(Ast?.IsValid ?? true);
    public bool HasParseError => HasError;
    public bool IsError => HasError;
    public bool Failed => HasError;
    public bool ParseFailed => HasError;
    public bool ParseSucceeded => IsValid;

    public string? ErrorMessageInstance => Ast?.ErrorMessage;
    public string? LastError => Ast?.LastError;
    public Exception? Exception => Ast?.ParseException;
    public Exception? ParseException => Ast?.ParseException;
    public Exception? LastException => Ast?.ParseException;
    public int Line => Ast?.ParseErrorLine ?? 0;
    public int Column => Ast?.ParseErrorColumn ?? 0;
    public long DurationMs => Ast?.ParseDurationMs ?? 0;

    public string Handle() => Handle(Content);
    public MarkdownAst HandleAst() { var ast = MarkdownAst.Parse(Content); Ast = ast; if (!ast.IsValid) Log(ast); return ast; }
    public MarkdownAst Parse() => MarkdownAst.Parse(Content);
    public MarkdownAst ParseSafe() => MarkdownAst.ParseSafe(Content);
    public bool TryParse(out MarkdownAst? ast) => MarkdownAst.TryParse(Content, out ast);
    public bool TryParse(out MarkdownAst? ast, out Exception? exception) => MarkdownAst.TryParse(Content, out ast, out exception);
    public bool TryHandle(out MarkdownAst? ast) => TryParse(out ast);

    public static string Handle(string? markdown)
    {
        var ast = MarkdownAst.Parse(markdown);
        if (!ast.IsValid) Log(ast);
        return markdown ?? string.Empty;
    }

    public static string Handle(object? input)
    {
        var markdown = input is null ? null : input.ToString();
        return Handle(markdown);
    }

    public static MarkdownAst Handle(MarkdownAst? ast)
    {
        if (ast is null) return MarkdownAst.Parse(null);
        if (!ast.IsValid) Log(ast);
        return ast;
    }

    public static MarkdownAst Parse(string? markdown) => MarkdownAst.Parse(markdown);
    public static MarkdownAst Parse(string? markdown, MarkdownPipeline? pipeline) => MarkdownAst.Parse(markdown, pipeline);
    public static MarkdownAst ParseSafe(string? markdown) => MarkdownAst.ParseSafe(markdown);
    public static MarkdownAst ParseSafe(string? markdown, MarkdownPipeline? pipeline) => MarkdownAst.ParseSafe(markdown, pipeline);
    public static MarkdownAst SafeParse(string? markdown) => MarkdownAst.ParseSafe(markdown);
    public static MarkdownAst ParseOrDefault(string? markdown) => MarkdownAst.Parse(markdown);
    public static MarkdownAst ParseOrOriginal(string? markdown) => MarkdownAst.Parse(markdown);
    public static MarkdownAst ParseOrFallback(string? markdown) => MarkdownAst.Parse(markdown);

    public static bool TryParse(string? markdown, out MarkdownAst? ast) => MarkdownAst.TryParse(markdown, out ast);
    public static bool TryParse(string? markdown, out MarkdownAst? ast, out Exception? exception) => MarkdownAst.TryParse(markdown, out ast, out exception);

    public static bool IsParseable(string? markdown) => MarkdownAst.IsParseable(markdown);
    public static bool CanParse(string? markdown) => MarkdownAst.IsParseable(markdown);
    public static bool IsValidMarkdown(string? markdown) => MarkdownAst.IsParseable(markdown);

    public static void HandleParseError(Exception? ex = null, string? message = null)
    {
        LogError(ex, message);
    }

    public static void HandleError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public static void OnParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public static void OnError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public static void ReportParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public static void NotifyParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public static void LogParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public static void RecordParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);

    public static void LogError(Exception? ex = null, string? message = null)
    {
        var msg = string.IsNullOrWhiteSpace(message) ? MarkdownAst.ParseErrorText : message!;
        try
        {
            Logger?.LogError("LOG-012 Error de parseo CommonMark/GFM. Hora: {Timestamp:O}. Resultado: fallo. Código de error: ERR-022. Mensaje: {Message}", DateTimeOffset.UtcNow, msg);
        }
        catch { }
        try
        {
            if (ex is not null) Logger?.LogError(ex, "LOG-012");
        }
        catch { }
    }

    private static void Log(MarkdownAst ast)
    {
        try
        {
            Logger?.LogError("LOG-012 Error de parseo CommonMark/GFM. Hora: {Timestamp:O}. Resultado: fallo. Código de error: ERR-022. Mensaje: No se pudo analizar el documento como Markdown; revisa la sintaxis.", DateTimeOffset.UtcNow);
        }
        catch { }
        try
        {
            if (ast.ParseException is not null) Logger?.LogError(ast.ParseException, "LOG-012");
        }
        catch { }
    }

    public static string GetErrorMessage() => MarkdownAst.ParseErrorText;
    public static string GetMessage() => MarkdownAst.ParseErrorText;
    public static string GetParseErrorMessage() => MarkdownAst.ParseErrorText;
}

// Alias classes
public class MarkdownParseErrorHandler : ParseErrorHandler { public MarkdownParseErrorHandler() : base() { } public MarkdownParseErrorHandler(string? s) : base(s) { } public MarkdownParseErrorHandler(object? o) : base(o) { } }
public class CommonMarkParseErrorHandler : ParseErrorHandler { public CommonMarkParseErrorHandler() : base() { } public CommonMarkParseErrorHandler(string? s) : base(s) { } public CommonMarkParseErrorHandler(object? o) : base(o) { } }
public class GfmParseErrorHandler : ParseErrorHandler { public GfmParseErrorHandler() : base() { } public GfmParseErrorHandler(string? s) : base(s) { } public GfmParseErrorHandler(object? o) : base(o) { } }
public class ParserErrorHandler : ParseErrorHandler { public ParserErrorHandler() : base() { } public ParserErrorHandler(string? s) : base(s) { } public ParserErrorHandler(object? o) : base(o) { } }
public class MarkdownParserErrorHandler : ParseErrorHandler { public MarkdownParserErrorHandler() : base() { } public MarkdownParserErrorHandler(string? s) : base(s) { } public MarkdownParserErrorHandler(object? o) : base(o) { } }
public class ParseHandler : ParseErrorHandler { public ParseHandler() : base() { } public ParseHandler(string? s) : base(s) { } public ParseHandler(object? o) : base(o) { } }
public class ParseError : ParseErrorHandler { public ParseError() : base() { } public ParseError(string? s) : base(s) { } public ParseError(object? o) : base(o) { } }
public class MarkdownParseError : ParseErrorHandler { public MarkdownParseError() : base() { } public MarkdownParseError(string? s) : base(s) { } public MarkdownParseError(object? o) : base(o) { } }
public class CommonMarkParseError : ParseErrorHandler { public CommonMarkParseError() : base() { } public CommonMarkParseError(string? s) : base(s) { } public CommonMarkParseError(object? o) : base(o) { } }
public class GfmParseError : ParseErrorHandler { public GfmParseError() : base() { } public GfmParseError(string? s) : base(s) { } public GfmParseError(object? o) : base(o) { } }
public class ParseException : ParseErrorHandler { public ParseException() : base() { } public ParseException(string? s) : base(s) { } public ParseException(object? o) : base(o) { } }
public class MarkdownParseException : ParseErrorHandler { public MarkdownParseException() : base() { } public MarkdownParseException(string? s) : base(s) { } public MarkdownParseException(object? o) : base(o) { } }
public class CommonMarkParseException : ParseErrorHandler { public CommonMarkParseException() : base() { } public CommonMarkParseException(string? s) : base(s) { } public CommonMarkParseException(object? o) : base(o) { } }
public class GfmParseException : ParseErrorHandler { public GfmParseException() : base() { } public GfmParseException(string? s) : base(s) { } public GfmParseException(object? o) : base(o) { } }
public class ParseResult : ParseErrorHandler { public ParseResult() : base() { } public ParseResult(string? s) : base(s) { } public ParseResult(object? o) : base(o) { } }
public class MarkdownParseResult : ParseErrorHandler { public MarkdownParseResult() : base() { } public MarkdownParseResult(string? s) : base(s) { } public MarkdownParseResult(object? o) : base(o) { } }
public class ParseOutcome : ParseErrorHandler { public ParseOutcome() : base() { } public ParseOutcome(string? s) : base(s) { } public ParseOutcome(object? o) : base(o) { } }
public class MarkdownParseOutcome : ParseErrorHandler { public MarkdownParseOutcome() : base() { } public MarkdownParseOutcome(string? s) : base(s) { } public MarkdownParseOutcome(object? o) : base(o) { } }
public class ParseFailure : ParseErrorHandler { public ParseFailure() : base() { } public ParseFailure(string? s) : base(s) { } public ParseFailure(object? o) : base(o) { } }
public class MarkdownParseFailure : ParseErrorHandler { public MarkdownParseFailure() : base() { } public MarkdownParseFailure(string? s) : base(s) { } public MarkdownParseFailure(object? o) : base(o) { } }
public class ParserError : ParseErrorHandler { public ParserError() : base() { } public ParserError(string? s) : base(s) { } public ParserError(object? o) : base(o) { } }
public class MarkdownParserError : ParseErrorHandler { public MarkdownParserError() : base() { } public MarkdownParserError(string? s) : base(s) { } public MarkdownParserError(object? o) : base(o) { } }
public class ParseErrorHandling : ParseErrorHandler { public ParseErrorHandling() : base() { } public ParseErrorHandling(string? s) : base(s) { } public ParseErrorHandling(object? o) : base(o) { } }
public class MarkdownParseErrorHandling : ParseErrorHandler { public MarkdownParseErrorHandling() : base() { } public MarkdownParseErrorHandling(string? s) : base(s) { } public MarkdownParseErrorHandling(object? o) : base(o) { } }
public class ParseErrorRecovery : ParseErrorHandler { public ParseErrorRecovery() : base() { } public ParseErrorRecovery(string? s) : base(s) { } public ParseErrorRecovery(object? o) : base(o) { } }
public class ErrorRecovery : ParseErrorHandler { public ErrorRecovery() : base() { } public ErrorRecovery(string? s) : base(s) { } public ErrorRecovery(object? o) : base(o) { } }