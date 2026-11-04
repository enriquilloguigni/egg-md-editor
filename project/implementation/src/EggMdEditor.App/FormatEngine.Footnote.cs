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
    // ---------- Footnotes (T-075, ENT-028, OP-032, BR-F004-009, BR-009, AC-038) ----------

    private static bool IsFootnoteKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("footnote") || k.Contains("nota al pie") || k.Contains("nota") || k.Contains("foot"))
            return true;
        if (k == "pie" || k.Contains("pie de página") || k.Contains("pie de pagina")) return true;
        return false;
    }

    private static string GenerateFootnoteId(string currentText)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match m in Regex.Matches(currentText ?? string.Empty, @"\[\^([^\]]+)\]"))
            ids.Add(m.Groups[1].Value);

        int n = 1;
        while (ids.Contains(n.ToString())) n++;
        return n.ToString();
    }

    private static string ResolveFootnoteId(string? keyOrType, object? selection, string? id, string? text, string? selectedText, string currentText)
    {
        // explicit id
        if (!string.IsNullOrWhiteSpace(id))
        {
            var raw = id!.Trim();
            var m = Regex.Match(raw, @"\[\^([^\]]+)\]");
            if (m.Success) raw = m.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(raw)) return raw;
        }

        // selection properties
        if (selection is not null)
        {
            var fromSel = ExtractProperty(selection, "Id", "Identifier", "FootnoteId", "Name", "Key",
                "Ref", "Reference", "Label", "NoteId", "Footnote");
            if (!string.IsNullOrWhiteSpace(fromSel))
            {
                var raw = fromSel!.Trim();
                var m = Regex.Match(raw, @"\[\^([^\]]+)\]");
                if (m.Success) raw = m.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(raw)) return raw;
            }
        }

        var k = (keyOrType ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(k))
        {
            var m = Regex.Match(k, @"\[\^([^\]]+)\]");
            if (m.Success) return m.Groups[1].Value;

            // "footnote: 2" / "nota id" (require separator to avoid splitting the keyword)
            var m2 = Regex.Match(k, @"(?:footnote|nota\s+al\s+pie|nota|foot|pie)\s*[:=]\s*([^\s\]]+)", RegexOptions.IgnoreCase);
            if (!m2.Success)
                m2 = Regex.Match(k, @"(?:footnote|nota\s+al\s+pie|nota|foot|pie)\s+([^\s\]]+)", RegexOptions.IgnoreCase);
            if (m2.Success)
            {
                var cand = m2.Groups[1].Value.Trim().Trim(']', ')', '"', '\'');
                if (!string.IsNullOrWhiteSpace(cand) && !IsFootnoteKey(cand)) return cand;
            }

            // if key is not a known footnote keyword, treat as id
            if (!IsFootnoteKey(k) && !k.Contains(" ") && k.Length <= 64)
                return k;
        }

        return GenerateFootnoteId(currentText);
    }

    private static string ResolveFootnoteText(string? keyOrType, object? selection, string? id, string? text, string? selectedText)
    {
        if (!string.IsNullOrWhiteSpace(text)) return text!.Trim();

        if (selection is not null)
        {
            var fromSel = ExtractProperty(selection, "Text", "Content", "Note", "FootnoteText",
                "Value", "Body", "Description", "Caption", "Title", "Message");
            if (!string.IsNullOrWhiteSpace(fromSel)) return fromSel!.Trim();
        }

        var k = (keyOrType ?? string.Empty).Trim();
        var m = Regex.Match(k, "\"([^\"]*)\"|'([^']*)'");
        if (m.Success)
        {
            var g = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
            if (!string.IsNullOrWhiteSpace(g)) return g.Trim();
        }

        return string.Empty;
    }

    private void PropagateFootnoteState(string? key)
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

    private static string UnwrapFootnoteText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // remove definition lines
        var result = Regex.Replace(text, @"(?m)^\[\^[^\]]+\]:.*(?:\r?\n)?", "");
        // remove reference markers
        result = Regex.Replace(result, @"\[\^[^\]]*\]", "");
        return result;
    }

    public FormatOutcome TransformFootnote(string? keyOrType, int start, int end, object? selection, int length = 0, string? id = null, string? text = null)
    {
        PropagateFootnoteState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);
        if (length > 0 && end < start) end = start + length;

        var current = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > current.Length) start = current.Length;
        if (end > current.Length) end = current.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        string selected;
        if (start >= 0 && end >= 0 && end > start)
        {
            selected = current.Substring(start, end - start);
        }
        else if (start >= 0 && end >= 0 && start == end)
        {
            selected = string.Empty;
        }
        else
        {
            start = current.Length;
            end = current.Length;
            selected = string.Empty;
        }

        var trimmedSelected = (selected ?? string.Empty).Trim();

        // unwrap if already a footnote reference
        if (Regex.IsMatch(trimmedSelected, @"^\[\^[^\]]+\]$"))
        {
            var m = Regex.Match(trimmedSelected, @"^\[\^([^\]]+)\]$");
            var existingId = m.Success ? m.Groups[1].Value : string.Empty;
            var inner = existingId;
            string newText;
            if (start == end)
            {
                newText = current.Insert(start, inner);
            }
            else
            {
                newText = current.Remove(start, end - start).Insert(start, inner);
            }
            // remove definition line for this id
            if (!string.IsNullOrEmpty(existingId))
            {
                newText = Regex.Replace(newText, @"(?m)^\[\^" + Regex.Escape(existingId) + @"\]:.*(?:\r?\n)?", "");
            }
            SetText(newText);
            _lastResult = true;
            return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
        }

        string resolvedId = ResolveFootnoteId(keyOrType, selection, id, text, selected, current);
        if (string.IsNullOrWhiteSpace(resolvedId)) resolvedId = "1";

        string resolvedText = ResolveFootnoteText(keyOrType, selection, id, text, selected);

        string reference = $"[^{resolvedId}]";

        string newText2;
        if (start == end)
        {
            newText2 = current.Insert(start, reference);
        }
        else
        {
            newText2 = current.Remove(start, end - start).Insert(start, reference);
        }

        // append definition at end if not present
        if (!Regex.IsMatch(newText2, @"(?m)^\[\^" + Regex.Escape(resolvedId) + @"\]:"))
        {
            var def = string.IsNullOrWhiteSpace(resolvedText)
                ? $"[^{resolvedId}]:"
                : $"[^{resolvedId}]: {resolvedText}";
            if (newText2.Length > 0 && !newText2.EndsWith("\n"))
                newText2 += "\n";
            if (newText2.Length > 0 && !newText2.EndsWith("\n\n"))
            {
                // ensure blank line before definition when there is content
                if (!newText2.EndsWith("\n\n"))
                {
                    if (newText2.EndsWith("\n")) newText2 += "\n";
                    else newText2 += "\n\n";
                }
            }
            newText2 += def;
            if (!newText2.EndsWith("\n")) newText2 += "\n";
        }

        SetText(newText2);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyFootnote() => ApplyFootnote("footnote");

    public FormatOutcome ApplyFootnote(string? type)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, -1, -1, null);
    }

    public FormatOutcome ApplyFootnote(string? type, int start, int end)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, start, end, null);
    }

    public FormatOutcome ApplyFootnote(int start, int end)
    {
        PropagateFootnoteState("footnote");
        return TransformFootnote("footnote", start, end, null);
    }

    public FormatOutcome ApplyFootnote(int start, int end, int length)
    {
        PropagateFootnoteState("footnote");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFootnote("footnote", start, e, null, length);
    }

    public FormatOutcome ApplyFootnote(string? type, int start, int end, int length)
    {
        PropagateFootnoteState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFootnote(type, start, e, null, length);
    }

    public FormatOutcome ApplyFootnote(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateFootnoteState(null);
            return TransformFootnote(null, -1, -1, value);
        }
        PropagateFootnoteState(value?.ToString());
        return TransformFootnote(value?.ToString(), -1, -1, null);
    }

    public FormatOutcome ApplyFootnote(string? type, object? selection)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, -1, -1, selection);
    }

    public FormatOutcome ApplyFootnote(object? selection, string? type)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, -1, -1, selection);
    }

    public FormatOutcome ApplyFootnote(string? type, string? id)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, -1, -1, null, 0, id, null);
    }

    public FormatOutcome ApplyFootnote(string? type, string? id, string? text)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, -1, -1, null, 0, id, text);
    }

    public FormatOutcome ApplyFootnote(string? type, int start, int end, string? id)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, start, end, null, 0, id, null);
    }

    public FormatOutcome ApplyFootnote(string? type, int start, int end, int length, string? id)
    {
        PropagateFootnoteState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFootnote(type, start, e, null, length, id, null);
    }

    public FormatOutcome ApplyFootnote(string? type, int start, int end, string? id, string? text)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, start, end, null, 0, id, text);
    }

    public FormatOutcome ApplyFootnote(string? type, int start, int end, object? selection)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, start, end, selection);
    }

    public FormatOutcome ApplyFootnote(string? type, int start, int end, object? selection, string? id)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, start, end, selection, 0, id, null);
    }

    public FormatOutcome ApplyFootnote(string? type, object? selection, string? id)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, -1, -1, selection, 0, id, null);
    }

    public FormatOutcome ApplyFootnote(string? type, string? id, int start, int end)
    {
        PropagateFootnoteState(type);
        return TransformFootnote(type, start, end, null, 0, id, null);
    }

    // Footnote() aliases
    public FormatOutcome Footnote() => Footnote("footnote");
    public FormatOutcome Footnote(string? type) => ApplyFootnote(type);
    public FormatOutcome Footnote(string? type, int start, int end) => ApplyFootnote(type, start, end);
    public FormatOutcome Footnote(int start, int end) => ApplyFootnote(start, end);
    public FormatOutcome Footnote(int start, int end, int length) => ApplyFootnote(start, end, length);
    public FormatOutcome Footnote(object? value) => ApplyFootnote(value);
    public FormatOutcome Footnote(string? type, object? selection) => ApplyFootnote(type, selection);
    public FormatOutcome Footnote(object? selection, string? type) => ApplyFootnote(selection, type);
    public FormatOutcome Footnote(string? type, string? id) => ApplyFootnote(type, id);
    public FormatOutcome Footnote(string? type, string? id, string? text) => ApplyFootnote(type, id, text);
    public FormatOutcome Footnote(string? type, int start, int end, string? id) => ApplyFootnote(type, start, end, id);
    public FormatOutcome Footnote(string? type, int start, int end, int length, string? id) => ApplyFootnote(type, start, end, length, id);
    public FormatOutcome Footnote(string? type, int start, int end, object? selection) => ApplyFootnote(type, start, end, selection);
    public FormatOutcome Footnote(string? type, int start, int end, object? selection, string? id) => ApplyFootnote(type, start, end, selection, id);
    public FormatOutcome Footnote(string? type, object? selection, string? id) => ApplyFootnote(type, selection, id);

    // NotaAlPie aliases
    public FormatOutcome NotaAlPie() => Footnote();
    public FormatOutcome NotaAlPie(string? type) => Footnote(type);
    public FormatOutcome NotaAlPie(string? type, int start, int end) => Footnote(type, start, end);
    public FormatOutcome NotaAlPie(int start, int end) => Footnote(start, end);
    public FormatOutcome NotaAlPie(int start, int end, int length) => Footnote(start, end, length);
    public FormatOutcome NotaAlPie(object? value) => Footnote(value);
    public FormatOutcome NotaAlPie(string? type, object? selection) => Footnote(type, selection);
    public FormatOutcome NotaAlPie(object? selection, string? type) => Footnote(selection, type);
    public FormatOutcome NotaAlPie(string? type, string? id) => Footnote(type, id);
    public FormatOutcome NotaAlPie(string? type, string? id, string? text) => Footnote(type, id, text);

    // Nota aliases
    public FormatOutcome Nota() => Footnote();
    public FormatOutcome Nota(string? type) => Footnote(type);
    public FormatOutcome Nota(string? type, int start, int end) => Footnote(type, start, end);
    public FormatOutcome Nota(int start, int end) => Footnote(start, end);
    public FormatOutcome Nota(object? value) => Footnote(value);
    public FormatOutcome Nota(string? type, object? selection) => Footnote(type, selection);
    public FormatOutcome Nota(string? type, string? id) => Footnote(type, id);
    public FormatOutcome Nota(string? type, string? id, string? text) => Footnote(type, id, text);

    // Foot aliases
    public FormatOutcome Foot() => Footnote();
    public FormatOutcome Foot(string? type) => Footnote(type);
    public FormatOutcome Foot(string? type, int start, int end) => Footnote(type, start, end);
    public FormatOutcome Foot(int start, int end) => Footnote(start, end);
    public FormatOutcome Foot(object? value) => Footnote(value);
    public FormatOutcome Foot(string? type, object? selection) => Footnote(type, selection);
    public FormatOutcome Foot(string? type, string? id) => Footnote(type, id);
    public FormatOutcome Foot(string? type, string? id, string? text) => Footnote(type, id, text);

    // Pie aliases
    public FormatOutcome Pie() => Footnote();
    public FormatOutcome Pie(string? type) => Footnote(type);
    public FormatOutcome Pie(string? type, string? id) => Footnote(type, id);

    // Apply* aliases
    public FormatOutcome ApplyNota() => ApplyFootnote();
    public FormatOutcome ApplyNota(string? type) => ApplyFootnote(type);
    public FormatOutcome ApplyNota(string? type, string? id) => ApplyFootnote(type, id);
    public FormatOutcome ApplyFoot() => ApplyFootnote();
    public FormatOutcome ApplyFoot(string? type) => ApplyFootnote(type);
    public FormatOutcome ApplyPie() => ApplyFootnote();
    public FormatOutcome ApplyNotaAlPie() => ApplyFootnote();
    public FormatOutcome ApplyNotaAlPie(string? type) => ApplyFootnote(type);
    public FormatOutcome ApplyNotaAlPie(string? type, string? id) => ApplyFootnote(type, id);
    public FormatOutcome ApplyNotaAlPie(string? type, string? id, string? text) => ApplyFootnote(type, id, text);

    // Unwrap helpers
    public FormatOutcome UnwrapFootnote()
    {
        PropagateFootnoteState("unwrap footnote");
        var t = CurrentText();
        SetText(UnwrapFootnoteText(t));
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast };
    }
    public FormatOutcome UnFootnote() => UnwrapFootnote();
    public FormatOutcome RemoveFootnote() => UnwrapFootnote();
    public FormatOutcome StripFootnote() => UnwrapFootnote();
    public FormatOutcome UnNota() => UnwrapFootnote();
    public FormatOutcome UnFoot() => UnwrapFootnote();
    public FormatOutcome UnNotaAlPie() => UnwrapFootnote();

    // ---------- Resolve helpers (public aliases) ----------

    public string ResolveId(string? keyOrType = null, object? selection = null, string? id = null, string? text = null, string? selectedText = null)
        => ResolveFootnoteId(keyOrType, selection, id, text, selectedText, CurrentText());

    public string ResolveFootnoteIdPublic(string? keyOrType = null, object? selection = null, string? id = null, string? text = null, string? selectedText = null)
        => ResolveFootnoteId(keyOrType, selection, id, text, selectedText, CurrentText());

    public string ResolveText(string? keyOrType = null, object? selection = null, string? id = null, string? text = null, string? selectedText = null)
        => ResolveFootnoteText(keyOrType, selection, id, text, selectedText);

    public string ResolveFootnoteTextPublic(string? keyOrType = null, object? selection = null, string? id = null, string? text = null, string? selectedText = null)
        => ResolveFootnoteText(keyOrType, selection, id, text, selectedText);

    // ---------- AST-based footnote info ----------

    public List<FootnoteInfo> GetFootnotes()
    {
        EnsureAst();
        var result = new List<FootnoteInfo>();
        var text = CurrentText();
        // definitions
        foreach (Match m in Regex.Matches(text, @"(?m)^\[\^([^\]]+)\]:\s*(.*)$"))
        {
            result.Add(new FootnoteInfo { Id = m.Groups[1].Value, Text = m.Groups[2].Value.Trim() });
        }
        // references without definitions
        if (result.Count == 0)
        {
            foreach (Match m in Regex.Matches(text, @"\[\^([^\]]+)\]"))
            {
                if (!result.Any(r => r.Id == m.Groups[1].Value))
                    result.Add(new FootnoteInfo { Id = m.Groups[1].Value, Text = string.Empty });
            }
        }
        return result;
    }

    public List<FootnoteInfo> Footnotes() => GetFootnotes();
    public List<FootnoteInfo> Notas() => GetFootnotes();
    public List<FootnoteInfo> NotasAlPie() => GetFootnotes();
    public List<FootnoteInfo> GetAllFootnotes() => GetFootnotes();
    public List<FootnoteInfo> FindFootnotes() => GetFootnotes();
    public List<FootnoteInfo> EnumerateFootnotes() => GetFootnotes();

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyFootnote(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyFootnote() : engine.ApplyFootnote(type);
    }

    public static FormatOutcome ApplyFootnote(object? input, string? type, string? id, bool _static = true, bool _extra = true)
        => new FormatEngine(input).ApplyFootnote(type, id);

    public static FormatOutcome ApplyFootnote(object? input, string? type, string? id, string? text, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).ApplyFootnote(type, id, text);

    public static FormatOutcome Footnote(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.Footnote() : engine.Footnote(type);
    }

    public static FormatOutcome Footnote(object? input, string? type, string? id, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Footnote(type, id);

    public static FormatOutcome Footnote(object? input, string? type, string? id, string? text, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).Footnote(type, id, text);

    public static FormatOutcome NotaAlPie(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).NotaAlPie() : new FormatEngine(input).NotaAlPie(type);
    public static FormatOutcome Nota(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Nota() : new FormatEngine(input).Nota(type);
    public static FormatOutcome Foot(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Foot() : new FormatEngine(input).Foot(type);

    public static List<FootnoteInfo> GetFootnotes(object? input, bool _static = true) => new FormatEngine(input).GetFootnotes();
    public static List<FootnoteInfo> Footnotes(object? input, bool _static = true) => new FormatEngine(input).GetFootnotes();
    public static List<FootnoteInfo> Notas(object? input, bool _static = true) => new FormatEngine(input).GetFootnotes();
}

// ---------- Enum definitions ----------

public enum FootnoteType { Footnote, Nota, NotaAlPie, Foot, Pie, NotaAlPieType = Footnote }
public enum FootnoteKind { Footnote, Nota, NotaAlPie, Foot, Pie }
public enum FootnoteMode { Footnote, Nota, NotaAlPie, Foot, Pie }

// ---------- Model ----------

public class FootnoteInfo
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Reference { get => string.IsNullOrEmpty(Id) ? string.Empty : $"[^{Id}]"; set { } }
    public string Definition { get => string.IsNullOrEmpty(Id) ? string.Empty : (string.IsNullOrEmpty(Text) ? $"[^{Id}]:" : $"[^{Id}]: {Text}"); set { } }
    public List<string> Items { get; set; } = new List<string>();
    public int Count => string.IsNullOrEmpty(Id) ? 0 : 1;
    public override string ToString() => string.IsNullOrEmpty(Id) ? string.Empty : (string.IsNullOrEmpty(Text) ? $"[^{Id}]" : $"[^{Id}]: {Text}");
}

// Alias classes
public class Footnote : FootnoteInfo { }
public class Nota : FootnoteInfo { }
public class NotaAlPie : FootnoteInfo { }
public class Foot : FootnoteInfo { }
public class Pie : FootnoteInfo { }
public class FootnoteData : FootnoteInfo { }
public class FootnoteEntry : FootnoteInfo { }
public class FootnoteItem : FootnoteInfo { }
public class FootnoteResult : FootnoteInfo { }
public class FootnoteModel : FootnoteInfo { }
public class FootnoteEntity : FootnoteInfo { }
public class FootnoteRecord : FootnoteInfo { }
public class FootnoteNode : FootnoteInfo { }

// ---------- Extension methods ----------

public static class FootnoteExtensions
{
    public static FormatOutcome ApplyFootnote(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyFootnote(type) : new FormatEngine(ast).ApplyFootnote(type, start, end);
    public static FormatOutcome Footnote(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).Footnote(type);
    public static FormatOutcome Footnote(this MarkdownAst? ast, string? type, string? id)
        => new FormatEngine(ast).Footnote(type, id);
    public static List<FootnoteInfo> GetFootnotes(this MarkdownAst? ast) => new FormatEngine(ast).GetFootnotes();

    public static FormatOutcome ApplyFootnote(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyFootnote(type);
    public static FormatOutcome Footnote(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Footnote(type);
    public static FormatOutcome Footnote(this OpenDocument? doc, string? type, string? id) => new FormatEngine(doc).Footnote(type, id);
    public static List<FootnoteInfo> GetFootnotes(this OpenDocument? doc) => new FormatEngine(doc).GetFootnotes();

    public static FormatOutcome ApplyFootnote(this string? md, string? type = null) => new FormatEngine(md).ApplyFootnote(type);
    public static FormatOutcome Footnote(this string? md, string? type = null) => new FormatEngine(md).Footnote(type);
    public static FormatOutcome Footnote(this string? md, string? type, string? id) => new FormatEngine(md).Footnote(type, id);
    public static List<FootnoteInfo> GetFootnotes(this string? md) => new FormatEngine(md).GetFootnotes();

    public static FormatOutcome NotaAlPie(this MarkdownAst? ast, string? type = null) => new FormatEngine(ast).NotaAlPie(type);
    public static FormatOutcome NotaAlPie(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).NotaAlPie(type);
    public static FormatOutcome NotaAlPie(this string? md, string? type = null) => new FormatEngine(md).NotaAlPie(type);
}