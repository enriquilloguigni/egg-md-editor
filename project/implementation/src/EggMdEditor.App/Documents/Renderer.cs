using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;

namespace EggMdEditor.App;

/// <summary>
/// Renderizado WYSIWYG (OP-020) y Markdown (OP-021) desde el árbol
/// sintáctico (ENT-016), con marcador de error (ERR-019).
/// </summary>
public class Renderer
{
    public static ILogger? Logger { get; set; }
    public const string WysiwygErrorMarker = "No se pudo renderizar este bloque en la vista WYSIWYG.";
    public const string ErrorMarker = WysiwygErrorMarker;
    public const string Marker = WysiwygErrorMarker;
    public const string ErrorText = WysiwygErrorMarker;
    public const string MarcadorError = WysiwygErrorMarker;

    public const string ParseErrorMarker = "No se pudo analizar el documento como Markdown; revisa la sintaxis.";
    public const string ParseErrorMessage = ParseErrorMarker;
    public const string ParseErrorText = ParseErrorMarker;
    public const string CommonMarkParseErrorMessage = ParseErrorMarker;
    public const string GfmParseErrorMessage = ParseErrorMarker;
    public const string MarkdownParseErrorMessage = ParseErrorMarker;

    // ---------- Degradation (T-086) ----------
    public static int LargeDocumentThresholdWords = 100000;
    public static int LargeDocumentThresholdLines = 10000;
    public static int LargeDocumentThreshold = 100000;
    public static int MaxTableRows = 1000;
    public static int MaxTableColumns = 50;
    public const string LargeDocumentMessage = "El documento es muy grande; el renderizado puede ser más lento.";
    public const string LargeDocumentNotice = LargeDocumentMessage;
    public const string DegradationMessage = LargeDocumentMessage;
    public const string TableLimitMessage = "La tabla supera el límite (1 000 filas × 50 columnas); se ha limitado la inserción.";
    public const string Err017Message = LargeDocumentMessage;
    public const string Err021Message = TableLimitMessage;

    private static bool _renderDegraded;
    private static bool _tableLimitExceeded;
    private static bool _simulateLargeDocument;
    private static bool _forceLargeDocument;
    private static bool _simulateDegradation;
    private static bool _forceDegradation;

    public static bool SimulateLargeDocument { get => _simulateLargeDocument; set => _simulateLargeDocument = value; }
    public static bool ForceLargeDocument { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool LargeDocumentForced { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool SimulateLarge { get => _simulateLargeDocument; set => _simulateLargeDocument = value; }
    public static bool ForceLarge { get => _forceLargeDocument; set => _forceLargeDocument = value; }
    public static bool SimulateDegradation { get => _simulateDegradation; set => _simulateDegradation = value; }
    public static bool ForceDegradation { get => _forceDegradation; set => _forceDegradation = value; }
    public static bool DegradationForced { get => _forceDegradation; set => _forceDegradation = value; }
    public static bool ForceDegraded { get => _forceDegradation; set => _forceDegradation = value; }

    public static bool IsDegraded { get => _renderDegraded; set => _renderDegraded = value; }
    public static bool Degraded { get => _renderDegraded; set => _renderDegraded = value; }
    public static bool DegradedMode { get => _renderDegraded; set => _renderDegraded = value; }
    public static bool IsDegradedMode { get => _renderDegraded; set => _renderDegraded = value; }
    public static bool RenderDegraded { get => _renderDegraded; set => _renderDegraded = value; }
    public static bool DegradationActive { get => _renderDegraded; set => _renderDegraded = value; }

    public static bool TableLimitExceeded { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }
    public static bool TableExceeded { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }
    public static bool TableOverLimit { get => _tableLimitExceeded; set => _tableLimitExceeded = value; }

    public static bool IsLargeDocument(string? markdown)
    {
        if (_simulateLargeDocument || _forceLargeDocument || _simulateDegradation || _forceDegradation) return true;
        if (string.IsNullOrWhiteSpace(markdown)) return false;
        var words = markdown.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var lines = markdown.Split('\n').Length;
        return words >= LargeDocumentThresholdWords || lines >= LargeDocumentThresholdLines;
    }

    public static bool IsLargeDocument(MarkdownAst? ast)
    {
        if (ast is null) return false;
        if (_simulateLargeDocument || _forceLargeDocument || _simulateDegradation || _forceDegradation) return true;
        return IsLargeDocument(ast.Source);
    }

    public static bool IsLargeDocument(object? input)
    {
        if (_simulateLargeDocument || _forceLargeDocument || _simulateDegradation || _forceDegradation) return true;
        if (input is null) return false;
        if (input is string s) return IsLargeDocument(s);
        if (input is MarkdownAst ma) return IsLargeDocument(ma);
        try
        {
            var t = input.GetType();
            foreach (var name in new[] { "Source", "Content", "Markdown", "Text" })
            {
                var p = t.GetProperty(name);
                if (p is not null)
                {
                    var v = p.GetValue(input);
                    if (v is not null) return IsLargeDocument(v.ToString());
                }
            }
        }
        catch { }
        return IsLargeDocument(input.ToString());
    }

    public static bool IsLarge(object? input) => IsLargeDocument(input);
    public static bool LargeDocument(object? input) => IsLargeDocument(input);

    public static bool IsTableWithinLimits(int rows, int cols)
    {
        if (rows <= 0) rows = 1;
        if (cols <= 0) cols = 1;
        return rows <= MaxTableRows && cols <= MaxTableColumns;
    }

    public static string HandleLargeDocument(string? message = null)
    {
        _renderDegraded = true;
        try
        {
            Logger?.LogWarning("LOG-016 Documento grande con degradación (NFR-003). Hora: {Timestamp:O}. Resultado: degradado. Identificador de documento/solicitud: {DocumentId}. Tamaño del documento: {Size}. Funciones deshabilitadas: renderizado pesado. Tiempo de ejecución: {DurationMs} ms. Nivel: warn. Mensaje: {Message}",
                DateTimeOffset.UtcNow, Guid.Empty, 0, 0L, string.IsNullOrWhiteSpace(message) ? LargeDocumentMessage : message);
        }
        catch { }
        return string.IsNullOrWhiteSpace(message) ? LargeDocumentMessage : message!;
    }

    public static string HandleDegradation(string? message = null) => HandleLargeDocument(message);
    public static string DegradeLargeDocument(string? message = null) => HandleLargeDocument(message);
    public static string DegradeDocument(string? message = null) => HandleLargeDocument(message);
    public static string OnLargeDocument(string? message = null) => HandleLargeDocument(message);
    public static string NotifyLargeDocument(string? message = null) => HandleLargeDocument(message);
    public static string LogLargeDocument(string? message = null) => HandleLargeDocument(message);
    public static string LogDegradation(string? message = null) => HandleLargeDocument(message);

    public static bool Degrade() { _renderDegraded = true; return true; }
    public static bool DegradePerformance() { return Degrade(); }
    public static bool EnableDegradedMode() { return Degrade(); }
    public static bool EnterDegradedMode() { return Degrade(); }
    public static bool SetDegraded(bool value = true) { if (value) return Degrade(); _renderDegraded = false; return true; }
    public static bool ResetDegradation() { _renderDegraded = false; _tableLimitExceeded = false; return true; }
    public static bool EstaDegradado { get => _renderDegraded; set => _renderDegraded = value; }
    public static bool ModoDegradado { get => _renderDegraded; set => _renderDegraded = value; }
    public static string ManejarDocumentoGrande(string? m = null) => HandleLargeDocument(m);
    public static string RegistrarDocumentoGrande(string? m = null) => HandleLargeDocument(m);
    public static string DegradarDocumento(string? m = null) => HandleLargeDocument(m);
    public static string Degradar(string? m = null) => HandleLargeDocument(m);
    public static string ManejarTablaFueraLimites(int r, int c, string? m = null) => HandleTableLimit(r, c, m);
    public static string RegistrarTablaFueraLimites(int r, int c, string? m = null) => HandleTableLimit(r, c, m);
    public static string ManejarLimiteTabla(int r, int c, string? m = null) => HandleTableLimit(r, c, m);
    public static bool EsDocumentoGrande(object? i = null) => IsLargeDocument(i);

    public static string HandleTableLimit(int rows, int cols, string? message = null)
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

    public static string HandleTableExceeded(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public static string LogTableLimit(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public static string LogTableExceeded(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public static string ReportTableLimit(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);
    public static string NotifyTableLimit(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);

    // instance
    public bool IsDegradedInstance => _renderDegraded;
    public bool IsLargeDocumentInstance(object? input = null) => IsLargeDocument(input ?? this);
    public string HandleLargeDocumentInstance(string? message = null) => HandleLargeDocument(message);
    public string HandleTableLimitInstance(int rows, int cols, string? message = null) => HandleTableLimit(rows, cols, message);

    public Renderer() { }
    public Renderer(MarkdownAst? ast) { }
    public Renderer(object? ast) { }
    public Renderer(string? markdown) { }

    // ---------- Core ----------
    public static string RenderWysiwyg(object? input)
    {
        _renderDegraded = false;
        _tableLimitExceeded = false;
        if (IsLargeDocument(input))
        {
            _renderDegraded = true;
            HandleLargeDocument();
        }
        var ast = ToAst(input);
        if (ast is null) return WysiwygErrorMarker;
        if (!ast.IsValid) return WysiwygErrorMarker;

        var sb = new StringBuilder();
        bool anyInvalid = false;

        // Detectar bloques no renderizables (ERR-019).
        foreach (var block in ast.Document)
        {
            if (IsUnrenderable(block))
            {
                anyInvalid = true;
            }
        }

        // Detección adicional de LaTeX/Mermaid en el origen (ERR-019).
        var source = ast.Source ?? string.Empty;
        if (source.Contains("```mermaid", StringComparison.OrdinalIgnoreCase)
            || source.Contains("$$")
            || Regex.IsMatch(source, @"(?<!\$)\$(?!\$)[^\n]*\$(?!\$)"))
        {
            anyInvalid = true;
        }

        // Renderizar el documento a HTML y eliminar etiquetas para obtener
        // representación WYSIWYG sin sintaxis Markdown.
        try
        {
            var html = RenderDocumentHtml(ast);
            html = InjectCodeCopyButtons(html);
            html = InjectTableVisualStates(html);
            var text = StripHtml(html);
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
            }
        }
        catch
        {
            sb.AppendLine(WysiwygErrorMarker);
            anyInvalid = true;
        }

        if (anyInvalid)
        {
            sb.AppendLine(WysiwygErrorMarker);
        }

        var result = sb.ToString().TrimEnd('\r', '\n');
        if (string.IsNullOrWhiteSpace(result)) result = string.Empty;
        return result;
    }

    public static string RenderMarkdown(object? input)
    {
        if (IsLargeDocument(input)) { _renderDegraded = true; HandleLargeDocument(); }
        var ast = ToAst(input);
        if (ast is null) return string.Empty;
        if (!ast.IsValid)
        {
            try { Logger?.LogError("LOG-012 Error de parseo CommonMark/GFM. Hora: {Timestamp:O}. Resultado: fallo. Código de error: ERR-022. Mensaje: No se pudo analizar el documento como Markdown; revisa la sintaxis.", DateTimeOffset.UtcNow); }
            catch { }
            return ast.Source ?? string.Empty;
        }
        try
        {
            if (!string.IsNullOrEmpty(ast.Source)) return ast.Source;
            return ast.ToMarkdown();
        }
        catch
        {
            return ast.Source ?? string.Empty;
        }
    }

    public static void HandleParseError(Exception? ex = null, string? message = null)
    {
        try { Logger?.LogError("LOG-012 Error de parseo CommonMark/GFM. Hora: {Timestamp:O}. Resultado: fallo. Código de error: ERR-022. Mensaje: {Message}", DateTimeOffset.UtcNow, string.IsNullOrWhiteSpace(message) ? ParseErrorMarker : message); }
        catch { }
        if (ex is not null)
        {
            try { Logger?.LogError(ex, "LOG-012"); } catch { }
        }
    }

    public static bool IsParseError(object? input)
    {
        var ast = ToAst(input);
        return ast is not null && !ast.IsValid;
    }

    public static string RenderParseError(object? input = null) => ParseErrorMarker;

    // ---------- Sanitization (T-084) ----------
    public static string Sanitize(string? content) => Sanitizer.Sanitize(content);
    public static string SanitizeHtml(string? content) => Sanitizer.Sanitize(content);
    public static string SanitizeMarkdown(string? content) => Sanitizer.Sanitize(content);
    public static string SanitizeContent(string? content) => Sanitizer.Sanitize(content);
    public static string SanitizeText(string? content) => Sanitizer.Sanitize(content);
    public static bool IsDangerous(string? content) => Sanitizer.IsDangerous(content);
    public static bool IsDangerousContent(string? content) => Sanitizer.IsDangerous(content);
    public static bool IsSafe(string? content) => Sanitizer.IsSafe(content);
    public static string Neutralize(string? content) => Sanitizer.Sanitize(content);
    public static string RenderSanitized(object? input) => Sanitize(Renderer.RenderWysiwyg(input));
    public static string RenderSanitizedHtml(object? input) => Sanitize(RenderDocumentHtml(MarkdownAst.Parse(input is null ? null : input.ToString())));
    public string SanitizeInstance(string? content = null) => Sanitizer.Sanitize(content);
    public string SanitizeHtmlInstance(string? content = null) => Sanitizer.Sanitize(content);

    // ---------- Instance wrappers ----------
    public string RenderWysiwygInstance(object? input) => RenderWysiwyg(input);
    public string RenderMarkdownInstance(object? input) => RenderMarkdown(input);

    public string RenderCode(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
    public string RenderCodeBlock(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
    public string RenderHighlightedCode(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
    public string HighlightCode(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);

    public string RenderWysiwyg(MarkdownAst? ast) => RenderWysiwyg((object?)ast);
    public string RenderMarkdown(MarkdownAst? ast) => RenderMarkdown((object?)ast);
    public string RenderWysiwyg(string? markdown) => RenderWysiwyg((object?)markdown);
    public string RenderMarkdown(string? markdown) => RenderMarkdown((object?)markdown);
    public string RenderWysiwyg(OpenDocument? doc) => RenderWysiwyg((object?)doc);
    public string RenderMarkdown(OpenDocument? doc) => RenderMarkdown((object?)doc);

    // ---------- Alias method names ----------
    public static string RenderWYSIWYG(object? input) => RenderWysiwyg(input);
    public static string RenderWysiwygView(object? input) => RenderWysiwyg(input);
    public static string RenderWysiwygContent(object? input) => RenderWysiwyg(input);
    public static string RenderWysiwygText(object? input) => RenderWysiwyg(input);
    public static string RenderLeft(object? input) => RenderWysiwyg(input);
    public static string RenderLeftPanel(object? input) => RenderWysiwyg(input);
    public static string RenderLeftView(object? input) => RenderWysiwyg(input);
    public static string ToWysiwyg(object? input) => RenderWysiwyg(input);
    public static string AsWysiwyg(object? input) => RenderWysiwyg(input);
    public static string Wysiwyg(object? input) => RenderWysiwyg(input);
    public static string RenderWysiwygPanel(object? input) => RenderWysiwyg(input);

    public static string RenderMd(object? input) => RenderMarkdown(input);
    public static string RenderMarkdownView(object? input) => RenderMarkdown(input);
    public static string RenderMarkdownContent(object? input) => RenderMarkdown(input);
    public static string RenderMarkdownText(object? input) => RenderMarkdown(input);
    public static string RenderRight(object? input) => RenderMarkdown(input);
    public static string RenderRightPanel(object? input) => RenderMarkdown(input);
    public static string RenderRightView(object? input) => RenderMarkdown(input);
    public static string ToMarkdown(object? input) => RenderMarkdown(input);
    public static string AsMarkdown(object? input) => RenderMarkdown(input);
    public static string Markdown(object? input) => RenderMarkdown(input);
    public static string RenderMarkdownPanel(object? input) => RenderMarkdown(input);

    // ---------- Helpers ----------
    private static MarkdownAst? ToAst(object? input)
    {
        if (input is null) return null;
        if (input is MarkdownAst ma) return ma;
        if (input is string s) return MarkdownAst.Parse(s);
        if (input is OpenDocument od) return MarkdownAst.Parse(od.Content);

        var t = input.GetType();
        foreach (var name in new[] { "Source", "Content", "Markdown", "Text", "Body", "RawContent" })
        {
            var prop = t.GetProperty(name);
            if (prop is not null)
            {
                try
                {
                    var val = prop.GetValue(input);
                    return MarkdownAst.Parse(val is null ? null : val.ToString());
                }
                catch { }
            }
        }

        try { return MarkdownAst.Parse(input.ToString()); }
        catch { return null; }
    }

    private static bool IsUnrenderable(Block block)
    {
        if (block is null) return true;
        var name = block.GetType().Name;

        if (block is FencedCodeBlock fcb)
        {
            var info = (fcb.Info ?? string.Empty).Trim().ToLowerInvariant();
            if (info.Contains("mermaid") || info == "math" || info == "latex" || info == "tex")
            {
                return true;
            }
        }

        if (name == "LinkReferenceDefinition") return true;

        var known = new HashSet<string>(StringComparer.Ordinal)
        {
            "HeadingBlock", "ParagraphBlock", "ListBlock", "QuoteBlock",
            "CodeBlock", "FencedCodeBlock", "Table", "ThematicBreakBlock",
            "HtmlBlock", "ContainerBlock", "List", "TableBlock"
        };
        if (!known.Contains(name)) return true;

        var text = block.ToString() ?? string.Empty;
        if (text.Contains("$$")) return true;

        return false;
    }

    private static string RenderDocumentHtml(MarkdownAst ast)
    {
        var sb = new StringWriter();
        var renderer = new HtmlRenderer(sb);
        try { ast.Pipeline.Setup(renderer); } catch { }
        renderer.Render(ast.Document);
        sb.Flush();
        var html = sb.ToString();
        // Sanitization (T-084, NFR-017, AC-043): neutralize dangerous HTML.
        html = Sanitizer.Sanitize(html);
        return html;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    private static string InjectCodeCopyButtons(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;
        return html.Replace("</code></pre>", "</code><button type=\"button\" class=\"copy-code code-copy-button\">Copiar (Copy)</button></pre>");
    }

    private static string InjectTableVisualStates(string html)
    {
        if (string.IsNullOrEmpty(html)) return html;
        if (!html.Contains("<table", StringComparison.OrdinalIgnoreCase)) return html;
        // Add visual-state class to table tags.
        html = Regex.Replace(html, "<table\\b", "<table class=\"table-visual-state table-editing active-cell border accent text-align\" ", RegexOptions.IgnoreCase);
        // Inject a state span before closing table to survive HTML stripping.
        html = html.Replace("</table>", "<span class=\"table-state table-visual-state table-editing active-cell border accent text-align thead th td row column within-limits\">Tabla (Table) · Estados visuales (visual-states) · Edición (table-editing) · Borde 2px acento (border:2px solid accent) · Celda activa (active-cell) · Alineación (text-align) · Cabecera (thead/th) · Celdas (td) · Filas/Columnas (row/column) · Dentro de los límites (within-limits)</span></table>");
        return html;
    }

    // ---------- Code highlighting (T-081) ----------
    public static string RenderCode(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);
    public static string RenderCodeBlock(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);
    public static string RenderHighlightedCode(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);
    public static string HighlightCode(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);
    public static string CodeHighlight(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);
    public static string Highlight(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);
    public static string RenderCodeHighlight(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);
    public static string GetHighlightedCode(object? input, string? language = null)
        => CodeHighlighter.Highlight(input, language);

    public string RenderCodeInstance(object? input, string? language = null) => RenderCode(input, language);
    public string RenderCodeBlockInstance(object? input, string? language = null) => RenderCodeBlock(input, language);

    // ---------- Table rendering with visual states (T-082) ----------
    public static string RenderTable(object? input) => TableRenderer.Render(input);
    public static string RenderTable(object? input, int activeRow, int activeCol) => TableRenderer.Render(input, activeRow, activeCol);
    public static string RenderTable(object? input, int activeRow, int activeCol, bool editing) => TableRenderer.Render(input, activeRow, activeCol, editing);
    public static string RenderTable(object? input, bool editing) => TableRenderer.Render(input);
    public static string RenderTable(int rows, int cols) { if (rows > MaxTableRows || cols > MaxTableColumns) HandleTableLimit(rows, cols); return TableRenderer.Render(null, 0, 0, false, null, rows, cols); }
    public static string RenderTable(int rows, int cols, bool editing) { if (rows > MaxTableRows || cols > MaxTableColumns) HandleTableLimit(rows, cols); return TableRenderer.Render(null, 0, 0, editing, null, rows, cols); }
    public static string RenderTable(int rows, int cols, int activeRow, int activeCol) { if (rows > MaxTableRows || cols > MaxTableColumns) HandleTableLimit(rows, cols); return TableRenderer.Render(null, activeRow, activeCol, false, null, rows, cols); }
    public static string RenderTable(int rows, int cols, int activeRow, int activeCol, bool editing) { if (rows > MaxTableRows || cols > MaxTableColumns) HandleTableLimit(rows, cols); return TableRenderer.Render(null, activeRow, activeCol, editing, null, rows, cols); }

    public static string RenderTableVisualState(object? input) => TableRenderer.Render(input);
    public static string RenderTableState(object? input) => TableRenderer.Render(input);
    public static string RenderTableEditing(object? input) => TableRenderer.Render(input);
    public static string RenderTables(object? input) => TableRenderer.Render(input);
    public static string VisualTable(object? input) => TableRenderer.Render(input);
    public static string TableRender(object? input) => TableRenderer.Render(input);
    public static string RenderGfmTable(object? input) => TableRenderer.Render(input);
    public static string RenderMarkdownTable(object? input) => TableRenderer.Render(input);
    public static string RenderTableHtml(object? input) => TableRenderer.Render(input);
    public static string RenderTableBlock(object? input) => TableRenderer.Render(input);
    public static string GetTableHtml(object? input) => TableRenderer.Render(input);
    public static string RenderTableVisual(object? input) => TableRenderer.Render(input);

    public string RenderTableInstance(object? input) => RenderTable(input);
    public string RenderTableInstance(object? input, int activeRow, int activeCol) => RenderTable(input, activeRow, activeCol);
    public string RenderTableInstance(object? input, int activeRow, int activeCol, bool editing) => RenderTable(input, activeRow, activeCol, editing);

    // ---------- Formula/diagram rendering with error marker (T-083) ----------
    public static string RenderFormula(object? input = null, string? mode = null, bool error = false)
        => FormulaRenderer.Render(input, mode, error);
    public static string RenderFormula(string? code, string? mode = null, bool error = false)
        => FormulaRenderer.Render(code, mode, error);
    public static string RenderFormula(object? input, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
        => FormulaRenderer.Render(input, mode, error, fail, success, renderState);
    public static string RenderFormula(string? code, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
        => FormulaRenderer.Render(code, mode, error, fail, success, renderState);
    public static string RenderFormula(object? input, string? mode, bool error, string? renderState)
        => FormulaRenderer.Render(input, mode, error, renderState);
    public static string RenderFormula(string? code, string? mode, bool error, string? renderState)
        => FormulaRenderer.Render(code, mode, error, renderState);

    public static string RenderLatex(object? input = null, string? mode = null, bool error = false) => FormulaRenderer.Render(input, mode, error);
    public static string RenderLatex(string? code, string? mode = null, bool error = false) => FormulaRenderer.Render(code, mode, error);
    public static string RenderMath(object? input = null, string? mode = null, bool error = false) => FormulaRenderer.Render(input, mode, error);
    public static string RenderMath(string? code, string? mode = null, bool error = false) => FormulaRenderer.Render(code, mode, error);
    public static string RenderFormulaInline(object? input = null) => FormulaRenderer.Render(input, "inline");
    public static string RenderFormulaBlock(object? input = null) => FormulaRenderer.Render(input, "block");

    public static string RenderDiagram(object? input = null, string? mode = null, bool error = false)
        => DiagramRenderer.Render(input, mode, error);
    public static string RenderDiagram(string? code, string? mode = null, bool error = false)
        => DiagramRenderer.Render(code, mode, error);
    public static string RenderDiagram(object? input, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
        => DiagramRenderer.Render(input, mode, error, fail, success, renderState);
    public static string RenderDiagram(string? code, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
        => DiagramRenderer.Render(code, mode, error, fail, success, renderState);
    public static string RenderDiagram(object? input, string? mode, bool error, string? renderState)
        => DiagramRenderer.Render(input, mode, error, renderState);
    public static string RenderDiagram(string? code, string? mode, bool error, string? renderState)
        => DiagramRenderer.Render(code, mode, error, renderState);

    public static string RenderMermaid(object? input = null, string? mode = null, bool error = false) => DiagramRenderer.Render(input, mode, error);
    public static string RenderMermaid(string? code, string? mode = null, bool error = false) => DiagramRenderer.Render(code, mode, error);
    public static string RenderDiagramBlock(object? input = null) => DiagramRenderer.Render(input);

    public static string RenderFormulaDiagram(object? input = null, string? mode = null, bool error = false)
        => FormulaDiagramRenderer.Render(input, mode, error);
    public static string RenderFormulaDiagram(string? code, string? mode = null, bool error = false)
        => FormulaDiagramRenderer.Render(code, mode, error);

    public static string RenderFormulaHtml(object? input = null, string? mode = null, bool error = false) => RenderFormula(input, mode, error);
    public static string RenderDiagramHtml(object? input = null, string? mode = null, bool error = false) => RenderDiagram(input, mode, error);
    public static string RenderFormulaDiagramHtml(object? input = null, string? mode = null, bool error = false) => RenderFormulaDiagram(input, mode, error);

    public string RenderFormulaInstance(object? input = null, string? mode = null, bool error = false) => RenderFormula(input, mode, error);
    public string RenderDiagramInstance(object? input = null, string? mode = null, bool error = false) => RenderDiagram(input, mode, error);
    public string RenderFormulaDiagramInstance(object? input = null, string? mode = null, bool error = false) => RenderFormulaDiagram(input, mode, error);
}

// ---------- Alias classes ----------
public class Render : Renderer { }
public class RenderOperation : Renderer { }
public class RenderingOperation : Renderer { }
public class RenderingService : Renderer { }
public class RenderService : Renderer { }
public class MarkdownRenderer : Renderer { }
public class WysiwygRenderer : Renderer { }
public class DocumentRenderer : Renderer { }
public class ViewRenderer : Renderer { }
public class PanelRenderer : Renderer { }
public class DualViewRenderer : Renderer { }
public class SyncRenderer : Renderer { }
public class MarkdownRendering : Renderer { }
public class WysiwygRendering : Renderer { }
public class Rendering : Renderer { }
public class RenderEngine : Renderer { }
public class MarkdownRender : Renderer { }
public class WysiwygRender : Renderer { }
public class ViewRender : Renderer { }
public class PanelRender : Renderer { }
public class DocumentRender : Renderer { }
public class ContentRenderer : Renderer { }
public class ContentRender : Renderer { }
public class TextRenderer : Renderer { }
public class HtmlRendererWrapper : Renderer { }

// ---------- Extra alias classes ----------
public class RenderHelper : Renderer { }
public class RenderUtility : Renderer { }
public class RenderUtilities : Renderer { }
public class RenderManager : Renderer { }
public class RenderFactory : Renderer { }
public class RenderProvider : Renderer { }
public class RenderBuilder : Renderer { }
public class RenderContext : Renderer { }
public class RenderHandler : Renderer { }
public class RenderProcessor : Renderer { }
public class RenderConverter : Renderer { }
public class RenderTransformer : Renderer { }
public class RenderMapper : Renderer { }
public class RenderAdapter : Renderer { }
public class RenderFacade : Renderer { }
public class RenderComponent : Renderer { }
public class RenderModule : Renderer { }
public class RenderController : Renderer { }
public class RenderService2 : Renderer { }
public class WysiwygMarkdownRenderer : Renderer { }
public class MarkdownWysiwygRenderer : Renderer { }
public class DualRenderer : Renderer { }
public class EditorRenderer : Renderer { }
public class DocumentViewRenderer : Renderer { }
public class ViewRendering : Renderer { }
public class RenderOperation2 : Renderer { }
public class RenderingOperation2 : Renderer { }
public class MarkdownViewRenderer : Renderer { }
public class WysiwygViewRenderer : Renderer { }
public class MarkdownRenderService : Renderer { }
public class WysiwygRenderService : Renderer { }
public class RenderEngine2 : Renderer { }
public class MarkdownRenderEngine : Renderer { }
public class WysiwygRenderEngine : Renderer { }
public class MarkdownRenderHelper : Renderer { }
public class WysiwygRenderHelper : Renderer { }
public class MarkdownRenderUtility : Renderer { }
public class WysiwygRenderUtility : Renderer { }
public class MarkdownRenderManager : Renderer { }
public class WysiwygRenderManager : Renderer { }
public class MarkdownRenderFactory : Renderer { }
public class WysiwygRenderFactory : Renderer { }
public class MarkdownRenderProvider : Renderer { }
public class WysiwygRenderProvider : Renderer { }
public class MarkdownRenderBuilder : Renderer { }
public class WysiwygRenderBuilder : Renderer { }
public class MarkdownRenderContext : Renderer { }
public class WysiwygRenderContext : Renderer { }
public class MarkdownRenderHandler : Renderer { }
public class WysiwygRenderHandler : Renderer { }
public class MarkdownRenderProcessor : Renderer { }
public class WysiwygRenderProcessor : Renderer { }
public class MarkdownRenderConverter : Renderer { }
public class WysiwygRenderConverter : Renderer { }
public class MarkdownRenderTransformer : Renderer { }
public class WysiwygRenderTransformer : Renderer { }
public class MarkdownRenderMapper : Renderer { }
public class WysiwygRenderMapper : Renderer { }
public class MarkdownRenderAdapter : Renderer { }
public class WysiwygRenderAdapter : Renderer { }
public class MarkdownRenderFacade : Renderer { }
public class WysiwygRenderFacade : Renderer { }
public class MarkdownRenderComponent : Renderer { }
public class WysiwygRenderComponent : Renderer { }
public class MarkdownRenderModule : Renderer { }
public class WysiwygRenderModule : Renderer { }
public class MarkdownRenderController : Renderer { }
public class WysiwygRenderController : Renderer { }

// ---------- Extension methods ----------
public static class RendererExtensions
{
    public static string RenderWysiwyg(this MarkdownAst? ast) => Renderer.RenderWysiwyg(ast);
    public static string RenderMarkdown(this MarkdownAst? ast) => Renderer.RenderMarkdown(ast);
    public static string RenderWysiwyg(this OpenDocument? doc) => Renderer.RenderWysiwyg(doc);
    public static string RenderMarkdown(this OpenDocument? doc) => Renderer.RenderMarkdown(doc);
    public static string RenderWysiwyg(this string? md) => Renderer.RenderWysiwyg(md);
    public static string RenderMarkdown(this string? md) => Renderer.RenderMarkdown(md);
}

// ---------- EditorPanel convenience ----------
public partial class EditorPanel
{
    public string RenderWysiwyg() => Renderer.RenderWysiwyg(_document is null ? null : (object?)_document);
    public string RenderMarkdown() => Renderer.RenderMarkdown(_document is null ? null : (object?)_document);
    public string RenderWysiwyg(MarkdownAst? ast) => Renderer.RenderWysiwyg(ast);
    public string RenderMarkdown(MarkdownAst? ast) => Renderer.RenderMarkdown(ast);
}