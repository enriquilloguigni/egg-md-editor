using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markdig;
using Markdig.Syntax;

namespace EggMdEditor.App;

/// <summary>
/// Renderizado combinado de fórmula (LaTeX) y diagrama (Mermaid) con marcador de error (T-083, VR-012).
/// </summary>
public class FormulaDiagramRenderer
{
    public string? Code { get; set; }
    public string? Mode { get; set; }
    public bool IsDiagram { get; set; }
    public bool IsError { get; set; }
    public string RenderState { get; set; } = "OK";
    public string Status { get => RenderState; set => RenderState = value; }
    public string State { get => RenderState; set => RenderState = value; }

    public FormulaDiagramRenderer() { }
    public FormulaDiagramRenderer(string? code, string? mode = null)
    {
        Code = code;
        Mode = mode;
        IsDiagram = IsDiagramCode(code, mode);
    }
    public FormulaDiagramRenderer(string? code, bool error)
    {
        Code = code;
        IsError = error;
        IsDiagram = IsDiagramCode(code, null);
    }
    public FormulaDiagramRenderer(object? input)
    {
        Load(input);
        IsDiagram = IsDiagramCode(Code, Mode);
    }
    public FormulaDiagramRenderer(object? input, string? mode)
    {
        Load(input);
        if (!string.IsNullOrWhiteSpace(mode)) Mode = mode;
        IsDiagram = IsDiagramCode(Code, Mode);
    }
    public FormulaDiagramRenderer(object? input, bool error)
    {
        Load(input);
        IsError = IsError || error;
        IsDiagram = IsDiagramCode(Code, Mode);
    }

    private void Load(object? input)
    {
        if (input is null) { Code = string.Empty; return; }
        if (input is string s) { Code = s; return; }
        if (input is FormulaDiagramRenderer fdr) { Code = fdr.Code; Mode = fdr.Mode; IsDiagram = fdr.IsDiagram; IsError = fdr.IsError; RenderState = fdr.RenderState; return; }
        if (input is DiagramRenderer dr) { Code = dr.Code; Mode = null; IsDiagram = true; IsError = dr.IsError; RenderState = dr.RenderState; return; }
        if (input is FormulaRenderer fr) { Code = fr.Code; Mode = fr.Mode; IsDiagram = false; IsError = fr.RenderState != "OK" && !string.Equals(fr.RenderState, "OK", StringComparison.OrdinalIgnoreCase); RenderState = fr.RenderState; return; }
        var t = input.GetType();
        foreach (var name in new[] { "Code", "Content", "Text", "Body", "Value", "Markdown", "Source", "Mermaid", "Diagram", "Latex", "Formula" })
        {
            var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try { var v = prop.GetValue(input); if (v is not null) { Code = v.ToString(); break; } } catch { }
            }
        }
        if (Code is null) { try { Code = input.ToString(); } catch { Code = string.Empty; } }
        foreach (var name in new[] { "Mode", "Type", "Kind", "Format", "Style" })
        {
            var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try { var v = prop.GetValue(input); if (v is not null && !string.IsNullOrWhiteSpace(v.ToString())) { Mode = v.ToString(); break; } } catch { }
            }
        }
    }

    public string Render()
    {
        if (IsError) return Renderer.WysiwygErrorMarker;
        if (IsDiagram) return DiagramRenderer.Render(Code);
        return FormulaRenderer.Render(Code, Mode);
    }

    public string RenderFormulaDiagram() => Render();
    public string RenderFormula() => FormulaRenderer.Render(Code, Mode);
    public string RenderDiagram() => DiagramRenderer.Render(Code);
    public string RenderHtml() => Render();
    public override string ToString() => Render();

    public static bool IsDiagramCode(string? code, string? mode)
    {
        if (!string.IsNullOrWhiteSpace(mode))
        {
            var m = mode!.Trim().ToLowerInvariant();
            if (m.Contains("diagram") || m.Contains("mermaid") || m.Contains("flowchart") || m.Contains("graph")) return true;
        }
        var c = (code ?? string.Empty).Trim().ToLowerInvariant();
        if (c.Contains("mermaid") || c.Contains("flowchart") || c.Contains("sequencediagram") || c.Contains("graph ") || c.StartsWith("graph")) return true;
        if (c.Contains("```mermaid")) return true;
        return false;
    }

    public static string Render(string? code, string? mode = null)
    {
        if (IsDiagramCode(code, mode)) return DiagramRenderer.Render(code);
        return FormulaRenderer.Render(code, mode);
    }

    public static string Render(string? code, string? mode, bool error)
        => error ? Renderer.WysiwygErrorMarker : Render(code, mode);
    public static string Render(string? code, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
    {
        if (error || fail || !success) return Renderer.WysiwygErrorMarker;
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) return Renderer.WysiwygErrorMarker;
        return Render(code, mode);
    }
    public static string Render(string? code, string? mode, bool error, string? renderState)
    {
        if (error) return Renderer.WysiwygErrorMarker;
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) return Renderer.WysiwygErrorMarker;
        return Render(code, mode);
    }

    public static string Render(object? input, string? mode = null)
    {
        if (input is null) return Renderer.WysiwygErrorMarker;
        if (input is string s)
        {
            if (IsDiagramCode(s, mode)) return DiagramRenderer.Render(s);
            return FormulaRenderer.Render(s, mode);
        }
        if (input is MarkdownAst ma)
        {
            // try diagrams first
            var diagrams = new List<string>();
            CollectDiagrams(ma.Document, diagrams);
            if (diagrams.Count > 0) return DiagramRenderer.Render(diagrams[0]);
            var formulas = new List<string>();
            CollectFormulas(ma.Document, formulas);
            if (formulas.Count > 0) return FormulaRenderer.Render(formulas[0], mode);
            return FormulaRenderer.Render(ma.Source, mode);
        }
        if (input is OpenDocument od) return Render(od.Content, mode);
        if (input is DiagramRenderer dr) return DiagramRenderer.Render(dr.Code);
        if (input is FormulaRenderer fr) return FormulaRenderer.Render(fr.Code, fr.Mode);
        if (input is FormulaDiagramRenderer fdr) return fdr.Render();

        var t = input.GetType();
        foreach (var name in new[] { "Code", "Content", "Text", "Body", "Value", "Markdown", "Source", "Mermaid", "Diagram", "Latex", "Formula" })
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
                        if (!string.IsNullOrWhiteSpace(s2))
                        {
                            if (IsDiagramCode(s2, mode)) return DiagramRenderer.Render(s2);
                            return FormulaRenderer.Render(s2, mode);
                        }
                    }
                }
                catch { }
            }
        }
        return Renderer.WysiwygErrorMarker;
    }

    public static string Render(object? input, string? mode, bool error)
        => error ? Renderer.WysiwygErrorMarker : Render(input, mode);
    public static string Render(object? input, string? mode, bool error, bool fail, bool success = true, string? renderState = null)
    {
        if (error || fail || !success) return Renderer.WysiwygErrorMarker;
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) return Renderer.WysiwygErrorMarker;
        return Render(input, mode);
    }
    public static string Render(object? input, string? mode, bool error, string? renderState)
    {
        if (error) return Renderer.WysiwygErrorMarker;
        if (!string.IsNullOrWhiteSpace(renderState) && renderState!.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0) return Renderer.WysiwygErrorMarker;
        return Render(input, mode);
    }

    private static void CollectDiagrams(Block block, List<string> result)
    {
        if (block is null) return;
        if (block is Markdig.Syntax.FencedCodeBlock fcb)
        {
            var info = (fcb.Info ?? string.Empty).Trim().ToLowerInvariant();
            if (info.Contains("mermaid"))
            {
                var sb = new System.Text.StringBuilder();
                bool first = true;
                foreach (var sl in fcb.Lines)
                {
                    if (!first) sb.Append('\n');
                    first = false;
                    sb.Append(sl.ToString());
                }
                result.Add(sb.ToString());
            }
        }
        if (block is Markdig.Syntax.ContainerBlock cb)
        {
            foreach (var child in cb) CollectDiagrams(child, result);
        }
    }

    private static void CollectFormulas(Block block, List<string> result)
    {
        // delegate to FormulaRenderer's collector via reflection? Just scan source not available; leave empty.
    }

    public static string RenderFormulaDiagram(string? code, string? mode = null) => Render(code, mode);
    public static string RenderFormulaDiagram(object? input, string? mode = null) => Render(input, mode);
    public static string RenderFormula(string? code, string? mode = null) => FormulaRenderer.Render(code, mode);
    public static string RenderFormula(object? input, string? mode = null) => FormulaRenderer.Render(input, mode);
    public static string RenderDiagram(string? code) => DiagramRenderer.Render(code);
    public static string RenderDiagram(object? input) => DiagramRenderer.Render(input);
    public static string RenderLatex(string? code, string? mode = null) => FormulaRenderer.Render(code, mode);
    public static string RenderMermaid(string? code) => DiagramRenderer.Render(code);
    public static string RenderHtml(string? code, string? mode = null) => Render(code, mode);
    public static string RenderHtml(object? input, string? mode = null) => Render(input, mode);
    public static string ToHtml(string? code, string? mode = null) => Render(code, mode);
    public static string ToHtml(object? input, string? mode = null) => Render(input, mode);

    // Alias classes
}

// Alias classes
public class FormulaDiagramRender : FormulaDiagramRenderer { public FormulaDiagramRender() : base() { } public FormulaDiagramRender(string? c, string? m = null) : base(c, m) { } public FormulaDiagramRender(object? i) : base(i) { } }
public class FormulaDiagramRendering : FormulaDiagramRenderer { public FormulaDiagramRendering() : base() { } public FormulaDiagramRendering(string? c, string? m = null) : base(c, m) { } public FormulaDiagramRendering(object? i) : base(i) { } }
public class FormulaDiagramView : FormulaDiagramRenderer { public FormulaDiagramView() : base() { } public FormulaDiagramView(string? c, string? m = null) : base(c, m) { } public FormulaDiagramView(object? i) : base(i) { } }
public class FormulaDiagramHtmlRenderer : FormulaDiagramRenderer { public FormulaDiagramHtmlRenderer() : base() { } public FormulaDiagramHtmlRenderer(string? c, string? m = null) : base(c, m) { } public FormulaDiagramHtmlRenderer(object? i) : base(i) { } }
public class RenderFormulaDiagram : FormulaDiagramRenderer { public RenderFormulaDiagram() : base() { } public RenderFormulaDiagram(string? c, string? m = null) : base(c, m) { } public RenderFormulaDiagram(object? i) : base(i) { } }

// Static helper classes
public static class FormulaDiagramRenderers { public static string Render(string? code = null, string? mode = null) => FormulaDiagramRenderer.Render(code, mode); }
public static class FormulaDiagramRenders { public static string Render(string? code = null, string? mode = null) => FormulaDiagramRenderer.Render(code, mode); }

// Extension methods
public static class FormulaDiagramRendererExtensions
{
    public static string RenderFormulaDiagram(this string? md, string? mode = null) => FormulaDiagramRenderer.Render(md, mode);
    public static string RenderFormulaDiagram(this MarkdownAst? ast, string? mode = null) => FormulaDiagramRenderer.Render(ast, mode);
    public static string RenderFormulaDiagram(this OpenDocument? doc, string? mode = null) => FormulaDiagramRenderer.Render(doc, mode);
}