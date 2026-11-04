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
    // ---------- Formula (T-076, ENT-029, OP-030, BR-F004-010, BR-011, NFR-013, ERR-019, AC-039) ----------

    private static bool IsFormulaKey(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(k)) return false;
        if (k.Contains("formula") || k.Contains("fórmula") || k.Contains("latex")
            || k.Contains("math") || k.Contains("matematica") || k.Contains("matemática"))
            return true;
        if (k.Contains("inline") || k.Contains("en linea") || k.Contains("en línea")
            || k.Contains("linea") || k.Contains("línea"))
            return true;
        if (k.Contains("block") || k.Contains("bloque"))
            return true;
        return false;
    }

    private void PropagateFormulaState(string? key)
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

    private static string ResolveFormulaMode(string? keyOrType, object? selection, string? mode, string? selectedText)
    {
        if (!string.IsNullOrWhiteSpace(mode))
        {
            var m = mode!.Trim().ToLowerInvariant();
            if (m.Contains("block") || m.Contains("bloque")) return "block";
            return "inline";
        }

        // selection properties
        if (selection is not null)
        {
            foreach (var name in new[] { "Mode", "Type", "Kind", "Format", "Style", "Value" })
            {
                var prop = selection.GetType().GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop is not null)
                {
                    try
                    {
                        var val = prop.GetValue(selection);
                        if (val is not null)
                        {
                            var s = val.ToString()!.Trim().ToLowerInvariant();
                            if (s.Contains("block") || s.Contains("bloque")) return "block";
                            if (s.Contains("inline") || s.Contains("en linea") || s.Contains("en línea") || s.Contains("linea") || s.Contains("línea")) return "inline";
                        }
                    }
                    catch { }
                }
            }
        }

        var k = (keyOrType ?? string.Empty).Trim().ToLowerInvariant();
        if (k.Contains("block") || k.Contains("bloque")) return "block";
        if (k.Contains("inline") || k.Contains("en linea") || k.Contains("en línea") || k.Contains("linea") || k.Contains("línea")) return "inline";

        // infer from selected text
        var t = (selectedText ?? string.Empty).Trim();
        if (t.StartsWith("$$", StringComparison.Ordinal) && t.EndsWith("$$", StringComparison.Ordinal) && t.Length >= 4)
            return "block";
        if (t.StartsWith("$", StringComparison.Ordinal) && t.EndsWith("$", StringComparison.Ordinal) && !t.StartsWith("$$", StringComparison.Ordinal) && t.Length >= 2)
            return "inline";

        return "inline";
    }

    // ---------- Resolve helpers (public aliases) ----------

    public string ResolveMode(string? keyOrType = null, object? selection = null, string? mode = null, string? selectedText = null)
        => ResolveFormulaMode(keyOrType, selection, mode, selectedText ?? (selection is null ? string.Empty : string.Empty));

    public string ResolveFormulaModePublic(string? keyOrType = null, object? selection = null, string? mode = null, string? selectedText = null)
        => ResolveFormulaMode(keyOrType, selection, mode, selectedText);



    // ---------- Transform ----------

    public FormatOutcome TransformFormula(string? keyOrType, int start, int end, object? selection, int length = 0, string? mode = null)
    {
        PropagateFormulaState(keyOrType);
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

        var trimmedSelected = (selected ?? string.Empty).Trim();

        // empty code -> no transform (state remains previous)
        if (string.IsNullOrWhiteSpace(trimmedSelected))
        {
            _lastResult = true;
            return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
        }

        var wantMode = ResolveFormulaMode(keyOrType, selection, mode, selected);
        bool wantBlock = string.Equals(wantMode, "block", StringComparison.OrdinalIgnoreCase);

        var t = trimmedSelected;
        bool isBlockFormula = t.StartsWith("$$", StringComparison.Ordinal) && t.EndsWith("$$", StringComparison.Ordinal) && t.Length >= 4;
        bool isInlineFormula = !isBlockFormula && t.StartsWith("$", StringComparison.Ordinal) && t.EndsWith("$", StringComparison.Ordinal) && t.Length >= 2 && !t.StartsWith("$$", StringComparison.Ordinal);

        string replacement;
        if (wantBlock)
        {
            if (isBlockFormula)
            {
                // unwrap
                var inner = t.Substring(2, t.Length - 4);
                if (inner.StartsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(1);
                if (inner.EndsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(0, inner.Length - 1);
                replacement = inner;
            }
            else
            {
                var body = selected ?? string.Empty;
                if (body.Length > 0 && !body.EndsWith("\n", StringComparison.Ordinal)) body += "\n";
                replacement = "$$\n" + body + "$$";
            }
        }
        else
        {
            if (isInlineFormula)
            {
                // unwrap
                replacement = t.Substring(1, t.Length - 2);
            }
            else
            {
                replacement = "$" + selected + "$";
            }
        }

        var newText = text.Remove(start, end - start).Insert(start, replacement);
        SetText(newText);
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = keyOrType };
    }

    // ---------- Public API ----------

    public FormatOutcome ApplyFormula() => ApplyFormula("formula");

    public FormatOutcome ApplyFormula(string? type)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, null);
    }

    public FormatOutcome ApplyFormula(string? type, int start, int end)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, null);
    }

    public FormatOutcome ApplyFormula(int start, int end)
    {
        PropagateFormulaState("formula");
        return TransformFormula("formula", start, end, null);
    }

    public FormatOutcome ApplyFormula(int start, int end, int length)
    {
        PropagateFormulaState("formula");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFormula("formula", start, e, null, length);
    }

    public FormatOutcome ApplyFormula(string? type, int start, int end, int length)
    {
        PropagateFormulaState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFormula(type, start, e, null, length);
    }

    public FormatOutcome ApplyFormula(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateFormulaState(null);
            return TransformFormula(null, -1, -1, value);
        }
        PropagateFormulaState(value?.ToString());
        return TransformFormula(value?.ToString(), -1, -1, null);
    }

    public FormatOutcome ApplyFormula(string? type, object? selection)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, selection);
    }

    public FormatOutcome ApplyFormula(object? selection, string? type)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, selection);
    }

    public FormatOutcome ApplyFormula(string? type, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, null, 0, mode);
    }

    public FormatOutcome ApplyFormula(string? type, int start, int end, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, null, 0, mode);
    }

    public FormatOutcome ApplyFormula(string? type, int start, int end, int length, string? mode)
    {
        PropagateFormulaState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFormula(type, start, e, null, length, mode);
    }

    public FormatOutcome ApplyFormula(string? type, int start, int end, object? selection)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, selection);
    }

    public FormatOutcome ApplyFormula(string? type, int start, int end, object? selection, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, selection, 0, mode);
    }

    public FormatOutcome ApplyFormula(string? type, object? selection, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, selection, 0, mode);
    }

    // Formula() aliases
    public FormatOutcome Formula() => Formula("formula");

    public FormatOutcome Formula(string? type)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, null);
    }

    public FormatOutcome Formula(string? type, int start, int end)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, null);
    }

    public FormatOutcome Formula(int start, int end)
    {
        PropagateFormulaState("formula");
        return TransformFormula("formula", start, end, null);
    }

    public FormatOutcome Formula(int start, int end, int length)
    {
        PropagateFormulaState("formula");
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFormula("formula", start, e, null, length);
    }

    public FormatOutcome Formula(string? type, int start, int end, int length)
    {
        PropagateFormulaState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFormula(type, start, e, null, length);
    }

    public FormatOutcome Formula(object? value)
    {
        if (IsSelectionLike(value))
        {
            PropagateFormulaState(null);
            return TransformFormula(null, -1, -1, value);
        }
        PropagateFormulaState(value?.ToString());
        return TransformFormula(value?.ToString(), -1, -1, null);
    }

    public FormatOutcome Formula(string? type, object? selection)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, selection);
    }

    public FormatOutcome Formula(object? selection, string? type)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, selection);
    }

    public FormatOutcome Formula(string? type, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, null, 0, mode);
    }

    public FormatOutcome Formula(string? type, int start, int end, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, null, 0, mode);
    }

    public FormatOutcome Formula(string? type, int start, int end, int length, string? mode)
    {
        PropagateFormulaState(type);
        int e = end;
        if (length > 0 && e < start) e = start + length;
        return TransformFormula(type, start, e, null, length, mode);
    }

    public FormatOutcome Formula(string? type, int start, int end, object? selection)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, selection);
    }

    public FormatOutcome Formula(string? type, int start, int end, object? selection, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, start, end, selection, 0, mode);
    }

    public FormatOutcome Formula(string? type, object? selection, string? mode)
    {
        PropagateFormulaState(type);
        return TransformFormula(type, -1, -1, selection, 0, mode);
    }

    // FormulaLatex aliases
    public FormatOutcome FormulaLatex() => Formula();
    public FormatOutcome FormulaLatex(string? type) => Formula(type);
    public FormatOutcome FormulaLatex(string? type, int start, int end) => Formula(type, start, end);
    public FormatOutcome FormulaLatex(int start, int end) => Formula(start, end);
    public FormatOutcome FormulaLatex(int start, int end, int length) => Formula(start, end, length);
    public FormatOutcome FormulaLatex(object? value) => Formula(value);
    public FormatOutcome FormulaLatex(string? type, object? selection) => Formula(type, selection);
    public FormatOutcome FormulaLatex(object? selection, string? type) => Formula(selection, type);
    public FormatOutcome FormulaLatex(string? type, string? mode) => Formula(type, mode);
    public FormatOutcome FormulaLatex(string? type, int start, int end, string? mode) => Formula(type, start, end, mode);
    public FormatOutcome FormulaLatex(string? type, int start, int end, int length, string? mode) => Formula(type, start, end, length, mode);
    public FormatOutcome FormulaLatex(string? type, object? selection, string? mode) => Formula(type, selection, mode);

    public FormatOutcome ApplyFormulaLatex() => ApplyFormula();
    public FormatOutcome ApplyFormulaLatex(string? type) => ApplyFormula(type);
    public FormatOutcome ApplyFormulaLatex(string? type, int start, int end) => ApplyFormula(type, start, end);
    public FormatOutcome ApplyFormulaLatex(object? value) => ApplyFormula(value);
    public FormatOutcome ApplyFormulaLatex(string? type, object? selection) => ApplyFormula(type, selection);
    public FormatOutcome ApplyFormulaLatex(string? type, string? mode) => ApplyFormula(type, mode);
    public FormatOutcome ApplyFormulaLatex(string? type, int start, int end, string? mode) => ApplyFormula(type, start, end, mode);

    // Latex aliases
    public FormatOutcome Latex() => Formula();
    public FormatOutcome Latex(string? type) => Formula(type);
    public FormatOutcome Latex(string? type, int start, int end) => Formula(type, start, end);
    public FormatOutcome Latex(int start, int end) => Formula(start, end);
    public FormatOutcome Latex(object? value) => Formula(value);
    public FormatOutcome Latex(string? type, object? selection) => Formula(type, selection);
    public FormatOutcome Latex(string? type, string? mode) => Formula(type, mode);
    public FormatOutcome ApplyLatex() => ApplyFormula();
    public FormatOutcome ApplyLatex(string? type) => ApplyFormula(type);
    public FormatOutcome ApplyLatex(object? value) => ApplyFormula(value);
    public FormatOutcome ApplyLatex(string? type, object? selection) => ApplyFormula(type, selection);



    // Inline formula aliases
    public FormatOutcome InlineFormula() => Formula("inline");
    public FormatOutcome InlineFormula(int start, int end) => Formula("inline", start, end);
    public FormatOutcome InlineFormula(object? selection) => Formula("inline", selection);
    public FormatOutcome InlineLatex() => Formula("inline");
    public FormatOutcome InlineMath() => Formula("inline");
    public FormatOutcome FormulaInline() => Formula("inline");
    public FormatOutcome FormulaInline(int start, int end) => Formula("inline", start, end);
    public FormatOutcome FormulaInline(object? selection) => Formula("inline", selection);

    // Block formula aliases
    public FormatOutcome BlockFormula() => Formula("block");
    public FormatOutcome BlockFormula(string? mode) => Formula("block", mode);
    public FormatOutcome BlockFormula(int start, int end) => Formula("block", start, end);
    public FormatOutcome BlockFormula(object? selection) => Formula("block", selection);
    public FormatOutcome BlockLatex() => Formula("block");
    public FormatOutcome BlockMath() => Formula("block");
    public FormatOutcome FormulaBlock() => Formula("block");
    public FormatOutcome FormulaBlock(int start, int end) => Formula("block", start, end);
    public FormatOutcome FormulaBlock(object? selection) => Formula("block", selection);

    // Spanish aliases
    public FormatOutcome Fórmula() => Formula();
    public FormatOutcome Fórmula(string? type) => Formula(type);
    public FormatOutcome Fórmula(string? type, int start, int end) => Formula(type, start, end);
    public FormatOutcome Fórmula(object? value) => Formula(value);
    public FormatOutcome Fórmula(string? type, object? selection) => Formula(type, selection);

    // enum overloads
    public FormatOutcome Formula(FormulaMode mode) => Formula(mode.ToString());
    public FormatOutcome Formula(FormulaType mode) => Formula(mode.ToString());
    public FormatOutcome Formula(FormulaKind mode) => Formula(mode.ToString());
    public FormatOutcome Formula(FormulaFormat mode) => Formula(mode.ToString());
    public FormatOutcome Formula(FormulaStyle mode) => Formula(mode.ToString());
    public FormatOutcome Formula(FormulaModo mode) => Formula(mode.ToString());
    public FormatOutcome ApplyFormula(FormulaMode mode) => ApplyFormula(mode.ToString());
    public FormatOutcome ApplyFormula(FormulaType mode) => ApplyFormula(mode.ToString());
    public FormatOutcome ApplyFormula(FormulaKind mode) => ApplyFormula(mode.ToString());
    public FormatOutcome ApplyFormula(FormulaFormat mode) => ApplyFormula(mode.ToString());
    public FormatOutcome ApplyFormula(FormulaStyle mode) => ApplyFormula(mode.ToString());
    public FormatOutcome ApplyFormula(FormulaModo mode) => ApplyFormula(mode.ToString());

    // Unwrap helpers
    public FormatOutcome UnwrapFormula()
    {
        PropagateFormulaState("unwrap formula");
        var t = CurrentText();
        SetText(UnwrapFormulaText(t));
        _lastResult = true;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast };
    }
    public FormatOutcome UnFormula() => UnwrapFormula();
    public FormatOutcome RemoveFormula() => UnwrapFormula();
    public FormatOutcome StripFormula() => UnwrapFormula();
    public FormatOutcome UnLatex() => UnwrapFormula();
    public FormatOutcome UnMath() => UnwrapFormula();

    private static string UnwrapFormulaText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var t = text.Trim();
        if (t.StartsWith("$$", StringComparison.Ordinal) && t.EndsWith("$$", StringComparison.Ordinal) && t.Length >= 4)
        {
            var inner = t.Substring(2, t.Length - 4);
            if (inner.StartsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(1);
            if (inner.EndsWith("\n", StringComparison.Ordinal)) inner = inner.Substring(0, inner.Length - 1);
            return inner;
        }
        if (t.StartsWith("$", StringComparison.Ordinal) && t.EndsWith("$", StringComparison.Ordinal) && !t.StartsWith("$$", StringComparison.Ordinal) && t.Length >= 2)
            return text.Substring(1, text.Length - 2);
        return text;
    }

    // ---------- AST-based formula info ----------

    public List<FormulaInfo> GetFormulas()
    {
        EnsureAst();
        var result = new List<FormulaInfo>();
        var text = CurrentText();
        foreach (Match m in Regex.Matches(text, @"\$\$(.*?)\$\$", RegexOptions.Singleline))
        {
            var code = m.Groups[1].Value;
            if (code.StartsWith("\n", StringComparison.Ordinal)) code = code.Substring(1);
            if (code.EndsWith("\n", StringComparison.Ordinal)) code = code.Substring(0, code.Length - 1);
            result.Add(new FormulaInfo { Mode = "Bloque", IsBlock = true, IsInline = false, Code = code });
        }
        foreach (Match m in Regex.Matches(text, @"(?<!\$)\$(?!\$)([^\$]+)\$(?!\$)"))
        {
            result.Add(new FormulaInfo { Mode = "En línea", IsInline = true, IsBlock = false, Code = m.Groups[1].Value });
        }
        return result;
    }

    public List<FormulaInfo> Formulas() => GetFormulas();
    public List<FormulaInfo> Latexes() => GetFormulas();
    public List<FormulaInfo> Maths() => GetFormulas();
    public List<FormulaInfo> GetAllFormulas() => GetFormulas();
    public List<FormulaInfo> FindFormulas() => GetFormulas();
    public List<FormulaInfo> EnumerateFormulas() => GetFormulas();

    // ---------- Static helpers ----------

    public static FormatOutcome ApplyFormula(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.ApplyFormula() : engine.ApplyFormula(type);
    }

    public static FormatOutcome ApplyFormula(object? input, string? type, string? mode, bool _static = true, bool _extra = true)
        => new FormatEngine(input).ApplyFormula(type, mode);

    public static FormatOutcome ApplyFormula(object? input, string? type, int start, int end, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).ApplyFormula(type, start, end);

    public static FormatOutcome Formula(object? input, string? type = null, bool _static = true)
    {
        var engine = new FormatEngine(input);
        return type is null ? engine.Formula() : engine.Formula(type);
    }

    public static FormatOutcome Formula(object? input, string? type, string? mode, bool _static = true, bool _extra = true)
        => new FormatEngine(input).Formula(type, mode);

    public static FormatOutcome Formula(object? input, string? type, int start, int end, bool _static = true, bool _extra = true, bool _more = true)
        => new FormatEngine(input).Formula(type, start, end);

    public static FormatOutcome FormulaLatex(object? input, string? type = null, bool _static = true)
        => type is null ? new FormatEngine(input).FormulaLatex() : new FormatEngine(input).FormulaLatex(type);

    public static FormatOutcome Latex(object? input, string? type = null, bool _static = true)
        => type is null ? new FormatEngine(input).Latex() : new FormatEngine(input).Latex(type);



    public static List<FormulaInfo> GetFormulas(object? input, bool _static = true) => new FormatEngine(input).GetFormulas();
    public static List<FormulaInfo> Formulas(object? input, bool _static = true) => new FormatEngine(input).GetFormulas();
}

// ---------- Enum definitions ----------

public enum FormulaMode { Inline, Block, EnLinea = Inline, Bloque = Block, InLine = Inline, BlockFormula = Block }
public enum FormulaType { Formula, Latex, Math, Inline, Block, EnLinea = Inline, Bloque = Block }
public enum FormulaKind { Formula, Latex, Math, Inline, Block, EnLinea = Inline, Bloque = Block }
public enum FormulaFormat { Formula, Latex, Math, Inline, Block, EnLinea = Inline, Bloque = Block }
public enum FormulaStyle { Formula, Latex, Math, Inline, Block, EnLinea = Inline, Bloque = Block }
public enum FormulaModo { EnLinea, Bloque, Inline = EnLinea, Block = Bloque }

// ---------- Model ----------

public class FormulaInfo
{
    public string Mode { get; set; } = "En línea";
    public bool IsInline { get; set; }
    public bool IsBlock { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Text { get => Code; set => Code = value; }
    public string Latex { get => Code; set => Code = value; }
    public string Formula { get => Code; set => Code = value; }
    public string Value { get => Code; set => Code = value; }
    public string Body { get => Code; set => Code = value; }
    public string Markdown { get => Code; set => Code = value; }
    public string Source { get => Code; set => Code = value; }
    public List<string> Items { get; set; } = new List<string>();
    public int Count => string.IsNullOrEmpty(Code) ? 0 : 1;
    public override string ToString() => Code;
}

// Alias classes
public class Formula : FormulaInfo { }
public class Latex : FormulaInfo { }

public class FormulaData : FormulaInfo { }
public class FormulaEntry : FormulaInfo { }
public class FormulaItem : FormulaInfo { }
public class FormulaResult : FormulaInfo { }
public class FormulaModel : FormulaInfo { }
public class FormulaEntity : FormulaInfo { }
public class FormulaRecord : FormulaInfo { }
public class FormulaNode : FormulaInfo { }
public class Fórmula : FormulaInfo { }
public class FórmulaInfo : FormulaInfo { }

// ---------- Extension methods ----------

public static class FormulaExtensions
{
    public static FormatOutcome ApplyFormula(this MarkdownAst? ast, string? type = null, int start = -1, int end = -1)
        => (start < 0 || end < 0) ? new FormatEngine(ast).ApplyFormula(type) : new FormatEngine(ast).ApplyFormula(type, start, end);
    public static FormatOutcome Formula(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).Formula(type);
    public static FormatOutcome Formula(this MarkdownAst? ast, string? type, string? mode)
        => new FormatEngine(ast).Formula(type, mode);
    public static FormatOutcome FormulaLatex(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).FormulaLatex(type);
    public static FormatOutcome Latex(this MarkdownAst? ast, string? type = null)
        => new FormatEngine(ast).Latex(type);

    public static List<FormulaInfo> GetFormulas(this MarkdownAst? ast) => new FormatEngine(ast).GetFormulas();

    public static FormatOutcome ApplyFormula(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).ApplyFormula(type);
    public static FormatOutcome Formula(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Formula(type);
    public static FormatOutcome Formula(this OpenDocument? doc, string? type, string? mode) => new FormatEngine(doc).Formula(type, mode);
    public static FormatOutcome FormulaLatex(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).FormulaLatex(type);
    public static FormatOutcome Latex(this OpenDocument? doc, string? type = null) => new FormatEngine(doc).Latex(type);

    public static List<FormulaInfo> GetFormulas(this OpenDocument? doc) => new FormatEngine(doc).GetFormulas();

    public static FormatOutcome ApplyFormula(this string? md, string? type = null) => new FormatEngine(md).ApplyFormula(type);
    public static FormatOutcome Formula(this string? md, string? type = null) => new FormatEngine(md).Formula(type);
    public static FormatOutcome Formula(this string? md, string? type, string? mode) => new FormatEngine(md).Formula(type, mode);
    public static FormatOutcome FormulaLatex(this string? md, string? type = null) => new FormatEngine(md).FormulaLatex(type);
    public static FormatOutcome Latex(this string? md, string? type = null) => new FormatEngine(md).Latex(type);

    public static List<FormulaInfo> GetFormulas(this string? md) => new FormatEngine(md).GetFormulas();

    public static FormatOutcome InlineFormula(this MarkdownAst? ast) => new FormatEngine(ast).InlineFormula();
    public static FormatOutcome BlockFormula(this MarkdownAst? ast) => new FormatEngine(ast).BlockFormula();
    public static FormatOutcome InlineFormula(this OpenDocument? doc) => new FormatEngine(doc).InlineFormula();
    public static FormatOutcome BlockFormula(this OpenDocument? doc) => new FormatEngine(doc).BlockFormula();
    public static FormatOutcome InlineFormula(this string? md) => new FormatEngine(md).InlineFormula();
    public static FormatOutcome BlockFormula(this string? md) => new FormatEngine(md).BlockFormula();
}