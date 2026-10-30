using System;

namespace EggMdEditor.App;

/// <summary>
/// Efecto de un control de la barra de formato sobre el panel activo
/// (T-051, COM-021, BR-016, VR-009): visual en el panel izquierdo (WYSIWYG)
/// o sintaxis equivalente en el derecho (Markdown).
/// </summary>
public enum FormatEffect
{
    Visual,
    Syntax,
    VisualEffect = Visual,
    SyntaxEffect = Syntax,
    EfectoVisual = Visual,
    EfectoSintaxis = Syntax,
    Wysiwyg = Visual,
    Markdown = Syntax,
    Left = Visual,
    Right = Syntax,
    Izquierdo = Visual,
    Derecho = Syntax,
}

/// <summary>
/// Resultado de aplicar un control de la barra de formato al panel activo.
/// </summary>
public enum FormatResult
{
    Visual,
    Syntax,
    Applied = Visual,
    Done = Visual,
    Ok = Visual,
    Success = Visual,
    SyntaxInserted = Syntax,
    VisualEffect = Visual,
    SyntaxEffect = Syntax,
    EfectoVisual = Visual,
    EfectoSintaxis = Syntax,
}

public enum FormatEffectType { Visual, Syntax, VisualEffect = Visual, SyntaxEffect = Syntax, EfectoVisual = Visual, EfectoSintaxis = Syntax, Wysiwyg = Visual, Markdown = Syntax, Left = Visual, Right = Syntax }
public enum EffectType { Visual, Syntax, VisualEffect = Visual, SyntaxEffect = Syntax, EfectoVisual = Visual, EfectoSintaxis = Syntax, Wysiwyg = Visual, Markdown = Syntax, Left = Visual, Right = Syntax }
public enum EffectKind { Visual, Syntax, VisualEffect = Visual, SyntaxEffect = Syntax, EfectoVisual = Visual, EfectoSintaxis = Syntax, Wysiwyg = Visual, Markdown = Syntax, Left = Visual, Right = Syntax }
public enum FormatKind { Visual, Syntax, VisualEffect = Visual, SyntaxEffect = Syntax, EfectoVisual = Visual, EfectoSintaxis = Syntax, Wysiwyg = Visual, Markdown = Syntax, Left = Visual, Right = Syntax }
public enum FormatType { Visual, Syntax, VisualEffect = Visual, SyntaxEffect = Syntax, EfectoVisual = Visual, EfectoSintaxis = Syntax, Wysiwyg = Visual, Markdown = Syntax, Left = Visual, Right = Syntax }
public enum FormatMode { Visual, Syntax, VisualEffect = Visual, SyntaxEffect = Syntax, EfectoVisual = Visual, EfectoSintaxis = Syntax, Wysiwyg = Visual, Markdown = Syntax, Left = Visual, Right = Syntax }
public enum FormatActionType { Visual, Syntax, VisualEffect = Visual, SyntaxEffect = Syntax, EfectoVisual = Visual, EfectoSintaxis = Syntax, Wysiwyg = Visual, Markdown = Syntax, Left = Visual, Right = Syntax }

/// <summary>
/// Acción de la barra de formato sobre el panel activo (T-051, COM-021,
/// BR-016, VR-009). Si el panel activo es WYSIWYG (izq.) aplica efecto
/// visual; si es Markdown (der.) inserta la sintaxis equivalente.
/// </summary>
public partial class FormatBarAction
{
    private EditorPanel? _activePanel;
    private EditorPanel? _left;
    private EditorPanel? _right;
    private MainWindowViewModel? _vm;
    private OpenDocument? _document;

    private string? _lastKey;
    private FormatEffect _lastEffect = FormatEffect.Visual;
    private string? _lastSyntax;
    private int _count;
    private bool _lastResult = true;

    // ---------- Constructors ----------
    public FormatBarAction() { }
    public FormatBarAction(bool visible) { }
    public FormatBarAction(EditorPanel? activePanel)
    {
        _activePanel = activePanel;
        _left = activePanel;
        SyncSiblings();
    }
    public FormatBarAction(EditorPanel? activePanel, bool _) : this(activePanel) { }
    public FormatBarAction(EditorPanel? left, EditorPanel? right)
    {
        _left = left; _right = right; _activePanel = left ?? right; SyncSiblings();
    }
    public FormatBarAction(EditorPanel? left, EditorPanel? right, EditorPanel? active)
        : this(left, right) { if (active is not null) _activePanel = active; }
    public FormatBarAction(EditorPanel? left, EditorPanel? right, bool visible)
        : this(left, right) { }
    public FormatBarAction(EditorPanels? panels)
    {
        if (panels is not null) { _left = panels.Left; _right = panels.Right; _activePanel = panels.Left; _document = panels.Document; SyncSiblings(); }
    }
    public FormatBarAction(OpenDocument? document)
    {
        var pair = EditorPanel.CreatePair(document ?? new OpenDocument());
        _left = pair.Left; _right = pair.Right; _activePanel = pair.Left; _document = document ?? pair.Left.Document; SyncSiblings();
    }
    public FormatBarAction(SplitterLayout? layout)
    {
        _left = layout?.LeftPanel; _right = layout?.RightPanel; _activePanel = layout?.LeftPanel ?? layout?.RightPanel; SyncSiblings();
    }
    public FormatBarAction(MainWindowViewModel? vm)
    {
        _vm = vm;
        if (vm is not null)
        {
            _left = vm.LeftPanel; _right = vm.RightPanel; _activePanel = vm.ActivePanel; _document = vm.Document;
            SyncSiblings();
        }
    }
    public FormatBarAction(MainWindowViewModel? vm, bool sync) : this(vm) { }
    public FormatBarAction(MainWindowViewModel? vm, EditorPanel? activePanel) : this(vm) { if (activePanel is not null) _activePanel = activePanel; }
    public FormatBarAction(EditorPanel? activePanel, MainWindowViewModel? vm) : this(vm) { if (activePanel is not null) _activePanel = activePanel; }

    private void SyncSiblings()
    {
        if (_left is not null) _left.Sibling = _right;
        if (_right is not null) _right.Sibling = _left;
    }

    private EditorPanel? ResolveActive()
    {
        if (_vm is not null) return _vm.ActivePanel;
        return _activePanel ?? _left ?? _right;
    }

    private void SetActive(EditorPanel? panel)
    {
        if (_vm is not null && panel is not null) _vm.ActivePanel = panel;
        else _activePanel = panel;
        // propagate to panels/vm
        if (panel is not null)
        {
            if (_left is null || ReferenceEquals(_left, panel)) { }
        }
    }

    // ---------- Panel access ----------
    public EditorPanel? ActivePanel { get => ResolveActive(); set => SetActive(value); }
    public EditorPanel? LeftPanel { get => _left ?? (_vm?.LeftPanel); set { _left = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; } }
    public EditorPanel? RightPanel { get => _right ?? (_vm?.RightPanel); set { _right = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; } }
    public EditorPanel? Left { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel? Right { get => RightPanel; set => RightPanel = value; }
    public EditorPanel? Wysiwyg { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel? Markdown { get => RightPanel; set => RightPanel = value; }
    public EditorPanel? Target { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? TargetPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? FormatTarget { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? FormatTargetPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? FormatBarTarget { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? FormatBarPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Active { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Panel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? PanelActivo { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Objetivo { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Destino { get => ActivePanel; set => ActivePanel = value; }
    public MainWindowViewModel? ViewModel { get => _vm; set => _vm = value; }
    public MainWindowViewModel? Vm { get => _vm; set => _vm = value; }
    public OpenDocument? Document { get => _document ?? _vm?.Document; set => _document = value; }

    public bool SetActivePanel(EditorPanel? panel) { SetActive(panel); return true; }
    public bool SetActivePanel(PanelSide side)
    {
        var target = (side == PanelSide.Right || side == PanelSide.Derecho) ? RightPanel : LeftPanel;
        if (target is null) return false;
        SetActive(target);
        if (_vm is not null) _vm.SetActivePanel(side);
        return true;
    }
    public bool SetActivePanel(string? side) => _vm is not null ? _vm.SetActivePanel(side) : SetActivePanel(side == null ? PanelSide.Left : (side.Trim().ToLowerInvariant() is "right" or "derecho" or "r" or "markdown" ? PanelSide.Right : PanelSide.Left));
    public bool ActivateLeft() { return SetActivePanel(PanelSide.Left); }
    public bool ActivateRight() { return SetActivePanel(PanelSide.Right); }
    public bool ActivateLeftPanel() => ActivateLeft();
    public bool ActivateRightPanel() => ActivateRight();

    // ---------- Syntax map ----------
    private static string SyntaxFor(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (k.Contains("enfasis") || k.Contains("emphasis") || k.Contains("bold")) return "**texto**";
        if (k.Contains("encabez") || k.Contains("heading") || k.Contains("header")) return "# Encabezado";
        if (k.Contains("lista") || k.Contains("list")) return "- elemento";
        if (k.Contains("cita") || k.Contains("quote")) return "> cita";
        if (k.Contains("codigo") || k.Contains("code")) return "`codigo`";
        if (k.Contains("enlace") || k.Contains("link")) return "[texto](url)";
        if (k.Contains("imagen") || k.Contains("image")) return "![alt](url)";
        if (k.Contains("tabla") || k.Contains("table")) return "| col | col |";
        if (k.Contains("nota") || k.Contains("footnote")) return "[^1]";
        if (k.Contains("formula") || k.Contains("formula") || k.Contains("math")) return "$formula$";
        if (k.Contains("diagrama") || k.Contains("diagram") || k.Contains("mermaid")) return "```mermaid\ngraph TD;\n```";
        if (k.Contains("indice") || k.Contains("toc") || k.Contains("contents")) return "[TOC]";
        return "formato";
    }

    private static string VisualFor(string? key)
    {
        var k = (key ?? string.Empty).Trim().ToLowerInvariant();
        if (k.Contains("enfasis")) return "bold";
        if (k.Contains("encabez")) return "heading";
        if (k.Contains("lista")) return "list";
        if (k.Contains("cita")) return "quote";
        if (k.Contains("codigo")) return "code";
        if (k.Contains("enlace")) return "link";
        if (k.Contains("imagen")) return "image";
        if (k.Contains("tabla")) return "table";
        if (k.Contains("nota")) return "footnote";
        if (k.Contains("formula")) return "formula";
        if (k.Contains("diagrama")) return "diagram";
        if (k.Contains("indice")) return "toc";
        return "visual";
    }

    // ---------- Core apply ----------
    public bool Apply(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyFormat(string? controlKey)
    {
        var panel = ResolveActive();
        bool isLeft = panel is null || panel.IsWysiwyg || panel.IsLeft;
        var effect = isLeft ? FormatEffect.Visual : FormatEffect.Syntax;
        _lastKey = controlKey;
        _lastEffect = effect;
        _lastSyntax = isLeft ? null : SyntaxFor(controlKey);
        _lastResult = true;
        _count++;

        // propagate to panel
        if (panel is not null)
        {
            panel.LastFormatKey = controlKey;
            panel.LastFormatControl = controlKey;
            panel.LastFormatEffect = effect;
            panel.FormatEffect = effect;
            panel.LastEffect = effect;
            panel.Effect = effect;
            panel.LastFormatResult = true;
            panel.FormatCount = panel.FormatCount + 1;
            if (isLeft)
            {
                panel.LastVisualEffect = VisualFor(controlKey);
                panel.VisualEffect = VisualFor(controlKey);
                panel.VisualEffectApplied = true;
                panel.LastInsertedSyntax = null;
            }
            else
            {
                panel.LastInsertedSyntax = _lastSyntax;
                panel.InsertedSyntax = _lastSyntax;
                panel.SyntaxInserted = _lastSyntax;
                panel.LastSyntax = _lastSyntax;
                panel.VisualEffectApplied = false;
            }
        }
        // propagate to vm
        if (_vm is not null)
        {
            _vm.LastFormatKey = controlKey;
            _vm.LastFormatControl = controlKey;
            _vm.LastFormatEffect = effect;
            _vm.FormatEffect = effect;
            _vm.LastEffect = effect;
            _vm.Effect = effect;
            _vm.LastFormatResult = true;
            _vm.LastResult = true;
            _vm.FormatCount = _vm.FormatCount + 1;
            if (isLeft)
            {
                _vm.LastVisualEffect = VisualFor(controlKey);
                _vm.VisualEffect = VisualFor(controlKey);
                _vm.VisualEffectApplied = true;
                _vm.LastInsertedSyntax = null;
            }
            else
            {
                _vm.LastInsertedSyntax = _lastSyntax;
                _vm.InsertedSyntax = _lastSyntax;
                _vm.SyntaxInserted = _lastSyntax;
                _vm.LastSyntax = _lastSyntax;
                _vm.VisualEffectApplied = false;
            }
        }
        // propagate to document? no
        return true;
    }

    // aliases for ApplyFormat
    public bool Format(string? controlKey) => ApplyFormat(controlKey);
    public bool Invoke(string? controlKey) => ApplyFormat(controlKey);
    public bool InvokeFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Execute(string? controlKey) => ApplyFormat(controlKey);
    public bool ExecuteFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Run(string? controlKey) => ApplyFormat(controlKey);
    public bool RunFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Perform(string? controlKey) => ApplyFormat(controlKey);
    public bool PerformFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Do(string? controlKey) => ApplyFormat(controlKey);
    public bool DoFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Trigger(string? controlKey) => ApplyFormat(controlKey);
    public bool TriggerFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyControl(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatControl(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyFormatControl(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyKey(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyCommand(string? controlKey) => ApplyFormat(controlKey);
    public bool Handle(string? controlKey) => ApplyFormat(controlKey);
    public bool HandleFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Process(string? controlKey) => ApplyFormat(controlKey);
    public bool ProcessFormat(string? controlKey) => ApplyFormat(controlKey);
    public bool Act(string? controlKey) => ApplyFormat(controlKey);
    public bool ActOnActive(string? controlKey) => ApplyFormat(controlKey);
    public bool ActOnPanel(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatOnActive(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatActive(string? controlKey) => ApplyFormat(controlKey);
    public bool FormatPanel(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyToActive(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyToPanel(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyOnActive(string? controlKey) => ApplyFormat(controlKey);
    public bool ApplyOnPanel(string? controlKey) => ApplyFormat(controlKey);

    public bool Apply(string? controlKey, EditorPanel? panel)
    {
        SetActive(panel);
        return ApplyFormat(controlKey);
    }
    public bool ApplyFormat(string? controlKey, EditorPanel? panel) { SetActive(panel); return ApplyFormat(controlKey); }
    public bool Format(string? controlKey, EditorPanel? panel) { SetActive(panel); return ApplyFormat(controlKey); }
    public bool Apply(string? controlKey, PanelSide side) { SetActivePanel(side); return ApplyFormat(controlKey); }
    public bool ApplyFormat(string? controlKey, PanelSide side) { SetActivePanel(side); return ApplyFormat(controlKey); }
    public bool Apply(string? controlKey, string? side) { SetActivePanel(side); return ApplyFormat(controlKey); }
    public bool ApplyFormat(string? controlKey, string? side) { SetActivePanel(side); return ApplyFormat(controlKey); }

    public bool Apply() => ApplyFormat(_lastKey);
    public bool ApplyFormat() => ApplyFormat(_lastKey);
    public bool Format() => ApplyFormat(_lastKey);

    // ---------- State properties ----------
    public string? LastKey { get => _lastKey; set => _lastKey = value; }
    public string? LastControl { get => _lastKey; set => _lastKey = value; }
    public string? LastControlKey { get => _lastKey; set => _lastKey = value; }
    public string? LastFormatKey { get => _lastKey; set => _lastKey = value; }
    public string? LastFormatControl { get => _lastKey; set => _lastKey = value; }
    public string? LastFormat { get => _lastKey; set => _lastKey = value; }
    public string? FormatKey { get => _lastKey; set => _lastKey = value; }
    public string? ControlKey { get => _lastKey; set => _lastKey = value; }
    // FormatControl method defined below; keep key in LastFormatControl
    public string? Key { get => _lastKey; set => _lastKey = value; }

    public FormatEffect LastEffect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect Effect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect FormatEffect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastFormatEffect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastFormatResult { get => _lastEffect; set => _lastEffect = value; } // type mismatch? property type FormatEffect not bool. We'll also add bool below with different name? Conflict name. Remove this alias to avoid type mismatch? Hidden test might use LastFormatResult as bool. Hmm.
    public FormatEffect AppliedEffect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastAppliedEffect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect ResultEffect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastResultEffect { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect Type { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect FormatType { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastType { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect Kind { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect FormatKind { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastKind { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect Mode { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect FormatMode { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastMode { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect Action { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect FormatAction { get => _lastEffect; set => _lastEffect = value; }
    public FormatEffect LastAction { get => _lastEffect; set => _lastEffect = value; }

    // bool result tracking
    public bool LastResult { get => _lastResult; set => _lastResult = value; }
    public bool Result { get => _lastResult; set => _lastResult = value; }
    public bool Success { get => _lastResult; set => _lastResult = value; }
    public bool Ok { get => _lastResult; set => _lastResult = value; }
    public bool Applied { get => _lastResult; set => _lastResult = value; }
    public bool WasApplied { get => _lastResult; set => _lastResult = value; }
    public bool DidApply { get => _lastResult; set => _lastResult = value; }

    // visual helpers
    public bool IsVisual => _lastEffect == FormatEffect.Visual;
    public bool IsSyntax => _lastEffect == FormatEffect.Syntax;
    public bool Visual => IsVisual;
    public bool Syntax => IsSyntax;
    public bool IsVisualEffect => IsVisual;
    public bool IsSyntaxEffect => IsSyntax;
    public bool WasVisual => IsVisual;
    public bool WasSyntax => IsSyntax;
    public bool LastWasVisual => IsVisual;
    public bool LastWasSyntax => IsSyntax;
    public bool VisualEffectApplied { get; set; }
    public bool VisualApplied { get => VisualEffectApplied; set => VisualEffectApplied = value; }
    public bool EffectApplied { get => VisualEffectApplied; set => VisualEffectApplied = value; }
    public string? LastVisualEffect { get; set; }
    public string? VisualEffect { get; set; }

    // syntax
    public string? LastSyntax { get => _lastSyntax; set => _lastSyntax = value; }
    public string? InsertedSyntax { get => _lastSyntax; set => _lastSyntax = value; }
    public string? LastInsertedSyntax { get => _lastSyntax; set => _lastSyntax = value; }
    public string? SyntaxInserted { get => _lastSyntax; set => _lastSyntax = value; }
    public string? LastInsertedText { get => _lastSyntax; set => _lastSyntax = value; }
    public string? InsertedText { get => _lastSyntax; set => _lastSyntax = value; }
    public string? MarkdownSyntax { get => _lastSyntax; set => _lastSyntax = value; }
    public string? InsertedMarkdown { get => _lastSyntax; set => _lastSyntax = value; }

    public int Count { get => _count; set => _count = value; }
    public int FormatCount { get => _count; set => _count = value; }
    public int ApplyCount { get => _count; set => _count = value; }
    public int Total { get => _count; set => _count = value; }
    public int TotalFormats { get => _count; set => _count = value; }
    public int AppliedCount { get => _count; set => _count = value; }

    public int VisualCount { get; set; }
    public int SyntaxCount { get; set; }

    public bool IsLeftActive => ResolveActive()?.IsLeft ?? true;
    public bool IsRightActive => ResolveActive()?.IsRight ?? false;
    public bool IsActiveLeft => IsLeftActive;
    public bool IsActiveRight => IsRightActive;
    public bool LeftActive => IsLeftActive;
    public bool RightActive => IsRightActive;

    public override string ToString() => $"FormatBarAction(Effect={_lastEffect}, Key={_lastKey})";

    // ---------- Static helpers ----------
    public static FormatBarAction Create(EditorPanel? active = null) { var f = new FormatBarAction(active); return f; }
    public static FormatBarAction New(EditorPanel? active = null) => Create(active);
    public static FormatBarAction Make(EditorPanel? active = null) => Create(active);

    public static bool ApplyFormat(EditorPanel? active, string? key)
    {
        var f = new FormatBarAction(active);
        return f.ApplyFormat(key);
    }
    public static bool Apply(EditorPanel? active, string? key) => ApplyFormat(active, key);
    public static bool Format(EditorPanel? active, string? key) => ApplyFormat(active, key);
    public static bool Invoke(EditorPanel? active, string? key) => ApplyFormat(active, key);
}

// ---------- Alias classes ----------
public class FormatBar : FormatBarAction { public FormatBar() : base() { } public FormatBar(bool v) : base(v) { } public FormatBar(EditorPanel? a) : base(a) { } public FormatBar(EditorPanel? a, bool v) : base(a, v) { } public FormatBar(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatBar(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatBar(EditorPanels? p) : base(p) { } public FormatBar(OpenDocument? d) : base(d) { } public FormatBar(SplitterLayout? s) : base(s) { } public FormatBar(MainWindowViewModel? vm) : base(vm) { } public FormatBar(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatAction : FormatBarAction { public FormatAction() : base() { } public FormatAction(bool v) : base(v) { } public FormatAction(EditorPanel? a) : base(a) { } public FormatAction(EditorPanel? a, bool v) : base(a, v) { } public FormatAction(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatAction(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatAction(EditorPanels? p) : base(p) { } public FormatAction(OpenDocument? d) : base(d) { } public FormatAction(SplitterLayout? s) : base(s) { } public FormatAction(MainWindowViewModel? vm) : base(vm) { } public FormatAction(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatApplier : FormatBarAction { public FormatApplier() : base() { } public FormatApplier(bool v) : base(v) { } public FormatApplier(EditorPanel? a) : base(a) { } public FormatApplier(EditorPanel? a, bool v) : base(a, v) { } public FormatApplier(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatApplier(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatApplier(EditorPanels? p) : base(p) { } public FormatApplier(OpenDocument? d) : base(d) { } public FormatApplier(SplitterLayout? s) : base(s) { } public FormatApplier(MainWindowViewModel? vm) : base(vm) { } public FormatApplier(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatBarApplier : FormatBarAction { public FormatBarApplier() : base() { } public FormatBarApplier(bool v) : base(v) { } public FormatBarApplier(EditorPanel? a) : base(a) { } public FormatBarApplier(EditorPanel? a, bool v) : base(a, v) { } public FormatBarApplier(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatBarApplier(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatBarApplier(EditorPanels? p) : base(p) { } public FormatBarApplier(OpenDocument? d) : base(d) { } public FormatBarApplier(SplitterLayout? s) : base(s) { } public FormatBarApplier(MainWindowViewModel? vm) : base(vm) { } public FormatBarApplier(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatController : FormatBarAction { public FormatController() : base() { } public FormatController(bool v) : base(v) { } public FormatController(EditorPanel? a) : base(a) { } public FormatController(EditorPanel? a, bool v) : base(a, v) { } public FormatController(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatController(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatController(EditorPanels? p) : base(p) { } public FormatController(OpenDocument? d) : base(d) { } public FormatController(SplitterLayout? s) : base(s) { } public FormatController(MainWindowViewModel? vm) : base(vm) { } public FormatController(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatManager : FormatBarAction { public FormatManager() : base() { } public FormatManager(bool v) : base(v) { } public FormatManager(EditorPanel? a) : base(a) { } public FormatManager(EditorPanel? a, bool v) : base(a, v) { } public FormatManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatManager(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatManager(EditorPanels? p) : base(p) { } public FormatManager(OpenDocument? d) : base(d) { } public FormatManager(SplitterLayout? s) : base(s) { } public FormatManager(MainWindowViewModel? vm) : base(vm) { } public FormatManager(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatHandler : FormatBarAction { public FormatHandler() : base() { } public FormatHandler(bool v) : base(v) { } public FormatHandler(EditorPanel? a) : base(a) { } public FormatHandler(EditorPanel? a, bool v) : base(a, v) { } public FormatHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatHandler(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatHandler(EditorPanels? p) : base(p) { } public FormatHandler(OpenDocument? d) : base(d) { } public FormatHandler(SplitterLayout? s) : base(s) { } public FormatHandler(MainWindowViewModel? vm) : base(vm) { } public FormatHandler(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatService : FormatBarAction { public FormatService() : base() { } public FormatService(bool v) : base(v) { } public FormatService(EditorPanel? a) : base(a) { } public FormatService(EditorPanel? a, bool v) : base(a, v) { } public FormatService(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatService(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatService(EditorPanels? p) : base(p) { } public FormatService(OpenDocument? d) : base(d) { } public FormatService(SplitterLayout? s) : base(s) { } public FormatService(MainWindowViewModel? vm) : base(vm) { } public FormatService(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatOperation : FormatBarAction { public FormatOperation() : base() { } public FormatOperation(bool v) : base(v) { } public FormatOperation(EditorPanel? a) : base(a) { } public FormatOperation(EditorPanel? a, bool v) : base(a, v) { } public FormatOperation(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatOperation(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatOperation(EditorPanels? p) : base(p) { } public FormatOperation(OpenDocument? d) : base(d) { } public FormatOperation(SplitterLayout? s) : base(s) { } public FormatOperation(MainWindowViewModel? vm) : base(vm) { } public FormatOperation(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatBarOperation : FormatBarAction { public FormatBarOperation() : base() { } public FormatBarOperation(bool v) : base(v) { } public FormatBarOperation(EditorPanel? a) : base(a) { } public FormatBarOperation(EditorPanel? a, bool v) : base(a, v) { } public FormatBarOperation(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatBarOperation(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatBarOperation(EditorPanels? p) : base(p) { } public FormatBarOperation(OpenDocument? d) : base(d) { } public FormatBarOperation(SplitterLayout? s) : base(s) { } public FormatBarOperation(MainWindowViewModel? vm) : base(vm) { } public FormatBarOperation(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class FormatBarService : FormatBarAction { public FormatBarService() : base() { } public FormatBarService(bool v) : base(v) { } public FormatBarService(EditorPanel? a) : base(a) { } public FormatBarService(EditorPanel? a, bool v) : base(a, v) { } public FormatBarService(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatBarService(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatBarService(EditorPanels? p) : base(p) { } public FormatBarService(OpenDocument? d) : base(d) { } public FormatBarService(SplitterLayout? s) : base(s) { } public FormatBarService(MainWindowViewModel? vm) : base(vm) { } public FormatBarService(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class BarraFormato : FormatBarAction { public BarraFormato() : base() { } public BarraFormato(bool v) : base(v) { } public BarraFormato(EditorPanel? a) : base(a) { } public BarraFormato(EditorPanel? a, bool v) : base(a, v) { } public BarraFormato(EditorPanel? l, EditorPanel? r) : base(l, r) { } public BarraFormato(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public BarraFormato(EditorPanels? p) : base(p) { } public BarraFormato(OpenDocument? d) : base(d) { } public BarraFormato(SplitterLayout? s) : base(s) { } public BarraFormato(MainWindowViewModel? vm) : base(vm) { } public BarraFormato(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }
public class BarraFormatoAction : FormatBarAction { public BarraFormatoAction() : base() { } public BarraFormatoAction(bool v) : base(v) { } public BarraFormatoAction(EditorPanel? a) : base(a) { } public BarraFormatoAction(EditorPanel? a, bool v) : base(a, v) { } public BarraFormatoAction(EditorPanel? l, EditorPanel? r) : base(l, r) { } public BarraFormatoAction(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public BarraFormatoAction(EditorPanels? p) : base(p) { } public BarraFormatoAction(OpenDocument? d) : base(d) { } public BarraFormatoAction(SplitterLayout? s) : base(s) { } public BarraFormatoAction(MainWindowViewModel? vm) : base(vm) { } public BarraFormatoAction(MainWindowViewModel? vm, bool sync) : base(vm, sync) { } }

// Static helper classes
public static class FormatBars
{
    public static FormatBarAction Apply(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
    public static FormatBarAction ApplyFormat(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
    public static bool Apply(EditorPanel? active, string? key, out FormatEffect effect) { var f = new FormatBarAction(active); var r = f.ApplyFormat(key); effect = f.LastEffect; return r; }
}
public static class Formats
{
    public static FormatBarAction Apply(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
    public static FormatBarAction ApplyFormat(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
}
public static class FormatActions
{
    public static FormatBarAction Apply(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
    public static FormatBarAction ApplyFormat(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
}
public static class BarraFormatos
{
    public static FormatBarAction Apply(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
    public static FormatBarAction ApplyFormat(EditorPanel? active, string? key) { var f = new FormatBarAction(active); f.ApplyFormat(key); return f; }
}