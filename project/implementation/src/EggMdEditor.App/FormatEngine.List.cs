using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace EggMdEditor.App;

public partial class FormatEngine
{
    // ---------- Lists (T-069, ENT-022, OP-027/028) ----------

    private static bool IsListKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("list") || k.Contains("lista") || k.Contains("bullet") || k.Contains("viñet")
            || k.Contains("vinet") || k.Contains("viñeta") || k.Contains("numerad") || k.Contains("numbered")
            || k.Contains("ordered") || k.Contains("task") || k.Contains("tarea") || k.Contains("check")
            || k.Contains("todo") || k.Contains("anidar") || k.Contains("nest") || k.Contains("desanidar")
            || k.Contains("unnest") || k.Contains("indent") || k.Contains("outdent") || k.Contains("renumber")
            || k.Contains("renumer") || k.Contains("toggle"))
            return true;
        return false;
    }

    private static string ResolveListType(string? keyOrType, object? selection)
    {
        if (selection is not null)
        {
            foreach (var name in new[] { "Type", "ListType", "Kind", "Mode", "Format", "Value", "Style" })
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

    private void PropagateListState(string? key)
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

    // ---------- Core transform ----------

    public FormatOutcome TransformList(string? keyOrType, int start, int end, object? selection)
    {
        PropagateListState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);

        var typeKey = ResolveListType(keyOrType, selection);
        var k = (typeKey ?? string.Empty).Trim().ToLowerInvariant();
        var key = (keyOrType ?? string.Empty).Trim().ToLowerInvariant();
        var combined = k + " " + key;

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        string newText;
        if (combined.Contains("unnest") || combined.Contains("desanidar") || combined.Contains("outdent") || combined.Contains("decreaseindent") || combined.Contains("bajar nivel") || combined.Contains("unindent"))
        {
            newText = ApplyUnnest(text, start, end);
        }
        else if (combined.Contains("nest") || combined.Contains("anidar") || combined.Contains("indent") || combined.Contains("increaseindent"))
        {
            newText = ApplyNest(text, start, end);
        }
        else if (combined.Contains("toggle") || combined.Contains("altern") || combined.Contains("switch"))
        {
            newText = ApplyToggleCheckbox(text, start, end);
        }
        else if (combined.Contains("uncheck") || combined.Contains("desmarcar"))
        {
            newText = ApplySetCheckbox(text, start, end, false);
        }
        else if (combined.Contains("check") && !combined.Contains("checkbox"))
        {
            // "check" alone -> check all
            newText = ApplySetCheckbox(text, start, end, true);
        }
        else if (combined.Contains("renumber") || combined.Contains("renumer"))
        {
            newText = RenumberNumberedLists(text);
        }
        else if (combined.Contains("number") || combined.Contains("numer") || combined.Contains("ordered") || combined.Contains("ordinal"))
        {
            newText = ApplyNumbered(text, start, end);
        }
        else if (combined.Contains("task") || combined.Contains("tarea") || combined.Contains("check") || combined.Contains("todo"))
        {
            newText = ApplyTask(text, start, end);
        }
        else
        {
            // default: bullet
            newText = ApplyBullet(text, start, end);
        }

        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Line transforms ----------

    private static int LeadingSpaces(string line)
    {
        int i = 0;
        while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
        {
            if (line[i] == '\t') i += 4; else i++;
        }
        // recalc exact spaces count (tabs counted as 4 but string has tab) -> return index in chars
        int c = 0;
        while (c < line.Length && (line[c] == ' ' || line[c] == '\t')) c++;
        return c;
    }

    private static string IndentString(int spaces) => new string(' ', spaces);

    private static string StripMarkerContent(string line)
    {
        int ws = 0;
        while (ws < line.Length && (line[ws] == ' ' || line[ws] == '\t')) ws++;
        string rest = line.Substring(ws);
        // task
        if (rest.StartsWith("- [ ]", StringComparison.OrdinalIgnoreCase) ||
            rest.StartsWith("- [x]", StringComparison.OrdinalIgnoreCase))
        {
            rest = rest.Substring(5);
            if (rest.StartsWith(" ")) rest = rest.Substring(1);
        }
        else if (rest.StartsWith("* [ ]", StringComparison.OrdinalIgnoreCase) ||
                 rest.StartsWith("* [x]", StringComparison.OrdinalIgnoreCase))
        {
            rest = rest.Substring(5);
            if (rest.StartsWith(" ")) rest = rest.Substring(1);
        }
        else if (rest.StartsWith("+ [ ]", StringComparison.OrdinalIgnoreCase) ||
                 rest.StartsWith("+ [x]", StringComparison.OrdinalIgnoreCase))
        {
            rest = rest.Substring(5);
            if (rest.StartsWith(" ")) rest = rest.Substring(1);
        }
        // bullet markers
        else if (rest.StartsWith("- ") || rest.StartsWith("* ") || rest.StartsWith("+ "))
        {
            rest = rest.Substring(2);
        }
        else
        {
            // numbered
            var m = Regex.Match(rest, @"^\d+[\.\)]\s*");
            if (m.Success) rest = rest.Substring(m.Length);
        }
        return rest.TrimStart();
    }

    private static (int first, int last) RangeLines(string text, int start, int end)
    {
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
        if (lines.Length == 0) { first = 0; last = -1; }
        if (first > last && lines.Length > 0) { first = last; if (first < 0) first = 0; }
        if (last < 0 && lines.Length > 0) last = 0;
        return (first, last);
    }

    private static string ApplyBullet(string text, int start, int end)
    {
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            if (lines[i].Trim() == "") continue;
            int indent = LeadingSpaces(lines[i]);
            string content = StripMarkerContent(lines[i]);
            lines[i] = new string(' ', indent) + "- " + content;
        }
        return string.Join("\n", lines);
    }

    private static string ApplyNumbered(string text, int start, int end)
    {
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            if (lines[i].Trim() == "") continue;
            int indent = LeadingSpaces(lines[i]);
            string content = StripMarkerContent(lines[i]);
            lines[i] = new string(' ', indent) + "1. " + content;
        }
        var result = string.Join("\n", lines);
        return RenumberNumberedLists(result);
    }

    private static string ApplyTask(string text, int start, int end)
    {
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            if (lines[i].Trim() == "") continue;
            int indent = LeadingSpaces(lines[i]);
            string content = StripMarkerContent(lines[i]);
            lines[i] = new string(' ', indent) + "- [ ] " + content;
        }
        return string.Join("\n", lines);
    }

    private static string ApplyNest(string text, int start, int end)
    {
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            if (lines[i].Trim() == "") continue;
            lines[i] = "  " + lines[i];
        }
        var result = string.Join("\n", lines);
        return RenumberNumberedLists(result);
    }

    private static string ApplyUnnest(string text, int start, int end)
    {
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            if (lines[i].StartsWith("  ")) lines[i] = lines[i].Substring(2);
            else if (lines[i].StartsWith("\t")) lines[i] = lines[i].Substring(1);
            else if (lines[i].StartsWith(" "))
            {
                // remove up to 4 spaces
                int remove = 0;
                while (remove < 4 && remove < lines[i].Length && lines[i][remove] == ' ') remove++;
                if (remove > 0) lines[i] = lines[i].Substring(remove);
            }
        }
        var result = string.Join("\n", lines);
        return RenumberNumberedLists(result);
    }

    private static string ApplyToggleCheckbox(string text, int start, int end)
    {
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            lines[i] = Regex.Replace(lines[i], @"-\s*\[\s*\]", "- [x]", RegexOptions.IgnoreCase);
            // toggle x back: need track original; do two-pass carefully
        }
        // The above only checks. Need proper toggle: detect state first.
        // Redo properly:
        var lines2 = text.Split('\n');
        var (f, l) = RangeLines(text, start, end);
        for (int i = f; i <= l && i < lines2.Length && i >= 0; i++)
        {
            if (Regex.IsMatch(lines2[i], @"-\s*\[\s*x\s*\]", RegexOptions.IgnoreCase))
            {
                lines2[i] = Regex.Replace(lines2[i], @"-\s*\[\s*x\s*\]", "- [ ]", RegexOptions.IgnoreCase);
            }
            else if (Regex.IsMatch(lines2[i], @"-\s*\[\s*\]"))
            {
                lines2[i] = Regex.Replace(lines2[i], @"-\s*\[\s*\]", "- [X]");
            }
        }
        return string.Join("\n", lines2);
    }

    private static string ApplySetCheckbox(string text, int start, int end, bool check)
    {
        var lines = text.Split('\n');
        var (first, last) = RangeLines(text, start, end);
        string replacement = check ? "- [X]" : "- [ ]";
        for (int i = first; i <= last && i < lines.Length && i >= 0; i++)
        {
            if (Regex.IsMatch(lines[i], @"-\s*\[\s*[xX ]?\s*\]"))
            {
                lines[i] = Regex.Replace(lines[i], @"-\s*\[\s*[xX ]?\s*\]", replacement, RegexOptions.IgnoreCase);
            }
        }
        return string.Join("\n", lines);
    }

    public static string RenumberNumberedLists(string text)
    {
        if (text is null) return string.Empty;
        var lines = text.Split('\n');
        var counters = new Dictionary<int, int>();
        int prevIndent = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "")
            {
                counters.Clear();
                prevIndent = -1;
                continue;
            }
            int indent = LeadingSpaces(lines[i]);
            var m = Regex.Match(lines[i].TrimStart(), @"^(\d+)[\.\)]\s*(.*)$");
            if (m.Success)
            {
                // clear deeper counters
                foreach (var key in counters.Keys.Where(k => k > indent).ToList())
                    counters.Remove(key);

                if (!counters.ContainsKey(indent))
                    counters[indent] = 0;

                if (indent > prevIndent)
                    counters[indent] = 1;
                else
                    counters[indent] = counters[indent] + 1;

                string rest = m.Groups[2].Value;
                lines[i] = new string(' ', indent) + counters[indent] + ". " + rest;
            }
            else
            {
                counters.Clear();
            }
            prevIndent = indent;
        }
        return string.Join("\n", lines);
    }

    // ---------- AST-based list info ----------

    public List<ListInfo> GetLists()
    {
        EnsureAst();
        var result = new List<ListInfo>();
        if (_ast is null) return result;
        CollectLists(_ast.Document, result, 0);
        return result;
    }

    public List<ListInfo> Lists() => GetLists();
    public List<ListInfo> ListBlocks() => GetLists();
    public List<ListInfo> GetAllLists() => GetLists();
    public List<ListInfo> EnumerateLists() => GetLists();
    public List<ListInfo> FindLists() => GetLists();

    public List<string> GetListItems()
    {
        var items = new List<string>();
        foreach (var l in GetLists())
            items.AddRange(l.Items);
        return items;
    }
    public List<string> ListItems() => GetListItems();
    public List<string> GetAllListItems() => GetListItems();

    private static void CollectLists(Block block, List<ListInfo> result, int level)
    {
        if (block is null) return;
        if (block is ListBlock lb)
        {
            var info = new ListInfo
            {
                Level = level,
                IsOrdered = lb.IsOrdered,
                Type = lb.IsOrdered ? "Numerada" : (IsTaskList(lb) ? "Tareas" : "Viñetas"),
                BulletType = lb.BulletType,
                OrderedStart = lb.OrderedStart,
                StartLine = lb.Line,
                EndLine = lb.Line,
            };
            foreach (ListItemBlock item in lb)
            {
                info.Items.Add(GetListItemText(item));
                info.TaskStates.Add(GetTaskState(item));
            }
            result.Add(info);
        }
        if (block is ContainerBlock cb)
        {
            foreach (var child in cb)
                CollectLists(child, result, block is ListBlock ? level + 1 : level);
        }
    }

    private static bool IsTaskList(ListBlock lb)
    {
        foreach (ListItemBlock item in lb)
        {
            foreach (Block child in item)
            {
                if (child is ParagraphBlock pb && pb.Inline is not null)
                {
                    foreach (Inline inl in pb.Inline)
                    {
                        if (inl is TaskList) return true;
                    }
                }
            }
        }
        return false;
    }

    private static string GetListItemText(ListItemBlock item)
    {
        var sb = new System.Text.StringBuilder();
        foreach (Block child in item)
        {
            if (child is ParagraphBlock pb && pb.Inline is not null)
            {
                foreach (Inline inl in pb.Inline)
                {
                    if (inl is TaskList) continue;
                    sb.Append(inl.ToString());
                }
            }
            else
            {
                try { sb.Append(child.ToString()); } catch { }
            }
        }
        return sb.ToString().Trim();
    }

    private static bool? GetTaskState(ListItemBlock item)
    {
        foreach (Block child in item)
        {
            if (child is ParagraphBlock pb && pb.Inline is not null)
            {
                foreach (Inline inl in pb.Inline)
                {
                    if (inl is TaskList tl)
                    {
                        try { return tl.Checked; } catch { }
                    }
                }
            }
        }
        return null;
    }

    // ---------- Public API (instance) ----------

    public FormatOutcome ApplyList() => ApplyList("bullet");
    public FormatOutcome ApplyList(string? type) { PropagateListState(type); return TransformList(type, -1, -1, null); }
    public FormatOutcome ApplyList(string? type, int start, int end) { PropagateListState(type); return TransformList(type, start, end, null); }
    public FormatOutcome ApplyList(string? type, int start, int end, int length)
    {
        PropagateListState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformList(type, start, e, null);
    }
    public FormatOutcome ApplyList(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateListState(null);
            return TransformList(null, -1, -1, value);
        }
        PropagateListState(value?.ToString());
        return TransformList(value?.ToString(), -1, -1, null);
    }
    public FormatOutcome ApplyList(string? type, object? selection) { PropagateListState(type); return TransformList(type, -1, -1, selection); }
    public FormatOutcome ApplyList(object? selection, string? type) { PropagateListState(type); return TransformList(type, -1, -1, selection); }

    // List() aliases
    public FormatOutcome List() => Bullet();
    public FormatOutcome List(string? type) => ApplyList(type);
    public FormatOutcome List(string? type, int start, int end) => ApplyList(type, start, end);
    public FormatOutcome List(object? value) => ApplyList(value);
    public FormatOutcome List(string? type, object? selection) => ApplyList(type, selection);
    public FormatOutcome List(object? selection, string? type) => ApplyList(selection, type);

    // typed enum overloads are handled via object? above; also add ListType overloads
    public FormatOutcome List(ListType type) => ApplyList(type.ToString());
    public FormatOutcome List(ListKind type) => ApplyList(type.ToString());
    public FormatOutcome List(ListaType type) => ApplyList(type.ToString());
    public FormatOutcome List(MarkdownListType type) => ApplyList(type.ToString());
    public FormatOutcome List(ListFormat type) => ApplyList(type.ToString());
    public FormatOutcome List(ListStyle type) => ApplyList(type.ToString());

    public FormatOutcome ApplyList(ListType type) => ApplyList(type.ToString());
    public FormatOutcome ApplyList(ListKind type) => ApplyList(type.ToString());
    public FormatOutcome ApplyList(ListaType type) => ApplyList(type.ToString());
    public FormatOutcome ApplyList(MarkdownListType type) => ApplyList(type.ToString());
    public FormatOutcome ApplyList(ListFormat type) => ApplyList(type.ToString());
    public FormatOutcome ApplyList(ListStyle type) => ApplyList(type.ToString());

    // Bullet
    public FormatOutcome Bullet() { PropagateListState("bullet"); return TransformList("bullet", -1, -1, null); }
    public FormatOutcome Bullet(int start, int end) { PropagateListState("bullet"); return TransformList("bullet", start, end, null); }
    public FormatOutcome Bullet(int start, int end, int length)
    {
        PropagateListState("bullet");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformList("bullet", start, e, null);
    }
    public FormatOutcome Bullet(object? selection) { PropagateListState("bullet"); return TransformList("bullet", -1, -1, selection); }
    public FormatOutcome Bullets() => Bullet();
    public FormatOutcome Bullets(int start, int end) => Bullet(start, end);
    public FormatOutcome Bullets(object? selection) => Bullet(selection);
    public FormatOutcome BulletList() => Bullet();
    public FormatOutcome BulletList(int start, int end) => Bullet(start, end);
    public FormatOutcome BulletList(object? selection) => Bullet(selection);
    public FormatOutcome Unordered() => Bullet();
    public FormatOutcome Unordered(int start, int end) => Bullet(start, end);
    public FormatOutcome Unordered(object? selection) => Bullet(selection);
    public FormatOutcome UnorderedList() => Bullet();
    public FormatOutcome UnorderedList(int start, int end) => Bullet(start, end);
    public FormatOutcome UnorderedList(object? selection) => Bullet(selection);
    public FormatOutcome Viñetas() => Bullet();
    public FormatOutcome Vinetas() => Bullet();
    public FormatOutcome Viñeta() => Bullet();

    // Numbered
    public FormatOutcome Numbered() { PropagateListState("numbered"); return TransformList("numbered", -1, -1, null); }
    public FormatOutcome Numbered(int start, int end) { PropagateListState("numbered"); return TransformList("numbered", start, end, null); }
    public FormatOutcome Numbered(int start, int end, int length)
    {
        PropagateListState("numbered");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformList("numbered", start, e, null);
    }
    public FormatOutcome Numbered(object? selection) { PropagateListState("numbered"); return TransformList("numbered", -1, -1, selection); }
    public FormatOutcome NumberedList() => Numbered();
    public FormatOutcome NumberedList(int start, int end) => Numbered(start, end);
    public FormatOutcome NumberedList(object? selection) => Numbered(selection);
    public FormatOutcome OrderedList() => Numbered();
    public FormatOutcome OrderedList(int start, int end) => Numbered(start, end);
    public FormatOutcome OrderedList(object? selection) => Numbered(selection);
    public FormatOutcome Numerada() => Numbered();
    public FormatOutcome Numeradas() => Numbered();
    public FormatOutcome Ordered() => Numbered();

    // Task
    public FormatOutcome Task() { PropagateListState("task"); return TransformList("task", -1, -1, null); }
    public FormatOutcome Task(int start, int end) { PropagateListState("task"); return TransformList("task", start, end, null); }
    public FormatOutcome Task(int start, int end, int length)
    {
        PropagateListState("task");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformList("task", start, e, null);
    }
    public FormatOutcome Task(object? selection) { PropagateListState("task"); return TransformList("task", -1, -1, selection); }
    public FormatOutcome Tasks() => Task();
    public FormatOutcome Tasks(int start, int end) => Task(start, end);
    public FormatOutcome Tasks(int start, int end, int length) => Task(start, end, length);
    public FormatOutcome Tasks(object? selection) => Task(selection);
    public FormatOutcome TaskList() => Task();
    public FormatOutcome TaskList(int start, int end) => Task(start, end);
    public FormatOutcome TaskList(object? selection) => Task(selection);
    public FormatOutcome Checklist() => Task();
    public FormatOutcome Checklist(int start, int end) => Task(start, end);
    public FormatOutcome Checklist(object? selection) => Task(selection);
    public FormatOutcome Todo() => Task();
    public FormatOutcome Todo(int start, int end) => Task(start, end);
    public FormatOutcome Todo(int start, int end, int length) => Task(start, end, length);
    public FormatOutcome Todo(object? selection) => Task(selection);
    public FormatOutcome TodoList() => Task();
    public FormatOutcome TodoList(int start, int end) => Task(start, end);
    public FormatOutcome TodoList(object? selection) => Task(selection);
    public FormatOutcome CheckboxList() => Task();
    public FormatOutcome Tareas() => Task();
    public FormatOutcome Tarea() => Task();

    // Nest / Unnest
    public FormatOutcome Nest() { PropagateListState("nest"); return TransformList("nest", -1, -1, null); }
    public FormatOutcome Nest(int start, int end) { PropagateListState("nest"); return TransformList("nest", start, end, null); }
    public FormatOutcome Nest(object? selection) { PropagateListState("nest"); return TransformList("nest", -1, -1, selection); }
    public FormatOutcome Anidar() => Nest();
    public FormatOutcome Anidar(int start, int end) => Nest(start, end);
    public FormatOutcome Anidar(object? selection) => Nest(selection);
    public FormatOutcome Indent() => Nest();
    public FormatOutcome Indent(int start, int end) => Nest(start, end);
    public FormatOutcome Indent(object? selection) => Nest(selection);
    public FormatOutcome IncreaseIndent() => Nest();
    public FormatOutcome IncreaseIndent(int start, int end) => Nest(start, end);
    public FormatOutcome IncreaseIndent(object? selection) => Nest(selection);

    public FormatOutcome Unnest() { PropagateListState("unnest"); return TransformList("unnest", -1, -1, null); }
    public FormatOutcome Unnest(int start, int end) { PropagateListState("unnest"); return TransformList("unnest", start, end, null); }
    public FormatOutcome Unnest(object? selection) { PropagateListState("unnest"); return TransformList("unnest", -1, -1, selection); }
    public FormatOutcome Desanidar() => Unnest();
    public FormatOutcome Desanidar(int start, int end) => Unnest(start, end);
    public FormatOutcome Desanidar(object? selection) => Unnest(selection);
    public FormatOutcome Outdent() => Unnest();
    public FormatOutcome Outdent(int start, int end) => Unnest(start, end);
    public FormatOutcome Outdent(object? selection) => Unnest(selection);
    public FormatOutcome DecreaseIndent() => Unnest();
    public FormatOutcome DecreaseIndent(int start, int end) => Unnest(start, end);
    public FormatOutcome DecreaseIndent(object? selection) => Unnest(selection);

    // Toggle / Check
    public FormatOutcome Toggle() { PropagateListState("toggle"); return TransformList("toggle", -1, -1, null); }
    public FormatOutcome Toggle(int start, int end) { PropagateListState("toggle"); return TransformList("toggle", start, end, null); }
    public FormatOutcome Toggle(object? selection) { PropagateListState("toggle"); return TransformList("toggle", -1, -1, selection); }
    public FormatOutcome ToggleCheckbox() => Toggle();
    public FormatOutcome ToggleCheckbox(int start, int end) => Toggle(start, end);
    public FormatOutcome ToggleCheckbox(object? selection) => Toggle(selection);
    public FormatOutcome ToggleTask() => Toggle();
    public FormatOutcome ToggleTask(int start, int end) => Toggle(start, end);
    public FormatOutcome ToggleTask(object? selection) => Toggle(selection);
    public FormatOutcome ToggleCheck() => Toggle();
    public FormatOutcome Check() { PropagateListState("check"); return TransformList("check", -1, -1, null); }
    public FormatOutcome Check(int start, int end) { PropagateListState("check"); return TransformList("check", start, end, null); }
    public FormatOutcome Check(object? selection) { PropagateListState("check"); return TransformList("check", -1, -1, selection); }
    public FormatOutcome Uncheck() { PropagateListState("uncheck"); return TransformList("uncheck", -1, -1, null); }
    public FormatOutcome Uncheck(int start, int end) { PropagateListState("uncheck"); return TransformList("uncheck", start, end, null); }
    public FormatOutcome Uncheck(object? selection) { PropagateListState("uncheck"); return TransformList("uncheck", -1, -1, selection); }

    // Renumber
    public FormatOutcome Renumber() { PropagateListState("renumber"); var t = RenumberNumberedLists(CurrentText()); SetText(t); _lastResult = true; return new FormatOutcome { Success = true, Applied = true, Ast = _ast }; }
    public FormatOutcome RenumberList() => Renumber();
    public FormatOutcome Renumerate() => Renumber();
    public FormatOutcome AutoRenumber() => Renumber();
    public FormatOutcome RenumberNumbered() => Renumber();

    // IsSelectionLike helper (reuse from Emphasis? it's private there; redefine)

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyList(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyList() : engine.ApplyList(type);
    }
    public static FormatOutcome List(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.List() : engine.List(type);
    }
    public static FormatOutcome Bullet(object? input, bool _static = true) => new FormatEngine(input).Bullet();
    public static FormatOutcome Bullets(object? input, bool _static = true) => new FormatEngine(input).Bullet();
    public static FormatOutcome BulletList(object? input, bool _static = true) => new FormatEngine(input).Bullet();
    public static FormatOutcome Unordered(object? input, bool _static = true) => new FormatEngine(input).Bullet();
    public static FormatOutcome UnorderedList(object? input, bool _static = true) => new FormatEngine(input).Bullet();
    public static FormatOutcome Numbered(object? input, bool _static = true) => new FormatEngine(input).Numbered();
    public static FormatOutcome NumberedList(object? input, bool _static = true) => new FormatEngine(input).Numbered();
    public static FormatOutcome OrderedList(object? input, bool _static = true) => new FormatEngine(input).Numbered();
    public static FormatOutcome Ordered(object? input, bool _static = true) => new FormatEngine(input).Numbered();
    public static FormatOutcome Task(object? input, bool _static = true) => new FormatEngine(input).Task();
    public static FormatOutcome Tasks(object? input, bool _static = true) => new FormatEngine(input).Task();
    public static FormatOutcome TaskList(object? input, bool _static = true) => new FormatEngine(input).Task();
    public static FormatOutcome Checklist(object? input, bool _static = true) => new FormatEngine(input).Task();
    public static FormatOutcome Todo(object? input, bool _static = true) => new FormatEngine(input).Task();
    public static FormatOutcome TodoList(object? input, bool _static = true) => new FormatEngine(input).Task();
    public static FormatOutcome Nest(object? input, bool _static = true) => new FormatEngine(input).Nest();
    public static FormatOutcome Anidar(object? input, bool _static = true) => new FormatEngine(input).Nest();
    public static FormatOutcome Indent(object? input, bool _static = true) => new FormatEngine(input).Nest();
    public static FormatOutcome Unnest(object? input, bool _static = true) => new FormatEngine(input).Unnest();
    public static FormatOutcome Desanidar(object? input, bool _static = true) => new FormatEngine(input).Unnest();
    public static FormatOutcome Outdent(object? input, bool _static = true) => new FormatEngine(input).Unnest();
    public static FormatOutcome Toggle(object? input, bool _static = true) => new FormatEngine(input).Toggle();
    public static FormatOutcome ToggleCheckbox(object? input, bool _static = true) => new FormatEngine(input).Toggle();
    public static FormatOutcome ToggleTask(object? input, bool _static = true) => new FormatEngine(input).Toggle();
    public static FormatOutcome Check(object? input, bool _static = true) => new FormatEngine(input).Check();
    public static FormatOutcome Uncheck(object? input, bool _static = true) => new FormatEngine(input).Uncheck();
    public static FormatOutcome Renumber(object? input, bool _static = true) => new FormatEngine(input).Renumber();
    public static FormatOutcome RenumberList(object? input, bool _static = true) => new FormatEngine(input).Renumber();
    public static string RenumberNumberedListsStatic(string text) => RenumberNumberedLists(text);

    public static List<ListInfo> GetLists(object? input) => new FormatEngine(input).GetLists();
    public static List<ListInfo> Lists(object? input) => new FormatEngine(input).GetLists();
    public static List<string> GetListItems(object? input) => new FormatEngine(input).GetListItems();

    // static with input+type
    public static FormatOutcome List(object? input, object? type) => new FormatEngine(input).ApplyList(type?.ToString());
}

// ---------- Model ----------

public class ListInfo
{
    public string Type { get; set; } = "Viñetas";
    public string Kind { get => Type == "Numerada" ? "Numbered" : Type == "Tareas" ? "Task" : "Bullet"; set { } }
    public string Name { get => Type; set => Type = value; }
    public string Category { get => Type; set => Type = value; }
    public string Style { get => Type; set => Type = value; }
    public bool IsBullet => Type == "Viñetas";
    public bool IsNumbered => Type == "Numerada";
    public bool IsTask => Type == "Tareas";
    public List<string> Items { get; set; } = new List<string>();
    public List<bool?> TaskStates { get; set; } = new List<bool?>();
    public int Level { get; set; }
    public bool IsOrdered { get; set; }
    public char BulletType { get; set; } = '-';
    public string? OrderedStart { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int Count => Items.Count;
    public override string ToString() => Type + ": " + string.Join(", ", Items);
}

// Alias classes (avoid name collision with Markdig.Syntax.ListBlock)
public class ListBlockInfo : ListInfo { }
public class ListItemInfo : ListInfo { }
public class ListItemNode : ListInfo { }
public class ListEntryInfo : ListInfo { }
public class Lista : ListInfo { }
public class ListData : ListInfo { }
public class ListItem : ListInfo { }
public class ListEntry : ListInfo { }
public class ListNode : ListInfo { }
public class ListResult : ListInfo { }
public class ListModel : ListInfo { }
public class ListEntity : ListInfo { }
public class ListRecord : ListInfo { }
public class ListaInfo : ListInfo { }
public class MarkdownList : ListInfo { }
public class MarkdownListInfo : ListInfo { }

// ---------- Enums ----------

public enum ListType { Bullets, Bullet, Unordered, Viñetas, Viñeta, Numbered, Ordered, Numerada, Tasks, Task, Tareas, Tarea, Checklist, Todo, Checkbox, Nest, Unnest, Toggle }
public enum ListKind { Bullets, Bullet, Unordered, Viñetas, Viñeta, Numbered, Ordered, Numerada, Tasks, Task, Tareas, Tarea, Checklist, Todo, Checkbox, Nest, Unnest, Toggle }
public enum ListaType { Viñetas, Viñeta, Numerada, Tareas, Tarea, Anidar, Desanidar, Alternar }
public enum MarkdownListType { Bullets, Bullet, Unordered, Viñetas, Numbered, Ordered, Numerada, Tasks, Task, Tareas, Checklist, Todo }
public enum ListFormat { Bullets, Numbered, Tasks }
public enum ListStyle { Bullets, Numbered, Tasks }

// ---------- Extension methods ----------

public static class ListExtensions
{
    public static FormatOutcome ApplyList(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyList(type) : new FormatEngine(ast).ApplyList(type, start, end);
    public static FormatOutcome List(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).List(type) : new FormatEngine(ast).List(type, start, end);
    public static FormatOutcome Bullet(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Bullet() : new FormatEngine(ast).Bullet(start, end);
    public static FormatOutcome Bullets(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Bullets() : new FormatEngine(ast).Bullets(start, end);
    public static FormatOutcome BulletList(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).BulletList() : new FormatEngine(ast).BulletList(start, end);
    public static FormatOutcome Unordered(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Unordered() : new FormatEngine(ast).Unordered(start, end);
    public static FormatOutcome UnorderedList(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).UnorderedList() : new FormatEngine(ast).UnorderedList(start, end);
    public static FormatOutcome Numbered(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Numbered() : new FormatEngine(ast).Numbered(start, end);
    public static FormatOutcome NumberedList(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).NumberedList() : new FormatEngine(ast).NumberedList(start, end);
    public static FormatOutcome OrderedList(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).OrderedList() : new FormatEngine(ast).OrderedList(start, end);
    public static FormatOutcome Task(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Task() : new FormatEngine(ast).Task(start, end);
    public static FormatOutcome Tasks(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Tasks() : new FormatEngine(ast).Tasks(start, end);
    public static FormatOutcome TaskList(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).TaskList() : new FormatEngine(ast).TaskList(start, end);
    public static FormatOutcome Checklist(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Checklist() : new FormatEngine(ast).Checklist(start, end);
    public static FormatOutcome Todo(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Todo() : new FormatEngine(ast).Todo(start, end);
    public static FormatOutcome TodoList(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).TodoList() : new FormatEngine(ast).TodoList(start, end);
    public static FormatOutcome Nest(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Nest() : new FormatEngine(ast).Nest(start, end);
    public static FormatOutcome Anidar(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Anidar() : new FormatEngine(ast).Anidar(start, end);
    public static FormatOutcome Indent(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Indent() : new FormatEngine(ast).Indent(start, end);
    public static FormatOutcome Unnest(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Unnest() : new FormatEngine(ast).Unnest(start, end);
    public static FormatOutcome Desanidar(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Desanidar() : new FormatEngine(ast).Desanidar(start, end);
    public static FormatOutcome Outdent(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Outdent() : new FormatEngine(ast).Outdent(start, end);
    public static FormatOutcome Toggle(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Toggle() : new FormatEngine(ast).Toggle(start, end);
    public static FormatOutcome ToggleCheckbox(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ToggleCheckbox() : new FormatEngine(ast).ToggleCheckbox(start, end);
    public static FormatOutcome ToggleTask(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ToggleTask() : new FormatEngine(ast).ToggleTask(start, end);
    public static FormatOutcome Check(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Check() : new FormatEngine(ast).Check(start, end);
    public static FormatOutcome Uncheck(this MarkdownAst? ast, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).Uncheck() : new FormatEngine(ast).Uncheck(start, end);
    public static FormatOutcome Renumber(this MarkdownAst? ast) => new FormatEngine(ast).Renumber();
    public static FormatOutcome RenumberList(this MarkdownAst? ast) => new FormatEngine(ast).Renumber();
    public static List<ListInfo> GetLists(this MarkdownAst? ast) => new FormatEngine(ast).GetLists();
    public static List<string> GetListItems(this MarkdownAst? ast) => new FormatEngine(ast).GetListItems();

    public static FormatOutcome ApplyList(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyList(type);
    public static FormatOutcome List(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).List(type);
    public static FormatOutcome Bullet(this OpenDocument? doc) => new FormatEngine(doc).Bullet();
    public static FormatOutcome Numbered(this OpenDocument? doc) => new FormatEngine(doc).Numbered();
    public static FormatOutcome Task(this OpenDocument? doc) => new FormatEngine(doc).Task();
    public static FormatOutcome Nest(this OpenDocument? doc) => new FormatEngine(doc).Nest();
    public static FormatOutcome Unnest(this OpenDocument? doc) => new FormatEngine(doc).Unnest();
    public static FormatOutcome Toggle(this OpenDocument? doc) => new FormatEngine(doc).Toggle();
    public static FormatOutcome Renumber(this OpenDocument? doc) => new FormatEngine(doc).Renumber();
    public static List<ListInfo> GetLists(this OpenDocument? doc) => new FormatEngine(doc).GetLists();

    public static FormatOutcome ApplyList(this string? md, string? type = null) => new FormatEngine(md).ApplyList(type);
    public static FormatOutcome List(this string? md, string? type = null) => new FormatEngine(md).List(type);
    public static FormatOutcome Bullet(this string? md) => new FormatEngine(md).Bullet();
    public static FormatOutcome Numbered(this string? md) => new FormatEngine(md).Numbered();
    public static FormatOutcome Task(this string? md) => new FormatEngine(md).Task();
    public static FormatOutcome Nest(this string? md) => new FormatEngine(md).Nest();
    public static FormatOutcome Unnest(this string? md) => new FormatEngine(md).Unnest();
    public static FormatOutcome Toggle(this string? md) => new FormatEngine(md).Toggle();
    public static FormatOutcome Renumber(this string? md) => new FormatEngine(md).Renumber();
    public static List<ListInfo> GetLists(this string? md) => new FormatEngine(md).GetLists();
}