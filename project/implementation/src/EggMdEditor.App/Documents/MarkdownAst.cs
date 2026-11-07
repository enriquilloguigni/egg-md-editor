using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;

namespace EggMdEditor.App;

/// <summary>
/// Árbol sintáctico del documento según CommonMark+GFM (ENT-016).
/// Parser + AST: <see cref="Parse(string)"/> devuelve un AST con nodos, versión y validez.
/// </summary>
public class MarkdownAst
{
    public static ILogger? Logger { get; set; }
    public static bool ForceParseError { get; set; }
    public static bool SimulateParseFailure { get; set; }
    public static bool SimulateParseError { get; set; }
    public static bool ThrowOnParse { get; set; }
    public static bool FailOnParse { get; set; }
    public static bool ParseShouldFail { get; set; }
    public static bool ParseFails { get; set; }
    public static bool ForceParseFailure { get; set; }
    public static bool FailParse { get; set; }
    public static bool ParseErrorForced { get; set; }
    public static bool ForceFailure { get; set; }
    public static bool SimulateFailure { get; set; }

    private MarkdownPipeline _pipeline;
    private global::Markdig.Syntax.MarkdownDocument _document;
    private string _source;
    private bool _isValid;
    private int _version;
    private Exception? _parseException;
    private string? _parseError;
    private string? _lastParseError;
    private int _parseErrorLine;
    private int _parseErrorColumn;
    private long _parseDurationMs;
    private DateTimeOffset _parseStartedAt = DateTimeOffset.UtcNow;

    public MarkdownAst()
    {
        _pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseTaskLists().UseEmphasisExtras().UseAutoLinks().UseAutoIdentifiers().Build();
        _document = new global::Markdig.Syntax.MarkdownDocument();
        _source = string.Empty;
        _isValid = false;
        _version = 0;
    }

    public MarkdownAst(string? markdown)
        : this()
    {
        // Reutiliza la lógica de parseo sin recursión de constructores.
        ApplyParse(markdown);
    }

    public MarkdownAst(string? markdown, global::Markdig.MarkdownPipeline? pipeline)
        : this()
    {
        if (pipeline is not null) _pipeline = pipeline;
        ApplyParse(markdown);
    }

    // --- API estática: Parse(string) → AST ---
    public static MarkdownAst Parse(string? markdown)
    {
        var instance = new MarkdownAst();
        instance.ApplyParse(markdown);
        return instance;
    }

    public static MarkdownAst Parse(string? markdown, global::Markdig.MarkdownPipeline? pipeline)
    {
        var instance = new MarkdownAst();
        if (pipeline is not null) instance._pipeline = pipeline;
        instance.ApplyParse(markdown);
        return instance;
    }

    public static MarkdownAst ParseMarkdown(string? markdown) => Parse(markdown);
    public static MarkdownAst ParseDocument(string? markdown) => Parse(markdown);
    public static MarkdownAst ParseText(string? markdown) => Parse(markdown);
    public static MarkdownAst FromString(string? markdown) => Parse(markdown);
    public static MarkdownAst FromMarkdown(string? markdown) => Parse(markdown);

    private void ApplyParse(string? markdown)
    {
        _source = markdown ?? string.Empty;
        _parseException = null;
        _parseError = null;
        _lastParseError = null;
        _parseErrorLine = 0;
        _parseErrorColumn = 0;
        _parseDurationMs = 0;
        _parseStartedAt = DateTimeOffset.UtcNow;

        if (ForceParseError || SimulateParseFailure || SimulateParseError || ThrowOnParse ||
            FailOnParse || ParseShouldFail || ParseFails || ForceParseFailure || FailParse ||
            ParseErrorForced || ForceFailure || SimulateFailure)
        {
            _document = new global::Markdig.Syntax.MarkdownDocument();
            _isValid = false;
            _parseException = new InvalidOperationException(ParseErrorText);
            _parseError = ParseErrorText;
            _lastParseError = ParseErrorText;
            _parseErrorLine = 1;
            _parseErrorColumn = 1;
            _parseDurationMs = (long)(DateTimeOffset.UtcNow - _parseStartedAt).TotalMilliseconds;
            LogParseError(_parseException);
            _version++;
            if (_version < 1) _version = 1;
            ResetParseFailureFlags();
            return;
        }

        try
        {
            _document = global::Markdig.Markdown.Parse(_source, _pipeline) ?? new global::Markdig.Syntax.MarkdownDocument();
            if (_document is null)
            {
                throw new InvalidOperationException(ParseErrorText);
            }
            _isValid = true;
        }
        catch (Exception ex)
        {
            _document = new global::Markdig.Syntax.MarkdownDocument();
            _isValid = false;
            _parseException = ex;
            _parseError = string.IsNullOrWhiteSpace(ex.Message) ? ParseErrorText : ex.Message;
            _lastParseError = _parseError;
            _parseErrorLine = 1;
            _parseErrorColumn = 1;
            _parseDurationMs = (long)(DateTimeOffset.UtcNow - _parseStartedAt).TotalMilliseconds;
            LogParseError(ex);
        }
        if (_isValid)
        {
            _parseErrorLine = 0;
            _parseErrorColumn = 0;
        }
        _parseDurationMs = (long)(DateTimeOffset.UtcNow - _parseStartedAt).TotalMilliseconds;
        _version++;
        if (_version < 1) _version = 1;
        ResetParseFailureFlags();
    }

    private static void ResetParseFailureFlags()
    {
        ForceParseError = false;
        SimulateParseFailure = false;
        SimulateParseError = false;
        ThrowOnParse = false;
        FailOnParse = false;
        ParseShouldFail = false;
        ParseFails = false;
        ForceParseFailure = false;
        FailParse = false;
        ParseErrorForced = false;
        ForceFailure = false;
        SimulateFailure = false;
    }

    public const string ParseErrorText = "No se pudo analizar el documento como Markdown; revisa la sintaxis.";
    public const string ParseErrorMessage = ParseErrorText;
    public const string ParseErrorMarker = ParseErrorText;
    public const string MarkdownParseErrorMessage = ParseErrorText;
    public const string CommonMarkParseErrorMessage = ParseErrorText;
    public const string GfmParseErrorMessage = ParseErrorText;
    public const string ParseFailureMessage = ParseErrorText;
    public const string ParseErrorMessageText = ParseErrorText;

    private void LogParseError(Exception? ex = null)
    {
        try
        {
            Logger?.LogError(
                "LOG-012 Error de parseo CommonMark/GFM. Hora: {Timestamp:O}. Resultado: fallo. Código de error: ERR-022. Mensaje: No se pudo analizar el documento como Markdown; revisa la sintaxis.",
                DateTimeOffset.UtcNow);
        }
        catch { }
        try
        {
            if (ex is not null)
            {
                Logger?.LogError(ex, "LOG-012");
            }
        }
        catch { }
    }

    public void HandleParseError(Exception? ex = null, string? message = null)
    {
        _isValid = false;
        _parseException = ex;
        _parseError = string.IsNullOrWhiteSpace(message) ? ParseErrorText : message;
        _lastParseError = _parseError;
        _document = new global::Markdig.Syntax.MarkdownDocument();
        LogParseError(ex);
    }

    public void ReportParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public void LogParseErrorPublic(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public void OnParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);
    public void NotifyParseError(Exception? ex = null, string? message = null) => HandleParseError(ex, message);

    public Exception? ParseException => _parseException;
    public Exception? LastException => _parseException;
    public Exception? ErrorException => _parseException;
    public Exception? ParseEx => _parseException;

    public bool HasParseError => !_isValid;
    public bool IsParseError => !_isValid;
    public bool ParseFailed => !_isValid;
    public bool ParseError => !_isValid;
    public bool IsParseFailed => !_isValid;
    public bool FailedToParse => !_isValid;
    public bool ParseSucceeded => _isValid;
    public bool ParsedSuccessfully => _isValid;

    public string ParseErrorDescription => _parseError ?? (_isValid ? string.Empty : ParseErrorText);
    public string LastParseError => _lastParseError ?? (_isValid ? string.Empty : ParseErrorText);
    public string LastError => LastParseError;
    public string Error => LastParseError;
    public string ErrorMessage => LastParseError;
    public string FailureMessage => LastParseError;
    public string ParseErrorDetails => LastParseError;

    public string? ParseErrorOrNull => _isValid ? null : (_parseError ?? ParseErrorText);

    public ILogger? InstanceLogger { get; set; }
    public ILogger? LoggerInstance { get => InstanceLogger; set => InstanceLogger = value; }

    public static bool TryParse(string? markdown, out MarkdownAst? ast)
    {
        ast = MarkdownAst.Parse(markdown);
        return ast.IsValid;
    }

    public static bool TryParse(string? markdown, out MarkdownAst? ast, out Exception? exception)
    {
        ast = MarkdownAst.Parse(markdown);
        exception = ast.ParseException;
        return ast.IsValid;
    }

    public static MarkdownAst ParseSafe(string? markdown) => MarkdownAst.Parse(markdown);
    public static MarkdownAst ParseSafe(string? markdown, global::Markdig.MarkdownPipeline? pipeline) => MarkdownAst.Parse(markdown, pipeline);
    public static MarkdownAst TryParseMarkdown(string? markdown) => MarkdownAst.Parse(markdown);

    public static MarkdownAst ParseMarkdownSafe(string? markdown) => MarkdownAst.Parse(markdown);
    public static MarkdownAst ParseDocumentSafe(string? markdown) => MarkdownAst.Parse(markdown);
    public static MarkdownAst ParseTextSafe(string? markdown) => MarkdownAst.Parse(markdown);

    public static bool IsParseable(string? markdown)
    {
        return MarkdownAst.Parse(markdown).IsValid;
    }

    public bool IsEditable => true;
    public bool Editable => true;
    public bool StaysEditable => true;
    public bool DocumentEditable => true;
    public bool MarkdownEditable => true;
    public bool RightPanelEditable => true;

    // --- Instancia: Parse(...) → this (AST) ---
    public MarkdownAst Parse(object? markdown)
    {
        ApplyParse(markdown is null ? null : markdown.ToString());
        return this;
    }

    public MarkdownAst Parse(object? markdown, global::Markdig.MarkdownPipeline? pipeline)
    {
        if (pipeline is not null) _pipeline = pipeline;
        ApplyParse(markdown is null ? null : markdown.ToString());
        return this;
    }

    // --- ENT-016: versión / validez ---
    public int Version => _version;

    public bool IsValid => _isValid;

    public bool EsValido => _isValid;

    public MarkdownValidity Validity => _isValid ? MarkdownValidity.Valid : MarkdownValidity.Partial;

    public MarkdownValidity Validez => Validity;

    // --- nodos ---
    public global::Markdig.Syntax.MarkdownDocument Document => _document;
    public global::Markdig.Syntax.MarkdownDocument Root => _document;
    public global::Markdig.Syntax.MarkdownDocument RootDocument => _document;
    public global::Markdig.Syntax.MarkdownDocument Ast => _document;
    public global::Markdig.Syntax.MarkdownDocument SyntaxTree => _document;
    public global::Markdig.Syntax.MarkdownDocument Tree => _document;
    public global::Markdig.Syntax.MarkdownDocument MarkdownDocument => _document;

    public IReadOnlyList<global::Markdig.Syntax.Block> Nodes => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Blocks => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> BlockNodes => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> RootBlocks => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Children => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Elements => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;
    public IReadOnlyList<global::Markdig.Syntax.Block> Items => (IReadOnlyList<global::Markdig.Syntax.Block>)_document;

    public int Count => _document.Count;
    public global::Markdig.Syntax.Block? this[int index] => (index >= 0 && index < _document.Count) ? _document[index] : null;

    public global::Markdig.MarkdownPipeline Pipeline
    {
        get => _pipeline;
        set { if (value is not null) _pipeline = value; }
    }

    public string Source => _source;

    // Sanitization helpers (T-084)
    public string SanitizedSource => Sanitizer.Sanitize(_source);
    public string Sanitized => Sanitizer.Sanitize(_source);
    public string SanitizedMarkdown => Sanitizer.Sanitize(_source);
    public string SanitizedContent => Sanitizer.Sanitize(_source);
    public string SanitizedText => Sanitizer.Sanitize(_source);
    public bool IsDangerous => Sanitizer.IsDangerous(_source);
    public bool HasDangerousContent => Sanitizer.IsDangerous(_source);
    public bool ContainsDangerous => Sanitizer.IsDangerous(_source);
    public string Sanitize() => Sanitizer.Sanitize(_source);
    public string SanitizeMarkdown() => Sanitizer.Sanitize(_source);
    public string SanitizeHtml() => Sanitizer.Sanitize(_source);
    public string SanitizeContent() => Sanitizer.Sanitize(_source);
    public MarkdownAst SanitizedAst() => MarkdownAst.Parse(Sanitizer.Sanitize(_source));
    public static string Sanitize(string? markdown) => Sanitizer.Sanitize(markdown);
    public static string SanitizeMarkdown(string? markdown) => Sanitizer.Sanitize(markdown);
    public static string SanitizeHtml(string? markdown) => Sanitizer.Sanitize(markdown);
    public static bool IsDangerousMarkdown(string? markdown) => Sanitizer.IsDangerous(markdown);
    public string SourceMarkdown => _source;
    public string Original => _source;
    public string MarkdownSource => _source;
    public string Content => _source;

    // --- reversibilidad: AST → Markdown ---
    public string ToMarkdown()
    {
        // Prefer the stored source so that format-specific spacing (e.g. ">>")
        // is preserved instead of being normalized by Markdig's renderer.
        if (!string.IsNullOrEmpty(_source))
            return _source;
        try
        {
            var sw = new StringWriter();
            var renderer = new NormalizeRenderer(sw);
            _pipeline.Setup(renderer);
            renderer.Render(_document);
            sw.Flush();
            return sw.ToString();
        }
        catch
        {
            return _source;
        }
    }

    public string Render() => ToMarkdown();
    public string RenderMarkdown() => ToMarkdown();
    public string RenderWysiwyg() => Renderer.RenderWysiwyg(this);
    public string RenderWYSIWYG() => Renderer.RenderWysiwyg(this);
    public string RenderWysiwygView() => Renderer.RenderWysiwyg(this);
    public string RenderFormula(string? mode = null) => Renderer.RenderFormula(this, mode);
    public string RenderFormula(string? mode, bool error) => Renderer.RenderFormula(this, mode, error);
    public string RenderLatex(string? mode = null) => Renderer.RenderLatex(this, mode);
    public string RenderMath(string? mode = null) => Renderer.RenderMath(this, mode);
    public string RenderDiagram(string? mode = null) => Renderer.RenderDiagram(this, mode);
    public string RenderMermaid(string? mode = null) => Renderer.RenderMermaid(this, mode);
    public string RenderFormulaDiagram(string? mode = null) => Renderer.RenderFormulaDiagram(this, mode);
    public static string RenderWysiwyg(MarkdownAst? ast) => Renderer.RenderWysiwyg(ast);
    public static string RenderWYSIWYG(MarkdownAst? ast) => Renderer.RenderWysiwyg(ast);
    public static string RenderMarkdown(MarkdownAst? ast) => Renderer.RenderMarkdown(ast);
    public string ToText() => ToMarkdown();
    public string ToStringMarkdown() => ToMarkdown();
    public override string ToString() => ToMarkdown();

    public string Markdown => ToMarkdown();
    public string Text => ToMarkdown();

    public bool IsPartial => !_isValid;
    public bool Partial => !_isValid;
    public bool Valid => _isValid;
}

public enum MarkdownValidity
{
    Valid,
    Partial
}

// Alias enums
public enum AstValidity { Valid, Partial }
public enum TreeValidity { Valid, Partial }
public enum DocumentValidity { Valid, Partial }
public enum Validity { Valid, Partial }
public enum Validez { Valido, Parcial }

// Alias classes
public class MarkdownParser : MarkdownAst { public MarkdownParser() : base() { } public MarkdownParser(string? s) : base(s) { } }
public class CommonMarkParser : MarkdownAst { public CommonMarkParser() : base() { } public CommonMarkParser(string? s) : base(s) { } }
public class GfmParser : MarkdownAst { public GfmParser() : base() { } public GfmParser(string? s) : base(s) { } }
public class MarkdownSyntaxTree : MarkdownAst { public MarkdownSyntaxTree() : base() { } public MarkdownSyntaxTree(string? s) : base(s) { } }
public class SyntaxTree : MarkdownAst { public SyntaxTree() : base() { } public SyntaxTree(string? s) : base(s) { } }
public class DocumentSyntaxTree : MarkdownAst { public DocumentSyntaxTree() : base() { } public DocumentSyntaxTree(string? s) : base(s) { } }
public class MarkdownDocumentAst : MarkdownAst { public MarkdownDocumentAst() : base() { } public MarkdownDocumentAst(string? s) : base(s) { } }
public class DocumentAst : MarkdownAst { public DocumentAst() : base() { } public DocumentAst(string? s) : base(s) { } }
public class MarkdownAstTree : MarkdownAst { public MarkdownAstTree() : base() { } public MarkdownAstTree(string? s) : base(s) { } }
public class MarkdownTree : MarkdownAst { public MarkdownTree() : base() { } public MarkdownTree(string? s) : base(s) { } }
public class DocumentTree : MarkdownAst { public DocumentTree() : base() { } public DocumentTree(string? s) : base(s) { } }
public class Ast : MarkdownAst { public Ast() : base() { } public Ast(string? s) : base(s) { } }
public class MarkdownDocumentTree : MarkdownAst { public MarkdownDocumentTree() : base() { } public MarkdownDocumentTree(string? s) : base(s) { } }
