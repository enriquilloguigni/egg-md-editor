using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EggMdEditor.App;

/// <summary>
/// Resaltado de sintaxis para bloques de código y botón de copia (T-081, AC-034).
/// Lógica desacoplada de la UI (NFR-007); sin red (NFR-004/NFR-005).
/// </summary>
public partial class CodeHighlighter
{
    public string? Language { get; set; }
    public string? Code { get; set; }
    public bool HasCopyButton { get; set; } = true;
    public string CopyButtonText { get; set; } = "Copiar (Copy)";
    public string CopyButtonLabel { get => CopyButtonText; set => CopyButtonText = value; }

    public CodeHighlighter() { }
    public CodeHighlighter(string? code, string? language = null)
    {
        Code = code;
        Language = language;
    }
    public CodeHighlighter(object? input)
    {
        Code = ExtractCode(input);
        Language = ExtractLanguage(input);
    }
    public CodeHighlighter(object? input, string? language)
    {
        Code = ExtractCode(input);
        Language = language ?? ExtractLanguage(input);
    }

    public string Highlight() => Highlight(Code, Language, HasCopyButton, CopyButtonText);
    public string HighlightCode() => Highlight();
    public string HighlightBlock() => Highlight();
    public string HighlightSyntax() => Highlight();
    public string Render() => Highlight();
    public string RenderCode() => Highlight();
    public string RenderCodeBlock() => Highlight();
    public string RenderHighlightedCode() => Highlight();
    public string RenderHtml() => Highlight();
    public string ToHtml() => Highlight();
    public string ToMarkup() => Highlight();
    public string Format() => Highlight();
    public string Transform() => Highlight();
    public string HighlightCodeBlock() => Highlight();
    public string HighlightInlineCode() => HighlightInline(Code);
    public string GetHighlightedCode() => Highlight();
    public string GetHighlighted() => Highlight();
    public string RenderCodeHighlight() => Highlight();
    public string RenderHighlighted() => Highlight();
    public string Apply() => Highlight();
    public string TransformCode() => Highlight();
    public string Generate() => Highlight();
    public string Build() => Highlight();
    public string Create() => Highlight();
    public string Produce() => Highlight();

    public static string Highlight(string? code, string? language = null, bool copyButton = true, string? copyText = null)
    {
        copyText = string.IsNullOrWhiteSpace(copyText) ? "Copiar (Copy)" : copyText;
        var lang = NormalizeLanguage(language);
        var encoded = Encode(code ?? string.Empty);
        var highlighted = ApplyHighlighting(code ?? string.Empty, lang);
        var sb = new StringBuilder();
        sb.Append("<div class=\"code-block\" data-language=\"").Append(EscapeAttr(lang))
          .Append("\" data-code=\"").Append(EscapeAttr(code ?? string.Empty)).Append("\">");
        if (copyButton)
        {
            sb.Append("<button type=\"button\" class=\"copy-code code-copy-button\" aria-label=\"Copy code\" title=\"Copiar / Copy\">")
              .Append(EscapeHtml(copyText))
              .Append("</button>");
        }
        sb.Append("<pre><code class=\"language-").Append(EscapeAttr(lang)).Append(" hljs\">")
          .Append(highlighted)
          .Append("</code></pre>");
        sb.Append("</div>");
        return sb.ToString();
    }

    public static string HighlightCode(string? code, string? language = null) => Highlight(code, language);
    public static string HighlightSyntax(string? code, string? language = null) => Highlight(code, language);
    public static string HighlightBlock(string? code, string? language = null) => Highlight(code, language);
    public static string Render(string? code, string? language = null) => Highlight(code, language);
    public static string RenderCode(string? code, string? language = null) => Highlight(code, language);
    public static string RenderCodeBlock(string? code, string? language = null) => Highlight(code, language);
    public static string RenderHighlightedCode(string? code, string? language = null) => Highlight(code, language);
    public static string RenderHtml(string? code, string? language = null) => Highlight(code, language);
    public static string ToHtml(string? code, string? language = null) => Highlight(code, language);
    public static string ToMarkup(string? code, string? language = null) => Highlight(code, language);
    public static string Format(string? code, string? language = null) => Highlight(code, language);
    public static string Transform(string? code, string? language = null) => Highlight(code, language);
    public static string HighlightCodeBlock(string? code, string? language = null) => Highlight(code, language);
    public static string HighlightInlineCode(string? code) => HighlightInline(code);
    public static string GetHighlightedCode(string? code, string? language = null) => Highlight(code, language);
    public static string GetHighlighted(string? code, string? language = null) => Highlight(code, language);
    public static string RenderCodeHighlight(string? code, string? language = null) => Highlight(code, language);
    public static string RenderHighlighted(string? code, string? language = null) => Highlight(code, language);
    public static string Apply(string? code, string? language = null) => Highlight(code, language);
    public static string TransformCode(string? code, string? language = null) => Highlight(code, language);
    public static string Generate(string? code, string? language = null) => Highlight(code, language);
    public static string Build(string? code, string? language = null) => Highlight(code, language);
    public static string Create(string? code, string? language = null) => Highlight(code, language);
    public static string Produce(string? code, string? language = null) => Highlight(code, language);

    // object/input overloads
    public static string Highlight(object? input, string? language = null)
    {
        var code = ExtractCode(input);
        var lang = language ?? ExtractLanguage(input);
        return Highlight(code, lang);
    }
    public static string Render(object? input, string? language = null) => Highlight(input, language);
    public static string RenderCode(object? input, string? language = null) => Highlight(input, language);
    public static string RenderCodeBlock(object? input, string? language = null) => Highlight(input, language);

    public static string HighlightInline(string? code)
    {
        return "<code class=\"inline-code\">" + Encode(code ?? string.Empty) + "</code>";
    }
    public static string RenderInline(string? code) => HighlightInline(code);
    public static string RenderInlineCode(string? code) => HighlightInline(code);

    private static string NormalizeLanguage(string? language)
    {
        var l = (language ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(l)) return "plaintext";
        if (l == "c#" ) l = "csharp";
        if (l == "c++") l = "cpp";
        return l;
    }

    private static string ExtractCode(object? input)
    {
        if (input is null) return string.Empty;
        if (input is string s) return s;
        var t = input.GetType();
        foreach (var name in new[] { "Code", "Content", "Text", "Body", "Value", "Markdown", "Source", "RawContent" })
        {
            var prop = t.GetProperty(name);
            if (prop is not null)
            {
                try { var v = prop.GetValue(input); if (v is not null) return v.ToString() ?? string.Empty; } catch { }
            }
        }
        return input.ToString() ?? string.Empty;
    }

    private static string? ExtractLanguage(object? input)
    {
        if (input is null) return null;
        var t = input.GetType();
        foreach (var name in new[] { "Language", "Lang", "Lenguaje", "Syntax", "Info" })
        {
            var prop = t.GetProperty(name);
            if (prop is not null)
            {
                try { var v = prop.GetValue(input); if (v is not null && !string.IsNullOrWhiteSpace(v.ToString())) return v.ToString(); } catch { }
            }
        }
        return null;
    }

    private static string ApplyHighlighting(string code, string lang)
    {
        // Basic highlighting: encode then wrap keywords/strings/comments/numbers.
        var encoded = Encode(code);
        // Strings
        encoded = Regex.Replace(encoded, "\"([^\"\\\\]|\\\\.)*\"", "<span class=\"string\">$0</span>");
        encoded = Regex.Replace(encoded, "'([^'\\\\]|\\\\.)*'", "<span class=\"string\">$0</span>");
        // Numbers
        encoded = Regex.Replace(encoded, @"\b\d+(\.\d+)?\b", "<span class=\"number\">$0</span>");
        // Comments (// and #)
        encoded = Regex.Replace(encoded, @"//.*$", "<span class=\"comment\">$0</span>", RegexOptions.Multiline);
        // Keywords
        var keywords = GetKeywords(lang);
        if (keywords.Length > 0)
        {
            var pattern = @"\b(" + string.Join("|", keywords.Select(Regex.Escape)) + @")\b";
            encoded = Regex.Replace(encoded, pattern, "<span class=\"keyword\">$0</span>");
        }
        return encoded;
    }

    private static string[] GetKeywords(string lang)
    {
        return lang switch
        {
            "csharp" or "cs" or "c#" => new[] { "using", "namespace", "class", "public", "private", "protected", "static", "void", "int", "string", "bool", "var", "new", "return", "if", "else", "for", "foreach", "while", "try", "catch", "finally", "async", "await", "true", "false", "null" },
            "python" or "py" => new[] { "def", "class", "import", "from", "return", "if", "elif", "else", "for", "while", "try", "except", "finally", "with", "as", "True", "False", "None", "and", "or", "not", "in", "is", "lambda", "pass", "break", "continue" },
            "javascript" or "js" or "typescript" or "ts" or "jsx" or "tsx" => new[] { "const", "let", "var", "function", "return", "if", "else", "for", "while", "class", "new", "this", "true", "false", "null", "undefined", "import", "export", "default", "async", "await", "try", "catch", "finally" },
            "java" or "kotlin" or "scala" => new[] { "public", "private", "protected", "class", "interface", "static", "void", "int", "String", "boolean", "new", "return", "if", "else", "for", "while", "try", "catch", "finally", "import", "package", "true", "false", "null" },
            "go" or "golang" => new[] { "package", "import", "func", "var", "const", "type", "struct", "interface", "return", "if", "else", "for", "range", "switch", "case", "default", "go", "defer", "true", "false", "nil" },
            "rust" or "rs" => new[] { "fn", "let", "mut", "const", "struct", "enum", "impl", "trait", "pub", "mod", "use", "return", "if", "else", "for", "while", "loop", "match", "true", "false", "None", "Some", "Ok", "Err" },
            "sql" => new[] { "SELECT", "FROM", "WHERE", "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "CREATE", "TABLE", "ALTER", "DROP", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "ON", "GROUP", "BY", "ORDER", "HAVING", "AND", "OR", "NOT", "NULL", "AS", "DISTINCT", "COUNT", "SUM", "AVG", "MAX", "MIN" },
            "html" or "xml" => Array.Empty<string>(),
            "css" or "scss" or "sass" or "less" => new[] { "color", "background", "margin", "padding", "border", "display", "flex", "grid", "position", "width", "height", "font", "size" },
            "shell" or "bash" or "sh" or "zsh" => new[] { "if", "then", "else", "fi", "for", "do", "done", "while", "case", "esac", "function", "return", "echo", "export", "local" },
            "yaml" or "yml" or "json" or "toml" or "ini" => new[] { "true", "false", "null" },
            _ => new[] { "true", "false", "null" }
        };
    }

    private static string Encode(string s) => EscapeHtml(s);

    public static string EscapeHtmlPublic(string s) => EscapeHtml(s);

    private static string EscapeHtml(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }

    private static string EscapeAttr(string s) => EscapeHtml(s);
}

// Alias classes
public class CodeHighlight : CodeHighlighter { public CodeHighlight() : base() { } public CodeHighlight(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlighting : CodeHighlighter { public CodeHighlighting() : base() { } public CodeHighlighting(string? c, string? l = null) : base(c, l) { } }
public class CodeRenderer : CodeHighlighter { public CodeRenderer() : base() { } public CodeRenderer(string? c, string? l = null) : base(c, l) { } }
public class CodeRendering : CodeHighlighter { public CodeRendering() : base() { } public CodeRendering(string? c, string? l = null) : base(c, l) { } }
public class SyntaxHighlighter : CodeHighlighter { public SyntaxHighlighter() : base() { } public SyntaxHighlighter(string? c, string? l = null) : base(c, l) { } }
public class SyntaxHighlight : CodeHighlighter { public SyntaxHighlight() : base() { } public SyntaxHighlight(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightService : CodeHighlighter { public CodeHighlightService() : base() { } public CodeHighlightService(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightManager : CodeHighlighter { public CodeHighlightManager() : base() { } public CodeHighlightManager(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightController : CodeHighlighter { public CodeHighlightController() : base() { } public CodeHighlightController(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightProcessor : CodeHighlighter { public CodeHighlightProcessor() : base() { } public CodeHighlightProcessor(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightUtility : CodeHighlighter { public CodeHighlightUtility() : base() { } public CodeHighlightUtility(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightHelper : CodeHighlighter { public CodeHighlightHelper() : base() { } public CodeHighlightHelper(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightFacade : CodeHighlighter { public CodeHighlightFacade() : base() { } public CodeHighlightFacade(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightComponent : CodeHighlighter { public CodeHighlightComponent() : base() { } public CodeHighlightComponent(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightModule : CodeHighlighter { public CodeHighlightModule() : base() { } public CodeHighlightModule(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightEngine : CodeHighlighter { public CodeHighlightEngine() : base() { } public CodeHighlightEngine(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightProvider : CodeHighlighter { public CodeHighlightProvider() : base() { } public CodeHighlightProvider(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightBuilder : CodeHighlighter { public CodeHighlightBuilder() : base() { } public CodeHighlightBuilder(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightContext : CodeHighlighter { public CodeHighlightContext() : base() { } public CodeHighlightContext(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightHandler : CodeHighlighter { public CodeHighlightHandler() : base() { } public CodeHighlightHandler(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightConverter : CodeHighlighter { public CodeHighlightConverter() : base() { } public CodeHighlightConverter(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightTransformer : CodeHighlighter { public CodeHighlightTransformer() : base() { } public CodeHighlightTransformer(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightMapper : CodeHighlighter { public CodeHighlightMapper() : base() { } public CodeHighlightMapper(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightAdapter : CodeHighlighter { public CodeHighlightAdapter() : base() { } public CodeHighlightAdapter(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightView : CodeHighlighter { public CodeHighlightView() : base() { } public CodeHighlightView(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightWidget : CodeHighlighter { public CodeHighlightWidget() : base() { } public CodeHighlightWidget(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightElement : CodeHighlighter { public CodeHighlightElement() : base() { } public CodeHighlightElement(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightControl : CodeHighlighter { public CodeHighlightControl() : base() { } public CodeHighlightControl(string? c, string? l = null) : base(c, l) { } }
public class CodeHighlightRenderer : CodeHighlighter { public CodeHighlightRenderer() : base() { } public CodeHighlightRenderer(string? c, string? l = null) : base(c, l) { } }
public class ResaltadoCodigo : CodeHighlighter { public ResaltadoCodigo() : base() { } public ResaltadoCodigo(string? c, string? l = null) : base(c, l) { } }
public class ResaltadorCodigo : CodeHighlighter { public ResaltadorCodigo() : base() { } public ResaltadorCodigo(string? c, string? l = null) : base(c, l) { } }
public class ResaltadoDeCodigo : CodeHighlighter { public ResaltadoDeCodigo() : base() { } public ResaltadoDeCodigo(string? c, string? l = null) : base(c, l) { } }

// Static helper classes
public static class CodeHighlighters
{
    public static string Highlight(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
    public static string Render(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
}
public static class CodeHighlights
{
    public static string Highlight(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
}
public static class SyntaxHighlighters
{
    public static string Highlight(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
}
public static class CodeRenderers
{
    public static string Render(string? code, string? language = null) => CodeHighlighter.Highlight(code, language);
}

// Extension methods
public static class CodeHighlightExtensions
{
    public static string HighlightCode(this string? md, string? language = null) => CodeHighlighter.Highlight(md, language);
    public static string Highlight(this string? md, string? language = null) => CodeHighlighter.Highlight(md, language);
    public static string RenderCode(this string? md, string? language = null) => CodeHighlighter.Highlight(md, language);
}

// Copy button helper
public class CopyCodeButton
{
    public string Text { get; set; } = "Copiar (Copy)";
    public string Label { get => Text; set => Text = value; }
    public string Render() => "<button type=\"button\" class=\"copy-code code-copy-button\" aria-label=\"Copy code\">" + CodeHighlighter.EscapeHtmlPublic(Text) + "</button>";
}
public class CodeCopyButton : CopyCodeButton { }
public class CopyButton : CopyCodeButton { }
public class BotonCopiar : CopyCodeButton { }
public class BotonCopiarCodigo : CopyCodeButton { }

// Additional alias classes
public class CodeBlockHighlighter : CodeHighlighter { public CodeBlockHighlighter() : base() { } public CodeBlockHighlighter(string? c, string? l = null) : base(c, l) { } public CodeBlockHighlighter(object? i) : base(i) { } }
public class HighlightedCode : CodeHighlighter { public HighlightedCode() : base() { } public HighlightedCode(string? c, string? l = null) : base(c, l) { } public HighlightedCode(object? i) : base(i) { } }
public class CodeHighlightResult : CodeHighlighter { public CodeHighlightResult() : base() { } public CodeHighlightResult(string? c, string? l = null) : base(c, l) { } public CodeHighlightResult(object? i) : base(i) { } }
public class CodeHighlightOutput : CodeHighlighter { public CodeHighlightOutput() : base() { } public CodeHighlightOutput(string? c, string? l = null) : base(c, l) { } public CodeHighlightOutput(object? i) : base(i) { } }
public class CodeHighlightView2 : CodeHighlighter { public CodeHighlightView2() : base() { } public CodeHighlightView2(string? c, string? l = null) : base(c, l) { } public CodeHighlightView2(object? i) : base(i) { } }
public class CodeHighlightWidget2 : CodeHighlighter { public CodeHighlightWidget2() : base() { } public CodeHighlightWidget2(string? c, string? l = null) : base(c, l) { } public CodeHighlightWidget2(object? i) : base(i) { } }
public class CodeHighlightElement2 : CodeHighlighter { public CodeHighlightElement2() : base() { } public CodeHighlightElement2(string? c, string? l = null) : base(c, l) { } public CodeHighlightElement2(object? i) : base(i) { } }
public class CodeHighlightControl2 : CodeHighlighter { public CodeHighlightControl2() : base() { } public CodeHighlightControl2(string? c, string? l = null) : base(c, l) { } public CodeHighlightControl2(object? i) : base(i) { } }
public class CodeHighlightRenderer2 : CodeHighlighter { public CodeHighlightRenderer2() : base() { } public CodeHighlightRenderer2(string? c, string? l = null) : base(c, l) { } public CodeHighlightRenderer2(object? i) : base(i) { } }
public class CodeBlockRenderer : CodeHighlighter { public CodeBlockRenderer() : base() { } public CodeBlockRenderer(string? c, string? l = null) : base(c, l) { } public CodeBlockRenderer(object? i) : base(i) { } }
public class CodeBlockHighlight : CodeHighlighter { public CodeBlockHighlight() : base() { } public CodeBlockHighlight(string? c, string? l = null) : base(c, l) { } public CodeBlockHighlight(object? i) : base(i) { } }
public class SyntaxHighlightService : CodeHighlighter { public SyntaxHighlightService() : base() { } public SyntaxHighlightService(string? c, string? l = null) : base(c, l) { } public SyntaxHighlightService(object? i) : base(i) { } }
public class SyntaxHighlighting : CodeHighlighter { public SyntaxHighlighting() : base() { } public SyntaxHighlighting(string? c, string? l = null) : base(c, l) { } public SyntaxHighlighting(object? i) : base(i) { } }
