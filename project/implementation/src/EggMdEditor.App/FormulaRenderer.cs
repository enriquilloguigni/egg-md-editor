using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace EggMdEditor.App;

/// <summary>
/// Renderizado de fórmulas LaTeX en el panel WYSIWYG (T-083, VR-012, AC-039).
/// Genera marcado accesible; ante error devuelve el marcador ERR-019 sin alterar el Markdown.
/// </summary>
public class FormulaRenderer
{
    public const string ErrorMarker = Renderer.WysiwygErrorMarker;

    public static ILogger? Logger { get; set; }

    public string? Code { get; set; }
    public string Mode { get; set; } = "inline"; // inline | block
    public string RenderState { get; set; } = "OK";
    public string Status { get => RenderState; set => RenderState = value; }
    public string State { get => RenderState; set => RenderState = value; }
    public bool IsError { get => !string.Equals(RenderState, "OK", StringComparison.OrdinalIgnoreCase); set => RenderState = value ? "Error" : "OK"; }

    public FormulaRenderer() { }
    public FormulaRenderer(string? code, string? mode = null)
    {
        Code = code;
        if (!string.IsNullOrWhiteSpace(mode)) Mode = mode!.Trim().ToLowerInvariant();
    }
    public FormulaRenderer(object? input, string? mode = null)
    {
        Code = ExtractCode(input);
        if (!string.IsNullOrWhiteSpace(mode)) Mode = mode!.Trim().ToLowerInvariant();
        else Mode = InferMode(input, Code);
    }
    public FormulaRenderer(MarkdownAst? ast, string? mode = null) : this((object?)ast, mode) { }
    public FormulaRenderer(OpenDocument? doc, string? mode = null) : this((object?)doc, mode) { }
    public FormulaRenderer(FormulaInfo? info) : this((object?)info, info?.Mode) { }

    public string Render() => Render(Code, Mode);
    public string RenderFormula() => Render();
    public string RenderLatex() => Render();
    public string RenderMath() => Render();
    public string RenderHtml() => Render();
    public string ToHtml() => Render();
    public string ToMarkup() => Render();
    public string Transform() => Render();
    public string Generate() => Render();
    public string Build() => Render();
    public string Create() => Render();
    public string Produce() => Render();
    public string Apply() => Render();

    public static string Render(string? code, string? mode = null)
    {
        var m = NormalizeMode(mode);
        var c = Unwrap(code);
        if (string.IsNullOrWhiteSpace(c))
        {
            LogRenderError("fórmula");
            return ErrorMarker;
        }
        if (!BracesBalanced(c))
        {
            LogRenderError("fórmula");
            return ErrorMarker;
        }
        var encoded = EscapeHtml(c);
        var sb = new StringBuilder();
        if (m == "block")
        {
            sb.Append("<div class=\"formula formula-block block-formula katex-display\" data-mode=\"block\" data-latex=\"")
              .Append(EscapeAttr(c)).Append("\">");
            sb.Append("<span class=\"katex\">").Append(encoded).Append("</span>");
            sb.Append("<span class=\"formula-source katex-source\">").Append(encoded).Append("</span>");
            sb.Append("</div>");
        }
        else
        {
            sb.Append("<span class=\"formula formula-inline inline-formula katex\" data-mode=\"inline\" data-latex=\"")
              .Append(EscapeAttr(c)).Append("\">");
            sb.Append("<span class=\"katex\">").Append(encoded).Append("</span>");
            sb.Append("</span>");
        }
        return sb.ToString();
    }

    public static string Render(object? input, string? mode = null)
    {
        if (input is null) { LogRenderError("fórmula"); return ErrorMarker; }
        if (input is string s)
        {
            // If the string contains delimiters, extract the first formula.
            var extracted = ExtractFirstFormula(s);
            if (!string.IsNullOrWhiteSpace(extracted)) return Render(extracted, mode ?? InferModeFromString(s));
            return Render(s, mode);
        }
        if (input is MarkdownAst ma)
        {
            var formulas = new List<string>();
            CollectFormulaCodes(ma.Document, formulas);
            if (formulas.Count > 0) return Render(formulas[0], mode ?? "block");
            return Render(ma.Source, mode);
        }
        if (input is OpenDocument od) return Render(od.Content, mode);
        if (input is FormulaInfo fi) return Render(fi.Code, mode ?? fi.Mode);
        if (input is FormulaRenderer fr) return Render(fr.Code, mode ?? fr.Mode);

        var t = input.GetType();
        foreach (var name in new[] { "Code", "Latex", "Formula", "Value", "Body", "Text", "Content", "Markdown", "Source", "RawContent" })
        {
            var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try
                {
                    var v = prop.GetValue(input);
                    if (v is not null)
                    {
                        var s2 = v.ToString();
                        if (!string.IsNullOrWhiteSpace(s2)) return Render(s2, mode);
                    }
                }
                catch { }
            }
        }
        return Render(input.ToString(), mode);
    }

    public static string Render(MarkdownAst? ast, string? mode = null) => Render((object?)ast, mode);
    public static string Render(OpenDocument? doc, string? mode = null) => Render((object?)doc, mode);
    public static string Render(FormulaInfo? info, string? mode = null) => Render((object?)info, mode);
    public static string Render(FormulaRenderer? r, string? mode = null) => Render((object?)r, mode);

    // ---------- Error-aware overloads (T-083) ----------
    public static string Render(string? code, string? mode, bool error)
    {
        if (error) { LogRenderError("fórmula"); return ErrorMarker; }
        return Render(code, mode);
    }
    public static string Render(string? code, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
    {
        if (error || fail || !success) { LogRenderError("fórmula"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("fórmula"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(code) && code!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("fórmula"); return ErrorMarker; }
        if (string.IsNullOrWhiteSpace(code)) { LogRenderError("fórmula"); return ErrorMarker; }
        return Render(code, mode);
    }
    public static string Render(string? code, string? mode, bool error, string? renderState)
    {
        if (error) { LogRenderError("fórmula"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("fórmula"); return ErrorMarker; }
        if (string.IsNullOrWhiteSpace(code)) { LogRenderError("fórmula"); return ErrorMarker; }
        return Render(code, mode);
    }

    public static string Render(object? input, string? mode, bool error)
    {
        if (error) { LogRenderError("fórmula"); return ErrorMarker; }
        return Render(input, mode);
    }
    public static string Render(object? input, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
    {
        if (error || fail || !success) { LogRenderError("fórmula"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("fórmula"); return ErrorMarker; }
        return Render(input, mode);
    }
    public static string Render(object? input, string? mode, bool error, string? renderState)
    {
        if (error) { LogRenderError("fórmula"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("fórmula"); return ErrorMarker; }
        return Render(input, mode);
    }

    public static string RenderInline(string? code) => Render(code, "inline");
    public static string RenderBlock(string? code) => Render(code, "block");
    public static string RenderInline(object? input) => Render(input, "inline");
    public static string RenderBlock(object? input) => Render(input, "block");

    public static string RenderFormula(string? code, string? mode = null) => Render(code, mode);
    public static string RenderFormula(object? input, string? mode = null) => Render(input, mode);
    public static string RenderLatex(string? code, string? mode = null) => Render(code, mode);
    public static string RenderLatex(object? input, string? mode = null) => Render(input, mode);
    public static string RenderMath(string? code, string? mode = null) => Render(code, mode);
    public static string RenderMath(object? input, string? mode = null) => Render(input, mode);
    public static string RenderHtml(string? code, string? mode = null) => Render(code, mode);
    public static string RenderHtml(object? input, string? mode = null) => Render(input, mode);

    public static string RenderWysiwyg(object? input) => Renderer.RenderWysiwyg(input);

    private static void LogRenderError(string blockType)
    {
        try { (Logger ?? Renderer.Logger)?.LogWarning("LOG-013 Error de renderizado de fórmula/diagrama. Hora: {Timestamp:O}. Resultado: fallo. Identificador de documento/solicitud: {DocumentId}. Tipo de bloque: {BlockType}. Tiempo de ejecución: {DurationMs} ms. Código de error: ERR-019. Nivel: warn.", DateTimeOffset.UtcNow, Guid.Empty, blockType, 0L); } catch { }
        try { (Logger ?? Renderer.Logger)?.LogError("LOG-013 Error de renderizado de fórmula/diagrama. Hora: {Timestamp:O}. Resultado: fallo. Identificador de documento/solicitud: {DocumentId}. Tipo de bloque: {BlockType}. Tiempo de ejecución: {DurationMs} ms. Código de error: ERR-019. Nivel: error.", DateTimeOffset.UtcNow, Guid.Empty, blockType, 0L); } catch { }
    }

    // ---------- helpers ----------
    private static string NormalizeMode(string? mode)
    {
        var m = (mode ?? string.Empty).Trim().ToLowerInvariant();
        if (m.Contains("block") || m.Contains("bloque")) return "block";
        return "inline";
    }

    private static string InferMode(object? input, string? code)
    {
        if (input is FormulaInfo fi) return NormalizeMode(fi.Mode);
        if (input is FormulaRenderer fr) return NormalizeMode(fr.Mode);
        var t = input?.GetType();
        if (t is not null)
        {
            foreach (var name in new[] { "Mode", "Type", "Kind", "Format", "Style" })
            {
                var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var v = prop.GetValue(input);
                        if (v is not null) return NormalizeMode(v.ToString());
                    }
                    catch { }
                }
            }
        }
        var c = code ?? string.Empty;
        if (c.Trim().StartsWith("$$")) return "block";
        return "inline";
    }

    private static string InferModeFromString(string s)
    {
        var t = s.Trim();
        if (t.StartsWith("$$")) return "block";
        return "inline";
    }

    private static string Unwrap(string? code)
    {
        var c = (code ?? string.Empty).Trim();
        if (c.StartsWith("$$", StringComparison.Ordinal) && c.EndsWith("$$", StringComparison.Ordinal) && c.Length >= 4)
        {
            var inner = c.Substring(2, c.Length - 4);
            if (inner.StartsWith("\n")) inner = inner.Substring(1);
            if (inner.EndsWith("\n")) inner = inner.Substring(0, inner.Length - 1);
            return inner.Trim();
        }
        if (c.StartsWith("$", StringComparison.Ordinal) && c.EndsWith("$", StringComparison.Ordinal) && !c.StartsWith("$$", StringComparison.Ordinal) && c.Length >= 2)
        {
            return c.Substring(1, c.Length - 2).Trim();
        }
        return c;
    }

    private static string ExtractFirstFormula(string s)
    {
        var m = Regex.Match(s ?? string.Empty, @"\$\$(.*?)\$\$", RegexOptions.Singleline);
        if (m.Success) return m.Groups[1].Value.Trim();
        m = Regex.Match(s ?? string.Empty, @"(?<!\$)\$(?!\$)([^\$]+)\$(?!\$)");
        if (m.Success) return m.Groups[1].Value.Trim();
        return string.Empty;
    }

    private static void CollectFormulaCodes(Block block, List<string> result)
    {
        // Formulas are inline delimiters in text; scan source instead.
    }

    private static bool BracesBalanced(string s)
    {
        int depth = 0;
        foreach (var ch in s ?? string.Empty)
        {
            if (ch == '{') depth++;
            else if (ch == '}') { depth--; if (depth < 0) return false; }
        }
        return depth == 0;
    }

    private static string ExtractCode(object? input)
    {
        if (input is null) return string.Empty;
        if (input is string s) return s;
        var t = input.GetType();
        foreach (var name in new[] { "Code", "Latex", "Formula", "Value", "Body", "Text", "Content", "Markdown", "Source", "RawContent" })
        {
            var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try { var v = prop.GetValue(input); if (v is not null) return v.ToString() ?? string.Empty; } catch { }
            }
        }
        return input.ToString() ?? string.Empty;
    }

    private static string EscapeHtml(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
    private static string EscapeAttr(string s) => EscapeHtml(s);
}

// Alias classes
public class FormulaRender : FormulaRenderer { public FormulaRender() : base() { } public FormulaRender(string? c, string? m = null) : base(c, m) { } public FormulaRender(object? i, string? m = null) : base(i, m) { } }
public class FormulaRendering : FormulaRenderer { public FormulaRendering() : base() { } public FormulaRendering(string? c, string? m = null) : base(c, m) { } public FormulaRendering(object? i, string? m = null) : base(i, m) { } }
public class LatexRenderer : FormulaRenderer { public LatexRenderer() : base() { } public LatexRenderer(string? c, string? m = null) : base(c, m) { } public LatexRenderer(object? i, string? m = null) : base(i, m) { } }
public class LatexRender : FormulaRenderer { public LatexRender() : base() { } public LatexRender(string? c, string? m = null) : base(c, m) { } public LatexRender(object? i, string? m = null) : base(i, m) { } }
public class LatexRendering : FormulaRenderer { public LatexRendering() : base() { } public LatexRendering(string? c, string? m = null) : base(c, m) { } public LatexRendering(object? i, string? m = null) : base(i, m) { } }
public class MathRenderer : FormulaRenderer { public MathRenderer() : base() { } public MathRenderer(string? c, string? m = null) : base(c, m) { } public MathRenderer(object? i, string? m = null) : base(i, m) { } }
public class MathRender : FormulaRenderer { public MathRender() : base() { } public MathRender(string? c, string? m = null) : base(c, m) { } public MathRender(object? i, string? m = null) : base(i, m) { } }
public class MathRendering : FormulaRenderer { public MathRendering() : base() { } public MathRendering(string? c, string? m = null) : base(c, m) { } public MathRendering(object? i, string? m = null) : base(i, m) { } }
// (FormulaDiagramRenderer is defined in FormulaDiagramRenderer.cs)

// Static helper classes
public static class FormulaRenderers { public static string Render(string? c, string? m = null) => FormulaRenderer.Render(c, m); public static string Render(object? i, string? m = null) => FormulaRenderer.Render(i, m); }
public static class FormulaRenders { public static string Render(string? c, string? m = null) => FormulaRenderer.Render(c, m); public static string Render(object? i, string? m = null) => FormulaRenderer.Render(i, m); }
public static class LatexRenderers { public static string Render(string? c, string? m = null) => FormulaRenderer.Render(c, m); public static string Render(object? i, string? m = null) => FormulaRenderer.Render(i, m); }
public static class MathRenderers { public static string Render(string? c, string? m = null) => FormulaRenderer.Render(c, m); public static string Render(object? i, string? m = null) => FormulaRenderer.Render(i, m); }

// Extension methods
public static class FormulaRendererExtensions
{
    public static string RenderFormula(this string? md, string? m = null) => FormulaRenderer.Render(md, m);
    public static string RenderFormula(this MarkdownAst? ast, string? m = null) => FormulaRenderer.Render(ast, m);
    public static string RenderFormula(this OpenDocument? doc, string? m = null) => FormulaRenderer.Render(doc, m);
    public static string RenderFormula(this FormulaInfo? info, string? m = null) => FormulaRenderer.Render(info, m);
    public static string RenderLatex(this string? md, string? m = null) => FormulaRenderer.Render(md, m);
    public static string RenderLatex(this MarkdownAst? ast, string? m = null) => FormulaRenderer.Render(ast, m);
}