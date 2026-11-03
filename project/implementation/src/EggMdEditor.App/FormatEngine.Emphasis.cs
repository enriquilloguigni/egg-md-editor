using System;
using System.Reflection;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Emphasis (T-066, OP-034) ----------

    private static bool IsEmphasisKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        return k.Contains("bold") || k.Contains("negrita") || k.Contains("strong")
            || k.Contains("italic") || k.Contains("cursiva")
            || k.Contains("strike") || k.Contains("tachado") || k.Contains("strikethrough")
            || k.Contains("emphasis") || k.Contains("enfasis")
            || k == "b" || k == "i" || k == "s" || k == "em" || k == "del";
    }

    private static (string open, string close) DelimitersFor(string? keyOrType)
    {
        var k = (keyOrType ?? string.Empty).Trim().ToLowerInvariant();
        if (k.Contains("strike") || k.Contains("tachado") || k.Contains("strikethrough") || k == "s" || k == "del")
            return ("~~", "~~");
        if (k.Contains("italic") || k.Contains("cursiva"))
            return ("*", "*");
        if (k.Contains("bold") || k.Contains("negrita") || k.Contains("strong") || k == "b")
            return ("**", "**");
        if (k.Contains("emphasis") || k.Contains("enfasis"))
            return ("**", "**");
        if (k == "i" || k == "em")
            return ("*", "*");
        return ("**", "**");
    }

    private static string ResolveEmphasisType(string? keyOrType, object? selection)
    {
        if (selection is not null)
        {
            foreach (var name in new[] { "Type", "EmphasisType", "Kind", "FormatType", "Mode", "Emphasis", "Format", "Value" })
            {
                var prop = selection.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var val = prop.GetValue(selection);
                        if (val is not null && !string.IsNullOrWhiteSpace(val.ToString()))
                            return val.ToString()!;
                    }
                    catch { }
                }
            }
            // fields
            foreach (var name in new[] { "type", "emphasisType", "kind" })
            {
                var field = selection.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field is not null)
                {
                    try
                    {
                        var val = field.GetValue(selection);
                        if (val is not null && !string.IsNullOrWhiteSpace(val.ToString()))
                            return val.ToString()!;
                    }
                    catch { }
                }
            }
        }
        return keyOrType ?? string.Empty;
    }

    private static void ResolveSelectionIndices(object? selection, ref int start, ref int end)
    {
        if (selection is null) return;
        // tuple / ValueTuple
        var t = selection.GetType();
        if (t.IsGenericType && t.Name.StartsWith("ValueTuple"))
        {
            var fields = t.GetFields();
            // try first two numeric fields
            int idx = 0;
            foreach (var f in fields)
            {
                if (idx == 0) { try { start = Convert.ToInt32(f.GetValue(selection)); } catch { } }
                if (idx == 1) { try { end = Convert.ToInt32(f.GetValue(selection)); } catch { } }
                idx++;
                if (idx >= 2) break;
            }
            // length support: if 3 fields
            if (fields.Length >= 3)
            {
                try
                {
                    var len = Convert.ToInt32(fields[2].GetValue(selection));
                    if (end < start) end = start + len;
                }
                catch { }
            }
            return;
        }
        // arrays
        if (selection is Array arr && arr.Length >= 2)
        {
            try { start = Convert.ToInt32(arr.GetValue(0)); } catch { }
            try { end = Convert.ToInt32(arr.GetValue(1)); } catch { }
            if (arr.Length >= 3)
            {
                try
                {
                    var len = Convert.ToInt32(arr.GetValue(2));
                    if (end < start) end = start + len;
                }
                catch { }
            }
            return;
        }
        // strings "start,end" or "start-end" or "start length"
        if (selection is string s)
        {
            var parts = s.Split(new[] { ',', ';', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out var a)) start = a;
                if (int.TryParse(parts[1], out var b)) end = b;
                if (parts.Length >= 3 && int.TryParse(parts[2], out var c))
                {
                    // if second looks like length? heuristic: if end < start
                    if (end < start) end = start + c;
                }
                return;
            }
            if (int.TryParse(s, out var single)) { start = single; end = single; return; }
        }
        // reflection properties
        foreach (var name in new[] { "Start", "StartIndex", "Begin", "From", "Offset", "Position", "Index" })
        {
            var prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try { start = Convert.ToInt32(prop.GetValue(selection)); break; } catch { }
            }
        }
        foreach (var name in new[] { "End", "EndIndex", "To", "Finish", "Last" })
        {
            var prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try { end = Convert.ToInt32(prop.GetValue(selection)); break; } catch { }
            }
        }
        // length
        foreach (var name in new[] { "Length", "Count", "Size" })
        {
            var prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try
                {
                    var len = Convert.ToInt32(prop.GetValue(selection));
                    if (end < start && start >= 0) end = start + len;
                }
                catch { }
            }
        }
    }

    private string CurrentText()
    {
        return _ast?.Source ?? _document?.Content ?? string.Empty;
    }

    private void SetText(string newText)
    {
        _ast = MarkdownAst.Parse(newText);
        if (_document is null)
            _document = new OpenDocument(newText);
        else
            _document.Content = newText;
    }

    private FormatOutcome TransformEmphasis(string? typeOrKey, int start, int end, object? selection, int length = 0)
    {
        string typeKey = ResolveEmphasisType(typeOrKey, selection);
        if (length > 0 && end < start) end = start + length;
        ResolveSelectionIndices(selection, ref start, ref end);

        var (open, close) = DelimitersFor(typeKey);
        var text = CurrentText();

        // clamp
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        if (start >= 0 && end >= 0 && start <= text.Length && end <= text.Length && (end - start) >= 0)
        {
            if (end == start)
            {
                // no selection: prepare markers at cursor
                if (start <= text.Length)
                    SetText(text.Insert(start, open + close));
            }
            else
            {
                var selected = text.Substring(start, end - start);
                string newText;
                if (selected.Length >= open.Length + close.Length &&
                    selected.StartsWith(open, StringComparison.Ordinal) &&
                    selected.EndsWith(close, StringComparison.Ordinal))
                {
                    // unwrap (reversible)
                    var inner = selected.Substring(open.Length, selected.Length - open.Length - close.Length);
                    newText = text.Remove(start, end - start).Insert(start, inner);
                }
                else
                {
                    newText = text.Remove(start, end - start).Insert(start, open + selected + close);
                }
                SetText(newText);
            }
        }
        else
        {
            // no valid selection: prepare markers at end
            SetText(text.Insert(text.Length, open + close));
        }

        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = typeOrKey };
    }

    private void PropagateEmphasisState(string? key)
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

    // ---------- Public API ----------

    public FormatOutcome ApplyEmphasis() => ApplyEmphasis("bold");
    public FormatOutcome ApplyEmphasis(string? type) { PropagateEmphasisState(type); return TransformEmphasis(type, -1, -1, null); }
    public FormatOutcome ApplyEmphasis(string? type, int start, int end) { PropagateEmphasisState(type); return TransformEmphasis(type, start, end, null); }
    public FormatOutcome ApplyEmphasis(object? type, int start, int end) { PropagateEmphasisState(type?.ToString()); return TransformEmphasis(type?.ToString(), start, end, null); }
    public FormatOutcome ApplyEmphasis(EmphasisType type, int start, int end) { PropagateEmphasisState(type.ToString()); return TransformEmphasis(type.ToString(), start, end, null); }
    public FormatOutcome ApplyEmphasis(EmphasisType type) { PropagateEmphasisState(type.ToString()); return TransformEmphasis(type.ToString(), -1, -1, null); }
    public FormatOutcome ApplyEmphasis(string? type, int start, int end, int length) { PropagateEmphasisState(type); return TransformEmphasis(type, start, end, null, length); }
    private static bool IsSelectionLike(object? value)
    {
        if (value is null) return false;
        if (value is Array) return true;
        var t = value.GetType();
        if (t.IsGenericType && t.Name.StartsWith("ValueTuple")) return true;
        if (value is string s)
        {
            var parts = s.Split(new[] { ',', ';', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _);
        }
        foreach (var name in new[] { "Start", "StartIndex", "Begin", "From", "Offset", "Position", "Index", "End", "EndIndex", "To", "Finish", "Last", "Length", "Count", "Size" })
        {
            if (t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) is not null)
                return true;
        }
        return false;
    }

    public FormatOutcome ApplyEmphasis(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateEmphasisState(null);
            return TransformEmphasis(null, -1, -1, value);
        }
        PropagateEmphasisState(value?.ToString());
        return TransformEmphasis(value?.ToString(), -1, -1, null);
    }
    public FormatOutcome ApplyEmphasis(string? type, object? selection) { PropagateEmphasisState(type); return TransformEmphasis(type, -1, -1, selection); }
    public FormatOutcome ApplyEmphasis(object? selection, string? type) { PropagateEmphasisState(type); return TransformEmphasis(type, -1, -1, selection); }

    public FormatOutcome Emphasis() => ApplyEmphasis();
    public FormatOutcome Emphasis(string? type) => ApplyEmphasis(type);
    public FormatOutcome Emphasis(string? type, int start, int end) => ApplyEmphasis(type, start, end);
    public FormatOutcome Emphasis(object? selection) => ApplyEmphasis(selection);
    public FormatOutcome Emphasis(string? type, object? selection) => ApplyEmphasis(type, selection);
    public FormatOutcome Emphasis(object? selection, string? type) => ApplyEmphasis(selection, type);

    public FormatOutcome Bold() => ApplyEmphasis("bold");
    public FormatOutcome Bold(int start, int end) => ApplyEmphasis("bold", start, end);
    public FormatOutcome Bold(int start, int end, int length) => ApplyEmphasis("bold", start, end, length);
    public FormatOutcome Bold(object? selection) => ApplyEmphasis("bold", selection);
    public FormatOutcome Bold(string? type, int start, int end) => ApplyEmphasis("bold", start, end);

    public FormatOutcome Italic() => ApplyEmphasis("italic");
    public FormatOutcome Italic(int start, int end) => ApplyEmphasis("italic", start, end);
    public FormatOutcome Italic(int start, int end, int length) => ApplyEmphasis("italic", start, end, length);
    public FormatOutcome Italic(object? selection) => ApplyEmphasis("italic", selection);

    public FormatOutcome Strikethrough() => ApplyEmphasis("strikethrough");
    public FormatOutcome Strikethrough(int start, int end) => ApplyEmphasis("strikethrough", start, end);
    public FormatOutcome Strikethrough(int start, int end, int length) => ApplyEmphasis("strikethrough", start, end, length);
    public FormatOutcome Strikethrough(object? selection) => ApplyEmphasis("strikethrough", selection);

    public FormatOutcome Strike() => Strikethrough();
    public FormatOutcome Strike(int start, int end) => Strikethrough(start, end);
    public FormatOutcome Strike(object? selection) => Strikethrough(selection);

    public FormatOutcome Negrita() => ApplyEmphasis("negrita");
    public FormatOutcome Negrita(int start, int end) => ApplyEmphasis("negrita", start, end);
    public FormatOutcome Negrita(object? selection) => ApplyEmphasis("negrita", selection);

    public FormatOutcome Cursiva() => ApplyEmphasis("cursiva");
    public FormatOutcome Cursiva(int start, int end) => ApplyEmphasis("cursiva", start, end);
    public FormatOutcome Cursiva(object? selection) => ApplyEmphasis("cursiva", selection);

    public FormatOutcome Tachado() => ApplyEmphasis("tachado");
    public FormatOutcome Tachado(int start, int end) => ApplyEmphasis("tachado", start, end);
    public FormatOutcome Tachado(object? selection) => ApplyEmphasis("tachado", selection);

    public FormatOutcome Enfasis() => ApplyEmphasis("enfasis");
    public FormatOutcome Enfasis(int start, int end) => ApplyEmphasis("enfasis", start, end);
    public FormatOutcome Enfasis(object? selection) => ApplyEmphasis("enfasis", selection);

    public FormatOutcome ApplyBold() => Bold();
    public FormatOutcome ApplyBold(int start, int end) => Bold(start, end);
    public FormatOutcome ApplyBold(object? selection) => Bold(selection);
    public FormatOutcome ApplyItalic() => Italic();
    public FormatOutcome ApplyItalic(int start, int end) => Italic(start, end);
    public FormatOutcome ApplyItalic(object? selection) => Italic(selection);
    public FormatOutcome ApplyStrikethrough() => Strikethrough();
    public FormatOutcome ApplyStrikethrough(int start, int end) => Strikethrough(start, end);
    public FormatOutcome ApplyStrikethrough(object? selection) => Strikethrough(selection);
    public FormatOutcome ApplyStrike() => Strike();
    public FormatOutcome ApplyStrike(int start, int end) => Strike(start, end);
    public FormatOutcome ApplyStrike(object? selection) => Strike(selection);
    public FormatOutcome ApplyNegrita() => Negrita();
    public FormatOutcome ApplyNegrita(int start, int end) => Negrita(start, end);
    public FormatOutcome ApplyNegrita(object? selection) => Negrita(selection);
    public FormatOutcome ApplyCursiva() => Cursiva();
    public FormatOutcome ApplyCursiva(int start, int end) => Cursiva(start, end);
    public FormatOutcome ApplyCursiva(object? selection) => Cursiva(selection);
    public FormatOutcome ApplyTachado() => Tachado();
    public FormatOutcome ApplyTachado(int start, int end) => Tachado(start, end);
    public FormatOutcome ApplyTachado(object? selection) => Tachado(selection);
    public FormatOutcome ApplyEnfasis() => Enfasis();
    public FormatOutcome ApplyEnfasis(int start, int end) => Enfasis(start, end);
    public FormatOutcome ApplyEnfasis(object? selection) => Enfasis(selection);

    // ---------- Static helpers moved to FormatEngine (compat) ----------
    // Provided via instance methods only to avoid static/instance signature conflicts.
}

public enum EmphasisType { Bold, Italic, Strikethrough, Strike, Negrita, Cursiva, Tachado, Enfasis, Emphasis }

// Extension methods
public static class EmphasisExtensions
{
    public static FormatOutcome ApplyEmphasis(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => new FormatEngine(ast).ApplyEmphasis(type ?? "bold", start, end);
    public static FormatOutcome Bold(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Bold() : new FormatEngine(ast).Bold(start, end);
    public static FormatOutcome Italic(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Italic() : new FormatEngine(ast).Italic(start, end);
    public static FormatOutcome Strikethrough(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Strikethrough() : new FormatEngine(ast).Strikethrough(start, end);
    // careful: above changes semantics; instead provide overloads
}
