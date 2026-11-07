using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public class Sanitizer
{
    public static ILogger? Logger { get; set; }

    public Sanitizer() { }
    public Sanitizer(string? content) { Content = content; }
    public string? Content { get; set; }

    private static readonly Regex ScriptBlockRegex = new Regex(
        @"<script\b[^>]*>.*?</script\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex ScriptTagRegex = new Regex(
        @"</?script\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DangerousTagRegex = new Regex(
        @"</?(iframe|object|embed|applet|base|meta|link|style)\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex EventHandlerRegex = new Regex(
        @"\s+on[a-z]+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex JavascriptUrlRegex = new Regex(
        @"(href|src)\s*=\s*(?:""|')?\s*(javascript|vbscript|data)\s*:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MdDangerousLinkRegex = new Regex(
        @"\]\(\s*(javascript|vbscript|data):[^)]*\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] DangerousTokens = new[]
    {
        "<script", "javascript:", "vbscript:", "data:text/html", "onload=", "onerror=", "<iframe", "<object", "<embed", "<applet"
    };

    public static bool IsDangerous(string? content)
    {
        if (string.IsNullOrEmpty(content)) return false;
        var s = content!;
        if (ScriptBlockRegex.IsMatch(s)) return true;
        if (ScriptTagRegex.IsMatch(s)) return true;
        if (DangerousTagRegex.IsMatch(s)) return true;
        if (EventHandlerRegex.IsMatch(s)) return true;
        if (JavascriptUrlRegex.IsMatch(s)) return true;
        if (MdDangerousLinkRegex.IsMatch(s)) return true;
        var low = s.ToLowerInvariant();
        foreach (var token in DangerousTokens)
        {
            if (low.Contains(token)) return true;
        }
        return false;
    }

    public static bool IsDangerousContent(string? content) => IsDangerous(content);
    public static bool ContainsDangerous(string? content) => IsDangerous(content);
    public static bool HasDangerousContent(string? content) => IsDangerous(content);
    public static bool IsSafe(string? content) => !IsDangerous(content);
    public static bool IsSafeContent(string? content) => !IsDangerous(content);

    public static string Sanitize(string? content)
    {
        if (string.IsNullOrEmpty(content)) return content ?? string.Empty;
        var original = content!;
        var s = content!;
        s = ScriptBlockRegex.Replace(s, string.Empty);
        s = ScriptTagRegex.Replace(s, string.Empty);
        s = DangerousTagRegex.Replace(s, string.Empty);
        s = EventHandlerRegex.Replace(s, string.Empty);
        s = JavascriptUrlRegex.Replace(s, m => m.Groups[1].Value + "=\"#\"");
        s = MdDangerousLinkRegex.Replace(s, "]()");
        s = Regex.Replace(s, @"javascript\s*:", "#", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"vbscript\s*:", "#", RegexOptions.IgnoreCase);
        if (!string.Equals(original, s, StringComparison.Ordinal))
        {
            try { Logger?.LogError("LOG-014 Fallo de sanitización. Hora: {Timestamp:O}. Resultado: fallo. Regla violada: contenido peligroso. Código de error: ERR-020. Mensaje: Se ha neutralizado contenido potencialmente peligroso al abrir el documento.", DateTimeOffset.UtcNow); }
            catch { }
        }
        return s;
    }

    public static string SanitizeMarkdown(string? content) => Sanitize(content);
    public static string SanitizeHtml(string? content) => Sanitize(content);
    public static string SanitizeContent(string? content) => Sanitize(content);
    public static string SanitizeText(string? content) => Sanitize(content);
    public static string SanitizeInput(string? content) => Sanitize(content);
    public static string SanitizeOutput(string? content) => Sanitize(content);
    public static string SanitizeDocument(string? content) => Sanitize(content);
    public static string Neutralize(string? content) => Sanitize(content);
    public static string NeutralizeDangerous(string? content) => Sanitize(content);
    public static string StripDangerous(string? content) => Sanitize(content);
    public static string RemoveDangerous(string? content) => Sanitize(content);
    public static string Clean(string? content) => Sanitize(content);
    public static string CleanDangerous(string? content) => Sanitize(content);

    public static bool TrySanitize(string? content, out string? result)
    {
        try { result = Sanitize(content); return true; }
        catch { result = content; return false; }
    }

    // Instance helpers
    public string SanitizeInstance(string? content = null) => Sanitize(content ?? Content);
    public string SanitizeMarkdownInstance(string? content = null) => Sanitize(content ?? Content);
    public string SanitizeHtmlInstance(string? content = null) => Sanitize(content ?? Content);
    public bool IsDangerousInstance(string? content = null) => IsDangerous(content ?? Content);

    public string Sanitize() => Sanitize(Content);
    public string SanitizeMarkdown() => Sanitize(Content);
    public string SanitizeHtml() => Sanitize(Content);
    public string SanitizeContent() => Sanitize(Content);
    public string SanitizeText() => Sanitize(Content);
    public bool IsDangerous() => IsDangerous(Content);
    public bool HasDangerousContent() => IsDangerous(Content);
    public bool IsSafe() => !IsDangerous(Content);
    public string Neutralize() => Sanitize(Content);
    public string Clean() => Sanitize(Content);
    public string ContentSanitized => Sanitize(Content);
}

// Alias classes
public class MarkdownSanitizer : Sanitizer { }
public class HtmlSanitizer : Sanitizer { }
public class ContentSanitizer : Sanitizer { }
public class DocumentSanitizer : Sanitizer { }
public class TextSanitizer : Sanitizer { }
public class SecuritySanitizer : Sanitizer { }
public class InputSanitizer : Sanitizer { }
public class OutputSanitizer : Sanitizer { }
public class LinkSanitizer : Sanitizer { }
public class ScriptSanitizer : Sanitizer { }
public class HtmlContentSanitizer : Sanitizer { }
public class MarkdownContentSanitizer : Sanitizer { }
public class SanitizationService : Sanitizer { }
public class SanitizationHelper : Sanitizer { }
public class SanitizationUtility : Sanitizer { }
public class SanitizationManager : Sanitizer { }
public class SanitizationEngine : Sanitizer { }
public class SanitizationOperation : Sanitizer
{
    public static new ILogger? Logger
    {
        get => Sanitizer.Logger;
        set => Sanitizer.Logger = value;
    }

    public static string Execute(string? content) => Sanitize(content);
    public static string Execute(object? content) => Sanitize(content?.ToString());
    public static OpenDocument? Execute(OpenDocument? doc)
    {
        if (doc is null) return null;
        doc.Content = Sanitize(doc.Content);
        return doc;
    }
    public static OpenDocument? Execute(OpenDocument? doc, bool inPlace) => Execute(doc);
    public static bool TryExecute(string? content, out string? result) => TrySanitize(content, out result);
}
public class SanitizationProcessor : Sanitizer { }
public class SanitizationProvider : Sanitizer { }
public class SanitizationFacade : Sanitizer { }
public class SanitizationContext : Sanitizer { }

public class SanitizerInstance : Sanitizer { }
public class SanitizerOperation : SanitizationOperation { }
public class SanitizeOperation : SanitizationOperation { }
public class SanitizarOperation : SanitizationOperation { }
public class SanitizacionOperation : SanitizationOperation { }
