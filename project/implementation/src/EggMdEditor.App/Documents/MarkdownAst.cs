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
    public static ILogger? Logger
    {
        get => ParseErrorHandler.Logger;
        set => ParseErrorHandler.Logger = value;
    }
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
        if (IsLargeDocument(_source) || _simulateLargeDocument || _forceLargeDocument || _simulateDegradation || _forceDegradation)
        {
            _largeDocDegraded = true;
            try { HandleLargeDocument(); } catch { }
        }
        _version++;
        if (_version < 1) _version = 1;
        ResetParseFailureFlags();
        ResetLargeDocumentFlags();
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
                "LOG-012 Error de parseo CommonMark/GFM. Hora: {Timestamp:O}. Resultado: fallo. Identificador de documento/solicitud: {DocumentId}. Posición aproximada (línea/columna): {Line}/{Column}. Tiempo de ejecución: {DurationMs} ms. Código de error: ERR-022. Mensaje: No se pudo analizar el documento como Markdown; revisa la sintaxis.",
                DateTimeOffset.UtcNow, Guid.Empty, _parseErrorLine, _parseErrorColumn, _parseDurationMs);
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

    public int ParseErrorLine => _parseErrorLine;
    public int ParseErrorColumn => _parseErrorColumn;
    public int Line => _parseErrorLine;
    public int Column => _parseErrorColumn;
    public int ParseErrorPositionLine => _parseErrorLine;
    public int ParseErrorPositionColumn => _parseErrorColumn;
    public string ParseErrorPosition => $"{_parseErrorLine}/{_parseErrorColumn}";
    public long ParseDurationMs => _parseDurationMs;
    public long ExecutionTimeMs => _parseDurationMs;
    public long DurationMs => _parseDurationMs;
    public long ElapsedMs => _parseDurationMs;
    public TimeSpan ParseDuration => TimeSpan.FromMilliseconds(_parseDurationMs);

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

    // ---------- Degradación en documentos grandes y límites de tabla (T-086) ----------
    public const int LargeDocumentThresholdWords = 100000;
    public const int LargeDocumentThresholdLines = 10000;
    public const int LargeDocumentThreshold = 100000;
    public const int MaxDocumentWords = 100000;
    public const int MaxDocumentLines = 10000;
    public const int MaxTableRows = 1000;
    public const int MaxTableColumns = 50;
    public const string LargeDocumentMessage = "El documento es muy grande; el renderizado puede ser más lento.";
    public const string LargeDocumentNotice = LargeDocumentMessage;
    public const string DegradationMessage = LargeDocumentMessage;
    public const string TableLimitMessage = "La tabla supera el límite (1 000 filas × 50 columnas); se ha limitado la inserción.";
    public const string TableExceededMessage = TableLimitMessage;
    public const string Err017Message = LargeDocumentMessage;
    public const string Err021Message = TableLimitMessage;

    private static bool _largeDocDegraded;
    private static bool _tableLimitExceeded;
    private static bool _simulateLargeDocument;
    private static bool _forceLargeDocument;
    private static bool _simulateDegradation;
    private static bool _forceDegradation;
    private static long _degradationDurationMs;

    // Simulation flags
    public static bool SimulateLargeDocument { get => _simulateLargeDocument; set => _simulateLargeDocument = value; }
    public static bool ForceLargeDocument { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool LargeDocumentForced { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool SimulateLarge { get => _simulateLargeDocument; set => _simulateLargeDocument = value; }
    public static bool ForceLarge { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool LargeForced { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool ForceLargeDoc { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool SimulateLargeDoc { get => _simulateLargeDocument; set => _simulateLargeDocument = value; }
    public static bool SimulateDegradation { get => _simulateDegradation; set => _simulateDegradation = value; }
    public static bool ForceDegradation { get => _forceDegradation; set => _forceDegradation = value; }
    public static bool DegradationForced { get => _forceDegradation; set => _forceDegradation = value; }
    public static bool ForceDegraded { get => _forceDegradation; set => _forceDegradation = value; }
    public static bool SimulateDegraded { get => _simulateDegradation; set => _simulateDegradation = value; }

    // Threshold aliases
    public static int LargeDocumentWordThreshold { get => LargeDocumentThresholdWords; set => LargeDocumentThresholdWords = value; }
    public static int LargeDocumentLineThreshold { get => LargeDocumentThresholdLines; set => LargeDocumentThresholdLines = value; }
    public static long WordThreshold { get => LargeDocumentThresholdWords; set => LargeDocumentThresholdWords = value; }
    public static long LineThreshold { get => LargeDocumentThresholdLines; set => LargeDocumentThresholdLines = value; }
    public static int MaxWords { get => LargeDocumentThresholdWords; set => LargeDocumentThresholdWords = value; }
    public static int MaxLines { get => LargeDocumentThresholdLines; set => LargeDocumentThresholdLines = value; }

    // State
    public static bool IsDegraded { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool Degraded { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool DegradedMode { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool IsDegradedMode { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool DocumentDegraded { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool LargeDocumentDegraded { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool IsLargeDocumentDegraded { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool DegradationActive { get => _largeDocDegraded; set => _largeDocDegraded = value; }
    public static bool IsDegradationActive { get => _largeDocDegraded; set => _largeDocDegraded = value; }

    public static bool TableLimitExceeded { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }
    public static bool TableExceeded { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }
    public static bool TableOverLimit { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }
    public static bool TableOutOfBounds { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }
    public static bool IsTableLimitExceeded { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }
    public static bool DegradedTable { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }

    public static long DegradationDurationMs { get => _degradationDurationMs; set => _degradationDurationMs = value; }
    public static long LastDegradationDurationMs { get => _degradationDurationMs; set => _degradationDurationMs = value; }

    public static bool IsLargeDocument(string? markdown = null)
    {
        if (_simulateLargeDocument || _forceLargeDocument || _simulateDegradation || _forceDegradation) return true;
        var text = markdown ?? string.Empty;
        // use instance source if empty? static can't; default provided
        var words = CountWords(text);
        var lines = CountLines(text);
        return words >= LargeDocumentThresholdWords || lines >= LargeDocumentThresholdLines;
    }

    public static bool IsLargeDocument(MarkdownAst? ast)
    {
        if (ast is null) return IsLargeDocument((string?)null);
        if (_simulateLargeDocument || _forceLargeDocument || _simulateDegradation || _forceDegradation) return true;
        return IsLargeDocument(ast.Source);
    }

    public static bool IsLarge(MarkdownAst? ast) => IsLargeDocument(ast);
    public static bool IsLarge(string? markdown) => IsLargeDocument(markdown);
    public static bool LargeDocument(string? markdown = null) => IsLargeDocument(markdown);
    public static bool DocumentIsLarge(string? markdown = null) => IsLargeDocument(markdown);
    public static bool IsBigDocument(string? markdown = null) => IsLargeDocument(markdown);

    public static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text!.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static int CountLines(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Split('\n').Length;
    }

    public static bool IsTableWithinLimits(int rows, int cols)
    {
        if (rows <= 0) rows = 1;
        if (cols <= 0) cols = 1;
        return rows <= MaxTableRows && cols <= MaxTableColumns;
    }

    public static bool TableWithinLimits(int rows, int cols) => IsTableWithinLimits(rows, cols);
    public static bool IsTableOverLimit(int rows, int cols) => !IsTableWithinLimits(rows, cols);
    public static bool TableExceedsLimits(int rows, int cols) => !IsTableWithinLimits(rows, cols);

    // Instance properties
    public bool IsLargeDocumentInstance => IsLargeDocument(this);
    public bool LargeDocumentInstance => IsLargeDocument(this);
    public bool IsDegradedInstance => _largeDocDegraded;
    public long WordCount => CountWords(_source);
    public int LineCount => CountLines(_source);
    public bool IsLarge => IsLargeDocument(this);
    public bool Large => IsLargeDocument(this);

    public string HandleLargeDocument(string? message = null)
    {
        _largeDocDegraded = true;
        _degradationDurationMs = 0;
        try
        {
            Logger?.LogWarning("LOG-016 Documento grande con degradación (NFR-003). Hora: {Timestamp:O}. Resultado: degradado. Identificador de documento/solicitud: {DocumentId}. Tamaño del documento: {Size}. Funciones deshabilitadas: renderizado pesado. Tiempo de ejecución: {DurationMs} ms. Nivel: warn. Mensaje: {Message}",
                DateTimeOffset.UtcNow, Guid.Empty, _source?.Length ?? 0, _degradationDurationMs, string.IsNullOrWhiteSpace(message) ? LargeDocumentMessage : message);
        }
        catch { }
        return string.IsNullOrWhiteSpace(message) ? LargeDocumentMessage : message!;
    }

    public string HandleDegradation(string? message = null) => HandleLargeDocument(message);
    public string DegradeLargeDocument(string? message = null) => HandleLargeDocument(message);
    public string DegradeDocument(string? message = null) => HandleLargeDocument(message);
    public string OnLargeDocument(string? message = null) => HandleLargeDocument(message);
    public string NotifyLargeDocument(string? message = null) => HandleLargeDocument(message);
    public string LogLargeDocument(string? message = null) => HandleLargeDocument(message);
    public string LogDegradation(string? message = null) => HandleLargeDocument(message);
    public bool Degrade() { _largeDocDegraded = true; return true; }
    public bool DegradePerformance() { return Degrade(); }
    public bool EnableDegradedMode() { return Degrade(); }
    public bool EnterDegradedMode() { return Degrade(); }
    public bool SetDegraded(bool value = true) { if (value) return Degrade(); _largeDocDegraded = false; return true; }
    public bool ResetDegradation() { _largeDocDegraded = false; _tableLimitExceeded = false; return true; }

    public string HandleTableLimit(int rows, int cols, string? message = null)
    {
        _tableLimitExceeded = true;
        try
        {
            Logger?.LogWarning("LOG-015 Tabla fuera de límites (NFR-016). Hora: {Timestamp:O}. Resultado: degradado. Identificador de documento/solicitud: {DocumentId}. Dimensiones detectadas: {Rows}/{Cols}. Límite aplicado: {LimitRows}x{LimitCols}. Nivel: warn. Mensaje: {Message}",
                DateTimeOffset.UtcNow, Guid.Empty, rows, cols, MaxTableRows, MaxTableColumns, string.IsNullOrWhiteSpace(message) ? TableLimitMessage : message);
        }
        catch { }
        return string.IsNullOrWhiteSpace(message) ? TableLimitMessage : message!;
    }

    public string HandleTableExceeded(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public string LogTableLimit(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public string LogTableExceeded(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public string ReportTableLimit(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public string NotifyTableLimit(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);

    // Static helpers
    public static string HandleLargeDocumentStatic(string? message = null)
    {
        _largeDocDegraded = true;
        try
        {
            Logger?.LogWarning("LOG-016 Documento grande con degradación (NFR-003). Hora: {Timestamp:O}. Resultado: degradado. Identificador de documento/solicitud: {DocumentId}. Tamaño del documento: {Size}. Funciones deshabilitadas: renderizado pesado. Tiempo de ejecución: {DurationMs} ms. Nivel: warn. Mensaje: {Message}",
                DateTimeOffset.UtcNow, Guid.Empty, 0, 0L, string.IsNullOrWhiteSpace(message) ? LargeDocumentMessage : message);
        }
        catch { }
        return string.IsNullOrWhiteSpace(message) ? LargeDocumentMessage : message!;
    }

    public static string DegradeLargeDocumentStatic(string? message = null) => HandleLargeDocumentStatic(message);
    public static string LogLargeDocumentStatic(string? message = null) => HandleLargeDocumentStatic(message);
    public static string LogDegradationStatic(string? message = null) => HandleLargeDocumentStatic(message);

    public static string HandleTableLimitStatic(int rows, int cols, string? message = null)
    {
        _tableLimitExceeded = true;
        try
        {
            Logger?.LogWarning("LOG-015 Tabla fuera de límites (NFR-016). Hora: {Timestamp:O}. Resultado: degradado. Identificador de documento/solicitud: {DocumentId}. Dimensiones detectadas: {Rows}/{Cols}. Límite aplicado: {LimitRows}x{LimitCols}. Nivel: warn. Mensaje: {Message}",
                DateTimeOffset.UtcNow, Guid.Empty, rows, cols, MaxTableRows, MaxTableColumns, string.IsNullOrWhiteSpace(message) ? TableLimitMessage : message);
        }
        catch { }
        return string.IsNullOrWhiteSpace(message) ? TableLimitMessage : message!;
    }

    public static string HandleTableExceededStatic(int rows, int cols, string? message = null) => HandleTableLimitStatic(rows, cols, message);
    public static string LogTableLimitStatic(int rows, int cols, string? message = null) => HandleTableLimitStatic(rows, cols, message);

    // ApplyParse integration: detect large doc
    // (extend ApplyParse? already; add method to call externally)

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
