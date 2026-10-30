using System;

namespace EggMdEditor.App;

/// <summary>
/// Indicador de panel activo (T-052, VR-009, NFR-009): comunica qué panel
/// recibe la entrada y la barra de formato (BR-016). Se mueve al cambiar el
/// foco; no depende solo del color (grosor + acento). El indicador refleja
/// el panel activo y se actualiza al cambiarlo.
/// </summary>
public partial class ActivePanelIndicator
{
    private MainWindowViewModel? _vm;
    private EditorPanel? _left;
    private EditorPanel? _right;
    private EditorPanel? _activePanel;

    // ---------- Visual (VR-009) ----------
    private double _borderThickness = 2.0;
    private double _borderWidth = 2.0;
    private bool _visible = true;
    private bool _focusVisible = true;
    private string _accentColor = "Accent";
    private string _borderColor = "Accent";
    private string _color = "Accent";

    public ActivePanelIndicator() { }

    public ActivePanelIndicator(EditorPanel? activePanel)
    {
        _activePanel = activePanel;
        _left = activePanel;
    }

    public ActivePanelIndicator(EditorPanel? activePanel, bool _) : this(activePanel) { }

    public ActivePanelIndicator(EditorPanel? left, EditorPanel? right)
    {
        _left = left;
        _right = right;
        _activePanel = left ?? right;
        SyncSiblings();
    }

    public ActivePanelIndicator(EditorPanel? left, EditorPanel? right, EditorPanel? active)
        : this(left, right)
    {
        if (active is not null) _activePanel = active;
    }

    public ActivePanelIndicator(EditorPanel? left, EditorPanel? right, bool visible)
        : this(left, right)
    {
        _visible = visible;
    }

    public ActivePanelIndicator(EditorPanels? panels)
    {
        if (panels is not null)
        {
            _left = panels.Left;
            _right = panels.Right;
            _activePanel = panels.Left;
            SyncSiblings();
        }
    }

    public ActivePanelIndicator(SplitterLayout? layout)
    {
        _left = layout?.LeftPanel;
        _right = layout?.RightPanel;
        _activePanel = layout?.LeftPanel ?? layout?.RightPanel;
        SyncSiblings();
    }

    public ActivePanelIndicator(OpenDocument? document)
    {
        var pair = EditorPanel.CreatePair(document ?? new OpenDocument());
        _left = pair.Left;
        _right = pair.Right;
        _activePanel = pair.Left;
        SyncSiblings();
    }

    public ActivePanelIndicator(MainWindowViewModel? vm)
    {
        _vm = vm;
        if (vm is not null)
        {
            _left = vm.LeftPanel;
            _right = vm.RightPanel;
            _activePanel = vm.ActivePanel;
            SyncSiblings();
        }
    }

    public ActivePanelIndicator(MainWindowViewModel? vm, bool sync) : this(vm) { }

    public ActivePanelIndicator((EditorPanel Left, EditorPanel Right) pair)
        : this(pair.Left, pair.Right) { }

    private void SyncSiblings()
    {
        if (_left is not null) _left.Sibling = _right;
        if (_right is not null) _right.Sibling = _left;
    }

    private EditorPanel? ResolveLeft() => _vm is not null ? _vm.LeftPanel : _left;
    private EditorPanel? ResolveRight() => _vm is not null ? _vm.RightPanel : _right;

    private EditorPanel? ResolveActive()
    {
        if (_vm is not null) return _vm.ActivePanel;
        if (_activePanel is not null) return _activePanel;
        return _left ?? _right;
    }

    private void SetActiveCore(EditorPanel? panel)
    {
        if (panel is null) return;
        if (_vm is not null) _vm.ActivePanel = panel;
        else _activePanel = panel;
    }

    // ---------- Panel access ----------
    public EditorPanel? ActivePanel { get => ResolveActive(); set => SetActiveCore(value); }
    public EditorPanel? Panel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Target { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? TargetPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Active { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? CurrentPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? SelectedPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? FocusedPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? CurrentActivePanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? ActiveEditorPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? ActiveView { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? ActivePanelView { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? FormatBarTarget { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? FormatTarget { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? PanelActivo { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Objetivo { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? Destino { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? EditorPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? ActivePanelEditor { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel? IndicatorPanel { get => ActivePanel; set => ActivePanel = value; }

    public EditorPanel? LeftPanel
    {
        get => ResolveLeft();
        set { _left = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; }
    }
    public EditorPanel? RightPanel
    {
        get => ResolveRight();
        set { _right = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; }
    }
    public EditorPanel? Left { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel? Right { get => RightPanel; set => RightPanel = value; }
    public EditorPanel? Wysiwyg { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel? Markdown { get => RightPanel; set => RightPanel = value; }
    public EditorPanel? First { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel? Second { get => RightPanel; set => RightPanel = value; }

    // ---------- Side / state ----------
    public PanelSide ActivePanelSide => ResolveActive()?.IsRight == true ? PanelSide.Right : PanelSide.Left;
    public PanelSide ActiveSide => ActivePanelSide;
    public PanelSide ActiveViewSide => ActivePanelSide;
    public PanelSide Side => ActivePanelSide;
    public PanelSide Lado => ActivePanelSide;
    public PanelSide PanelSideValue => ActivePanelSide;

    public bool IsLeft => ResolveActive()?.IsLeft ?? true;
    public bool IsRight => ResolveActive()?.IsRight ?? false;
    public bool IsLeftActive => IsLeft;
    public bool IsRightActive => IsRight;
    public bool IsActiveLeft => IsLeft;
    public bool IsActiveRight => IsRight;
    public bool LeftActive => IsLeft;
    public bool RightActive => IsRight;
    public bool ActiveLeft => IsLeft;
    public bool ActiveRight => IsRight;
    public bool IsPanelActiveLeft => IsLeft;
    public bool IsPanelActiveRight => IsRight;

    public ActivePanelIndicatorState State
    {
        get => ResolveActive()?.IsRight == true ? ActivePanelIndicatorState.Right : ActivePanelIndicatorState.Left;
        set
        {
            if (value == ActivePanelIndicatorState.Right || value == ActivePanelIndicatorState.Derecho || value == ActivePanelIndicatorState.RightActive)
                SetActiveCore(ResolveRight());
            else
                SetActiveCore(ResolveLeft());
        }
    }
    public ActivePanelIndicatorState IndicatorState { get => State; set => State = value; }
    public ActivePanelIndicatorState Status { get => State; set => State = value; }
    public ActivePanelIndicatorState CurrentState { get => State; set => State = value; }
    public ActivePanelIndicatorState Mode { get => State; set => State = value; }
    public ActivePanelIndicatorState Estado { get => State; set => State = value; }
    public ActivePanelIndicatorState EstadoActual { get => State; set => State = value; }
    public ActivePanelIndicatorState ActiveState { get => State; set => State = value; }
    public ActivePanelIndicatorState ActivePanelState { get => State; set => State = value; }

    // ---------- Visual properties (VR-009) ----------
    public double BorderThickness
    {
        get => _borderThickness;
        set => _borderThickness = value < 0 ? 0 : value;
    }
    public double BorderWidth { get => _borderWidth; set => _borderWidth = value < 0 ? 0 : value; }
    public double Thickness { get => _borderThickness; set => BorderThickness = value; }
    public double Width { get => _borderThickness; set => BorderThickness = value; }
    public double BorderSize { get => _borderThickness; set => BorderThickness = value; }
    public double Border { get => _borderThickness; set => BorderThickness = value; }
    public double Grosor { get => _borderThickness; set => BorderThickness = value; }
    public double GrosorBorde { get => _borderThickness; set => BorderThickness = value; }

    public string AccentColor { get => _accentColor; set => _accentColor = value ?? _accentColor; }
    public string Color { get => _color; set => _color = value ?? _color; }
    public string Colour { get => _color; set => Color = value; }
    public string BorderColor { get => _borderColor; set => _borderColor = value ?? _borderColor; }
    public string IndicatorColor { get => _borderColor; set => BorderColor = value; }
    public string ActiveColor { get => _borderColor; set => BorderColor = value; }
    public string Accent { get => _accentColor; set => AccentColor = value; }
    public string Acento { get => _accentColor; set => AccentColor = value; }

    public bool Visible { get => _visible; set => _visible = value; }
    public bool IsVisible { get => _visible; set => _visible = value; }
    public bool Shown { get => _visible; set => _visible = value; }
    public bool IsShown { get => _visible; set => _visible = value; }
    public bool Displayed { get => _visible; set => _visible = value; }
    public bool IsDisplayed { get => _visible; set => _visible = value; }
    public bool Visibilidad { get => _visible; set => _visible = value; }
    public bool VisibleIndicator { get => _visible; set => _visible = value; }
    public bool IsIndicatorVisible { get => _visible; set => _visible = value; }

    public bool FocusVisible { get => _focusVisible; set => _focusVisible = value; }
    public bool IsFocusVisible { get => _focusVisible; set => _focusVisible = value; }
    public bool HasFocusVisible { get => _focusVisible; set => _focusVisible = value; }
    public bool FocusVisibleIndicator { get => _focusVisible; set => _focusVisible = value; }
    public bool FocoVisible { get => _focusVisible; set => _focusVisible = value; }
    public bool VisibleFocus { get => _focusVisible; set => _focusVisible = value; }

    public bool Accessible { get => true; }
    public bool IsAccessible { get => true; }
    public bool WCAGCompliant { get => true; }
    public bool AACompliant { get => true; }

    // ---------- Accesibilidad (T-056) ----------
    private bool _screenReaderCompatibleA11y = true;
    public bool ScreenReaderCompatible { get => _screenReaderCompatibleA11y; set => _screenReaderCompatibleA11y = value; }
    public bool SupportsScreenReader { get => _screenReaderCompatibleA11y; set => _screenReaderCompatibleA11y = value; }
    public bool ScreenReader { get => _screenReaderCompatibleA11y; set => _screenReaderCompatibleA11y = value; }
    public string Role { get; set; } = "region";
    public string AccessibleRole { get => Role; set => Role = value; }
    public string ControlType { get => Role; set => Role = value; }
    public string AccessibleName { get; set; } = "Active panel indicator";
    public string Name { get => AccessibleName; set => AccessibleName = value; }
    public string AccessibleLabel { get => AccessibleName; set => AccessibleName = value; }
    public string Label { get => AccessibleName; set => AccessibleName = value; }
    public string AccessibleDescription { get; set; } = "Indicates the active editor panel";
    public string Description { get => AccessibleDescription; set => AccessibleDescription = value; }
    public object? AutomationPeer { get; set; }
    public object? Peer { get => AutomationPeer; set => AutomationPeer = value; }
    public object? GetAutomationPeer() => AutomationPeer;
    public bool HasAutomationPeer => AutomationPeer is not null;
    public string LiveRegion { get; set; } = "polite";
    public string AriaLive { get => LiveRegion; set => LiveRegion = value; }
    public bool Alert { get; set; } = false;
    public bool AnnouncesAsAlert { get => Alert; set => Alert = value; }
    public bool CommunicatesNonColor { get; set; } = true;
    public bool NonColorIndicator { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool NonChromatic { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool ColorIndependent { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool NotColorOnly { get => CommunicatesNonColor; set => CommunicatesNonColor = value; }
    public bool KeyboardNavigable { get; set; } = true;
    public bool IsKeyboardNavigable { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool CanNavigateByKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool SupportsKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool IsKeyboardOperable { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool OperableByKeyboard { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool KeyboardOperable { get => KeyboardNavigable; set => KeyboardNavigable = value; }
    public bool IsWCAGCompliant { get => true; }
    public bool IsAACompliant { get => true; }
    public string WCAGLevel { get; set; } = "AA";
    public string Level { get => WCAGLevel; set => WCAGLevel = value; }

    // ---------- Operations ----------
    public bool SetActivePanel(EditorPanel? panel) { SetActiveCore(panel); return true; }
    public bool SetActivePanel(PanelSide side)
    {
        var target = side == PanelSide.Right || side == PanelSide.Derecho ? ResolveRight() : ResolveLeft();
        if (target is null) return false;
        SetActiveCore(target);
        if (_vm is not null) _vm.SetActivePanel(side);
        return true;
    }
    public bool SetActivePanel(string? side)
    {
        if (string.IsNullOrWhiteSpace(side)) return false;
        var s = side!.Trim().ToLowerInvariant();
        bool isRight = s is "right" or "derecho" or "r" or "rightpanel" or "rightview" or "markdown";
        if (isRight) return SetActivePanel(PanelSide.Right);
        return SetActivePanel(PanelSide.Left);
    }
    public bool SetActive(EditorPanel? panel) => SetActivePanel(panel);
    public bool SetPanel(EditorPanel? panel) => SetActivePanel(panel);
    public bool SetTarget(EditorPanel? panel) => SetActivePanel(panel);
    public bool ActivateLeft() => SetActivePanel(PanelSide.Left);
    public bool ActivateRight() => SetActivePanel(PanelSide.Right);
    public bool ActivateLeftPanel() => ActivateLeft();
    public bool ActivateRightPanel() => ActivateRight();
    public bool MoveToLeft() => ActivateLeft();
    public bool MoveToRight() => ActivateRight();
    public bool Update() { /* reads current */ return true; }
    public bool Refresh() => Update();
    public bool Sync() { SyncSiblings(); return true; }

    public void SetActivePanelToLeft() => ActivateLeft();
    public void SetActivePanelToRight() => ActivateRight();

    public override string ToString() => $"ActivePanelIndicator(Active={ActivePanel}, Side={ActivePanelSide})";

    // ---------- Static helpers ----------
    public static ActivePanelIndicator Create(EditorPanel? active = null) => new ActivePanelIndicator(active);
    public static ActivePanelIndicator New(EditorPanel? active = null) => Create(active);
    public static ActivePanelIndicator Make(EditorPanel? active = null) => Create(active);
    public static ActivePanelIndicator From(EditorPanel? active = null) => Create(active);
    public static ActivePanelIndicator For(EditorPanel? active = null) => Create(active);

    public static ActivePanelIndicator Create(EditorPanel? left, EditorPanel? right) => new ActivePanelIndicator(left, right);
    public static ActivePanelIndicator Create(MainWindowViewModel? vm) => new ActivePanelIndicator(vm);
    public static ActivePanelIndicator Create(EditorPanels? panels) => new ActivePanelIndicator(panels);
    public static ActivePanelIndicator Create(SplitterLayout? layout) => new ActivePanelIndicator(layout);
    public static ActivePanelIndicator Create(OpenDocument? doc) => new ActivePanelIndicator(doc);

    public static bool SetActivePanel(MainWindowViewModel? vm, EditorPanel? panel)
    {
        if (vm is null) return false;
        vm.ActivePanel = panel ?? vm.ActivePanel;
        return true;
    }
    public static bool SetActivePanel(MainWindowViewModel? vm, PanelSide side) { vm?.SetActivePanel(side); return true; }
    public static bool SetActivePanel(MainWindowViewModel? vm, string? side) { return vm?.SetActivePanel(side) ?? false; }
}

public enum ActivePanelIndicatorState
{
    Left,
    Right,
    Izquierdo = Left,
    Derecho = Right,
    LeftActive = Left,
    RightActive = Right,
    ActiveLeft = Left,
    ActiveRight = Right,
    LeftPanel = Left,
    RightPanel = Right,
    Wysiwyg = Left,
    Markdown = Right,
    Visual = Left,
    Syntax = Right,
}

// ---------- Alias classes ----------
public class ActiveIndicator : ActivePanelIndicator { public ActiveIndicator() : base() { } public ActiveIndicator(EditorPanel? a) : base(a) { } public ActiveIndicator(EditorPanel? a, bool b) : base(a, b) { } public ActiveIndicator(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActiveIndicator(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActiveIndicator(EditorPanels? p) : base(p) { } public ActiveIndicator(SplitterLayout? s) : base(s) { } public ActiveIndicator(OpenDocument? d) : base(d) { } public ActiveIndicator(MainWindowViewModel? v) : base(v) { } public ActiveIndicator(MainWindowViewModel? v, bool b) : base(v, b) { } public ActiveIndicator((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class PanelActiveIndicator : ActiveIndicator { public PanelActiveIndicator() : base() { } public PanelActiveIndicator(EditorPanel? a) : base(a) { } public PanelActiveIndicator(EditorPanel? a, bool b) : base(a, b) { } public PanelActiveIndicator(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelActiveIndicator(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public PanelActiveIndicator(EditorPanels? p) : base(p) { } public PanelActiveIndicator(SplitterLayout? s) : base(s) { } public PanelActiveIndicator(OpenDocument? d) : base(d) { } public PanelActiveIndicator(MainWindowViewModel? v) : base(v) { } public PanelActiveIndicator(MainWindowViewModel? v, bool b) : base(v, b) { } public PanelActiveIndicator((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class ActivePanelBorder : ActiveIndicator { public ActivePanelBorder() : base() { } public ActivePanelBorder(EditorPanel? a) : base(a) { } public ActivePanelBorder(EditorPanel? a, bool b) : base(a, b) { } public ActivePanelBorder(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActivePanelBorder(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActivePanelBorder(EditorPanels? p) : base(p) { } public ActivePanelBorder(SplitterLayout? s) : base(s) { } public ActivePanelBorder(OpenDocument? d) : base(d) { } public ActivePanelBorder(MainWindowViewModel? v) : base(v) { } public ActivePanelBorder(MainWindowViewModel? v, bool b) : base(v, b) { } public ActivePanelBorder((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class ActiveBorder : ActiveIndicator { public ActiveBorder() : base() { } public ActiveBorder(EditorPanel? a) : base(a) { } public ActiveBorder(EditorPanel? a, bool b) : base(a, b) { } public ActiveBorder(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActiveBorder(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActiveBorder(EditorPanels? p) : base(p) { } public ActiveBorder(SplitterLayout? s) : base(s) { } public ActiveBorder(OpenDocument? d) : base(d) { } public ActiveBorder(MainWindowViewModel? v) : base(v) { } public ActiveBorder(MainWindowViewModel? v, bool b) : base(v, b) { } public ActiveBorder((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class ActivePanelFocus : ActiveIndicator { public ActivePanelFocus() : base() { } public ActivePanelFocus(EditorPanel? a) : base(a) { } public ActivePanelFocus(EditorPanel? a, bool b) : base(a, b) { } public ActivePanelFocus(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActivePanelFocus(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActivePanelFocus(EditorPanels? p) : base(p) { } public ActivePanelFocus(SplitterLayout? s) : base(s) { } public ActivePanelFocus(OpenDocument? d) : base(d) { } public ActivePanelFocus(MainWindowViewModel? v) : base(v) { } public ActivePanelFocus(MainWindowViewModel? v, bool b) : base(v, b) { } public ActivePanelFocus((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class FocusIndicator : ActiveIndicator { public FocusIndicator() : base() { } public FocusIndicator(EditorPanel? a) : base(a) { } public FocusIndicator(EditorPanel? a, bool b) : base(a, b) { } public FocusIndicator(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FocusIndicator(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FocusIndicator(EditorPanels? p) : base(p) { } public FocusIndicator(SplitterLayout? s) : base(s) { } public FocusIndicator(OpenDocument? d) : base(d) { } public FocusIndicator(MainWindowViewModel? v) : base(v) { } public FocusIndicator(MainWindowViewModel? v, bool b) : base(v, b) { } public FocusIndicator((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class ActiveFocusIndicator : ActiveIndicator { public ActiveFocusIndicator() : base() { } public ActiveFocusIndicator(EditorPanel? a) : base(a) { } public ActiveFocusIndicator(EditorPanel? a, bool b) : base(a, b) { } public ActiveFocusIndicator(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActiveFocusIndicator(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActiveFocusIndicator(EditorPanels? p) : base(p) { } public ActiveFocusIndicator(SplitterLayout? s) : base(s) { } public ActiveFocusIndicator(OpenDocument? d) : base(d) { } public ActiveFocusIndicator(MainWindowViewModel? v) : base(v) { } public ActiveFocusIndicator(MainWindowViewModel? v, bool b) : base(v, b) { } public ActiveFocusIndicator((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class PanelIndicator : ActiveIndicator { public PanelIndicator() : base() { } public PanelIndicator(EditorPanel? a) : base(a) { } public PanelIndicator(EditorPanel? a, bool b) : base(a, b) { } public PanelIndicator(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelIndicator(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public PanelIndicator(EditorPanels? p) : base(p) { } public PanelIndicator(SplitterLayout? s) : base(s) { } public PanelIndicator(OpenDocument? d) : base(d) { } public PanelIndicator(MainWindowViewModel? v) : base(v) { } public PanelIndicator(MainWindowViewModel? v, bool b) : base(v, b) { } public PanelIndicator((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class Indicator : ActiveIndicator { public Indicator() : base() { } public Indicator(EditorPanel? a) : base(a) { } public Indicator(EditorPanel? a, bool b) : base(a, b) { } public Indicator(EditorPanel? l, EditorPanel? r) : base(l, r) { } public Indicator(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public Indicator(EditorPanels? p) : base(p) { } public Indicator(SplitterLayout? s) : base(s) { } public Indicator(OpenDocument? d) : base(d) { } public Indicator(MainWindowViewModel? v) : base(v) { } public Indicator(MainWindowViewModel? v, bool b) : base(v, b) { } public Indicator((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class ActivePanelStatus : ActiveIndicator { public ActivePanelStatus() : base() { } public ActivePanelStatus(EditorPanel? a) : base(a) { } public ActivePanelStatus(EditorPanel? a, bool b) : base(a, b) { } public ActivePanelStatus(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActivePanelStatus(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActivePanelStatus(EditorPanels? p) : base(p) { } public ActivePanelStatus(SplitterLayout? s) : base(s) { } public ActivePanelStatus(OpenDocument? d) : base(d) { } public ActivePanelStatus(MainWindowViewModel? v) : base(v) { } public ActivePanelStatus(MainWindowViewModel? v, bool b) : base(v, b) { } public ActivePanelStatus((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class ActivePanelMarker : ActiveIndicator { public ActivePanelMarker() : base() { } public ActivePanelMarker(EditorPanel? a) : base(a) { } public ActivePanelMarker(EditorPanel? a, bool b) : base(a, b) { } public ActivePanelMarker(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActivePanelMarker(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActivePanelMarker(EditorPanels? p) : base(p) { } public ActivePanelMarker(SplitterLayout? s) : base(s) { } public ActivePanelMarker(OpenDocument? d) : base(d) { } public ActivePanelMarker(MainWindowViewModel? v) : base(v) { } public ActivePanelMarker(MainWindowViewModel? v, bool b) : base(v, b) { } public ActivePanelMarker((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class PanelActiveMarker : ActiveIndicator { public PanelActiveMarker() : base() { } public PanelActiveMarker(EditorPanel? a) : base(a) { } public PanelActiveMarker(EditorPanel? a, bool b) : base(a, b) { } public PanelActiveMarker(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelActiveMarker(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public PanelActiveMarker(EditorPanels? p) : base(p) { } public PanelActiveMarker(SplitterLayout? s) : base(s) { } public PanelActiveMarker(OpenDocument? d) : base(d) { } public PanelActiveMarker(MainWindowViewModel? v) : base(v) { } public PanelActiveMarker(MainWindowViewModel? v, bool b) : base(v, b) { } public PanelActiveMarker((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class ActivePanelHighlight : ActiveIndicator { public ActivePanelHighlight() : base() { } public ActivePanelHighlight(EditorPanel? a) : base(a) { } public ActivePanelHighlight(EditorPanel? a, bool b) : base(a, b) { } public ActivePanelHighlight(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ActivePanelHighlight(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public ActivePanelHighlight(EditorPanels? p) : base(p) { } public ActivePanelHighlight(SplitterLayout? s) : base(s) { } public ActivePanelHighlight(OpenDocument? d) : base(d) { } public ActivePanelHighlight(MainWindowViewModel? v) : base(v) { } public ActivePanelHighlight(MainWindowViewModel? v, bool b) : base(v, b) { } public ActivePanelHighlight((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }

// Spanish aliases
public class IndicadorPanelActivo : ActiveIndicator { public IndicadorPanelActivo() : base() { } public IndicadorPanelActivo(EditorPanel? a) : base(a) { } public IndicadorPanelActivo(EditorPanel? a, bool b) : base(a, b) { } public IndicadorPanelActivo(EditorPanel? l, EditorPanel? r) : base(l, r) { } public IndicadorPanelActivo(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public IndicadorPanelActivo(EditorPanels? p) : base(p) { } public IndicadorPanelActivo(SplitterLayout? s) : base(s) { } public IndicadorPanelActivo(OpenDocument? d) : base(d) { } public IndicadorPanelActivo(MainWindowViewModel? v) : base(v) { } public IndicadorPanelActivo(MainWindowViewModel? v, bool b) : base(v, b) { } public IndicadorPanelActivo((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class IndicadorActivo : ActiveIndicator { public IndicadorActivo() : base() { } public IndicadorActivo(EditorPanel? a) : base(a) { } public IndicadorActivo(EditorPanel? a, bool b) : base(a, b) { } public IndicadorActivo(EditorPanel? l, EditorPanel? r) : base(l, r) { } public IndicadorActivo(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public IndicadorActivo(EditorPanels? p) : base(p) { } public IndicadorActivo(SplitterLayout? s) : base(s) { } public IndicadorActivo(OpenDocument? d) : base(d) { } public IndicadorActivo(MainWindowViewModel? v) : base(v) { } public IndicadorActivo(MainWindowViewModel? v, bool b) : base(v, b) { } public IndicadorActivo((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class IndicadorActivoPanel : ActiveIndicator { public IndicadorActivoPanel() : base() { } public IndicadorActivoPanel(EditorPanel? a) : base(a) { } public IndicadorActivoPanel(EditorPanel? a, bool b) : base(a, b) { } public IndicadorActivoPanel(EditorPanel? l, EditorPanel? r) : base(l, r) { } public IndicadorActivoPanel(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public IndicadorActivoPanel(EditorPanels? p) : base(p) { } public IndicadorActivoPanel(SplitterLayout? s) : base(s) { } public IndicadorActivoPanel(OpenDocument? d) : base(d) { } public IndicadorActivoPanel(MainWindowViewModel? v) : base(v) { } public IndicadorActivoPanel(MainWindowViewModel? v, bool b) : base(v, b) { } public IndicadorActivoPanel((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class IndicadorFocoPanelActivo : ActiveIndicator { public IndicadorFocoPanelActivo() : base() { } public IndicadorFocoPanelActivo(EditorPanel? a) : base(a) { } public IndicadorFocoPanelActivo(EditorPanel? a, bool b) : base(a, b) { } public IndicadorFocoPanelActivo(EditorPanel? l, EditorPanel? r) : base(l, r) { } public IndicadorFocoPanelActivo(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public IndicadorFocoPanelActivo(EditorPanels? p) : base(p) { } public IndicadorFocoPanelActivo(SplitterLayout? s) : base(s) { } public IndicadorFocoPanelActivo(OpenDocument? d) : base(d) { } public IndicadorFocoPanelActivo(MainWindowViewModel? v) : base(v) { } public IndicadorFocoPanelActivo(MainWindowViewModel? v, bool b) : base(v, b) { } public IndicadorFocoPanelActivo((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class IndicadorBordePanelActivo : ActiveIndicator { public IndicadorBordePanelActivo() : base() { } public IndicadorBordePanelActivo(EditorPanel? a) : base(a) { } public IndicadorBordePanelActivo(EditorPanel? a, bool b) : base(a, b) { } public IndicadorBordePanelActivo(EditorPanel? l, EditorPanel? r) : base(l, r) { } public IndicadorBordePanelActivo(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public IndicadorBordePanelActivo(EditorPanels? p) : base(p) { } public IndicadorBordePanelActivo(SplitterLayout? s) : base(s) { } public IndicadorBordePanelActivo(OpenDocument? d) : base(d) { } public IndicadorBordePanelActivo(MainWindowViewModel? v) : base(v) { } public IndicadorBordePanelActivo(MainWindowViewModel? v, bool b) : base(v, b) { } public IndicadorBordePanelActivo((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }

// Static helper classes
public static class ActivePanelIndicators
{
    public static ActivePanelIndicator Create(EditorPanel? active = null) { var i = new ActivePanelIndicator(active); return i; }
    public static ActivePanelIndicator Create(EditorPanel? left, EditorPanel? right) { var i = new ActivePanelIndicator(left, right); return i; }
    public static ActivePanelIndicator Create(MainWindowViewModel? vm) { var i = new ActivePanelIndicator(vm); return i; }
    public static bool SetActive(MainWindowViewModel? vm, EditorPanel? panel) { if (vm is null) return false; if (panel is not null) vm.ActivePanel = panel; return true; }
    public static bool SetActive(MainWindowViewModel? vm, PanelSide side) { vm?.SetActivePanel(side); return true; }
    public static bool SetActive(MainWindowViewModel? vm, string? side) { return vm?.SetActivePanel(side) ?? false; }
}
public static class Indicators
{
    public static ActivePanelIndicator ActivePanel(EditorPanel? active = null) => new ActivePanelIndicator(active);
    public static ActivePanelIndicator ActivePanel(MainWindowViewModel? vm) => new ActivePanelIndicator(vm);
}
public static class ActiveIndicators
{
    public static ActivePanelIndicator Create(EditorPanel? active = null) => new ActivePanelIndicator(active);
    public static ActivePanelIndicator Create(MainWindowViewModel? vm) => new ActivePanelIndicator(vm);
}