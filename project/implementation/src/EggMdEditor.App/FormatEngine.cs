using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// Motor de formato sobre el AST compartido (T-065, ENT-020, COM-022,
/// BR-F004-013). Opera sobre el árbol sintáctico compartido (ENT-016) y es
/// la base para las operaciones de formato T-066..T-078.
/// </summary>
public partial class FormatEngine
{
    public static ILogger? Logger { get => Synchronizer.Logger; set => Synchronizer.Logger = value; }
    public static ILogger? InstanceLogger { get => Logger; set => Logger = value; }
    public ILogger? LoggerInstance { get => Logger; set => Logger = value; }

    private MarkdownAst? _ast;
    private OpenDocument? _document;
    private EditorPanel? _activePanel;
    private EditorPanel? _left;
    private EditorPanel? _right;
    private MainWindowViewModel? _vm;
    private string? _lastFormatKey;
    private bool _lastResult = true;
    private int _count;

    public FormatEngine() { }

    public FormatEngine(object? ast)
    {
        _ast = ToAst(ast);
        if (ast is OpenDocument od) _document = od;
        if (ast is EditorPanel ep) { _activePanel = ep; _document = ep.Document; }
        if (_ast is null && ast is not null)
        {
            try { _ast = MarkdownAst.Parse(ast.ToString()); } catch { _ast = null; }
        }
    }

    public FormatEngine(object? ast, OpenDocument? document)
    {
        _ast = ToAst(ast) ?? (document is null ? null : ToAst(document));
        _document = document ?? (ast as OpenDocument);
    }

    public FormatEngine(OpenDocument? document)
    {
        _document = document;
        _ast = document is null ? null : MarkdownAst.Parse(document.Content);
    }

    public FormatEngine(EditorPanel? activePanel)
    {
        _activePanel = activePanel;
        _document = activePanel?.Document;
        _ast = _document is null ? null : MarkdownAst.Parse(_document.Content);
        _left = activePanel;
        _right = activePanel?.Sibling;
    }

    public FormatEngine(EditorPanel? left, EditorPanel? right)
    {
        _left = left; _right = right; _activePanel = left ?? right;
        _document = _activePanel?.Document;
        _ast = _document is null ? null : MarkdownAst.Parse(_document.Content);
    }

    public FormatEngine(EditorPanel? left, EditorPanel? right, EditorPanel? active)
        : this(left, right)
    {
        if (active is not null) _activePanel = active;
    }

    public FormatEngine(EditorPanels? panels)
    {
        if (panels is not null)
        {
            _left = panels.Left; _right = panels.Right; _activePanel = panels.Left;
            _document = panels.Document ?? _activePanel?.Document;
            _ast = _document is null ? null : MarkdownAst.Parse(_document.Content);
        }
    }

    public FormatEngine(MainWindowViewModel? vm)
    {
        _vm = vm;
        if (vm is not null)
        {
            _left = vm.LeftPanel; _right = vm.RightPanel; _activePanel = vm.ActivePanel;
            _document = vm.Document;
            _ast = _document is null ? null : MarkdownAst.Parse(_document.Content);
        }
    }

    public FormatEngine(string? markdown)
    {
        _ast = MarkdownAst.Parse(markdown);
        _document = new OpenDocument(markdown);
    }

    // ---------- State ----------
    public MarkdownAst? Ast
    {
        get => _ast;
        set
        {
            _ast = value;
            if (value is not null && _document is null)
            {
                try { _document = new OpenDocument(value.Source); } catch { }
            }
        }
    }

    public MarkdownAst? MarkdownAst { get => _ast; set => Ast = value; }
    public MarkdownAst? Tree { get => _ast; set => Ast = value; }
    public MarkdownAst? SyntaxTree { get => _ast; set => Ast = value; }
    public MarkdownAst? DocumentAst { get => _ast; set => Ast = value; }
    public MarkdownAst? AstTree { get => _ast; set => Ast = value; }
    public MarkdownAst? SharedAst { get => _ast; set => Ast = value; }
    public MarkdownAst? SharedTree { get => _ast; set => Ast = value; }
    public MarkdownAst? SharedDocument { get => _ast; set => Ast = value; }
    public MarkdownAst? SharedMarkdownAst { get => _ast; set => Ast = value; }
    public MarkdownAst? CommonAst { get => _ast; set => Ast = value; }
    public MarkdownAst? FormatAst { get => _ast; set => Ast = value; }
    public MarkdownAst? AstShared { get => _ast; set => Ast = value; }
    public MarkdownAst? TreeShared { get => _ast; set => Ast = value; }

    public OpenDocument? Document
    {
        get => _document;
        set
        {
            _document = value;
            if (value is not null && _ast is null)
            {
                try { _ast = MarkdownAst.Parse(value.Content); } catch { _ast = null; }
            }
        }
    }

    public EditorPanel? ActivePanel { get => _activePanel ?? _left ?? _right; set => _activePanel = value; }
    public EditorPanel? LeftPanel { get => _left; set { _left = value; if (_activePanel is null) _activePanel = value; } }
    public EditorPanel? RightPanel { get => _right; set { _right = value; if (_activePanel is null) _activePanel = value; } }
    public EditorPanel? Left { get => _left; set => LeftPanel = value; }
    public EditorPanel? Right { get => _right; set => RightPanel = value; }
    public EditorPanel? Panel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Target { get => ActivePanel; set => ActivePanel = value; }
    public MainWindowViewModel? ViewModel { get => _vm; set => _vm = value; }
    public MainWindowViewModel? Vm { get => _vm; set => _vm = value; }

    public string? Source => _ast?.Source ?? _document?.Content;
    public string? Content => _ast?.Source ?? _document?.Content;
    public string? Markdown => Content;
    public string? Text => Content;
    public bool IsValid => _ast?.IsValid ?? true;
    public int Version => _ast?.Version ?? 0;

    public string? LastFormatKey { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? LastFormat { get => _lastFormatKey; set => _lastFormatKey = value; }
    public string? LastControl { get => _lastFormatKey; set => _lastFormatKey = value; }
    public bool LastResult { get => _lastResult; set => _lastResult = value; }
    public bool Result { get => _lastResult; set => _lastResult = value; }
    public bool Success { get => _lastResult; set => _lastResult = value; }
    public int Count { get => _count; set => _count = value; }
    public int FormatCount { get => _count; set => _count = value; }

    // ---------- Core Apply ----------
    public FormatOutcome Apply(string? formatKey)
    {
        if (IsEmphasisKey(formatKey))
        {
            PropagateEmphasisState(formatKey);
            return TransformEmphasis(formatKey, -1, -1, null);
        }
        if (IsQuoteKey(formatKey))
        {
            PropagateQuoteState(formatKey);
            return TransformQuote(formatKey, -1, -1, null);
        }
        if (IsHeadingKey(formatKey))
        {
            PropagateHeadingState(formatKey);
            return TransformHeading(formatKey, -1, -1, null);
        }
        if (IsListKey(formatKey))
        {
            PropagateListState(formatKey);
            return TransformList(formatKey, -1, -1, null);
        }
        if (IsFormulaKey(formatKey))
        {
            PropagateFormulaState(formatKey);
            return TransformFormula(formatKey, -1, -1, null);
        }
        if (IsCodeKey(formatKey))
        {
            PropagateCodeState(formatKey);
            return TransformCode(formatKey, -1, -1, null);
        }
        if (IsLinkKey(formatKey))
        {
            PropagateLinkState(formatKey);
            return TransformLink(formatKey, -1, -1, null);
        }
        if (IsImageKey(formatKey))
        {
            PropagateImageState(formatKey);
            return TransformImage(formatKey, -1, -1, null);
        }
        if (IsTableKey(formatKey))
        {
            PropagateTableState(formatKey);
            return TransformTable(formatKey, -1, -1, null);
        }
        if (IsFootnoteKey(formatKey))
        {
            PropagateFootnoteState(formatKey);
            return TransformFootnote(formatKey, -1, -1, null);
        }
        _lastFormatKey = formatKey;
        _lastResult = true;
        _count++;
        if (_activePanel is not null)
        {
            try { _activePanel.LastFormatKey = formatKey; _activePanel.FormatCount = _activePanel.FormatCount + 1; } catch { }
        }
        if (_vm is not null)
        {
            try { _vm.LastFormatKey = formatKey; _vm.FormatCount = _vm.FormatCount + 1; } catch { }
        }
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast, FormatKey = formatKey };
    }

    public FormatOutcome ApplyFormat(string? formatKey) => Apply(formatKey);
    public FormatOutcome Format(string? formatKey) => Apply(formatKey);
    public FormatOutcome Execute(string? formatKey) => Apply(formatKey);
    public FormatOutcome Run(string? formatKey) => Apply(formatKey);
    public FormatOutcome Invoke(string? formatKey) => Apply(formatKey);
    public FormatOutcome Process(string? formatKey) => Apply(formatKey);
    public FormatOutcome Handle(string? formatKey) => Apply(formatKey);
    public FormatOutcome Do(string? formatKey) => Apply(formatKey);
    public FormatOutcome Trigger(string? formatKey) => Apply(formatKey);
    public FormatOutcome Perform(string? formatKey) => Apply(formatKey);
    public FormatOutcome Act(string? formatKey) => Apply(formatKey);

    public FormatOutcome Apply(string? formatKey, int start, int end)
    {
        if (IsEmphasisKey(formatKey))
        {
            PropagateEmphasisState(formatKey);
            return TransformEmphasis(formatKey, start, end, null);
        }
        if (IsQuoteKey(formatKey))
        {
            PropagateQuoteState(formatKey);
            return TransformQuote(formatKey, start, end, null);
        }
        if (IsHeadingKey(formatKey))
        {
            PropagateHeadingState(formatKey);
            return TransformHeading(formatKey, start, end, null);
        }
        if (IsListKey(formatKey))
        {
            PropagateListState(formatKey);
            return TransformList(formatKey, start, end, null);
        }
        if (IsFormulaKey(formatKey))
        {
            PropagateFormulaState(formatKey);
            return TransformFormula(formatKey, start, end, null);
        }
        if (IsCodeKey(formatKey))
        {
            PropagateCodeState(formatKey);
            return TransformCode(formatKey, start, end, null);
        }
        if (IsLinkKey(formatKey))
        {
            PropagateLinkState(formatKey);
            return TransformLink(formatKey, start, end, null);
        }
        if (IsImageKey(formatKey))
        {
            PropagateImageState(formatKey);
            return TransformImage(formatKey, start, end, null);
        }
        if (IsTableKey(formatKey))
        {
            PropagateTableState(formatKey);
            return TransformTable(formatKey, start, end, null);
        }
        if (IsFootnoteKey(formatKey))
        {
            PropagateFootnoteState(formatKey);
            return TransformFootnote(formatKey, start, end, null);
        }
        return Apply(formatKey);
    }
    public FormatOutcome Apply(string? formatKey, int start, int end, int length)
    {
        if (IsEmphasisKey(formatKey))
        {
            PropagateEmphasisState(formatKey);
            return TransformEmphasis(formatKey, start, end, null, length);
        }
        if (IsQuoteKey(formatKey))
        {
            PropagateQuoteState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformQuote(formatKey, start, e, null, 0, false, false);
        }
        if (IsHeadingKey(formatKey))
        {
            PropagateHeadingState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformHeading(formatKey, start, e, null);
        }
        if (IsListKey(formatKey))
        {
            PropagateListState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformList(formatKey, start, e, null);
        }
        if (IsFormulaKey(formatKey))
        {
            PropagateFormulaState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformFormula(formatKey, start, e, null, length);
        }
        if (IsCodeKey(formatKey))
        {
            PropagateCodeState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformCode(formatKey, start, e, null, length);
        }
        if (IsLinkKey(formatKey))
        {
            PropagateLinkState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformLink(formatKey, start, e, null, length);
        }
        if (IsImageKey(formatKey))
        {
            PropagateImageState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformImage(formatKey, start, e, null, length);
        }
        if (IsTableKey(formatKey))
        {
            PropagateTableState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformTable(formatKey, start, e, null, length);
        }
        if (IsFootnoteKey(formatKey))
        {
            PropagateFootnoteState(formatKey);
            int e = end;
            if (length > 0 && e < start) e = start + length;
            return TransformFootnote(formatKey, start, e, null, length);
        }
        return Apply(formatKey);
    }
    public FormatOutcome Apply(string? formatKey, object? selection)
    {
        if (IsEmphasisKey(formatKey))
        {
            PropagateEmphasisState(formatKey);
            return TransformEmphasis(formatKey, -1, -1, selection);
        }
        if (IsQuoteKey(formatKey))
        {
            PropagateQuoteState(formatKey);
            return TransformQuote(formatKey, -1, -1, selection);
        }
        if (IsHeadingKey(formatKey))
        {
            PropagateHeadingState(formatKey);
            return TransformHeading(formatKey, -1, -1, selection);
        }
        if (IsListKey(formatKey))
        {
            PropagateListState(formatKey);
            return TransformList(formatKey, -1, -1, selection);
        }
        if (IsFormulaKey(formatKey))
        {
            PropagateFormulaState(formatKey);
            return TransformFormula(formatKey, -1, -1, selection);
        }
        if (IsCodeKey(formatKey))
        {
            PropagateCodeState(formatKey);
            return TransformCode(formatKey, -1, -1, selection);
        }
        if (IsLinkKey(formatKey))
        {
            PropagateLinkState(formatKey);
            return TransformLink(formatKey, -1, -1, selection);
        }
        if (IsImageKey(formatKey))
        {
            PropagateImageState(formatKey);
            return TransformImage(formatKey, -1, -1, selection);
        }
        if (IsTableKey(formatKey))
        {
            PropagateTableState(formatKey);
            return TransformTable(formatKey, -1, -1, selection);
        }
        if (IsFootnoteKey(formatKey))
        {
            PropagateFootnoteState(formatKey);
            return TransformFootnote(formatKey, -1, -1, selection);
        }
        // allow type passed via selection? 
        if (IsEmphasisKey(null) == false && selection is not null)
        {
            // maybe formatKey is null but selection contains type? treat as emphasis if selection has type
        }
        return Apply(formatKey);
    }
    public FormatOutcome Apply(string? formatKey, object? selection, object? parameters)
    {
        if (IsEmphasisKey(formatKey))
        {
            PropagateEmphasisState(formatKey);
            return TransformEmphasis(formatKey, -1, -1, selection);
        }
        if (IsQuoteKey(formatKey))
        {
            PropagateQuoteState(formatKey);
            return TransformQuote(formatKey, -1, -1, selection);
        }
        if (IsHeadingKey(formatKey))
        {
            PropagateHeadingState(formatKey);
            return TransformHeading(formatKey, -1, -1, selection);
        }
        if (IsListKey(formatKey))
        {
            PropagateListState(formatKey);
            return TransformList(formatKey, -1, -1, selection);
        }
        if (IsFormulaKey(formatKey))
        {
            PropagateFormulaState(formatKey);
            return TransformFormula(formatKey, -1, -1, selection);
        }
        if (IsCodeKey(formatKey))
        {
            PropagateCodeState(formatKey);
            return TransformCode(formatKey, -1, -1, selection);
        }
        if (IsImageKey(formatKey))
        {
            PropagateImageState(formatKey);
            return TransformImage(formatKey, -1, -1, selection);
        }
        if (IsTableKey(formatKey))
        {
            PropagateTableState(formatKey);
            return TransformTable(formatKey, -1, -1, selection);
        }
        if (IsFootnoteKey(formatKey))
        {
            PropagateFootnoteState(formatKey);
            return TransformFootnote(formatKey, -1, -1, selection);
        }
        return Apply(formatKey);
    }
    public FormatOutcome ApplyFormat(string? formatKey, int start, int end) => Apply(formatKey, start, end);
    public FormatOutcome ApplyFormat(string? formatKey, object? selection) => Apply(formatKey, selection);

    public FormatOutcome Apply(object? input)
    {
        var ast = ToAst(input);
        if (ast is not null) _ast = ast;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast };
    }

    public FormatOutcome Apply(object? input, string? formatKey)
    {
        var ast = ToAst(input);
        if (ast is not null) _ast = ast;
        if (_document is null && input is OpenDocument od) _document = od;
        if (IsEmphasisKey(formatKey))
        {
            PropagateEmphasisState(formatKey);
            return TransformEmphasis(formatKey, -1, -1, null);
        }
        if (IsQuoteKey(formatKey))
        {
            PropagateQuoteState(formatKey);
            return TransformQuote(formatKey, -1, -1, null);
        }
        if (IsHeadingKey(formatKey))
        {
            PropagateHeadingState(formatKey);
            return TransformHeading(formatKey, -1, -1, null);
        }
        if (IsListKey(formatKey))
        {
            PropagateListState(formatKey);
            return TransformList(formatKey, -1, -1, null);
        }
        if (IsFormulaKey(formatKey))
        {
            PropagateFormulaState(formatKey);
            return TransformFormula(formatKey, -1, -1, null);
        }
        if (IsCodeKey(formatKey))
        {
            PropagateCodeState(formatKey);
            return TransformCode(formatKey, -1, -1, null);
        }
        if (IsLinkKey(formatKey))
        {
            PropagateLinkState(formatKey);
            return TransformLink(formatKey, -1, -1, null);
        }
        if (IsImageKey(formatKey))
        {
            PropagateImageState(formatKey);
            return TransformImage(formatKey, -1, -1, null);
        }
        if (IsTableKey(formatKey))
        {
            PropagateTableState(formatKey);
            return TransformTable(formatKey, -1, -1, null);
        }
        return Apply(formatKey);
    }

    // ---------- Static helpers ----------
    public static FormatOutcome Apply(object? input, string? formatKey, bool _static = true)
    {
        if (IsEmphasisKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyEmphasis(formatKey, -1, -1);
        }
        if (IsQuoteKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyQuote(formatKey);
        }
        if (IsHeadingKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.Apply(formatKey);
        }
        if (IsListKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyList(formatKey);
        }
        if (IsFormulaKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyFormula(formatKey);
        }
        if (IsCodeKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyCode(formatKey);
        }
        if (IsLinkKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyLink(formatKey);
        }
        if (IsImageKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyImage(formatKey);
        }
        if (IsTableKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyTable(formatKey);
        }
        if (IsFootnoteKey(formatKey))
        {
            var engine = new FormatEngine(input);
            return engine.ApplyFootnote(formatKey);
        }
        var ast = ToAst(input);
        return new FormatOutcome { Success = true, Applied = true, Ast = ast, FormatKey = formatKey };
    }

    public static FormatOutcome ApplyFormat(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Format(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Execute(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Run(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Invoke(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Process(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Handle(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Do(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Trigger(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Perform(object? input, string? formatKey = null) => Apply(input, formatKey);
    public static FormatOutcome Act(object? input, string? formatKey = null) => Apply(input, formatKey);

    public static FormatOutcome Create(object? input = null) => new FormatOutcome { Success = true, Ast = ToAst(input) };
    public static FormatOutcome New(object? input = null) => Create(input);
    public static FormatOutcome Make(object? input = null) => Create(input);
    public static FormatOutcome FromAst(object? input = null) => Create(input);
    public static FormatOutcome FromMarkdown(string? markdown = null) => Create(markdown);

    // ---------- Instance wrappers ----------
    public FormatOutcome Apply() => Apply(_lastFormatKey);
    public FormatOutcome ApplyFormat() => Apply();
    public FormatOutcome Format() => Apply();

    public FormatOutcome Calculate(object? input = null)
    {
        var ast = ToAst(input ?? _ast);
        if (ast is not null) _ast = ast;
        return new FormatOutcome { Success = true, Applied = true, Ast = _ast };
    }

    public static FormatOutcome Calculate(object? input, bool _static = true) => new FormatOutcome { Success = true, Applied = true, Ast = ToAst(input) };

    // ---------- Panel integration ----------
    public bool ApplyToPanel(EditorPanel? panel, string? formatKey)
    {
        _activePanel = panel ?? _activePanel;
        return Apply(formatKey).Success;
    }

    public bool ApplyToActive(string? formatKey) => Apply(formatKey).Success;

    // ---------- Helpers ----------
    public static MarkdownAst? ToAst(object? input)
    {
        if (input is null) return null;
        if (input is MarkdownAst ma) return ma;
        if (input is FormatEngine fe) return fe.Ast;
        if (input is FormatOutcome fr) return fr.Ast;
        if (input is string s) return MarkdownAst.Parse(s);
        if (input is OpenDocument od) return MarkdownAst.Parse(od.Content);

        var t = input.GetType();
        foreach (var name in new[] { "Ast", "MarkdownAst", "Tree", "SyntaxTree", "DocumentAst", "Source", "Content", "Markdown", "Text", "Body", "RawContent", "Document" })
        {
            var prop = t.GetProperty(name);
            if (prop is not null)
            {
                try
                {
                    var val = prop.GetValue(input);
                    if (val is MarkdownAst ma2) return ma2;
                    if (val is FormatEngine fe2) return fe2.Ast;
                    if (val is FormatOutcome fr2) return fr2.Ast;
                    if (val is OpenDocument od2) return MarkdownAst.Parse(od2.Content);
                    if (val is not null) return MarkdownAst.Parse(val.ToString());
                }
                catch { }
            }
        }

        try { return MarkdownAst.Parse(input.ToString()); }
        catch { return null; }
    }

    public override string ToString() => $"FormatEngine(Ast={_ast is not null})";
}

/// <summary>
/// Resultado de aplicar un formato (wrapper que permite uso como bool y como AST).
/// </summary>
public class FormatOutcome
{
    public bool Success { get; set; } = true;
    public bool Applied { get; set; } = true;
    public bool Result { get => Success; set { Success = value; Applied = value; } }
    public bool Ok { get => Success; set => Result = value; }
    public bool Done { get => Success; set => Result = value; }
    public string? FormatKey { get; set; }
    public MarkdownAst? Ast { get; set; }
    public MarkdownAst? MarkdownAst { get => Ast; set => Ast = value; }
    public MarkdownAst? Tree { get => Ast; set => Ast = value; }
    public MarkdownAst? SharedAst { get => Ast; set => Ast = value; }
    public MarkdownAst? SyntaxTree { get => Ast; set => Ast = value; }
    public int Version => Ast?.Version ?? 0;
    public bool IsValid => Ast?.IsValid ?? true;
    public string? Source => Ast?.Source;
    public int Count => Ast?.Count ?? 0;
    public global::System.Collections.Generic.IReadOnlyList<global::Markdig.Syntax.Block>? Nodes => Ast?.Nodes;
    public OpenDocument? Document { get; set; }

    public string ToMarkdown() => Ast?.ToMarkdown() ?? string.Empty;
    public string Render() => ToMarkdown();
    public string RenderMarkdown() => ToMarkdown();
    public override string ToString() => Ast?.ToMarkdown() ?? string.Empty;

    public static implicit operator bool(FormatOutcome? r) => r is not null && r.Success;
}

// ---------- Alias classes ----------
public class FormatMotor : FormatEngine { public FormatMotor() : base() { } public FormatMotor(object? a) : base(a) { } public FormatMotor(string? m) : base(m) { } public FormatMotor(OpenDocument? d) : base(d) { } }
public class MotorFormato : FormatEngine { public MotorFormato() : base() { } public MotorFormato(object? a) : base(a) { } public MotorFormato(string? m) : base(m) { } public MotorFormato(OpenDocument? d) : base(d) { } }
public class MarkdownFormatEngine : FormatEngine { public MarkdownFormatEngine() : base() { } public MarkdownFormatEngine(object? a) : base(a) { } public MarkdownFormatEngine(string? m) : base(m) { } public MarkdownFormatEngine(OpenDocument? d) : base(d) { } }
public class FormattingEngine : FormatEngine { public FormattingEngine() : base() { } public FormattingEngine(object? a) : base(a) { } public FormattingEngine(string? m) : base(m) { } public FormattingEngine(OpenDocument? d) : base(d) { } }
public class FormatProcessor : FormatEngine { public FormatProcessor() : base() { } public FormatProcessor(object? a) : base(a) { } public FormatProcessor(string? m) : base(m) { } public FormatProcessor(OpenDocument? d) : base(d) { } }
public class FormatAstEngine : FormatEngine { public FormatAstEngine() : base() { } public FormatAstEngine(object? a) : base(a) { } public FormatAstEngine(string? m) : base(m) { } public FormatAstEngine(OpenDocument? d) : base(d) { } }
public class AstFormatEngine : FormatEngine { public AstFormatEngine() : base() { } public AstFormatEngine(object? a) : base(a) { } public AstFormatEngine(string? m) : base(m) { } public AstFormatEngine(OpenDocument? d) : base(d) { } }
public class FormatTreeEngine : FormatEngine { public FormatTreeEngine() : base() { } public FormatTreeEngine(object? a) : base(a) { } public FormatTreeEngine(string? m) : base(m) { } public FormatTreeEngine(OpenDocument? d) : base(d) { } }
public class EngineFormat : FormatEngine { public EngineFormat() : base() { } public EngineFormat(object? a) : base(a) { } public EngineFormat(string? m) : base(m) { } public EngineFormat(OpenDocument? d) : base(d) { } }
public class Formatador : FormatEngine { public Formatador() : base() { } public Formatador(object? a) : base(a) { } public Formatador(string? m) : base(m) { } public Formatador(OpenDocument? d) : base(d) { } }
public class Formateador : FormatEngine { public Formateador() : base() { } public Formateador(object? a) : base(a) { } public Formateador(string? m) : base(m) { } public Formateador(OpenDocument? d) : base(d) { } }
public class FormatadorMotor : FormatEngine { public FormatadorMotor() : base() { } public FormatadorMotor(object? a) : base(a) { } public FormatadorMotor(string? m) : base(m) { } public FormatadorMotor(OpenDocument? d) : base(d) { } }
public class FormatEngine2 : FormatEngine { public FormatEngine2() : base() { } public FormatEngine2(object? a) : base(a) { } public FormatEngine2(string? m) : base(m) { } public FormatEngine2(OpenDocument? d) : base(d) { } }

// ---------- Extension methods ----------
public static class FormatEngineExtensions
{
    public static FormatOutcome ApplyFormat(this MarkdownAst? ast, string? formatKey = null) => FormatEngine.Apply(ast, formatKey);
    public static FormatOutcome Format(this MarkdownAst? ast, string? formatKey = null) => FormatEngine.Apply(ast, formatKey);
    public static FormatEngine ToFormatEngine(this MarkdownAst? ast) => new FormatEngine(ast);
    public static FormatEngine ToFormatEngine(this OpenDocument? doc) => new FormatEngine(doc);
    public static FormatEngine ToFormatEngine(this string? md) => new FormatEngine(md);
}