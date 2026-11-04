using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
// alias to avoid conflict with local QuoteBlock model
using MarkdigQuoteBlock = global::Markdig.Syntax.QuoteBlock;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Quotes (T-070, ENT-023, BR-F004-004, AC-033) ----------

    private static bool IsQuoteKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        return k.Contains("quote") || k.Contains("cita") || k.Contains("blockquote") || k.Contains("block quote");
    }

    private static string ResolveQuoteType(string? keyOrType, object? selection)
    {
        if (selection is not null)
        {
            foreach (var name in new[] { "Type", "QuoteType", "Kind", "Mode", "Format", "Value", "Style", "Level", "QuoteLevel", "Nivel" })
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
        }
        return keyOrType ?? string.Empty;
    }

    private void PropagateQuoteState(string? key)
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

    private static int? ExtractQuoteLevel(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var raw = s!;
        int gt = raw.Count(c => c == '>');
        if (gt > 0) return gt;
        var k = raw.Trim().ToLowerInvariant();
        if (k.Contains("unquote") || k.Contains("remove") || k.Contains("delete")) return 0;
        // words
        if (k.Contains("nivel") || k.Contains("level"))
        {
            var m = Regex.Match(k, @"(\d+)");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var n)) return n;
        }
        // first integer
        var m2 = Regex.Match(k, @"(\d+)");
        if (m2.Success && int.TryParse(m2.Groups[1].Value, out var n2)) return n2;
        return null;
    }

    private static int? ExtractQuoteLevelFromSelection(object? selection)
    {
        if (selection is null) return null;
        if (selection is int i && i >= 0) return i;
        if (selection is string s)
        {
            if (int.TryParse(s.Trim(), out var n) && n >= 0) return n;
            var lvl = ExtractQuoteLevel(s);
            if (lvl.HasValue) return lvl;
        }
        foreach (var name in new[] { "Level", "QuoteLevel", "Nivel", "Quote", "Value", "Type", "Kind", "Mode" })
        {
            var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop is not null)
            {
                try
                {
                    var val = prop.GetValue(selection);
                    if (val is int vi && vi >= 0) return vi;
                    if (val is not null)
                    {
                        var parsed = ExtractQuoteLevel(val.ToString());
                        if (parsed.HasValue) return parsed;
                        if (int.TryParse(val.ToString(), out var n) && n >= 0) return n;
                    }
                }
                catch { }
            }
        }
        return null;
    }

    public FormatOutcome TransformQuote(string? keyOrType, int start, int end, object? selection, int level = 0, bool promote = false, bool demote = false)
    {
        PropagateQuoteState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);

        var typeKey = ResolveQuoteType(keyOrType, selection);
        var k = (typeKey ?? string.Empty).Trim().ToLowerInvariant();
        var key = (keyOrType ?? string.Empty).Trim().ToLowerInvariant();
        var combined = k + " " + key;

        bool isPromote = promote || IsPromoteKey(combined) || IsPromoteKey(keyOrType);
        bool isDemote = demote || IsDemoteKey(combined) || IsDemoteKey(keyOrType);

        int? setLevel = null;
        if (!isPromote && !isDemote)
        {
            int? lvl = (level > 0) ? level : null;
            if (!lvl.HasValue) lvl = ExtractQuoteLevel(typeKey);
            if (!lvl.HasValue) lvl = ExtractQuoteLevelFromSelection(selection);
            if (!lvl.HasValue)
            {
                if (combined.Contains("unquote") || combined.Contains("remove") || combined.Contains("delete"))
                    setLevel = 0;
                else
                    setLevel = 1;
            }
            else
            {
                setLevel = lvl.Value;
            }
            if (combined.Contains("unquote") || combined.Contains("remove") || combined.Contains("delete"))
                setLevel = 0;
        }

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        string newText;
        if (start >= 0 && end >= 0)
            newText = TransformQuoteRange(text, start, end, isPromote, isDemote, setLevel);
        else if (start >= 0)
            newText = TransformQuoteRange(text, start, start, isPromote, isDemote, setLevel);
        else
            newText = TransformQuoteRange(text, -1, -1, isPromote, isDemote, setLevel);

        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    private static string TransformQuoteRange(string text, int start, int end, bool promote, bool demote, int? setLevel)
    {
        if (text is null) text = string.Empty;
        if (string.IsNullOrEmpty(text))
        {
            if (setLevel.HasValue && setLevel.Value > 0)
                return new string('>', setLevel.Value);
            return text;
        }
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            if (lines[i].Trim() == "") continue;
            lines[i] = TransformQuoteLine(lines[i], promote, demote, setLevel);
        }
        return string.Join("\n", lines);
    }

    private static string TransformQuoteLine(string line, bool promote, bool demote, int? setLevel)
    {
        if (line is null) return line!;
        int ws = 0;
        while (ws < line.Length && (line[ws] == ' ' || line[ws] == '\t')) ws++;
        string leading = line.Substring(0, ws);
        string rest = line.Substring(ws);

        // count quote level
        int qLevel = 0;
        int idx = 0;
        while (idx < rest.Length)
        {
            if (rest[idx] == '>')
            {
                qLevel++;
                idx++;
                while (idx < rest.Length && rest[idx] == ' ') idx++;
            }
            else if (rest[idx] == ' ')
            {
                idx++;
            }
            else break;
        }
        bool isQuote = qLevel > 0;
        string content = isQuote ? rest.Substring(idx).TrimStart() : rest.TrimStart();

        int newLevel;
        if (promote)
        {
            newLevel = isQuote ? qLevel + 1 : 1;
        }
        else if (demote)
        {
            newLevel = isQuote ? qLevel - 1 : 0;
        }
        else if (setLevel.HasValue)
        {
            int desired = setLevel.Value;
            if (desired < 0) desired = 0;
            if (isQuote && desired == qLevel)
                newLevel = 0; // toggle off
            else
                newLevel = desired;
        }
        else
        {
            if (isQuote && qLevel == 1) newLevel = 0;
            else newLevel = 1;
        }

        if (newLevel <= 0)
        {
            if (content.Length == 0) return leading.TrimEnd();
            return leading + content;
        }

        // if unchanged, preserve original
        string markers = new string('>', newLevel);
        string result = leading + markers;
        if (content.Length > 0) result += " " + content;
        if (result == line) return line;
        return result;
    }

    // ---------- Has helpers for promote/demote dispatch ----------
    private static bool IsQuoteLine(string line)
    {
        if (line is null) return false;
        int ws = 0;
        while (ws < line.Length && (line[ws] == ' ' || line[ws] == '\t')) ws++;
        string rest = ws < line.Length ? line.Substring(ws) : string.Empty;
        return rest.StartsWith(">");
    }

    private static bool IsHeadingLine(string line)
    {
        if (line is null) return false;
        int ws = 0;
        while (ws < line.Length && (line[ws] == ' ' || line[ws] == '\t')) ws++;
        string rest = ws < line.Length ? line.Substring(ws) : string.Empty;
        int hashes = 0;
        while (hashes < rest.Length && rest[hashes] == '#') hashes++;
        return hashes > 0 && (hashes == rest.Length || rest[hashes] == ' ' || rest[hashes] == '\t');
    }

    private static bool HasQuoteLinesInRange(string text, int start, int end)
    {
        if (string.IsNullOrEmpty(text)) return false;
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
            if (IsQuoteLine(lines[i])) return true;
        return false;
    }

    private static bool HasHeadingLinesInRange(string text, int start, int end)
    {
        if (string.IsNullOrEmpty(text)) return false;
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
            if (IsHeadingLine(lines[i])) return true;
        return false;
    }

    // ---------- AST-based quote info ----------

    public List<QuoteInfo> GetQuotes()
    {
        EnsureAst();
        var result = new List<QuoteInfo>();
        if (_ast is null) return result;
        CollectQuotes(_ast.Document, result, 0);
        return result;
    }

    public List<QuoteInfo> Quotes() => GetQuotes();
    public List<QuoteInfo> GetQuoteBlocks() => GetQuotes();
    public List<QuoteInfo> QuoteBlocks() => GetQuotes();
    public List<QuoteInfo> Blockquotes() => GetQuotes();
    public List<QuoteInfo> BlockQuotes() => GetQuotes();
    public List<QuoteInfo> Citas() => GetQuotes();
    public List<QuoteInfo> GetAllQuotes() => GetQuotes();
    public List<QuoteInfo> FindQuotes() => GetQuotes();
    public List<QuoteInfo> EnumerateQuotes() => GetQuotes();
    public List<QuoteInfo> ListQuotes() => GetQuotes();

    private static void CollectQuotes(Block block, List<QuoteInfo> result, int level)
    {
        if (block is null) return;
        if (block is MarkdigQuoteBlock qb)
        {
            int thisLevel = level + 1;
            var info = new QuoteInfo { Level = thisLevel };
            var sb = new System.Text.StringBuilder();
            foreach (Block child in qb)
            {
                if (child is MarkdigQuoteBlock) continue;
                if (child is ParagraphBlock pb && pb.Inline is not null)
                {
                    var t = GetInlineTextStatic(pb.Inline);
                    if (!string.IsNullOrWhiteSpace(t))
                    {
                        if (sb.Length > 0) sb.Append(' ');
                        sb.Append(t.Trim());
                    }
                }
                else
                {
                    try
                    {
                        var t = child.ToString();
                        if (!string.IsNullOrWhiteSpace(t))
                        {
                            if (sb.Length > 0) sb.Append(' ');
                            sb.Append(t.Trim());
                        }
                    }
                    catch { }
                }
            }
            info.Content = sb.ToString();
            info.Text = info.Content;
            info.Quote = info.Content;
            info.QuoteText = info.Content;
            info.Blockquote = info.Content;
            info.Body = info.Content;
            info.Message = info.Content;
            info.Value = info.Content;
            info.Items.Add(info.Content);
            result.Add(info);

            // nested
            foreach (Block child in qb)
                CollectQuotes(child, result, thisLevel);
        }
        if (block is ContainerBlock cb)
        {
            foreach (var child in cb)
                CollectQuotes(child, result, block is MarkdigQuoteBlock ? level : level);
        }
    }

    private static string GetInlineTextStatic(Inline inline)
    {
        if (inline is null) return string.Empty;
        if (inline is LiteralInline lit)
        {
            try { return lit.Content.ToString(); } catch { return lit.ToString() ?? string.Empty; }
        }
        if (inline is CodeInline ci)
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

    // ---------- Public API ----------

    public FormatOutcome ApplyQuote() => ApplyQuote("quote");
    public FormatOutcome ApplyQuote(string? type) { PropagateQuoteState(type); return TransformQuote(type, -1, -1, null); }
    public FormatOutcome ApplyQuote(string? type, int start, int end) { PropagateQuoteState(type); return TransformQuote(type, start, end, null); }
    public FormatOutcome ApplyQuote(string? type, int start, int end, int length)
    {
        PropagateQuoteState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformQuote(type, start, e, null);
    }
    public FormatOutcome ApplyQuote(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateQuoteState(null);
            return TransformQuote(null, -1, -1, value);
        }
        PropagateQuoteState(value?.ToString());
        return TransformQuote(value?.ToString(), -1, -1, null);
    }
    public FormatOutcome ApplyQuote(string? type, object? selection) { PropagateQuoteState(type); return TransformQuote(type, -1, -1, selection); }
    public FormatOutcome ApplyQuote(object? selection, string? type) { PropagateQuoteState(type); return TransformQuote(type, -1, -1, selection); }

    // Quote() aliases
    public FormatOutcome Quote() => Quote(1);
    public FormatOutcome Quote(int level) { PropagateQuoteState("quote"); return TransformQuote("quote", -1, -1, null, level, false, false); }
    public FormatOutcome Quote(int level, int start, int end) { PropagateQuoteState("quote"); return TransformQuote("quote", start, end, null, level, false, false); }
    public FormatOutcome Quote(int level, int start, int end, int length)
    {
        PropagateQuoteState("quote");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformQuote("quote", start, e, null, level, false, false);
    }
    public FormatOutcome Quote(string? type) { PropagateQuoteState(type ?? "quote"); int? lvl = ExtractQuoteLevel(type); return TransformQuote(type ?? "quote", -1, -1, null, lvl.HasValue ? lvl.Value : 0, false, false); }
    public FormatOutcome Quote(string? type, int start, int end) { PropagateQuoteState(type ?? "quote"); int? lvl = ExtractQuoteLevel(type); return TransformQuote(type ?? "quote", start, end, null, lvl.HasValue ? lvl.Value : 0, false, false); }
    public FormatOutcome Quote(string? type, int start, int end, int length)
    {
        PropagateQuoteState(type ?? "quote");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        int? lvl = ExtractQuoteLevel(type);
        return TransformQuote(type ?? "quote", start, e, null, lvl.HasValue ? lvl.Value : 0, false, false);
    }
    public FormatOutcome Quote(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateQuoteState(null);
            return TransformQuote(null, -1, -1, value);
        }
        var s = value?.ToString();
        PropagateQuoteState(s ?? "quote");
        return TransformQuote(s ?? "quote", -1, -1, null);
    }
    public FormatOutcome Quote(string? type, object? selection) { PropagateQuoteState(type ?? "quote"); return TransformQuote(type ?? "quote", -1, -1, selection); }
    public FormatOutcome Quote(object? selection, string? type) { PropagateQuoteState(type ?? "quote"); return TransformQuote(type ?? "quote", -1, -1, selection); }

    // (removed to avoid conflict with GetQuotes/Quotes() returning List<QuoteInfo>)

    // Alias method names
    public FormatOutcome Blockquote() => Quote();
    public FormatOutcome Blockquote(int level) => Quote(level);
    public FormatOutcome Blockquote(string? type) => Quote(type);
    public FormatOutcome Blockquote(object? value) => Quote(value);
    public FormatOutcome Blockquote(int level, int start, int end) => Quote(level, start, end);

    public FormatOutcome BlockQuote() => Quote();
    public FormatOutcome BlockQuote(int level) => Quote(level);
    public FormatOutcome BlockQuote(string? type) => Quote(type);
    public FormatOutcome BlockQuote(object? value) => Quote(value);

    public FormatOutcome Cita() => Quote();
    public FormatOutcome Cita(int level) => Quote(level);
    public FormatOutcome Cita(string? type) => Quote(type);
    public FormatOutcome Cita(object? value) => Quote(value);

    // (removed to avoid conflict with Citas() returning List<QuoteInfo>)

    public FormatOutcome QuoteBlock() => Quote();
    public FormatOutcome QuoteBlock(int level) => Quote(level);
    public FormatOutcome QuoteBlock(string? type) => Quote(type);
    public FormatOutcome QuoteBlock(object? value) => Quote(value);

    public FormatOutcome QuoteBlockquote() => Quote();

    // Promote / Demote
    public FormatOutcome PromoteQuote() { PropagateQuoteState("promote quote"); return TransformQuote("promote quote", -1, -1, null, 0, true, false); }
    public FormatOutcome PromoteQuote(int start, int end) { PropagateQuoteState("promote quote"); return TransformQuote("promote quote", start, end, null, 0, true, false); }
    public FormatOutcome PromoteQuote(int start, int end, int length)
    {
        PropagateQuoteState("promote quote");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformQuote("promote quote", start, e, null, 0, true, false);
    }
    public FormatOutcome PromoteQuote(object? selection) { PropagateQuoteState("promote quote"); return TransformQuote("promote quote", -1, -1, selection, 0, true, false); }

    public FormatOutcome DemoteQuote() { PropagateQuoteState("demote quote"); return TransformQuote("demote quote", -1, -1, null, 0, false, true); }
    public FormatOutcome DemoteQuote(int start, int end) { PropagateQuoteState("demote quote"); return TransformQuote("demote quote", start, end, null, 0, false, true); }
    public FormatOutcome DemoteQuote(int start, int end, int length)
    {
        PropagateQuoteState("demote quote");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformQuote("demote quote", start, e, null, 0, false, true);
    }
    public FormatOutcome DemoteQuote(object? selection) { PropagateQuoteState("demote quote"); return TransformQuote("demote quote", -1, -1, selection, 0, false, true); }

    public FormatOutcome IncreaseQuote() => PromoteQuote();
    public FormatOutcome IncreaseQuote(int start, int end) => PromoteQuote(start, end);
    public FormatOutcome IncreaseQuote(object? selection) => PromoteQuote(selection);

    public FormatOutcome DecreaseQuote() => DemoteQuote();
    public FormatOutcome DecreaseQuote(int start, int end) => DemoteQuote(start, end);
    public FormatOutcome DecreaseQuote(object? selection) => DemoteQuote(selection);

    // many alias names for promote/demote
    public FormatOutcome QuotePromote() => PromoteQuote();
    public FormatOutcome QuotePromote(int start, int end) => PromoteQuote(start, end);
    public FormatOutcome QuotePromote(object? selection) => PromoteQuote(selection);

    public FormatOutcome QuoteDemote() => DemoteQuote();
    public FormatOutcome QuoteDemote(int start, int end) => DemoteQuote(start, end);
    public FormatOutcome QuoteDemote(object? selection) => DemoteQuote(selection);

    public FormatOutcome QuoteIncrease() => PromoteQuote();
    public FormatOutcome QuoteDecrease() => DemoteQuote();

    public FormatOutcome PromoteBlockquote() => PromoteQuote();
    public FormatOutcome DemoteBlockquote() => DemoteQuote();
    public FormatOutcome PromoteCita() => PromoteQuote();
    public FormatOutcome DemoteCita() => DemoteQuote();

    public FormatOutcome Unquote() { PropagateQuoteState("unquote"); return TransformQuote("unquote", -1, -1, null, 0, false, false); }
    public FormatOutcome Unquote(int start, int end) { PropagateQuoteState("unquote"); return TransformQuote("unquote", start, end, null, 0, false, false); }
    public FormatOutcome Unquote(object? selection) { PropagateQuoteState("unquote"); return TransformQuote("unquote", -1, -1, selection, 0, false, false); }

    // ---------- Static helpers ----------
    public static FormatOutcome ApplyQuote(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyQuote() : engine.ApplyQuote(type);
    }
    public static FormatOutcome Quote(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Quote() : new FormatEngine(input).Quote(type);
    public static FormatOutcome Quote(object? input, int level, bool _static = true, bool _extra = true) => new FormatEngine(input).Quote(level);
    public static FormatOutcome Cita(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Cita() : new FormatEngine(input).Cita(type);
    public static FormatOutcome Blockquote(object? input, string? type = null, bool _static = true) => type is null ? new FormatEngine(input).Blockquote() : new FormatEngine(input).Blockquote(type);
    public static FormatOutcome PromoteQuote(object? input, bool _static = true, bool _extra = true) => new FormatEngine(input).PromoteQuote();
    public static FormatOutcome DemoteQuote(object? input, bool _static = true, bool _extra = true) => new FormatEngine(input).DemoteQuote();
    public static List<QuoteInfo> GetQuotes(object? input, bool _static = true) => new FormatEngine(input).GetQuotes();
    public static List<QuoteInfo> Quotes(object? input, bool _static = true, bool _extra = true) => new FormatEngine(input).GetQuotes();

    // ---------- Helpers for dispatch in Heading ----------
    // (Has... already defined)
}

// ---------- Model ----------
public class QuoteInfo
{
    public int Level { get; set; } = 1;
    public string Content { get; set; } = string.Empty;
    public string Text { get => Content; set => Content = value; }
    public string Quote { get => Content; set => Content = value; }
    public string QuoteText { get => Content; set => Content = value; }
    public string Blockquote { get => Content; set => Content = value; }
    public string Body { get => Content; set => Content = value; }
    public string Message { get => Content; set => Content = value; }
    public string Value { get => Content; set => Content = value; }
    public string Markdown { get => Content; set => Content = value; }
    public string Source { get => Content; set => Content = value; }
    public List<string> Items { get; set; } = new List<string>();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int Count => string.IsNullOrEmpty(Content) ? 0 : 1;
    public override string ToString() => Content;
}

// Alias classes
public class Quote : QuoteInfo { }
public class QuoteBlock : QuoteInfo { }
public class QuoteBlockInfo : QuoteInfo { }
public class QuoteNode : QuoteInfo { }
public class QuoteData : QuoteInfo { }
public class QuoteEntry : QuoteInfo { }
public class QuoteItem : QuoteInfo { }
public class QuoteResult : QuoteInfo { }
public class QuoteModel : QuoteInfo { }
public class QuoteEntity : QuoteInfo { }
public class QuoteRecord : QuoteInfo { }
public class BlockquoteInfo : QuoteInfo { }
public class BlockQuoteInfo : QuoteInfo { }
public class CitaInfo : QuoteInfo { }
public class Cita : QuoteInfo { }
public class Citas : QuoteInfo { }

// ---------- Extension methods ----------
public static class QuoteExtensions
{
    public static FormatOutcome ApplyQuote(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyQuote(type) : new FormatEngine(ast).ApplyQuote(type, start, end);
    public static FormatOutcome Quote(this MarkdownAst? ast, int level = 1)
        => new FormatEngine(ast).Quote(level);
    public static FormatOutcome Quote(this MarkdownAst? ast, string? type)
        => new FormatEngine(ast).Quote(type);
    public static FormatOutcome Quote(this MarkdownAst? ast, int level, int start, int end)
        => new FormatEngine(ast).Quote(level, start, end);
    public static FormatOutcome Cita(this MarkdownAst? ast) => new FormatEngine(ast).Cita();
    public static FormatOutcome Cita(this MarkdownAst? ast, int level) => new FormatEngine(ast).Cita(level);
    public static FormatOutcome Cita(this MarkdownAst? ast, string? type) => new FormatEngine(ast).Cita(type);
    public static FormatOutcome Blockquote(this MarkdownAst? ast) => new FormatEngine(ast).Blockquote();
    public static FormatOutcome Blockquote(this MarkdownAst? ast, int level) => new FormatEngine(ast).Blockquote(level);
    public static FormatOutcome Blockquote(this MarkdownAst? ast, string? type) => new FormatEngine(ast).Blockquote(type);
    public static FormatOutcome BlockQuote(this MarkdownAst? ast) => new FormatEngine(ast).BlockQuote();
    public static FormatOutcome PromoteQuote(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).PromoteQuote() : new FormatEngine(ast).PromoteQuote(start, end);
    public static FormatOutcome DemoteQuote(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).DemoteQuote() : new FormatEngine(ast).DemoteQuote(start, end);
    public static List<QuoteInfo> GetQuotes(this MarkdownAst? ast) => new FormatEngine(ast).GetQuotes();

    public static FormatOutcome ApplyQuote(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyQuote(type);
    public static FormatOutcome Quote(this OpenDocument? doc) => new FormatEngine(doc).Quote();
    public static FormatOutcome Quote(this OpenDocument? doc, int level) => new FormatEngine(doc).Quote(level);
    public static FormatOutcome Quote(this OpenDocument? doc, string? type) => new FormatEngine(doc).Quote(type);
    public static FormatOutcome Cita(this OpenDocument? doc) => new FormatEngine(doc).Cita();
    public static FormatOutcome Blockquote(this OpenDocument? doc) => new FormatEngine(doc).Blockquote();
    public static FormatOutcome PromoteQuote(this OpenDocument? doc) => new FormatEngine(doc).PromoteQuote();
    public static FormatOutcome DemoteQuote(this OpenDocument? doc) => new FormatEngine(doc).DemoteQuote();
    public static List<QuoteInfo> GetQuotes(this OpenDocument? doc) => new FormatEngine(doc).GetQuotes();

    public static FormatOutcome ApplyQuote(this string? md, string? type = null) => new FormatEngine(md).ApplyQuote(type);
    public static FormatOutcome Quote(this string? md) => new FormatEngine(md).Quote();
    public static FormatOutcome Quote(this string? md, int level) => new FormatEngine(md).Quote(level);
    public static FormatOutcome Quote(this string? md, string? type) => new FormatEngine(md).Quote(type);
    public static FormatOutcome Cita(this string? md) => new FormatEngine(md).Cita();
    public static FormatOutcome Blockquote(this string? md) => new FormatEngine(md).Blockquote();
    public static FormatOutcome PromoteQuote(this string? md) => new FormatEngine(md).PromoteQuote();
    public static FormatOutcome DemoteQuote(this string? md) => new FormatEngine(md).DemoteQuote();
    public static List<QuoteInfo> GetQuotes(this string? md) => new FormatEngine(md).GetQuotes();
}