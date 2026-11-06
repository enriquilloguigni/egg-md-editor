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
    // ---------- Diagram (T-077, ENT-030, OP-031, BR-F004-011, BR-011, NFR-013, ERR-019, AC-040) ----------

    private static bool IsDiagramKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("diagram")) return true;
        if (k.Contains("diagrama")) return true;
        return false;
    }

    private static bool IsMermaidKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("mermaid")) return true;
        if (k.Contains("flowchart")) return true;
        if (k.Contains("graph")) return true;
        return false;
    }

    private static bool IsDiagramKeyType(string? key)
    {
        var k = (key ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains(' ') || k.Contains('\t') || k.Contains('\n') || k.Contains('\r')
            || k.Contains(';') || k.Contains(',') || k.Length > 24)
            return false;
        return IsDiagramKey(k) || IsMermaidKey(k);
    }

    private void PropagateDiagramState(string? key)
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

    // ---------- Resolve helpers ----------

    public string ResolveDiagramCode(string? keyOrType = null, object? selection = null, string? code = null, string? selectedText = null)
    {
        if (!string.IsNullOrWhiteSpace(code)) return code!.Trim();
        if (selection is not null)
        {
            var fromSel = ExtractProperty(selection, "Code", "Content", "Text", "Body", "Value", "Markdown", "Source", "Diagram", "Mermaid");
            if (!string.IsNullOrWhiteSpace(fromSel)) return fromSel!.Trim();
        }
        if (!string.IsNullOrWhiteSpace(selectedText)) return selectedText!.Trim();
        var k = (keyOrType ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(k) && !IsDiagramKey(k) && !IsMermaidKey(k)) return k;
        return string.Empty;
    }

    public string ResolveCode(string? keyOrType = null, object? selection = null, string? code = null, string? selectedText = null)
        => ResolveDiagramCode(keyOrType, selection, code, selectedText);

    public string ResolveMermaidCode(string? keyOrType = null, object? selection = null, string? code = null, string? selectedText = null)
        => ResolveDiagramCode(keyOrType, selection, code, selectedText);

    // ---------- Transform ----------

    public FormatOutcome TransformDiagram(string? keyOrType, int start, int end, object? selection, int length = 0, string? code = null)
    {
        PropagateDiagramState(keyOrType);
        ResolveSelectionIndices(selection, ref start, ref end);
        if (length > 0 && end < start) end = start + length;

        var text = CurrentText();
        if (start < 0) start = -1;
        if (end < 0) end = -1;
        if (start > text.Length) start = text.Length;
        if (end > text.Length) end = text.Length;
        if (start >= 0 && end >= 0 && start > end) { var tmp = start; start = end; end = tmp; }

        // no selection -> whole document
        if (start < 0 || end < 0)
        {
            start = 0;
            end = text.Length;
        }

        string selected = string.Empty;
        if (start >= 0 && end >= 0 && end >= start)
            selected = text.Substring(start, end - start);

        bool hasCode = !string.IsNullOrWhiteSpace(code);

        // empty code -> no transform (state remains previous)
        if (!hasCode && string.IsNullOrWhiteSpace(selected))
        {
            _lastResult = true;
            return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
        }

        var trimmedSel = (selected ?? string.Empty).Trim();

        // detect existing mermaid fenced block -> unwrap (reversible)
        bool isWrapped = false;
        if (!hasCode && trimmedSel.StartsWith("```", StringComparison.Ordinal) && trimmedSel.EndsWith("```", StringComparison.Ordinal) && trimmedSel.Length >= 6)
        {
            var firstLine = trimmedSel.Split('\n')[0].Trim().ToLowerInvariant();
            if (firstLine.Contains("mermaid")) isWrapped = true;
        }

        string replacement;
        if (isWrapped)
        {
            var lines = (selected ?? string.Empty).Split('\n');
            int first = 0;
            if (lines.Length > 0 && lines[0].TrimStart().StartsWith("```", StringComparison.Ordinal)) first = 1;
            int last = lines.Length - 1;
            if (last >= first && lines[last].Trim().StartsWith("```", StringComparison.Ordinal)) last -= 1;
            var inner = (first <= last) ? string.Join("\n", lines.Skip(first).Take(last - first + 1)) : string.Empty;
            if (inner.StartsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(1);
            if (inner.EndsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(0, inner.Length - 1);
            replacement = inner;
        }
        else
        {
            string body = hasCode ? (code ?? string.Empty) : (selected ?? string.Empty);
            body = body.Trim('\r', '\n');
            if (string.IsNullOrWhiteSpace(body))
            {
                _lastResult = true;
                return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
            }
            replacement = "```mermaid\n" + body + "\n```";
        }

        var newText = text.Remove(start, end - start).Insert(start, replacement);
        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyDiagram() => ApplyDiagram("diagram");

    public FormatOutcome ApplyDiagram(string? type)
    {
        if (!string.IsNullOrWhiteSpace(type) && !IsDiagramKeyType(type))
        {
            PropagateDiagramState("mermaid");
            return TransformDiagram("mermaid", -1, -1, null, 0, type);
        }
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, null);
    }

    public FormatOutcome ApplyDiagram(string? type, int start, int end)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, null);
    }

    public FormatOutcome ApplyDiagram(int start, int end)
    {
        PropagateDiagramState("diagram");
        return TransformDiagram("diagram", start, end, null);
    }

    public FormatOutcome ApplyDiagram(int start, int end, int length)
    {
        PropagateDiagramState("diagram");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram("diagram", start, e, null, length);
    }

    public FormatOutcome ApplyDiagram(string? type, int start, int end, int length)
    {
        PropagateDiagramState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram(type, start, e, null, length);
    }

    public FormatOutcome ApplyDiagram(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateDiagramState(null);
            return TransformDiagram(null, -1, -1, value);
        }
        var s = value?.ToString();
        if (!string.IsNullOrWhiteSpace(s) && !IsDiagramKeyType(s))
        {
            PropagateDiagramState("mermaid");
            return TransformDiagram("mermaid", -1, -1, null, 0, s);
        }
        PropagateDiagramState(s);
        return TransformDiagram(s, -1, -1, null);
    }

    public FormatOutcome ApplyDiagram(string? type, object? selection)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, selection);
    }

    public FormatOutcome ApplyDiagram(object? selection, string? type)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, selection);
    }

    public FormatOutcome ApplyDiagram(string? type, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, null, 0, code);
    }

    public FormatOutcome ApplyDiagram(string? type, int start, int end, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, null, 0, code);
    }

    public FormatOutcome ApplyDiagram(string? type, int start, int end, int length, string? code)
    {
        PropagateDiagramState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram(type, start, e, null, length, code);
    }

    public FormatOutcome ApplyDiagram(string? type, object? selection, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, selection, 0, code);
    }

    public FormatOutcome ApplyDiagram(string? type, int start, int end, object? selection)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, selection);
    }

    public FormatOutcome ApplyDiagram(string? type, int start, int end, object? selection, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, selection, 0, code);
    }

    // Diagram() aliases
    public FormatOutcome Diagram() => Diagram("diagram");

    public FormatOutcome Diagram(string? type)
    {
        if (!string.IsNullOrWhiteSpace(type) && !IsDiagramKeyType(type))
        {
            PropagateDiagramState("mermaid");
            return TransformDiagram("mermaid", -1, -1, null, 0, type);
        }
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, null);
    }

    public FormatOutcome Diagram(string? type, int start, int end)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, null);
    }

    public FormatOutcome Diagram(int start, int end)
    {
        PropagateDiagramState("diagram");
        return TransformDiagram("diagram", start, end, null);
    }

    public FormatOutcome Diagram(int start, int end, int length)
    {
        PropagateDiagramState("diagram");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram("diagram", start, e, null, length);
    }

    public FormatOutcome Diagram(string? type, int start, int end, int length)
    {
        PropagateDiagramState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram(type, start, e, null, length);
    }

    public FormatOutcome Diagram(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateDiagramState(null);
            return TransformDiagram(null, -1, -1, value);
        }
        var s = value?.ToString();
        if (!string.IsNullOrWhiteSpace(s) && !IsDiagramKeyType(s))
        {
            PropagateDiagramState("mermaid");
            return TransformDiagram("mermaid", -1, -1, null, 0, s);
        }
        PropagateDiagramState(s);
        return TransformDiagram(s, -1, -1, null);
    }

    public FormatOutcome Diagram(string? type, object? selection)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, selection);
    }

    public FormatOutcome Diagram(object? selection, string? type)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, selection);
    }

    public FormatOutcome Diagram(string? type, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, null, 0, code);
    }

    public FormatOutcome Diagram(string? type, int start, int end, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, null, 0, code);
    }

    public FormatOutcome Diagram(string? type, int start, int end, int length, string? code)
    {
        PropagateDiagramState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram(type, start, e, null, length, code);
    }

    public FormatOutcome Diagram(string? type, object? selection, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, -1, -1, selection, 0, code);
    }

    public FormatOutcome Diagram(string? type, int start, int end, object? selection)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, selection);
    }

    public FormatOutcome Diagram(string? type, int start, int end, object? selection, string? code)
    {
        PropagateDiagramState(type);
        return TransformDiagram(type, start, end, selection, 0, code);
    }

    // Mermaid() aliases (default mermaid)
    public FormatOutcome Mermaid() => Mermaid("mermaid");

    public FormatOutcome Mermaid(string? type)
    {
        if (!string.IsNullOrWhiteSpace(type) && !IsDiagramKeyType(type))
        {
            PropagateDiagramState("mermaid");
            return TransformDiagram("mermaid", -1, -1, null, 0, type);
        }
        var t = string.IsNullOrWhiteSpace(type) ? "mermaid" : type;
        PropagateDiagramState(t);
        return TransformDiagram(t, -1, -1, null);
    }

    public FormatOutcome Mermaid(string? type, int start, int end)
    {
        var t = string.IsNullOrWhiteSpace(type) ? "mermaid" : type;
        PropagateDiagramState(t);
        return TransformDiagram(t, start, end, null);
    }

    public FormatOutcome Mermaid(int start, int end)
    {
        PropagateDiagramState("mermaid");
        return TransformDiagram("mermaid", start, end, null);
    }

    public FormatOutcome Mermaid(int start, int end, int length)
    {
        PropagateDiagramState("mermaid");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram("mermaid", start, e, null, length);
    }

    public FormatOutcome Mermaid(string? type, int start, int end, int length)
    {
        var t = string.IsNullOrWhiteSpace(type) ? "mermaid" : type;
        PropagateDiagramState(t);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformDiagram(t, start, e, null, length);
    }

    public FormatOutcome Mermaid(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateDiagramState("mermaid");
            return TransformDiagram("mermaid", -1, -1, value);
        }
        var s = value?.ToString();
        if (!string.IsNullOrWhiteSpace(s) && !IsDiagramKeyType(s))
        {
            PropagateDiagramState("mermaid");
            return TransformDiagram("mermaid", -1, -1, null, 0, s);
        }
        var t = string.IsNullOrWhiteSpace(s) ? "mermaid" : s;
        PropagateDiagramState(t);
        return TransformDiagram(t, -1, -1, null);
    }

    public FormatOutcome Mermaid(string? type, object? selection)
    {
        var t = string.IsNullOrWhiteSpace(type) ? "mermaid" : type;
        PropagateDiagramState(t);
        return TransformDiagram(t, -1, -1, selection);
    }

    public FormatOutcome Mermaid(object? selection, string? type)
    {
        var t = string.IsNullOrWhiteSpace(type) ? "mermaid" : type;
        PropagateDiagramState(t);
        return TransformDiagram(t, -1, -1, selection);
    }

    public FormatOutcome Mermaid(string? type, string? code)
    {
        var t = string.IsNullOrWhiteSpace(type) ? "mermaid" : type;
        PropagateDiagramState(t);
        return TransformDiagram(t, -1, -1, null, 0, code);
    }

    // ApplyMermaid aliases
    public FormatOutcome ApplyMermaid() => ApplyDiagram("mermaid");
    public FormatOutcome ApplyMermaid(string? type) => ApplyDiagram(string.IsNullOrWhiteSpace(type) ? "mermaid" : type);
    public FormatOutcome ApplyMermaid(string? type, int start, int end) => ApplyDiagram(string.IsNullOrWhiteSpace(type) ? "mermaid" : type, start, end);
    public FormatOutcome ApplyMermaid(int start, int end) => ApplyDiagram("mermaid", start, end);
    public FormatOutcome ApplyMermaid(object? value) => ApplyDiagram(value);
    public FormatOutcome ApplyMermaid(string? type, object? selection) => ApplyDiagram(string.IsNullOrWhiteSpace(type) ? "mermaid" : type, selection);
    public FormatOutcome ApplyMermaid(string? type, string? code) => ApplyDiagram(string.IsNullOrWhiteSpace(type) ? "mermaid" : type, code);

    // Spanish / other aliases
    public FormatOutcome Diagrama() => Diagram();
    public FormatOutcome Diagrama(string? type) => Diagram(type);
    public FormatOutcome Diagrama(string? type, int start, int end) => Diagram(type, start, end);
    public FormatOutcome Diagrama(int start, int end) => Diagram(start, end);
    public FormatOutcome Diagrama(object? value) => Diagram(value);
    public FormatOutcome Diagrama(string? type, object? selection) => Diagram(type, selection);
    public FormatOutcome Diagrama(string? type, string? code) => Diagram(type, code);
    public FormatOutcome ApplyDiagrama() => ApplyDiagram();
    public FormatOutcome ApplyDiagrama(string? type) => ApplyDiagram(type);
    public FormatOutcome ApplyDiagrama(string? type, int start, int end) => ApplyDiagram(type, start, end);
    public FormatOutcome ApplyDiagrama(object? value) => ApplyDiagram(value);
    public FormatOutcome ApplyDiagrama(string? type, object? selection) => ApplyDiagram(type, selection);

    public FormatOutcome Flowchart() => Diagram("flowchart");
    public FormatOutcome Flowchart(string? code) => Diagram("flowchart", code);
    public FormatOutcome Graph() => Diagram("graph");
    public FormatOutcome Graph(string? code) => Diagram("graph", code);

    // enum overloads
    public FormatOutcome Diagram(DiagramMode mode) => Diagram(mode.ToString());
    public FormatOutcome Diagram(DiagramType mode) => Diagram(mode.ToString());
    public FormatOutcome Diagram(DiagramKind mode) => Diagram(mode.ToString());
    public FormatOutcome Diagram(DiagramFormat mode) => Diagram(mode.ToString());
    public FormatOutcome Diagram(DiagramStyle mode) => Diagram(mode.ToString());
    public FormatOutcome Diagram(DiagramModo mode) => Diagram(mode.ToString());
    public FormatOutcome Diagram(DiagramRenderState mode) => Diagram(mode.ToString());
    public FormatOutcome ApplyDiagram(DiagramMode mode) => ApplyDiagram(mode.ToString());
    public FormatOutcome ApplyDiagram(DiagramType mode) => ApplyDiagram(mode.ToString());
    public FormatOutcome ApplyDiagram(DiagramKind mode) => ApplyDiagram(mode.ToString());
    public FormatOutcome ApplyDiagram(DiagramFormat mode) => ApplyDiagram(mode.ToString());
    public FormatOutcome ApplyDiagram(DiagramStyle mode) => ApplyDiagram(mode.ToString());
    public FormatOutcome ApplyDiagram(DiagramModo mode) => ApplyDiagram(mode.ToString());
    public FormatOutcome ApplyDiagram(DiagramRenderState mode) => ApplyDiagram(mode.ToString());
    public FormatOutcome Mermaid(DiagramMode mode) => Mermaid(mode.ToString());
    public FormatOutcome Mermaid(DiagramRenderState mode) => Mermaid(mode.ToString());

    // Unwrap helpers
    public FormatOutcome UnwrapDiagram()
    {
        PropagateDiagramState("unwrap diagram");
        var t = CurrentText();
        SetText(UnwrapDiagramText(t));
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast };
    }
    public FormatOutcome UnDiagram() => UnwrapDiagram();
    public FormatOutcome RemoveDiagram() => UnwrapDiagram();
    public FormatOutcome StripDiagram() => UnwrapDiagram();
    public FormatOutcome UnMermaid() => UnwrapDiagram();
    public FormatOutcome UnFlowchart() => UnwrapDiagram();

    private static string UnwrapDiagramText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var t = text.Trim();
        if (t.StartsWith("```", StringComparison.Ordinal) && t.EndsWith("```", StringComparison.Ordinal))
        {
            var firstLine = t.Split('\n')[0].Trim().ToLowerInvariant();
            if (firstLine.Contains("mermaid"))
            {
                var lines = text.Split('\n');
                int first = 0;
                if (lines.Length > 0 && lines[0].TrimStart().StartsWith("```")) first = 1;
                int last = lines.Length - 1;
                if (last >= first && lines[last].Trim().StartsWith("```")) last -= 1;
                var inner = string.Join("\n", lines.Skip(first).Take(Math.Max(0, last - first + 1)));
                if (inner.StartsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(1);
                if (inner.EndsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(0, inner.Length - 1);
                return inner;
            }
        }
        return text;
    }

    // ---------- AST-based diagram info ----------

    public List<DiagramInfo> GetDiagrams()
    {
        EnsureAst();
        var result = new List<DiagramInfo>();
        if (_ast is null) return result;
        CollectDiagrams(_ast.Document, result);
        return result;
    }

    public List<DiagramInfo> Diagrams() => GetDiagrams();
    public List<DiagramInfo> Mermaids() => GetDiagrams();
    public List<DiagramInfo> GetMermaids() => GetDiagrams();
    public List<DiagramInfo> GetAllDiagrams() => GetDiagrams();
    public List<DiagramInfo> FindDiagrams() => GetDiagrams();
    public List<DiagramInfo> EnumerateDiagrams() => GetDiagrams();
    public List<string> GetDiagramContents()
    {
        return GetDiagrams().Select(d => d.Code).ToList();
    }
    public List<string> DiagramContents() => GetDiagramContents();

    private static void CollectDiagrams(Block block, List<DiagramInfo> result)
    {
        if (block is null) return;
        if (block is FencedCodeBlock fcb)
        {
            var lang = string.Empty;
            try { lang = (fcb.Info ?? string.Empty).Trim(); } catch { lang = string.Empty; }
            if (lang.ToLowerInvariant().Contains("mermaid"))
            {
                var info = new DiagramInfo { Mode = "Bloque", IsBlock = true };
                var sb = new System.Text.StringBuilder();
                try
                {
                    bool first = true;
                    foreach (var sl in fcb.Lines)
                    {
                        if (!first) sb.Append('\n');
                        first = false;
                        sb.Append(sl.ToString());
                    }
                }
                catch
                {
                    try { sb.Append(fcb.ToString()); } catch { }
                }
                var content = sb.ToString();
                info.Code = content;
                info.Text = content;
                info.Content = content;
                info.Value = content;
                info.Body = content;
                info.Markdown = content;
                info.Source = content;
                info.Items.Add(content);
                try { info.StartLine = fcb.Line; info.EndLine = fcb.Line; } catch { }
                result.Add(info);
            }
        }
        if (block is ContainerBlock cb)
        {
            foreach (var child in cb)
                CollectDiagrams(child, result);
        }
        try
        {
            var inlineProp = block.GetType().GetProperty("Inline");
            if (inlineProp is not null)
            {
                // diagrams are block only
            }
        }
        catch { }
    }

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyDiagram(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyDiagram() : engine.ApplyDiagram(type);
    }

    public static FormatOutcome ApplyDiagram(object? input, string? type, string? code, bool _static = true, bool _extra = true)
        => new FormatEngine(input).ApplyDiagram(type, code);

    public static FormatOutcome ApplyDiagram(object? input, string? type, int start, int end, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).ApplyDiagram(type, start, end);

    public static FormatOutcome ApplyDiagram(object? input, string? type, int start, int end, string? code, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).ApplyDiagram(type, start, end, code);

    public static FormatOutcome Diagram(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.Diagram() : engine.Diagram(type);
    }

    public static FormatOutcome Diagram(object? input, string? type, string? code, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Diagram(type, code);

    public static FormatOutcome Diagram(object? input, string? type, int start, int end, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).Diagram(type, start, end);

    public static FormatOutcome Mermaid(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.Mermaid() : engine.Mermaid(type);
    }

    public static FormatOutcome Mermaid(object? input, string? type, string? code, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Mermaid(type, code);

    public static List<DiagramInfo> GetDiagrams(object? input, bool _static = true) => new FormatEngine(input).GetDiagrams();
    public static List<DiagramInfo> Diagrams(object? input, bool _static = true) => new FormatEngine(input).GetDiagrams();
    public static List<DiagramInfo> Mermaids(object? input, bool _static = true) => new FormatEngine(input).GetDiagrams();
}

// ---------- Enum definitions ----------

public enum DiagramMode { Block, Bloque = Block }
public enum DiagramType { Diagram, Mermaid, Flowchart, Graph, Block, Bloque = Block }
public enum DiagramKind { Diagram, Mermaid, Flowchart, Graph, Block, Bloque = Block }
public enum DiagramFormat { Diagram, Mermaid, Flowchart, Graph, Block, Bloque = Block }
public enum DiagramStyle { Diagram, Mermaid, Flowchart, Graph, Block, Bloque = Block }
public enum DiagramModo { Bloque, Block = Bloque }
public enum DiagramRenderState { OK, Error, Ok = OK, Err = Error }
public enum DiagramStatus { OK, Error, Ok = OK }
public enum DiagramState { OK, Error }
public enum RenderState { OK, Error }

// ---------- Model ----------

public class DiagramInfo
{
    public string Mode { get; set; } = "Bloque";
    public bool IsBlock { get; set; } = true;
    public bool IsInline { get; set; } = false;
    public string RenderState { get; set; } = "OK";
    public string Status { get => RenderState; set => RenderState = value; }
    public string State { get => RenderState; set => RenderState = value; }
    public string Code { get; set; } = string.Empty;
    public string Text { get => Code; set => Code = value; }
    public string Content { get => Code; set => Code = value; }
    public string Value { get => Code; set => Code = value; }
    public string Body { get => Code; set => Code = value; }
    public string Markdown { get => Code; set => Code = value; }
    public string Source { get => Code; set => Code = value; }
    public string Mermaid { get => Code; set => Code = value; }
    public List<string> Items { get; set; } = new List<string>();
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int Count => string.IsNullOrEmpty(Code) ? 0 : 1;
    public override string ToString() => Code;
}

// Alias classes
public class Diagram : DiagramInfo { }
public class DiagramData : DiagramInfo { }
public class DiagramEntry : DiagramInfo { }
public class DiagramItem : DiagramInfo { }
public class DiagramResult : DiagramInfo { }
public class DiagramModel : DiagramInfo { }
public class DiagramEntity : DiagramInfo { }
public class DiagramRecord : DiagramInfo { }
public class DiagramNode : DiagramInfo { }
public class Mermaid : DiagramInfo { }
public class MermaidInfo : DiagramInfo { }
public class MermaidDiagram : DiagramInfo { }
public class Diagrama : DiagramInfo { }
public class DiagramaInfo : DiagramInfo { }
public class Flowchart : DiagramInfo { }
public class FlowchartInfo : DiagramInfo { }
public class Graph : DiagramInfo { }
public class GraphInfo : DiagramInfo { }

// ---------- Extension methods ----------

public static class DiagramExtensions
{
    public static FormatOutcome ApplyDiagram(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyDiagram(type) : new FormatEngine(ast).ApplyDiagram(type, start, end);
    public static FormatOutcome Diagram(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).Diagram(type);
    public static FormatOutcome Diagram(this MarkdownAst? ast, string? type, string? code)
        => new FormatEngine(ast).Diagram(type, code);
    public static FormatOutcome Mermaid(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).Mermaid(type);
    public static List<DiagramInfo> GetDiagrams(this MarkdownAst? ast) => new FormatEngine(ast).GetDiagrams();

    public static FormatOutcome ApplyDiagram(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyDiagram(type);
    public static FormatOutcome Diagram(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Diagram(type);
    public static FormatOutcome Diagram(this OpenDocument? doc, string? type, string? code) => new FormatEngine(doc).Diagram(type, code);
    public static FormatOutcome Mermaid(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Mermaid(type);
    public static List<DiagramInfo> GetDiagrams(this OpenDocument? doc) => new FormatEngine(doc).GetDiagrams();

    public static FormatOutcome ApplyDiagram(this string? md, string? type = null) => new FormatEngine(md).ApplyDiagram(type);
    public static FormatOutcome Diagram(this string? md, string? type = null) => new FormatEngine(md).Diagram(type);
    public static FormatOutcome Diagram(this string? md, string? type, string? code) => new FormatEngine(md).Diagram(type, code);
    public static FormatOutcome Mermaid(this string? md, string? type = null) => new FormatEngine(md).Mermaid(type);
    public static List<DiagramInfo> GetDiagrams(this string? md) => new FormatEngine(md).GetDiagrams();
}