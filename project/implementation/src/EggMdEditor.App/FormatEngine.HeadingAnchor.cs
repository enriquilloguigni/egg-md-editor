using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Heading anchor & internal link resolution (T-068) ----------

    // ----- Slugify / anchor generation -----

    public static string Slugify(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var s = text!.Trim().ToLowerInvariant();
        var sb = new System.Text.StringBuilder();
        foreach (char c in s)
        {
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                sb.Append(c);
            else if (char.IsWhiteSpace(c))
                sb.Append(' ');
            else
                sb.Append(' '); // drop specials -> space
        }
        s = sb.ToString();
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"-+", "-");
        s = s.Trim('-');
        return s;
    }

    public static string SlugifyHeading(string? text) => Slugify(text);
    public static string GenerateAnchor(string? text) => Slugify(text);
    public static string GenerateSlug(string? text) => Slugify(text);
    public static string CreateAnchor(string? text) => Slugify(text);
    public static string MakeAnchor(string? text) => Slugify(text);
    public static string ComputeAnchor(string? text) => Slugify(text);
    public static string ToAnchor(string? text) => Slugify(text);
    public static string ToSlug(string? text) => Slugify(text);
    public static string Anchorize(string? text) => Slugify(text);
    public static string NormalizeAnchor(string? text) => Slugify(text);
    // removed duplicate

    // Instance wrappers
    public string SlugifyInstance(string? text) => Slugify(text);
    public string AnchorFor(string? text) => Slugify(text);
    public string HeadingSlug(string? text) => Slugify(text);
    public string HeadingAnchorSlug(string? text) => Slugify(text);

    // Static slug-returning aliases (callable on instances too)
    public static string Slug(string? text) => Slugify(text);
    public static string Anchor(string? text) => Slugify(text);
    public static string HeadingAnchor(string? text) => Slugify(text);
    public static string GetAnchor(string? text) => Slugify(text);
    public static string GetHeadingAnchor(string? text) => Slugify(text);
    // removed duplicate
    public static string HeadingAnchorText(string? text) => Slugify(text);
    public static string GetAnchorText(string? text) => Slugify(text);
    public static string GetHeadingAnchorText(string? text) => Slugify(text);
    public static string ComputeHeadingAnchor(string? text) => Slugify(text);
    public static string MakeHeadingAnchor(string? text) => Slugify(text);
    public static string CreateHeadingAnchor(string? text) => Slugify(text);
    public static string ToHeadingAnchor(string? text) => Slugify(text);

    // Overloads for objects
    public static string Slugify(object? input) => input is null ? string.Empty : Slugify(input.ToString());
    public static string GenerateAnchor(object? input) => Slugify(input);
    public static string GetAnchor(object? input) => Slugify(input);
    public static string GetHeadingAnchor(object? input) => Slugify(input);
    public static string HeadingAnchor(object? input) => Slugify(input);

    // ----- Heading model -----

    // Instance heading anchor list
    public List<HeadingAnchorInfo> GetHeadings()
    {
        EnsureAst();
        var result = new List<HeadingAnchorInfo>();
        if (_ast is null) return result;
        var blocks = new List<HeadingBlock>();
        CollectHeadings(_ast.Document, blocks);
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int idx = 0;
        foreach (var hb in blocks)
        {
            string text = GetHeadingText(hb).Trim();
            string baseSlug = Slugify(text);
            string slug = baseSlug;
            int i = 1;
            while (!used.Add(slug))
            {
                slug = string.IsNullOrEmpty(baseSlug) ? (i.ToString()) : (baseSlug + "-" + i);
                i++;
                if (i > 10000) break;
            }
            var info = new HeadingAnchorInfo
            {
                Index = idx,
                Level = hb.Level,
                Text = text,
                HeadingText = text,
                Title = text,
                Content = text,
                Anchor = slug,
                Slug = slug,
                Id = slug,
                Name = slug,
                Href = "#" + slug,
                Url = "#" + slug,
                Destination = "#" + slug,
                Target = slug,
                Start = hb.Span.Start,
                End = hb.Span.End,
            };
            result.Add(info);
            idx++;
        }
        return result;
    }

    public List<HeadingAnchorInfo> Headings() => GetHeadings();
    public List<HeadingAnchorInfo> ListHeadings() => GetHeadings();
    public List<HeadingAnchorInfo> EnumerateHeadings() => GetHeadings();
    public List<HeadingAnchorInfo> GetAllHeadings() => GetHeadings();
    public List<HeadingAnchorInfo> FindHeadings() => GetHeadings();
    public List<HeadingAnchorInfo> HeadingList() => GetHeadings();

    public List<string> GetHeadingAnchors()
    {
        return GetHeadings().Select(h => h.Anchor).ToList();
    }
    public List<string> HeadingAnchors() => GetHeadingAnchors();
    public List<string> GetAnchors() => GetHeadingAnchors();
    public List<string> Anchors() => GetHeadingAnchors();
    public List<string> AllAnchors() => GetHeadingAnchors();
    public List<string> AnchorList() => GetHeadingAnchors();

    public List<string> GetHeadingTexts()
    {
        return GetHeadings().Select(h => h.Text).ToList();
    }

    // Properties
    public List<HeadingAnchorInfo> HeadingsProperty => GetHeadings();
    public List<string> HeadingAnchorsProperty => GetHeadingAnchors();
    public List<string> HeadingAnchorList => GetHeadingAnchors();

    // Access by index
    public HeadingAnchorInfo? GetHeading(int index)
    {
        var hs = GetHeadings();
        if (index < 0 || index >= hs.Count) return null;
        return hs[index];
    }
    // Heading(int) conflicts with Heading.cs; use GetHeading instead.
    public HeadingAnchorInfo? GetHeadingAt(int index) => GetHeading(index);
    public HeadingAnchorInfo? HeadingAt(int index) => GetHeading(index);
    public HeadingAnchorInfo? GetHeadingByIndex(int index) => GetHeading(index);

    public string? GetHeadingAnchor(int index)
    {
        return GetHeading(index)?.Anchor;
    }
    public string? GetAnchor(int index) => GetHeading(index)?.Anchor;
    public string? Anchor(int index) => GetHeading(index)?.Anchor;
    public string? HeadingAnchor(int index) => GetHeading(index)?.Anchor;

    // Access by text
    public HeadingAnchorInfo? GetHeading(string? text)
    {
        if (text is null) return null;
        var slug = Slugify(text);
        return GetHeadings().FirstOrDefault(h => string.Equals(h.Anchor, slug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(h.Text, text, StringComparison.Ordinal));
    }
    public HeadingAnchorInfo? FindHeading(string? text) => GetHeading(text);
    public HeadingAnchorInfo? FindHeadingByText(string? text) => GetHeading(text);
    public HeadingAnchorInfo? GetHeadingByText(string? text) => GetHeading(text);
    public HeadingAnchorInfo? GetHeadingByTitle(string? text) => GetHeading(text);

    public string? GetAnchorForHeading(string? text) => FormatEngine.GetHeadingAnchor(text);
    public string? AnchorForHeading(string? text) => FormatEngine.GetHeadingAnchor(text);
    public string? GetAnchorOfHeading(string? text) => FormatEngine.GetHeadingAnchor(text);
    public string? AnchorOfHeading(string? text) => FormatEngine.GetHeadingAnchor(text);

    // Resolution of internal links
    public HeadingAnchorInfo? ResolveHeading(object? anchorOrLink)
    {
        if (anchorOrLink is null) return null;
        string raw = anchorOrLink.ToString() ?? string.Empty;
        string anchor = raw.Trim();
        if (anchor.StartsWith("#")) anchor = anchor.Substring(1);
        anchor = anchor.Trim();

        // direct anchor match
        var hs = GetHeadings();
        var match = hs.FirstOrDefault(h => string.Equals(h.Anchor, anchor, StringComparison.OrdinalIgnoreCase));
        if (match is not null) return match;

        // slugify input and match
        string slug = Slugify(anchor);
        match = hs.FirstOrDefault(h => string.Equals(h.Anchor, slug, StringComparison.OrdinalIgnoreCase));
        if (match is not null) return match;

        // text match
        match = hs.FirstOrDefault(h => string.Equals(h.Text, anchor, StringComparison.Ordinal));
        if (match is not null) return match;

        // contains
        match = hs.FirstOrDefault(h => h.Text.IndexOf(anchor, StringComparison.OrdinalIgnoreCase) >= 0);
        return match;
    }

    public HeadingAnchorInfo? ResolveAnchor(object? anchorOrLink) => ResolveHeading(anchorOrLink);
    public HeadingAnchorInfo? ResolveInternalLink(object? anchorOrLink) => ResolveHeading(anchorOrLink);
    public HeadingAnchorInfo? ResolveLink(object? anchorOrLink) => ResolveHeading(anchorOrLink);
    public HeadingAnchorInfo? ResolveHeadingAnchor(object? anchorOrLink) => ResolveHeading(anchorOrLink);
    public HeadingAnchorInfo? FindHeadingByAnchor(object? anchorOrLink) => ResolveHeading(anchorOrLink);
    public HeadingAnchorInfo? GetHeadingByAnchor(object? anchorOrLink) => ResolveHeading(anchorOrLink);
    public HeadingAnchorInfo? GetHeadingForAnchor(object? anchorOrLink) => ResolveHeading(anchorOrLink);
    public HeadingAnchorInfo? HeadingForAnchor(object? anchorOrLink) => ResolveHeading(anchorOrLink);

    public string? ResolveAnchorToText(object? anchorOrLink) => ResolveHeading(anchorOrLink)?.Text;
    public string? ResolveToHeadingText(object? anchorOrLink) => ResolveHeading(anchorOrLink)?.Text;

    public bool IsInternalLink(object? url)
    {
        var s = url?.ToString() ?? string.Empty;
        return s.TrimStart().StartsWith("#");
    }
    public bool IsInternal(object? url) => IsInternalLink(url);
    public bool IsInternalAnchor(object? url) => IsInternalLink(url);
    public bool IsAnchorLink(object? url) => IsInternalLink(url);
    public bool IsInternalDestination(object? url) => IsInternalLink(url);
    public bool IsExternalLink(object? url) => !IsInternalLink(url) && !string.IsNullOrWhiteSpace(url?.ToString());

    // ----- Static helpers -----

    public static List<HeadingAnchorInfo> GetHeadings(object? input)
    {
        var engine = new FormatEngine(input);
        return engine.GetHeadings();
    }
    public static List<HeadingAnchorInfo> Headings(object? input) => GetHeadings(input);
    public static List<string> GetHeadingAnchors(object? input) => new FormatEngine(input).GetHeadingAnchors();
    public static List<string> HeadingAnchors(object? input) => GetHeadingAnchors(input);

    public static HeadingAnchorInfo? ResolveHeading(object? input, object? anchorOrLink)
    {
        return new FormatEngine(input).ResolveHeading(anchorOrLink);
    }
    public static HeadingAnchorInfo? ResolveInternalLink(object? input, object? anchorOrLink) => ResolveHeading(input, anchorOrLink);
    public static HeadingAnchorInfo? ResolveAnchor(object? input, object? anchorOrLink) => ResolveHeading(input, anchorOrLink);

    public static string? GetHeadingAnchor(object? input, int index) => new FormatEngine(input).GetHeadingAnchor(index);
    public static HeadingAnchorInfo? GetHeading(object? input, int index) => new FormatEngine(input).GetHeading(index);
    public static HeadingAnchorInfo? GetHeading(object? input, string? text) => new FormatEngine(input).GetHeading(text);
    // already defined above
    // duplicate removed
    public static string? GetHeadingAnchor(object? input, string? text) => FormatEngine.GetHeadingAnchor(text);

    // ----- Private helpers -----

    private void EnsureAst()
    {
        if (_ast is null)
        {
            try
            {
                if (_document is not null) _ast = MarkdownAst.Parse(_document.Content);
                else if (!string.IsNullOrEmpty(_lastFormatKey)) { }
            }
            catch { _ast = null; }
        }
        // If still null but source available
        if (_ast is null)
        {
            try
            {
                var src = CurrentSourceText();
                if (!string.IsNullOrEmpty(src)) _ast = MarkdownAst.Parse(src);
            }
            catch { }
        }
    }

    private static void CollectHeadings(Block block, List<HeadingBlock> list)
    {
        if (block is null) return;
        if (block is HeadingBlock hb) list.Add(hb);
        if (block is ContainerBlock cb)
        {
            foreach (var child in cb)
                CollectHeadings(child, list);
        }
    }

    private static string GetHeadingText(HeadingBlock hb)
    {
        try
        {
            if (hb.Inline is not null)
            {
                var txt = GetInlineText(hb.Inline);
                if (!string.IsNullOrWhiteSpace(txt)) return txt.Trim();
            }
        }
        catch { }
        try
        {
            // fallback: render block to markdown-ish text
            var s = hb.ToString();
            if (!string.IsNullOrWhiteSpace(s))
            {
                var t = s!.Trim();
                // remove leading hashes
                while (t.StartsWith("#")) t = t.Substring(1).TrimStart();
                if (!string.IsNullOrWhiteSpace(t)) return t;
            }
        }
        catch { }
        return string.Empty;
    }

    private static string GetInlineText(Inline inline)
    {
        if (inline is null) return string.Empty;
        if (inline is LiteralInline lit)
        {
            try { return lit.Content.ToString(); } catch { return lit.ToString() ?? string.Empty; }
        }
        if (inline is Markdig.Syntax.Inlines.CodeInline ci)
        {
            try { return ci.Content; } catch { return ci.ToString() ?? string.Empty; }
        }
        if (inline is ContainerInline container)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var child in container)
                sb.Append(GetInlineText(child));
            return sb.ToString();
        }
        try { return inline.ToString() ?? string.Empty; }
        catch { return string.Empty; }
    }

    private string CurrentSourceText()
    {
        return _ast?.Source ?? _document?.Content ?? string.Empty;
    }
}

// Model class + aliases
public class HeadingAnchorInfo
{
    public int Index { get; set; }
    public int Level { get; set; }
    public string Text { get; set; } = string.Empty;
    public string HeadingText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Heading { get => Text; set => Text = value; }
    public string Name { get; set; } = string.Empty;
    public string Anchor { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Start { get; set; }
    public int End { get; set; }

    public override string ToString() => Text;
}

// Alias classes
public class HeadingAnchor : HeadingAnchorInfo { }
public class Heading : HeadingAnchorInfo { }
public class HeadingNode : HeadingAnchorInfo { }
public class HeadingData : HeadingAnchorInfo { }
public class HeadingEntry : HeadingAnchorInfo { }
public class HeadingItem : HeadingAnchorInfo { }
public class Encabezado : HeadingAnchorInfo { }
public class HeadingAnchorModel : HeadingAnchorInfo { }
public class AnchorHeading : HeadingAnchorInfo { }
public class HeadingAnchorResult : HeadingAnchorInfo { }
public class ResolvedHeading : HeadingAnchorInfo { }
public class HeadingResolution : HeadingAnchorInfo { }
public class HeadingAnchorRecord : HeadingAnchorInfo { }
public class HeadingInfo : HeadingAnchorInfo { }
public class HeadingAnchorEntity : HeadingAnchorInfo { }
public class EncabezadoAncla : HeadingAnchorInfo { }

// Extension methods
public static class HeadingAnchorExtensions
{
    public static List<HeadingAnchorInfo> GetHeadings(this MarkdownAst? ast) => new FormatEngine(ast).GetHeadings();
    public static List<string> GetHeadingAnchors(this MarkdownAst? ast) => new FormatEngine(ast).GetHeadingAnchors();
    public static HeadingAnchorInfo? ResolveHeading(this MarkdownAst? ast, object? anchorOrLink) => new FormatEngine(ast).ResolveHeading(anchorOrLink);
    public static string Slugify(this string? text) => FormatEngine.Slugify(text);
    public static string ToAnchor(this string? text) => FormatEngine.Slugify(text);

    public static List<HeadingAnchorInfo> GetHeadings(this OpenDocument? doc) => new FormatEngine(doc).GetHeadings();
    public static List<string> GetHeadingAnchors(this OpenDocument? doc) => new FormatEngine(doc).GetHeadingAnchors();
    public static HeadingAnchorInfo? ResolveHeading(this OpenDocument? doc, object? anchorOrLink) => new FormatEngine(doc).ResolveHeading(anchorOrLink);

    public static List<HeadingAnchorInfo> GetHeadings(this string? md) => new FormatEngine(md).GetHeadings();
    public static List<string> GetHeadingAnchors(this string? md) => new FormatEngine(md).GetHeadingAnchors();
    public static HeadingAnchorInfo? ResolveHeading(this string? md, object? anchorOrLink) => new FormatEngine(md).ResolveHeading(anchorOrLink);
}