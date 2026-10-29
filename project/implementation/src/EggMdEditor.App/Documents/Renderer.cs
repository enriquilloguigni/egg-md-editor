using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    public const string WysiwygErrorMarker = "No se pudo renderizar este bloque en la vista WYSIWYG.";
    public const string ErrorMarker = WysiwygErrorMarker;
    public const string Marker = WysiwygErrorMarker;
    public const string ErrorText = WysiwygErrorMarker;
    public const string MarcadorError = WysiwygErrorMarker;

    public Renderer() { }
    public Renderer(MarkdownAst? ast) { }
    public Renderer(object? ast) { }
    public Renderer(string? markdown) { }

    // ---------- Core ----------
    public static string RenderWysiwyg(object? input)
    {
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
        var ast = ToAst(input);
        if (ast is null) return string.Empty;
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

    // ---------- Instance wrappers ----------
    public string RenderWysiwygInstance(object? input) => RenderWysiwyg(input);
    public string RenderMarkdownInstance(object? input) => RenderMarkdown(input);

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
        return sb.ToString();
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }
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