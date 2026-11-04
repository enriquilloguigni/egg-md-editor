using System;
using System.Text.RegularExpressions;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Heading (T-067, OP-025/026, ENT-021) ----------

    private static bool IsHeadingKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("heading") || k.Contains("encabez") || k.Contains("header")
            || k.Contains("titulo") || k.Contains("title") || k.Contains("cabecera"))
            return true;
        if (k.Contains("promot") || k.Contains("promover") || k.Contains("demot")
            || k.Contains("degrad") || k.Contains("degrade"))
            return true;
        if (k.Contains("increase") || k.Contains("decrease")
            || k.Contains("increment") || k.Contains("decrement"))
            return true;
        if (k.Contains("subir") || k.Contains("bajar")) return true;
        if (k.Contains("nivel") || k.Contains("level")) return true;
        // h1..h6
        if (k.Length >= 2 && k[0] == 'h' && k[1] >= '1' && k[1] <= '6'
            && (k.Length == 2 || !char.IsLetterOrDigit(k[2])))
            return true;
        if (Regex.IsMatch(k, @"\bh[1-6]\b")) return true;
        return false;
    }

    private static string ResolveHeadingType(string? keyOrType, object? selection)
    {
        if (selection is not null)
        {
            foreach (var name in new[] { "Level", "HeadingLevel", "Nivel", "LevelNumber", "Heading", "HeadingType", "Type", "Kind", "Format", "Value", "Mode" })
            {
                var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
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
            foreach (var name in new[] { "level", "headingLevel", "nivel" })
            {
                var field = selection.GetType().GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
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

    private static int? ExtractHeadingLevel(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        // words
        var k = s!.Trim().ToLowerInvariant();
        if (k == "h1" || k.Contains("nivel 1") || k.Contains("level 1") || k.Contains("heading 1") || k.Contains("heading1")) return 1;
        if (k == "h2" || k.Contains("nivel 2") || k.Contains("level 2") || k.Contains("heading 2") || k.Contains("heading2")) return 2;
        if (k == "h3" || k.Contains("nivel 3") || k.Contains("level 3") || k.Contains("heading 3") || k.Contains("heading3")) return 3;
        if (k == "h4" || k.Contains("nivel 4") || k.Contains("level 4") || k.Contains("heading 4") || k.Contains("heading4")) return 4;
        if (k == "h5" || k.Contains("nivel 5") || k.Contains("level 5") || k.Contains("heading 5") || k.Contains("heading5")) return 5;
        if (k == "h6" || k.Contains("nivel 6") || k.Contains("level 6") || k.Contains("heading 6") || k.Contains("heading6")) return 6;
        // first digit 1-6
        foreach (char c in k)
        {
            if (c >= '1' && c <= '6') return c - '0';
        }
        return null;
    }

    private static int? ExtractHeadingLevelFromSelection(object? selection)
    {
        if (selection is null) return null;
        if (selection is int i && i >= 1 && i <= 6) return i;
        if (selection is string s)
        {
            if (int.TryParse(s.Trim(), out var n) && n >= 1 && n <= 6) return n;
            var lvl = ExtractHeadingLevel(s);
            if (lvl.HasValue) return lvl;
        }
        foreach (var name in new[] { "Level", "HeadingLevel", "Nivel", "LevelNumber", "Heading", "Value", "Type", "Kind", "Mode" })
        {
            var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try
                {
                    var val = prop.GetValue(selection);
                    if (val is int vi && vi >= 1 && vi <= 6) return vi;
                    if (val is not null)
                    {
                        var parsed = ExtractHeadingLevel(val.ToString());
                        if (parsed.HasValue) return parsed;
                        if (int.TryParse(val.ToString(), out var n) && n >= 1 && n <= 6) return n;
                    }
                }
                catch { }
            }
        }
        return null;
    }

    private static bool IsPromoteKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        return k.Contains("promot") || k.Contains("promover") || k.Contains("increase")
            || k.Contains("increment") || k.Contains("subir") || k.Contains("up");
    }

    private static bool IsDemoteKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        return k.Contains("demot") || k.Contains("degrad") || k.Contains("degrade")
            || k.Contains("decrease") || k.Contains("decrement") || k.Contains("bajar")
            || k.Contains("down");
    }

    private void PropagateHeadingState(string? key)
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

    private static int LineIndexFor(string text, int pos)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        if (pos <= 0) return 0;
        if (pos >= text.Length)
        {
            int l = 0;
            for (int i = 0; i < text.Length; i++) if (text[i] == '\n') l++;
            return l;
        }
        int line = 0;
        for (int i = 0; i < pos; i++) if (text[i] == '\n') line++;
        return line;
    }

    private static string TransformHeadingLine(string line, bool promote, bool demote, int? setLevel)
    {
        if (line is null) return line!;
        int ws = 0;
        while (ws < line.Length && (line[ws] == ' ' || line[ws] == '\t')) ws++;
        string leading = line.Substring(0, ws);
        string rest = line.Substring(ws);

        int hashes = 0;
        while (hashes < rest.Length && rest[hashes] == '#') hashes++;
        bool isHeading = hashes > 0 && (hashes == rest.Length || rest[hashes] == ' ' || rest[hashes] == '\t');

        if (isHeading)
        {
            string content = rest.Substring(hashes).TrimStart();
            int newLevel;
            if (promote) newLevel = Math.Min(6, hashes + 1);
            else if (demote) newLevel = Math.Max(1, hashes - 1);
            else if (setLevel.HasValue) newLevel = Math.Clamp(setLevel.Value, 1, 6);
            else newLevel = hashes;

            if (newLevel == hashes && !setLevel.HasValue && !promote && !demote)
                return line;

            if (content.Length == 0)
                return leading + new string('#', Math.Clamp(newLevel, 1, 6));
            return leading + new string('#', Math.Clamp(newLevel, 1, 6)) + " " + content;
        }

        // not a heading
        if (promote || demote)
            return line;

        if (setLevel.HasValue)
        {
            int lvl = Math.Clamp(setLevel.Value, 1, 6);
            string c = rest.TrimStart();
            if (c.Length == 0) return leading + new string('#', lvl);
            return leading + new string('#', lvl) + " " + c;
        }
        return line;
    }

    private static string TransformHeadingRange(string text, int start, int end, bool promote, bool demote, int? setLevel)
    {
        if (text is null) text = string.Empty;
        if (string.IsNullOrEmpty(text))
        {
            if (setLevel.HasValue)
                return new string('#', Math.Clamp(setLevel.Value, 1, 6));
            return text;
        }

        var lines = text.Split('\n');
        int first, last;
        if (start < 0)
        {
            first = 0;
            last = lines.Length - 1;
        }
        else
        {
            int s = Math.Min(Math.Max(start, 0), text.Length);
            int e = Math.Min(Math.Max(end, 0), text.Length);
            first = LineIndexFor(text, s);
            last = LineIndexFor(text, e);
        }
        if (first < 0) first = 0;
        if (last >= lines.Length) last = lines.Length - 1;
        if (first > last) { first = last; if (first < 0) first = 0; }
        if (last < 0) last = 0;

        for (int i = first; i <= last && i < lines.Length; i++)
        {
            lines[i] = TransformHeadingLine(lines[i], promote, demote, setLevel);
        }
        return string.Join("\n", lines);
    }

    private FormatOutcome TransformHeading(string? keyOrType, int start, int end, object? selection, int level = 0, bool promote = false, bool demote = false)
    {
        string typeKey = ResolveHeadingType(keyOrType, selection);

        bool isPromote = promote || IsPromoteKey(typeKey) || IsPromoteKey(keyOrType);
        bool isDemote = demote || IsDemoteKey(typeKey) || IsDemoteKey(keyOrType);

        if (isPromote || isDemote)
        {
            try
            {
                var txt = CurrentText();
                if (HasQuoteLinesInRange(txt, start, end) && !HasHeadingLinesInRange(txt, start, end))
                {
                    return TransformQuote(isPromote ? "promote quote" : "demote quote", start, end, selection, 0, isPromote, isDemote);
                }
            }
            catch { }
        }

        int? setLevel = null;
        if (!isPromote && !isDemote)
        {
            int? lvl = (level > 0 && level <= 6) ? level : null;
            if (!lvl.HasValue) lvl = ExtractHeadingLevel(typeKey);
            if (!lvl.HasValue) lvl = ExtractHeadingLevelFromSelection(selection);
            if (lvl.HasValue) setLevel = lvl.Value;
            else if (IsHeadingKey(typeKey) || IsHeadingKey(keyOrType)) setLevel = 1;
        }

        if (selection is not null)
            ResolveSelectionIndices(selection, ref start, ref end);

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        string newText;
        if (start >= 0 && end >= 0)
        {
            newText = TransformHeadingRange(text, start, end, isPromote, isDemote, setLevel);
        }
        else if (start >= 0)
        {
            newText = TransformHeadingRange(text, start, start, isPromote, isDemote, setLevel);
        }
        else
        {
            newText = TransformHeadingRange(text, -1, -1, isPromote, isDemote, setLevel);
        }

        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyHeading() => ApplyHeading(1);
    public FormatOutcome ApplyHeading(int level) { PropagateHeadingState("heading"); return TransformHeading("heading", -1, -1, null, level, false, false); }
    public FormatOutcome ApplyHeading(int level, int start, int end) { PropagateHeadingState("heading"); if (start >= 0 && end >= start) { } return TransformHeading("heading", start, end, null, level, false, false); }
    public FormatOutcome ApplyHeading(int level, int start, int end, int length)
    {
        PropagateHeadingState("heading");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformHeading("heading", start, e, null, level, false, false);
    }
    public FormatOutcome ApplyHeading(object? selection)
    {
        if (selection is int i) return ApplyHeading(i);
        if (selection is string s && int.TryParse(s.Trim(), out var n)) return ApplyHeading(n);
        PropagateHeadingState("heading");
        return TransformHeading("heading", -1, -1, selection, 1, false, false);
    }
    public FormatOutcome ApplyHeading(string? type, int start, int end) { PropagateHeadingState(type ?? "heading"); int? lvl = ExtractHeadingLevel(type); return TransformHeading(type ?? "heading", start, end, null, lvl ?? 1, false, false); }
    public FormatOutcome ApplyHeading(string? type, object? selection) { PropagateHeadingState(type ?? "heading"); int? lvl = ExtractHeadingLevel(type) ?? ExtractHeadingLevelFromSelection(selection); PropagateHeadingState(type ?? "heading"); return TransformHeading(type ?? "heading", -1, -1, selection, lvl ?? 1, false, false); }

    public FormatOutcome Heading() => Heading(1);
    public FormatOutcome Heading(int level) { PropagateHeadingState("heading"); return TransformHeading("heading", -1, -1, null, level, false, false); }
    public FormatOutcome Heading(int level, int start, int end) { PropagateHeadingState("heading"); return TransformHeading("heading", start, end, null, level, false, false); }
    public FormatOutcome Heading(int level, int start, int end, int length)
    {
        PropagateHeadingState("heading");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformHeading("heading", start, e, null, level, false, false);
    }
    public FormatOutcome Heading(object? selection)
    {
        if (selection is int i) return Heading(i);
        if (selection is string s && int.TryParse(s.Trim(), out var n) && n >= 1 && n <= 6) return Heading(n);
        PropagateHeadingState("heading");
        int? lvl = ExtractHeadingLevelFromSelection(selection);
        return TransformHeading("heading", -1, -1, selection, lvl ?? 1, false, false);
    }
    public FormatOutcome Heading(string? type) { int? lvl = ExtractHeadingLevel(type); return Heading(lvl ?? 1); }
    public FormatOutcome Heading(string? type, int start, int end) { int? lvl = ExtractHeadingLevel(type); PropagateHeadingState(type ?? "heading"); return TransformHeading(type ?? "heading", start, end, null, lvl ?? 1, false, false); }
    public FormatOutcome Heading(string? type, object? selection) { int? lvl = ExtractHeadingLevel(type) ?? ExtractHeadingLevelFromSelection(selection); PropagateHeadingState(type ?? "heading"); return TransformHeading(type ?? "heading", -1, -1, selection, lvl ?? 1, false, false); }
    public FormatOutcome Heading(object? selection, string? type) { int? lvl = ExtractHeadingLevel(type) ?? ExtractHeadingLevelFromSelection(selection); PropagateHeadingState(type ?? "heading"); return TransformHeading(type ?? "heading", -1, -1, selection, lvl ?? 1, false, false); }
    public FormatOutcome Heading(int start, int end) => Heading(1, start, end);

    public FormatOutcome Promote() { PropagateHeadingState("promote heading"); return TransformHeading("promote heading", -1, -1, null, 0, true, false); }
    public FormatOutcome Promote(int start, int end) { PropagateHeadingState("promote heading"); return TransformHeading("promote heading", start, end, null, 0, true, false); }
    public FormatOutcome Promote(int start, int end, int length)
    {
        PropagateHeadingState("promote heading");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformHeading("promote heading", start, e, null, 0, true, false);
    }
    public FormatOutcome Promote(object? selection) { PropagateHeadingState("promote heading"); return TransformHeading("promote heading", -1, -1, selection, 0, true, false); }
    public FormatOutcome PromoteHeading() => Promote();
    public FormatOutcome PromoteHeading(int start, int end) => Promote(start, end);
    public FormatOutcome PromoteHeading(object? selection) => Promote(selection);
    public FormatOutcome PromoteHeading(int levelIgnored, int start, int end) => Promote(start, end);

    public FormatOutcome Demote() { PropagateHeadingState("demote heading"); return TransformHeading("demote heading", -1, -1, null, 0, false, true); }
    public FormatOutcome Demote(int start, int end) { PropagateHeadingState("demote heading"); return TransformHeading("demote heading", start, end, null, 0, false, true); }
    public FormatOutcome Demote(int start, int end, int length)
    {
        PropagateHeadingState("demote heading");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformHeading("demote heading", start, e, null, 0, false, true);
    }
    public FormatOutcome Demote(object? selection) { PropagateHeadingState("demote heading"); return TransformHeading("demote heading", -1, -1, selection, 0, false, true); }
    public FormatOutcome DemoteHeading() => Demote();
    public FormatOutcome DemoteHeading(int start, int end) => Demote(start, end);
    public FormatOutcome DemoteHeading(object? selection) => Demote(selection);

    public FormatOutcome IncreaseHeading() => Promote();
    public FormatOutcome DecreaseHeading() => Demote();
    public FormatOutcome Increase() => Promote();
    public FormatOutcome Decrease() => Demote();

    public FormatOutcome Encabezado() => Heading();
    public FormatOutcome Encabezado(int level) => Heading(level);
    public FormatOutcome Encabezado(int level, int start, int end) => Heading(level, start, end);
    public FormatOutcome Encabezado(object? selection) => Heading(selection);
    public FormatOutcome Promover() => Promote();
    public FormatOutcome Promover(int start, int end) => Promote(start, end);
    public FormatOutcome Promover(object? selection) => Promote(selection);
    public FormatOutcome Degradar() => Demote();
    public FormatOutcome Degradar(int start, int end) => Demote(start, end);
    public FormatOutcome Degradar(object? selection) => Demote(selection);

    public FormatOutcome ApplyPromote() => Promote();
    public FormatOutcome ApplyPromote(int start, int end) => Promote(start, end);
    public FormatOutcome ApplyPromote(object? selection) => Promote(selection);
    public FormatOutcome ApplyDemote() => Demote();
    public FormatOutcome ApplyDemote(int start, int end) => Demote(start, end);
    public FormatOutcome ApplyDemote(object? selection) => Demote(selection);
    public FormatOutcome ApplyPromoteHeading() => Promote();
    public FormatOutcome ApplyDemoteHeading() => Demote();
    public FormatOutcome ApplyIncreaseHeading() => Promote();
    public FormatOutcome ApplyDecreaseHeading() => Demote();

    public FormatOutcome H1() => Heading(1);
    public FormatOutcome H2() => Heading(2);
    public FormatOutcome H3() => Heading(3);
    public FormatOutcome H4() => Heading(4);
    public FormatOutcome H5() => Heading(5);
    public FormatOutcome H6() => Heading(6);
    public FormatOutcome Heading1() => Heading(1);
    public FormatOutcome Heading2() => Heading(2);
    public FormatOutcome Heading3() => Heading(3);
    public FormatOutcome Heading4() => Heading(4);
    public FormatOutcome Heading5() => Heading(5);
    public FormatOutcome Heading6() => Heading(6);

    public FormatOutcome SetHeading(int level) => Heading(level);
    public FormatOutcome SetHeadingLevel(int level) => Heading(level);
    public FormatOutcome SetLevel(int level) => Heading(level);

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyHeading(object? input, int level = 1)
    {
        var engine = new FormatEngine(input);
        return engine.Heading(level);
    }
    public static FormatOutcome Heading(object? input, int level = 1)
    {
        var engine = new FormatEngine(input);
        return engine.Heading(level);
    }
}

// Extension methods
public static class HeadingExtensions
{
    public static FormatOutcome ApplyHeading(this MarkdownAst? ast, int level = 1, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Heading(level) : new FormatEngine(ast).Heading(level, start, end);
    public static FormatOutcome Heading(this MarkdownAst? ast, int level = 1, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Heading(level) : new FormatEngine(ast).Heading(level, start, end);
    public static FormatOutcome Promote(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Promote() : new FormatEngine(ast).Promote(start, end);
    public static FormatOutcome Demote(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Demote() : new FormatEngine(ast).Demote(start, end);
    public static FormatOutcome ApplyHeading(this OpenDocument? doc, int level = 1) => new FormatEngine(doc).Heading(level);
    public static FormatOutcome Heading(this OpenDocument? doc, int level = 1) => new FormatEngine(doc).Heading(level);
    public static FormatOutcome Promote(this OpenDocument? doc) => new FormatEngine(doc).Promote();
    public static FormatOutcome Demote(this OpenDocument? doc) => new FormatEngine(doc).Demote();
    public static FormatOutcome ApplyHeading(this string? md, int level = 1) => new FormatEngine(md).Heading(level);
    public static FormatOutcome Heading(this string? md, int level = 1) => new FormatEngine(md).Heading(level);
    public static FormatOutcome Promote(this string? md) => new FormatEngine(md).Promote();
    public static FormatOutcome Demote(this string? md) => new FormatEngine(md).Demote();
}