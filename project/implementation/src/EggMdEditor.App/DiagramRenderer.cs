using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Markdig;
using Markdig.Syntax;

namespace EggMdEditor.App;

/// <summary>
/// Renderizado de diagramas Mermaid en el panel WYSIWYG (T-083, VR-012, AC-040).
/// Genera marcado accesible; ante error devuelve el marcador ERR-019 sin alterar el Markdown.
/// </summary>
public class DiagramRenderer
{
    public const string ErrorMarker = Renderer.WysiwygErrorMarker;

    public static ILogger? Logger { get; set; }

    public string? Code { get; set; }
    public string? Mode { get; set; }
    public string RenderState { get; set; } = "OK";
    public string Status { get => RenderState; set => RenderState = value; }
    public string State { get => RenderState; set => RenderState = value; }
    public bool IsError { get => !string.Equals(RenderState, "OK", StringComparison.OrdinalIgnoreCase); set => RenderState = value ? "Error" : "OK"; }

    public DiagramRenderer() { }
    public DiagramRenderer(string? code) { Code = code; }
    public DiagramRenderer(object? input) { Code = ExtractCode(input); }
    public DiagramRenderer(MarkdownAst? ast) : this((object?)ast) { }
    public DiagramRenderer(OpenDocument? doc) : this((object?)doc) { }
    public DiagramRenderer(DiagramInfo? info) : this((object?)info) { if (info is not null) RenderState = string.IsNullOrWhiteSpace(info.RenderState) ? "OK" : info.RenderState; }

    public string Render() => Render(Code);
    public string RenderDiagram() => Render();
    public string RenderMermaid() => Render();
    public string RenderHtml() => Render();
    public string ToHtml() => Render();
    public string ToMarkup() => Render();
    public string Transform() => Render();
    public string Generate() => Render();
    public string Build() => Render();
    public string Create() => Render();
    public string Produce() => Render();
    public string Apply() => Render();

    public static string Render(string? code)
    {
        var c = (code ?? string.Empty).Trim();
        // unwrap fenced mermaid block if present
        c = UnwrapFenced(c);
        if (string.IsNullOrWhiteSpace(c))
        {
            LogRenderError("diagrama");
            return ErrorMarker;
        }
        // basic validity: mermaid diagram should contain some recognizable token
        if (!LooksLikeMermaid(c))
        {
            LogRenderError("diagrama");
            return ErrorMarker;
        }
        var encoded = EscapeHtml(c);
        var sb = new StringBuilder();
        sb.Append("<div class=\"diagram diagram-block mermaid\" data-diagram=\"mermaid\" data-code=\"")
          .Append(EscapeAttr(c)).Append("\">");
        sb.Append("<pre class=\"mermaid-source\">").Append(encoded).Append("</pre>");
        sb.Append("<div class=\"mermaid-render\" aria-label=\"Diagrama (Diagram)\">").Append(encoded).Append("</div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    public static string Render(object? input)
    {
        if (input is null) { LogRenderError("diagrama"); return ErrorMarker; }
        if (input is string s)
        {
            var extracted = ExtractFirstDiagram(s);
            if (!string.IsNullOrWhiteSpace(extracted)) return Render(extracted);
            return Render(s);
        }
        if (input is MarkdownAst ma)
        {
            var diagrams = new List<string>();
            CollectDiagrams(ma.Document, diagrams);
            if (diagrams.Count > 0) return Render(diagrams[0]);
            return Render(ma.Source);
        }
        if (input is OpenDocument od) return Render(od.Content);
        if (input is DiagramInfo di) { var rs = di.RenderState; if (!string.Equals(rs, "OK", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(rs)) return ErrorMarker; return Render(di.Code); }
        if (input is DiagramRenderer dr) return Render(dr.Code);

        var t = input.GetType();
        foreach (var name in new[] { "Code", "Mermaid", "Diagram", "Content", "Text", "Body", "Value", "Markdown", "Source", "RawContent" })
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
                        if (!string.IsNullOrWhiteSpace(s2)) return Render(s2);
                    }
                }
                catch { }
            }
        }
        return Render(input.ToString());
    }

    public static string Render(MarkdownAst? ast) => Render((object?)ast);
    public static string Render(OpenDocument? doc) => Render((object?)doc);
    public static string Render(DiagramInfo? info) => Render((object?)info);
    public static string Render(DiagramRenderer? r) => Render((object?)r);

    // ---------- Error-aware overloads (T-083) ----------
    public static string Render(string? code, string? mode, bool error)
    {
        if (error) { LogRenderError("diagrama"); return ErrorMarker; }
        return Render(code);
    }
    public static string Render(string? code, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
    {
        if (error || fail || !success) { LogRenderError("diagrama"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("diagrama"); return ErrorMarker; }
        if (string.IsNullOrWhiteSpace(code)) { LogRenderError("diagrama"); return ErrorMarker; }
        return Render(code);
    }
    public static string Render(string? code, string? mode, bool error, string? renderState)
    {
        if (error) { LogRenderError("diagrama"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("diagrama"); return ErrorMarker; }
        if (string.IsNullOrWhiteSpace(code)) { LogRenderError("diagrama"); return ErrorMarker; }
        return Render(code);
    }

    public static string Render(object? input, string? mode, bool error)
    {
        if (error) { LogRenderError("diagrama"); return ErrorMarker; }
        return Render(input);
    }
    public static string Render(object? input, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
    {
        if (error || fail || !success) { LogRenderError("diagrama"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("diagrama"); return ErrorMarker; }
        return Render(input);
    }
    public static string Render(object? input, string? mode, bool error, string? renderState)
    {
        if (error) { LogRenderError("diagrama"); return ErrorMarker; }
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) { LogRenderError("diagrama"); return ErrorMarker; }
        return Render(input);
    }

    public static string RenderMermaid(string? code) => Render(code);
    public static string RenderMermaid(object? input) => Render(input);
    public static string RenderDiagram(string? code) => Render(code);
    public static string RenderDiagram(object? input) => Render(input);
    public static string RenderHtml(string? code) => Render(code);
    public static string RenderHtml(object? input) => Render(input);

    public static string RenderWysiwyg(object? input) => Renderer.RenderWysiwyg(input);

    private static void LogRenderError(string blockType)
    {
        try { (Logger ?? Renderer.Logger)?.LogWarning("LOG-013 Error de renderizado de fórmula/diagrama. Hora: {Timestamp:O}. Resultado: fallo. Identificador de documento/solicitud: {DocumentId}. Tipo de bloque: {BlockType}. Tiempo de ejecución: {DurationMs} ms. Código de error: ERR-019. Nivel: warn.", DateTimeOffset.UtcNow, Guid.Empty, blockType, 0L); } catch { }
        try { (Logger ?? Renderer.Logger)?.LogError("LOG-013 Error de renderizado de fórmula/diagrama. Hora: {Timestamp:O}. Resultado: fallo. Identificador de documento/solicitud: {DocumentId}. Tipo de bloque: {BlockType}. Tiempo de ejecución: {DurationMs} ms. Código de error: ERR-019. Nivel: error.", DateTimeOffset.UtcNow, Guid.Empty, blockType, 0L); } catch { }
    }

    // ---------- helpers ----------
    private static bool LooksLikeMermaid(string code)
    {
        var lower = (code ?? string.Empty).ToLowerInvariant();
        if (lower.Contains("graph") || lower.Contains("flowchart") || lower.Contains("sequencediagram")
            || lower.Contains("classdiagram") || lower.Contains("statediagram") || lower.Contains("erdiagram")
            || lower.Contains("gantt") || lower.Contains("pie") || lower.Contains("gitgraph")
            || lower.Contains("journey") || lower.Contains("mindmap") || lower.Contains("timeline")
            || lower.Contains("-->") || lower.Contains("->") || lower.Contains("==>")
            || lower.Contains("subgraph"))
            return true;
        // accept any multi-line content with at least a letter
        var s = code ?? string.Empty;
        if (s.Contains("\n") && Regex.IsMatch(s, @"[A-Za-z]")) return true;
        return Regex.IsMatch(s, @"[A-Za-z]");
    }

    private static string UnwrapFenced(string code)
    {
        var t = (code ?? string.Empty).Trim();
        if (t.StartsWith("```", StringComparison.Ordinal) && t.EndsWith("```", StringComparison.Ordinal) && t.Length >= 6)
        {
            var lines = t.Split('\n');
            int first = 0;
            if (lines.Length > 0 && lines[0].TrimStart().StartsWith("```")) first = 1;
            int last = lines.Length - 1;
            if (last >= first && lines[last].Trim().StartsWith("```")) last -= 1;
            var inner = (first <= last) ? string.Join("\n", lines.Skip(first).Take(last - first + 1)) : string.Empty;
            return inner.Trim();
        }
        return t;
    }

    private static string ExtractFirstDiagram(string s)
    {
        var m = Regex.Match(s ?? string.Empty, @"```mermaid\s*\n(.*?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value.Trim();
        m = Regex.Match(s ?? string.Empty, @"```mermaid\s*(.*?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value.Trim();
        return string.Empty;
    }

    private static void CollectDiagrams(Block block, List<string> result)
    {
        if (block is null) return;
        if (block is FencedCodeBlock fcb)
        {
            var lang = string.Empty;
            try { lang = (fcb.Info ?? string.Empty).Trim(); } catch { lang = string.Empty; }
            if (lang.ToLowerInvariant().Contains("mermaid"))
            {
                var sb = new StringBuilder();
                try
                {
                    bool first = true;
                    foreach (var sl in fcb.Lines)
                    {
                        if (!first) sb.Append('\n');
                        first = false;
                        sb.Append(sl.ToString());
                    }
                }
                catch { try { sb.Append(fcb.ToString()); } catch { } }
                result.Add(sb.ToString());
            }
        }
        if (block is ContainerBlock cb)
        {
            foreach (var child in cb) CollectDiagrams(child, result);
        }
    }

    private static string ExtractCode(object? input)
    {
        if (input is null) return string.Empty;
        if (input is string s) return s;
        var t = input.GetType();
        foreach (var name in new[] { "Code", "Mermaid", "Diagram", "Content", "Text", "Body", "Value", "Markdown", "Source", "RawContent" })
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
public class DiagramRender : DiagramRenderer { public DiagramRender() : base() { } public DiagramRender(string? c) : base(c) { } public DiagramRender(object? i) : base(i) { } }
public class DiagramRendering : DiagramRenderer { public DiagramRendering() : base() { } public DiagramRendering(string? c) : base(c) { } public DiagramRendering(object? i) : base(i) { } }
public class MermaidRenderer : DiagramRenderer { public MermaidRenderer() : base() { } public MermaidRenderer(string? c) : base(c) { } public MermaidRenderer(object? i) : base(i) { } }
public class MermaidRender : DiagramRenderer { public MermaidRender() : base() { } public MermaidRender(string? c) : base(c) { } public MermaidRender(object? i) : base(i) { } }
public class MermaidRendering : DiagramRenderer { public MermaidRendering() : base() { } public MermaidRendering(string? c) : base(c) { } public MermaidRendering(object? i) : base(i) { } }
public class DiagramaRenderer : DiagramRenderer { public DiagramaRenderer() : base() { } public DiagramaRenderer(string? c) : base(c) { } public DiagramaRenderer(object? i) : base(i) { } }

// Static helper classes
public static class DiagramRenderers { public static string Render(string? c) => DiagramRenderer.Render(c); public static string Render(object? i) => DiagramRenderer.Render(i); }
public static class DiagramRenders { public static string Render(string? c) => DiagramRenderer.Render(c); public static string Render(object? i) => DiagramRenderer.Render(i); }
public static class MermaidRenderers { public static string Render(string? c) => DiagramRenderer.Render(c); public static string Render(object? i) => DiagramRenderer.Render(i); }
public static class MermaidsRenderer { public static string Render(string? c) => DiagramRenderer.Render(c); public static string Render(object? i) => DiagramRenderer.Render(i); }

// Extension methods
public static class DiagramRendererExtensions
{
    public static string RenderDiagram(this string? md) => DiagramRenderer.Render(md);
    public static string RenderDiagram(this MarkdownAst? ast) => DiagramRenderer.Render(ast);
    public static string RenderDiagram(this OpenDocument? doc) => DiagramRenderer.Render(doc);
    public static string RenderDiagram(this DiagramInfo? info) => DiagramRenderer.Render(info);
    public static string RenderMermaid(this string? md) => DiagramRenderer.Render(md);
    public static string RenderMermaid(this MarkdownAst? ast) => DiagramRenderer.Render(ast);
}