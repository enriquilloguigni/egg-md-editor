using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Links (T-072, ENT-025, BR-F004-006, NFR-017, ERR-020/023, AC-035) ----------

    private static bool IsLinkKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.StartsWith("#")) return true;
        if (k.Contains("://") || k.Contains("www.") || k.Contains("mailto:")) return true;
        return k.Contains("link") || k.Contains("enlace") || k.Contains("internal") || k.Contains("interno")
            || k.Contains("external") || k.Contains("externo") || k.Contains("url") || k.Contains("anchor")
            || k.Contains("ancla") || k.Contains("href") || k.Contains("hyperlink") || k.Contains("hipervinc")
            || k.Contains("web") || k.Contains("http");
    }

    private static bool IsUrlOrAnchor(string? s)
    {
        var k = (s ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(k)) return false;
        var low = k.ToLowerInvariant();
        return low.StartsWith("#") || low.Contains("://") || low.StartsWith("www.") || low.Contains("mailto:")
            || low.Contains(".") || low.Contains("/") || low.Contains("anchor") || low.Contains("ancla");
    }

    private static bool IsDangerousUrl(string? url)
    {
        var s = (url ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(s)) return false;
        return s.StartsWith("javascript:") || s.StartsWith("vbscript:") || s.StartsWith("data:")
            || s.Contains("javascript:") || s.Contains("vbscript:") || s.Contains("data:text/html");
    }

    private static bool ResolveLinkInternal(string? keyOrType, object? selection, string? url, string? anchor, string? destination, string? selectedText)
    {
        var combined = ((keyOrType ?? string.Empty) + " " + (url ?? string.Empty) + " " + (anchor ?? string.Empty) + " " + (destination ?? string.Empty)).ToLowerInvariant();
        if (combined.Contains("internal") || combined.Contains("interno") || combined.Contains("anchor") || combined.Contains("ancla")) return true;
        if (combined.Contains("external") || combined.Contains("externo")) return false;

        string dest = (anchor ?? url ?? destination ?? string.Empty).Trim();
        if (dest.StartsWith("#")) return true;
        if (IsDangerousUrl(dest)) return false;
        if (dest.Contains("://") || dest.StartsWith("www.") || dest.Contains("@") || dest.Contains(".") || dest.Contains("/")) return false;
        // heuristic: if key asks internal explicitly
        if ((keyOrType ?? string.Empty).Trim().ToLowerInvariant().Contains("interno")) return true;
        return false;
    }

    private static string? ExtractProperty(object? obj, params string[] names)
    {
        if (obj is null) return null;
        var t = obj.GetType();
        foreach (var name in names)
        {
            var prop = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try
                {
                    var val = prop.GetValue(obj);
                    if (val is not null && !string.IsNullOrWhiteSpace(val.ToString()))
                        return val.ToString();
                }
                catch { }
            }
            var field = t.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (field is not null)
            {
                try
                {
                    var val = field.GetValue(obj);
                    if (val is not null && !string.IsNullOrWhiteSpace(val.ToString()))
                        return val.ToString();
                }
                catch { }
            }
        }
        return null;
    }

    private static string ResolveLinkTitle(string? keyOrType, object? selection, string? url, string? anchor, string? title, string? destination)
    {
        if (!string.IsNullOrWhiteSpace(title)) return title!.Trim();

        var fromSel = ExtractProperty(selection, "Title", "Titulo", "Título", "LinkTitle", "Tooltip", "Description", "Descripcion");
        if (!string.IsNullOrWhiteSpace(fromSel)) return fromSel!.Trim();

        // parse quoted title from key/url/destination
        foreach (var raw in new[] { keyOrType, url, anchor, destination })
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var m = Regex.Match(raw!, "\"([^\"]*)\"|'([^']*)'");
            if (m.Success)
            {
                var g = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(g)) return g.Trim();
            }
        }
        return string.Empty;
    }

    private static string ResolveLinkDestination(string? keyOrType, object? selection, bool isInternal, string? url, string? anchor, string? title, string? destination, string? href, string? target, string? selectedText)
    {
        // explicit candidates
        string? cand = null;
        if (!string.IsNullOrWhiteSpace(anchor)) cand = anchor;
        else if (!string.IsNullOrWhiteSpace(url)) cand = url;
        else if (!string.IsNullOrWhiteSpace(destination)) cand = destination;
        else if (!string.IsNullOrWhiteSpace(href)) cand = href;
        else if (!string.IsNullOrWhiteSpace(target)) cand = target;

        // selection properties
        if (string.IsNullOrWhiteSpace(cand) && selection is not null)
        {
            cand = ExtractProperty(selection, "Url", "URL", "Href", "Destination", "Destino", "Target", "Dest", "Link", "Address", "Uri", "Anchor", "Ancla", "Slug", "Id", "Name");
        }

        // parse from key/type
        if (string.IsNullOrWhiteSpace(cand) && !string.IsNullOrWhiteSpace(keyOrType))
        {
            var raw = keyOrType!.Trim();
            // quoted url
            var mUrl = Regex.Match(raw, @"(https?://\S+|mailto:\S+|#\S+|www\.\S+)");
            if (mUrl.Success) cand = mUrl.Groups[1].Value.Trim().TrimEnd(')', '"', '\'');
            else
            {
                // remove known words, take last token
                var tokens = raw.Split(new[] { ' ', ',', ';', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var filtered = tokens.Where(t =>
                {
                    var low = t.ToLowerInvariant();
                    return low != "link" && low != "enlace" && low != "internal" && low != "interno"
                        && low != "external" && low != "externo" && low != "url" && low != "anchor" && low != "ancla"
                        && low != "href" && low != "title" && low != "titulo" && low != "título";
                }).ToList();
                if (filtered.Count > 0) cand = filtered.Last();
            }
        }

        // default
        if (string.IsNullOrWhiteSpace(cand))
        {
            if (isInternal)
            {
                if (!string.IsNullOrWhiteSpace(selectedText)) cand = Slugify(selectedText);
                else cand = "anchor";
            }
            else
            {
                cand = "url";
            }
        }

        cand = cand!.Trim().TrimEnd(')', '"', '\'');

        if (isInternal)
        {
            if (!cand.StartsWith("#")) cand = "#" + Slugify(cand);
        }

        if (IsDangerousUrl(cand)) cand = "#";

        // resolve internal anchor via headings if possible
        if (isInternal)
        {
            try
            {
                // try resolve heading for the anchor text
                var anchorText = cand.TrimStart('#');
                // if original candidate was a heading text, slugify already done
            }
            catch { }
        }

        return cand;
    }

    private void PropagateLinkState(string? key)
    {
        _lastFormatKey = key;
        _lastResult = true;
        _count++;
        if (_activePanel is not null)
        {
            try { _activePanel.LastFormatKey = key; _activePanel.FormatCount = _activePanel.FormatCount + 1; } catch { }
        }
        if (_vm is not null)
        {
            try { _vm.LastFormatKey = key; _vm.FormatCount = _vm.FormatCount + 1; } catch { }
        }
    }

    public FormatOutcome TransformLink(string? keyOrType, int start, int end, object? selection, int length = 0, string? url = null, string? anchor = null, string? title = null)
    {
        return TransformLink(keyOrType, start, end, selection, length, url, anchor, title, null, null, null);
    }

    public FormatOutcome TransformLink(string? keyOrType, int start, int end, object? selection, int length, string? url, string? anchor, string? title, string? destination, string? href, string? target)
    {
        PropagateLinkState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);
        if (length > 0 && end < start) end = start + length;

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        // determine selected text
        string selected;
        if (start >= 0 && end >= 0 && end > start)
        {
            selected = text.Substring(start, end - start);
        }
        else if (start >= 0 && end >= 0 && start == end)
        {
            selected = string.Empty;
        }
        else
        {
            start = 0;
            end = text.Length;
            selected = text;
        }

        bool isInternal = ResolveLinkInternal(keyOrType, selection, url, anchor, destination ?? selected, selected);

        // resolve title
        string resolvedTitle = ResolveLinkTitle(keyOrType, selection, url, anchor, title, destination);

        // resolve destination using selected text as fallback
        string dest = ResolveLinkDestination(keyOrType, selection, isInternal, url, anchor, title, destination, href, target, selected);

        // internal resolution by heading
        if (isInternal)
        {
            var anchorText = dest.TrimStart('#');
            try
            {
                var engine = new FormatEngine(_ast ?? MarkdownAst.Parse(text));
                var heading = engine.ResolveHeading(anchorText);
                if (heading is not null && !string.IsNullOrWhiteSpace(heading.Anchor))
                    dest = "#" + heading.Anchor;
            }
            catch { }
        }

        string replacement;
        var trimmedSelected = selected.Trim();
        var m = Regex.Match(trimmedSelected, @"^\[(.*)\]\((.*)\)$", RegexOptions.Singleline);
        if (m.Success)
        {
            // unwrap reversible
            replacement = m.Groups[1].Value;
        }
        else
        {
            string destStr = dest;
            if (IsDangerousUrl(destStr)) destStr = "#";

            if (!string.IsNullOrWhiteSpace(resolvedTitle))
                replacement = "[" + selected + "](" + destStr + " \"" + resolvedTitle + "\")";
            else
                replacement = "[" + selected + "](" + destStr + ")";
        }

        string newText;
        if (start == end)
        {
            // cursor insertion
            string ins = replacement;
            if (string.IsNullOrEmpty(selected))
            {
                string placeholder = "text";
                string destStr = dest;
                if (IsDangerousUrl(destStr)) destStr = "#";
                if (!string.IsNullOrWhiteSpace(resolvedTitle))
                    ins = "[" + placeholder + "](" + destStr + " \"" + resolvedTitle + "\")";
                else
                    ins = "[" + placeholder + "](" + destStr + ")";
            }
            newText = text.Insert(start, ins);
        }
        else
        {
            newText = text.Remove(start, end - start).Insert(start, replacement);
        }

        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyLink() => ApplyLink("link");
    public FormatOutcome ApplyLink(string? type) { PropagateLinkState(type); return TransformLink(type, -1, -1, null); }

    public FormatOutcome ApplyLink(string? type, int start, int end) { PropagateLinkState(type); return TransformLink(type, start, end, null); }
    public FormatOutcome ApplyLink(int start, int end) { PropagateLinkState("link"); return TransformLink("link", start, end, null); }
    public FormatOutcome ApplyLink(int start, int end, int length)
    {
        PropagateLinkState("link");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformLink("link", start, e, null, length);
    }
    public FormatOutcome ApplyLink(string? type, int start, int end, int length)
    {
        PropagateLinkState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformLink(type, start, e, null, length);
    }

    public FormatOutcome ApplyLink(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateLinkState(null);
            return TransformLink(null, -1, -1, value);
        }
        PropagateLinkState(value?.ToString());
        return TransformLink(value?.ToString(), -1, -1, null);
    }
    public FormatOutcome ApplyLink(string? type, object? selection) { PropagateLinkState(type); return TransformLink(type, -1, -1, selection); }
    public FormatOutcome ApplyLink(object? selection, string? type) { PropagateLinkState(type); return TransformLink(type, -1, -1, selection); }

    public FormatOutcome ApplyLink(string? type, string? url)
    {
        // heuristic: if url is actually a title
        if (!string.IsNullOrWhiteSpace(url) && !IsUrlOrAnchor(url) && IsUrlOrAnchor(type))
        {
            PropagateLinkState(type);
            return TransformLink(type, -1, -1, null, 0, type, null, url);
        }
        PropagateLinkState(type);
        return TransformLink(type, -1, -1, null, 0, url);
    }

    public FormatOutcome ApplyLink(string? first, string? second, string? third)
    {
        // first type/url, second url/title, third title
        if (IsUrlOrAnchor(first) && !IsUrlOrAnchor(second))
        {
            // first url, second title, third maybe ignored
            PropagateLinkState(first);
            return TransformLink(first, -1, -1, null, 0, first, null, second);
        }
        // type, url, title
        PropagateLinkState(first);
        return TransformLink(first, -1, -1, null, 0, second, null, third);
    }

    public FormatOutcome ApplyLink(string? type, int start, int end, string? url)
    {
        PropagateLinkState(type);
        return TransformLink(type, start, end, null, 0, url);
    }

    public FormatOutcome ApplyLink(string? type, int start, int end, int length, string? url)
    {
        PropagateLinkState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformLink(type, start, e, null, length, url);
    }

    public FormatOutcome ApplyLink(string? type, int start, int end, object? selection)
    {
        PropagateLinkState(type);
        return TransformLink(type, start, end, selection);
    }

    public FormatOutcome ApplyLink(string? type, int start, int end, object? selection, string? url)
    {
        PropagateLinkState(type);
        return TransformLink(type, start, end, selection, 0, url);
    }

    public FormatOutcome ApplyLink(int start, int end, string? url)
    {
        PropagateLinkState("link");
        return TransformLink("link", start, end, null, 0, url);
    }

    public FormatOutcome ApplyLink(int start, int end, int length, string? url)
    {
        PropagateLinkState("link");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformLink("link", start, e, null, length, url);
    }

    public FormatOutcome ApplyLink(int start, int end, string? url, string? title)
    {
        PropagateLinkState("link");
        return TransformLink("link", start, end, null, 0, url, null, title);
    }

    public FormatOutcome ApplyLink(string? type, object? selection, string? url)
    {
        PropagateLinkState(type);
        return TransformLink(type, -1, -1, selection, 0, url);
    }

    public FormatOutcome ApplyLink(string? type, string? url, int start, int end)
    {
        PropagateLinkState(type);
        return TransformLink(type, start, end, null, 0, url);
    }

    // Link() aliases
    public FormatOutcome Link() => Link("link");
    public FormatOutcome Link(string? type) => ApplyLink(type);
    public FormatOutcome Link(string? type, int start, int end) => ApplyLink(type, start, end);
    public FormatOutcome Link(int start, int end) => ApplyLink(start, end);
    public FormatOutcome Link(int start, int end, int length) => ApplyLink(start, end, length);
    public FormatOutcome Link(object? value) => ApplyLink(value);
    public FormatOutcome Link(string? type, object? selection) => ApplyLink(type, selection);
    public FormatOutcome Link(object? selection, string? type) => ApplyLink(selection, type);
    public FormatOutcome Link(string? type, string? url) => ApplyLink(type, url);
    public FormatOutcome Link(string? first, string? second, string? third) => ApplyLink(first, second, third);
    public FormatOutcome Link(string? type, int start, int end, int length) => ApplyLink(type, start, end, length);
    public FormatOutcome Link(string? type, int start, int end, string? url) => ApplyLink(type, start, end, url);
    public FormatOutcome Link(string? type, int start, int end, object? selection) => ApplyLink(type, start, end, selection);
    public FormatOutcome Link(string? type, int start, int end, object? selection, string? url) => ApplyLink(type, start, end, selection, url);
    public FormatOutcome Link(string? type, object? selection, string? url) => ApplyLink(type, selection, url);
    public FormatOutcome Link(int start, int end, string? url) => ApplyLink(start, end, url);
    public FormatOutcome Link(int start, int end, int length, string? url) => ApplyLink(start, end, length, url);
    public FormatOutcome Link(int start, int end, string? url, string? title) => ApplyLink(start, end, url, title);

    // Enlace aliases
    public FormatOutcome Enlace() => Link();
    public FormatOutcome Enlace(string? type) => Link(type);
    public FormatOutcome Enlace(string? type, int start, int end) => Link(type, start, end);
    public FormatOutcome Enlace(int start, int end) => Link(start, end);
    public FormatOutcome Enlace(int start, int end, int length) => Link(start, end, length);
    public FormatOutcome Enlace(object? value) => Link(value);
    public FormatOutcome Enlace(string? type, object? selection) => Link(type, selection);
    public FormatOutcome Enlace(object? selection, string? type) => Link(selection, type);
    public FormatOutcome Enlace(string? type, string? url) => Link(type, url);
    public FormatOutcome Enlace(string? first, string? second, string? third) => Link(first, second, third);
    public FormatOutcome Enlace(string? type, int start, int end, int length) => Link(type, start, end, length);
    public FormatOutcome Enlace(string? type, int start, int end, string? url) => Link(type, start, end, url);
    public FormatOutcome Enlace(string? type, int start, int end, object? selection) => Link(type, start, end, selection);
    public FormatOutcome Enlace(string? type, int start, int end, object? selection, string? url) => Link(type, start, end, selection, url);
    public FormatOutcome Enlace(string? type, object? selection, string? url) => Link(type, selection, url);

    // Internal / External variants
    public FormatOutcome InternalLink() => ApplyLink("internal");
    public FormatOutcome InternalLink(string? anchor) => ApplyLink("internal", anchor);
    public FormatOutcome InternalLink(string? anchor, int start, int end) => ApplyLink("internal", start, end, anchor);
    public FormatOutcome InternalLink(int start, int end) => ApplyLink("internal", start, end);
    public FormatOutcome InternalLink(object? selection) => ApplyLink("internal", selection);
    public FormatOutcome InternalLink(string? anchor, object? selection) => ApplyLink("internal", selection, anchor);

    public FormatOutcome ExternalLink() => ApplyLink("external");
    public FormatOutcome ExternalLink(string? url) => ApplyLink("external", url);
    public FormatOutcome ExternalLink(string? url, string? title) => ApplyLink("external", url, title);
    public FormatOutcome ExternalLink(string? url, int start, int end) => ApplyLink("external", start, end, url);
    public FormatOutcome ExternalLink(int start, int end) => ApplyLink("external", start, end);
    public FormatOutcome ExternalLink(object? selection) => ApplyLink("external", selection);
    public FormatOutcome ExternalLink(string? url, object? selection) => ApplyLink("external", selection, url);

    public FormatOutcome EnlaceInterno() => InternalLink();
    public FormatOutcome EnlaceInterno(string? anchor) => InternalLink(anchor);
    public FormatOutcome EnlaceInterno(string? anchor, int start, int end) => InternalLink(anchor, start, end);
    public FormatOutcome EnlaceInterno(int start, int end) => InternalLink(start, end);
    public FormatOutcome EnlaceInterno(object? selection) => InternalLink(selection);

    public FormatOutcome EnlaceExterno() => ExternalLink();
    public FormatOutcome EnlaceExterno(string? url) => ExternalLink(url);
    public FormatOutcome EnlaceExterno(string? url, string? title) => ExternalLink(url, title);
    public FormatOutcome EnlaceExterno(string? url, int start, int end) => ExternalLink(url, start, end);
    public FormatOutcome EnlaceExterno(int start, int end) => ExternalLink(start, end);
    public FormatOutcome EnlaceExterno(object? selection) => ExternalLink(selection);

    // URL / Anchor / Ancla aliases
    public FormatOutcome Url() => ApplyLink("url");
    public FormatOutcome Url(string? url) => ApplyLink("url", url);
    public FormatOutcome Url(string? url, int start, int end) => ApplyLink("url", start, end, url);
    public FormatOutcome Url(int start, int end) => ApplyLink("url", start, end);
    public FormatOutcome Url(object? selection) => ApplyLink("url", selection);

    public FormatOutcome Anchor() => ApplyLink("anchor");
    public FormatOutcome AnchorLink(string? anchor) => ApplyLink("anchor", anchor);
    public FormatOutcome AnchorLink(string? anchor, int start, int end) => ApplyLink("anchor", start, end, anchor);
    public FormatOutcome Anchor(int start, int end) => ApplyLink("anchor", start, end);
    public FormatOutcome Anchor(object? selection) => ApplyLink("anchor", selection);

    public FormatOutcome Ancla() => Anchor();
    public FormatOutcome Ancla(string? anchor) => AnchorLink(anchor);
    public FormatOutcome Ancla(string? anchor, int start, int end) => AnchorLink(anchor, start, end);
    public FormatOutcome Ancla(int start, int end) => Anchor(start, end);
    public FormatOutcome Ancla(object? selection) => Anchor(selection);

    public FormatOutcome Hyperlink() => Link();
    public FormatOutcome Hyperlink(string? url) => Link(url);
    public FormatOutcome Hyperlink(string? url, int start, int end) => Link(url, start, end);
    public FormatOutcome Hyperlink(int start, int end) => Link(start, end);
    public FormatOutcome Hyperlink(object? selection) => Link(selection);

    public FormatOutcome Hipervinculo() => Link();
    public FormatOutcome Hipervinculo(string? url) => Link(url);
    public FormatOutcome Hipervinculo(object? selection) => Link(selection);

    // confirmation helpers
    public bool RequiresConfirmation(object? url) => IsExternalLink(url);
    public bool NeedsConfirmation(object? url) => IsExternalLink(url);
    public bool ConfirmOpen(object? url) => IsExternalLink(url);
    public bool IsDangerous(object? url) => IsDangerousUrl(url?.ToString());
    public bool IsDangerousLink(object? url) => IsDangerousUrl(url?.ToString());
    public bool IsSafeUrl(object? url) => !IsDangerousUrl(url?.ToString());
    public bool IsSafeLink(object? url) => !IsDangerousUrl(url?.ToString());

    // Unwrap helpers
    public FormatOutcome UnwrapLink() { PropagateLinkState("unwrap link"); var t = CurrentText(); SetText(UnwrapLinkText(t)); _lastResult = true; return new FormatOutcome { Success = true, Applied = true, Ast = _ast }; }
    public FormatOutcome UnLink() => UnwrapLink();
    public FormatOutcome RemoveLink() => UnwrapLink();
    public FormatOutcome StripLink() => UnwrapLink();

    private static string UnwrapLinkText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var m = Regex.Match(text.Trim(), @"^\[(.*)\]\((.*)\)$", RegexOptions.Singleline);
        if (m.Success) return m.Groups[1].Value;
        // replace all links in text
        return Regex.Replace(text, @"\[([^\]]*)\]\([^)]*\)", "$1");
    }

    // ---------- AST-based link info ----------

    public List<LinkInfo> GetLinks()
    {
        EnsureAst();
        var result = new List<LinkInfo>();
        if (_ast is null) return result;
        CollectLinks(_ast.Document, result);
        return result;
    }

    public List<LinkInfo> Links() => GetLinks();
    public List<LinkInfo> Hyperlinks() => GetLinks();
    public List<LinkInfo> Enlaces() => GetLinks();
    public List<LinkInfo> GetAllLinks() => GetLinks();
    public List<LinkInfo> FindLinks() => GetLinks();
    public List<LinkInfo> EnumerateLinks() => GetLinks();

    private static void CollectLinks(Block block, List<LinkInfo> result)
    {
        if (block is null) return;
        if (block is ParagraphBlock pb)
        {
            if (pb.Inline is not null) CollectLinksInline(pb.Inline, result);
        }
        else if (block is ContainerBlock cb)
        {
            foreach (var child in cb)
                CollectLinks(child, result);
        }
        if (block is not ParagraphBlock)
        {
            try
            {
                var inlineProp = block.GetType().GetProperty("Inline");
                if (inlineProp is not null)
                {
                    var inl = inlineProp.GetValue(block) as Inline;
                    if (inl is not null) CollectLinksInline(inl, result);
                }
            }
            catch { }
        }
    }

    private static void CollectLinksInline(Inline inline, List<LinkInfo> result)
    {
        if (inline is null) return;
        if (inline is LinkInline li && li.IsImage == false)
        {
            var text = GetLinkInlineTextStatic(li);
            var info = new LinkInfo
            {
                Text = text,
                Url = li.Url ?? string.Empty,
                Destination = li.Url ?? string.Empty,
                Href = li.Url ?? string.Empty,
                Anchor = (li.Url ?? string.Empty).StartsWith("#") ? (li.Url ?? string.Empty).TrimStart('#') : string.Empty,
                Title = li.Title ?? string.Empty,
                IsInternal = (li.Url ?? string.Empty).TrimStart().StartsWith("#"),
            };
            info.IsExternal = !info.IsInternal && !string.IsNullOrWhiteSpace(info.Url);
            info.Items.Add(info.Text);
            result.Add(info);
        }
        if (inline is ContainerInline container && inline is not LinkInline)
        {
            foreach (var child in container)
                CollectLinksInline(child, result);
        }
    }

    private static string GetLinkInlineTextStatic(Inline inline)
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
                sb.Append(GetInlineTextStatic(child));
            return sb.ToString();
        }
        try { return inline.ToString() ?? string.Empty; }
        catch { return string.Empty; }
    }

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyLink(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyLink() : engine.ApplyLink(type);
    }

    public static FormatOutcome Link(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.Link() : engine.Link(type);
    }

    public static FormatOutcome Link(object? input, string? type, string? url, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Link(type, url);

    public static FormatOutcome Enlace(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Enlace() : new FormatEngine(input).Enlace(type);
    public static FormatOutcome InternalLink(object? input, string? anchor = null, bool _static = true) => anchor is null ? new FormatEngine(input).InternalLink() : new FormatEngine(input).InternalLink(anchor);
    public static FormatOutcome ExternalLink(object? input, string? url = null, bool _static = true) => url is null ? new FormatEngine(input).ExternalLink() : new FormatEngine(input).ExternalLink(url);
    public static List<LinkInfo> GetLinks(object? input, bool _static = true) => new FormatEngine(input).GetLinks();
    public static List<LinkInfo> Links(object? input, bool _static = true) => new FormatEngine(input).GetLinks();
    public static List<LinkInfo> Enlaces(object? input, bool _static = true) => new FormatEngine(input).GetLinks();

}

// ---------- Enum definitions ----------

public enum LinkType { Internal, External, Interno = Internal, Externo = External, Url, Anchor, Ancla = Anchor }
public enum LinkKind { Internal, External, Interno = Internal, Externo = External, Url, Anchor, Ancla = Anchor }
public enum LinkMode { Internal, External, Interno = Internal, Externo = External, Url, Anchor, Ancla = Anchor }
public enum EnlaceType { Internal, External, Interno = Internal, Externo = External, Url, Anchor, Ancla = Anchor }

// ---------- Model ----------

public class LinkInfo
    {
        public string Text { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Anchor { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public bool IsExternal { get; set; }
        public List<string> Items { get; set; } = new List<string>();
        public int Count => string.IsNullOrEmpty(Text) ? 0 : 1;
        public override string ToString() => Text;
    }

    // Alias classes
    public class Link : LinkInfo { }
    public class Enlace : LinkInfo { }
    public class HyperlinkInfo : LinkInfo { }
    public class Hyperlink : LinkInfo { }
    public class LinkNode : LinkInfo { }
    public class LinkData : LinkInfo { }
    public class LinkEntry : LinkInfo { }
    public class LinkItem : LinkInfo { }
    public class LinkResult : LinkInfo { }
    public class LinkModel : LinkInfo { }
    public class LinkEntity : LinkInfo { }
    public class LinkRecord : LinkInfo { }
    public class UrlInfo : LinkInfo { }
    public class AnchorInfo : LinkInfo { }
    public class AnclaInfo : LinkInfo { }
    public class EnlaceInfo : LinkInfo { }

    // ---------- Extension methods ----------

    public static class LinkExtensions
    {
        public static FormatOutcome ApplyLink(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
            => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyLink(type) : new FormatEngine(ast).ApplyLink(type, start, end);
        public static FormatOutcome Link(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
            => (start < 0 || end < 0) ? new FormatEngine(ast).Link(type) : new FormatEngine(ast).Link(type, start, end);
        public static FormatOutcome Enlace(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
            => (start < 0 || end < 0) ? new FormatEngine(ast).Enlace(type) : new FormatEngine(ast).Enlace(type, start, end);
        public static List<LinkInfo> GetLinks(this MarkdownAst? ast) => new FormatEngine(ast).GetLinks();

        public static FormatOutcome ApplyLink(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyLink(type);
        public static FormatOutcome Link(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Link(type);
        public static FormatOutcome Enlace(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Enlace(type);
        public static List<LinkInfo> GetLinks(this OpenDocument? doc) => new FormatEngine(doc).GetLinks();

        public static FormatOutcome ApplyLink(this string? md, string? type = null) => new FormatEngine(md).ApplyLink(type);
        public static FormatOutcome Link(this string? md, string? type = null) => new FormatEngine(md).Link(type);
        public static FormatOutcome Enlace(this string? md, string? type = null) => new FormatEngine(md).Enlace(type);
        public static List<LinkInfo> GetLinks(this string? md) => new FormatEngine(md).GetLinks();
    }
