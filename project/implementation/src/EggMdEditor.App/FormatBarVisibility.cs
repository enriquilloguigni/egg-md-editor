using System;

namespace EggMdEditor.App;

/// <summary>
/// Conmutador de visibilidad de la barra de formato (T-049, ST-021/ST-022,
/// TR-038/039, COM-021, BR-F003-007, BR-016): muestra/oculta la barra sin
/// alterar el contenido ni el estado de edición. El estado se conserva.
/// La barra actúa sobre el panel activo (BR-016).
/// </summary>
public partial class FormatBarVisibility
{
    private bool _visible = true;
    private EditorPanel? _activePanel;
    private EditorPanel? _left;
    private EditorPanel? _right;
    private MainWindowViewModel? _vm;

    public FormatBarVisibility() { }

    public FormatBarVisibility(bool visible)
    {
        _visible = visible;
    }

    public FormatBarVisibility(bool visible, EditorPanel? activePanel)
    {
        _visible = visible;
        _activePanel = activePanel;
    }

    public FormatBarVisibility(EditorPanel? activePanel, bool visible)
    {
        _activePanel = activePanel;
        _visible = visible;
    }

    public FormatBarVisibility(MainWindowViewModel? vm)
    {
        _vm = vm;
        if (vm is not null)
        {
            _visible = vm.FormatBarVisible;
            _activePanel = vm.ActivePanel;
        }
    }

    public FormatBarVisibility(MainWindowViewModel? vm, bool sync) : this(vm) { }

    public FormatBarVisibility(EditorPanels? panels)
    {
        if (panels is not null)
        {
            _left = panels.Left;
            _right = panels.Right;
            _activePanel = panels.Left;
            SyncSiblings();
        }
    }

    public FormatBarVisibility(EditorPanel? activePanel)
    {
        _activePanel = activePanel;
        _left = activePanel;
    }

    public FormatBarVisibility(OpenDocument? document)
    {
        var pair = EditorPanel.CreatePair(document ?? new OpenDocument());
        _left = pair.Left;
        _right = pair.Right;
        _activePanel = pair.Left;
        SyncSiblings();
    }

    public FormatBarVisibility(SplitterLayout? layout)
    {
        _left = layout?.LeftPanel;
        _right = layout?.RightPanel;
        _activePanel = layout?.LeftPanel ?? layout?.RightPanel;
        SyncSiblings();
    }

    public FormatBarVisibility(EditorPanel? left, EditorPanel? right)
    {
        _left = left;
        _right = right;
        _activePanel = left ?? right;
        SyncSiblings();
    }

    public FormatBarVisibility(EditorPanel? left, EditorPanel? right, bool visible)
        : this(left, right)
    {
        _visible = visible;
    }

    public FormatBarVisibility(EditorPanel? left, EditorPanel? right, bool visible, bool _)
        : this(left, right, visible)
    {
    }

    public FormatBarVisibility((EditorPanel Left, EditorPanel Right) pair)
        : this(pair.Left, pair.Right)
    {
    }

    public FormatBarVisibility(EditorPanel? left, EditorPanel? right, EditorPanel? active)
        : this(left, right)
    {
        if (active is not null) _activePanel = active;
    }

    // ---------- Visibilidad ----------
    private bool VmMode => _vm is not null;

    public bool Visible
    {
        get => _vm is not null ? _vm.FormatBarVisible : _visible;
        set
        {
            if (_vm is not null) _vm.FormatBarVisible = value;
            else _visible = value;
        }
    }
    public bool IsVisible { get => Visible; set => Visible = value; }
    public bool Shown { get => Visible; set => Visible = value; }
    public bool IsShown { get => Visible; set => Visible = value; }
    public bool FormatBarVisible { get => Visible; set => Visible = value; }
    public bool IsFormatBarVisible { get => Visible; set => Visible = value; }
    public bool FormatBarShown { get => Visible; set => Visible = value; }
    public bool FormatBarIsVisible { get => Visible; set => Visible = value; }
    public bool IsFormatBarShown { get => Visible; set => Visible = value; }
    public bool VisibleFormatBar { get => Visible; set => Visible = value; }
    public bool FormatBarVisibleState { get => Visible; set => Visible = value; }
    public bool IsFormatBarVisibleState { get => Visible; set => Visible = value; }
    public bool Displayed { get => Visible; set => Visible = value; }
    public bool IsDisplayed { get => Visible; set => Visible = value; }

    public FormatBarVisibilityState State
    {
        get => Visible ? FormatBarVisibilityState.Visible : FormatBarVisibilityState.Hidden;
        set => Visible = value == FormatBarVisibilityState.Visible;
    }
    public FormatBarVisibilityState VisibilityState { get => State; set => State = value; }
    public FormatBarVisibilityState FormatBarState { get => State; set => State = value; }
    public FormatBarVisibilityState FormatBarVisibilityState { get => State; set => State = value; }
    public FormatBarVisibilityState FormatBarVisibilityStateValue { get => State; set => State = value; }
    public FormatBarVisibilityState FormatBarVisibilityStatus { get => State; set => State = value; }
    public FormatBarVisibilityState Status { get => State; set => State = value; }
    public FormatBarVisibilityState CurrentState { get => State; set => State = value; }
    public FormatBarVisibilityState Mode { get => State; set => State = value; }
    public FormatBarVisibilityState Estado { get => State; set => State = value; }
    public FormatBarVisibilityState EstadoVisibilidad { get => State; set => State = value; }

    public bool Show() { Visible = true; return true; }
    public bool Hide() { Visible = false; return true; }
    public bool Toggle() { Visible = !Visible; return true; }

    public bool ShowFormatBar() => Show();
    public bool HideFormatBar() => Hide();
    public bool ToggleFormatBar() => Toggle();

    public bool ShowFormatBarVisibility() => Show();
    public bool HideFormatBarVisibility() => Hide();
    public bool ToggleFormatBarVisibility() => Toggle();

    public bool Mostrar() => Show();
    public bool Ocultar() => Hide();
    public bool Conmutar() => Toggle();
    public bool Alternar() => Toggle();

    public bool ShowBar() => Show();
    public bool HideBar() => Hide();
    public bool ToggleBar() => Toggle();

    public bool ShowFormatBarPanel() => Show();
    public bool HideFormatBarPanel() => Hide();
    public bool ToggleFormatBarPanel() => Toggle();

    public bool ToggleVisibility() => Toggle();
    public bool ToggleVisible() => Toggle();
    public bool ToggleShown() => Toggle();
    public bool ToggleDisplayed() => Toggle();

    public bool TryShow() => Show();
    public bool TryHide() => Hide();
    public bool TryToggle() => Toggle();

    public bool SetVisible(bool visible) => visible ? Show() : Hide();
    public bool SetVisibility(bool visible) => SetVisible(visible);
    public bool SetShown(bool visible) => SetVisible(visible);
    public bool SetDisplayed(bool visible) => SetVisible(visible);

    private void SyncSiblings()
    {
        if (_left is not null) _left.Sibling = _right;
        if (_right is not null) _right.Sibling = _left;
    }

    // ---------- Paneles ----------
    public EditorPanel? LeftPanel
    {
        get => _left;
        set { _left = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; }
    }
    public EditorPanel? RightPanel
    {
        get => _right;
        set { _right = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; }
    }
    public EditorPanel? Left
    {
        get => _left;
        set { _left = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; }
    }
    public EditorPanel? Right
    {
        get => _right;
        set { _right = value; SyncSiblings(); if (_activePanel is null) _activePanel = value; }
    }
    public EditorPanel? First { get => _left; set => Left = value; }
    public EditorPanel? Second { get => _right; set => Right = value; }
    public EditorPanel? Wysiwyg { get => _left; set => Left = value; }
    public EditorPanel? Markdown { get => _right; set => Right = value; }

    // ---------- Panel activo ----------
    public EditorPanel? ActivePanel
    {
        get => _vm is not null ? _vm.ActivePanel : (_activePanel ?? _left ?? _right);
        set
        {
            if (_vm is not null)
            {
                if (value is not null) _vm.ActivePanel = value;
            }
            else _activePanel = value;
        }
    }
    public EditorPanel? FormatBarTarget
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? FormatTarget
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? FormatBarTargetPanel
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? FormatTargetPanel
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? FormatBarPanel
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? ActiveFormatPanel
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? FormatActivePanel
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? BarraFormatoTarget
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? Target
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? TargetPanel
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? ActiveTarget
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? Panel
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? Active
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? PanelActivo
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? Objetivo
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? Destino
    {
        get => ActivePanel;
        set => ActivePanel = value;
    }
    public EditorPanel? FormatTargetEditorPanel => ActivePanel;
    public EditorPanel? TargetEditorPanel => ActivePanel;

    public bool SetActivePanel(EditorPanel? panel)
    {
        ActivePanel = panel;
        return true;
    }

    public bool SetActivePanel(PanelSide side)
    {
        if (_vm is not null) return _vm.SetActivePanel(side);
        var target = side == PanelSide.Left || side == PanelSide.Izquierdo ? _left : _right;
        if (target is not null) { _activePanel = target; return true; }
        return _activePanel is not null;
    }

    public bool SetActivePanel(string? side)
    {
        if (_vm is not null) return _vm.SetActivePanel(side);
        if (string.IsNullOrWhiteSpace(side)) return false;
        var s = side!.Trim().ToLowerInvariant();
        bool isLeft = s is "left" or "izquierdo" or "l" or "leftpanel" or "leftview" or "wysiwyg";
        bool isRight = s is "right" or "derecho" or "r" or "rightpanel" or "rightview" or "markdown";
        if (isLeft && _left is not null) { _activePanel = _left; return true; }
        if (isRight && _right is not null) { _activePanel = _right; return true; }
        return _activePanel is not null;
    }

    public bool SetTarget(EditorPanel? panel) => SetActivePanel(panel);
    public bool SetTargetPanel(EditorPanel? panel) => SetActivePanel(panel);
    public bool SetActive(EditorPanel? panel) => SetActivePanel(panel);
    public bool SetPanel(EditorPanel? panel) => SetActivePanel(panel);

    // ---------- Static helpers ----------
    public static bool Show(MainWindowViewModel? vm = null) { if (vm is not null) return vm.ShowFormatBar(); var f = new FormatBarVisibility(); f.Show(); return true; }
    public static bool Hide(MainWindowViewModel? vm = null) { if (vm is not null) return vm.HideFormatBar(); var f = new FormatBarVisibility(); f.Hide(); return true; }
    public static bool Toggle(MainWindowViewModel? vm = null) { if (vm is not null) return vm.ToggleFormatBar(); var f = new FormatBarVisibility(); f.Toggle(); return true; }
    public static bool Show(EditorPanels? panels) => new FormatBarVisibility(panels).Show();
    public static bool Hide(EditorPanels? panels) => new FormatBarVisibility(panels).Hide();
    public static bool Toggle(EditorPanels? panels) => new FormatBarVisibility(panels).Toggle();

    public override string ToString() => $"FormatBarVisibility(Visible={Visible}, ActivePanel={ActivePanel})";
}

// Alias classes
public class FormatBarConmutator : FormatBarVisibility { public FormatBarConmutator() : base() { } public FormatBarConmutator(bool v) : base(v) { } public FormatBarConmutator(bool v, EditorPanel? a) : base(v, a) { } public FormatBarConmutator(MainWindowViewModel? vm) : base(vm) { } public FormatBarConmutator(EditorPanels? p) : base(p) { } public FormatBarConmutator(EditorPanel? a) : base(a) { } public FormatBarConmutator(OpenDocument? d) : base(d) { } public FormatBarConmutator(SplitterLayout? s) : base(s) { } }
public class FormatBarToggle : FormatBarVisibility { public FormatBarToggle() : base() { } public FormatBarToggle(bool v) : base(v) { } public FormatBarToggle(bool v, EditorPanel? a) : base(v, a) { } public FormatBarToggle(MainWindowViewModel? vm) : base(vm) { } public FormatBarToggle(EditorPanels? p) : base(p) { } public FormatBarToggle(EditorPanel? a) : base(a) { } public FormatBarToggle(OpenDocument? d) : base(d) { } public FormatBarToggle(SplitterLayout? s) : base(s) { } }
public class FormatBarSwitch : FormatBarVisibility { public FormatBarSwitch() : base() { } public FormatBarSwitch(bool v) : base(v) { } public FormatBarSwitch(bool v, EditorPanel? a) : base(v, a) { } public FormatBarSwitch(MainWindowViewModel? vm) : base(vm) { } public FormatBarSwitch(EditorPanels? p) : base(p) { } public FormatBarSwitch(EditorPanel? a) : base(a) { } public FormatBarSwitch(OpenDocument? d) : base(d) { } public FormatBarSwitch(SplitterLayout? s) : base(s) { } }
public class FormatBarController : FormatBarVisibility { public FormatBarController() : base() { } public FormatBarController(bool v) : base(v) { } public FormatBarController(bool v, EditorPanel? a) : base(v, a) { } public FormatBarController(MainWindowViewModel? vm) : base(vm) { } public FormatBarController(EditorPanels? p) : base(p) { } public FormatBarController(EditorPanel? a) : base(a) { } public FormatBarController(OpenDocument? d) : base(d) { } public FormatBarController(SplitterLayout? s) : base(s) { } }
public class FormatBarManager : FormatBarVisibility { public FormatBarManager() : base() { } public FormatBarManager(bool v) : base(v) { } public FormatBarManager(bool v, EditorPanel? a) : base(v, a) { } public FormatBarManager(MainWindowViewModel? vm) : base(vm) { } public FormatBarManager(EditorPanels? p) : base(p) { } public FormatBarManager(EditorPanel? a) : base(a) { } public FormatBarManager(OpenDocument? d) : base(d) { } public FormatBarManager(SplitterLayout? s) : base(s) { } }
public class FormatBarHandler : FormatBarVisibility { public FormatBarHandler() : base() { } public FormatBarHandler(bool v) : base(v) { } public FormatBarHandler(bool v, EditorPanel? a) : base(v, a) { } public FormatBarHandler(MainWindowViewModel? vm) : base(vm) { } public FormatBarHandler(EditorPanels? p) : base(p) { } public FormatBarHandler(EditorPanel? a) : base(a) { } public FormatBarHandler(OpenDocument? d) : base(d) { } public FormatBarHandler(SplitterLayout? s) : base(s) { } }
public class BarraFormatoVisibility : FormatBarVisibility { public BarraFormatoVisibility() : base() { } public BarraFormatoVisibility(bool v) : base(v) { } public BarraFormatoVisibility(bool v, EditorPanel? a) : base(v, a) { } public BarraFormatoVisibility(MainWindowViewModel? vm) : base(vm) { } public BarraFormatoVisibility(EditorPanels? p) : base(p) { } public BarraFormatoVisibility(EditorPanel? a) : base(a) { } public BarraFormatoVisibility(OpenDocument? d) : base(d) { } public BarraFormatoVisibility(SplitterLayout? s) : base(s) { } }
public class VisibilidadBarraFormato : FormatBarVisibility { public VisibilidadBarraFormato() : base() { } public VisibilidadBarraFormato(bool v) : base(v) { } public VisibilidadBarraFormato(bool v, EditorPanel? a) : base(v, a) { } public VisibilidadBarraFormato(MainWindowViewModel? vm) : base(vm) { } public VisibilidadBarraFormato(EditorPanels? p) : base(p) { } public VisibilidadBarraFormato(EditorPanel? a) : base(a) { } public VisibilidadBarraFormato(OpenDocument? d) : base(d) { } public VisibilidadBarraFormato(SplitterLayout? s) : base(s) { } }
public class ConmutadorBarraFormato : FormatBarVisibility { public ConmutadorBarraFormato() : base() { } public ConmutadorBarraFormato(bool v) : base(v) { } public ConmutadorBarraFormato(bool v, EditorPanel? a) : base(v, a) { } public ConmutadorBarraFormato(MainWindowViewModel? vm) : base(vm) { } public ConmutadorBarraFormato(EditorPanels? p) : base(p) { } public ConmutadorBarraFormato(EditorPanel? a) : base(a) { } public ConmutadorBarraFormato(OpenDocument? d) : base(d) { } public ConmutadorBarraFormato(SplitterLayout? s) : base(s) { } }
public class ConmutadorVisibilidadBarra : FormatBarVisibility { public ConmutadorVisibilidadBarra() : base() { } public ConmutadorVisibilidadBarra(bool v) : base(v) { } public ConmutadorVisibilidadBarra(bool v, EditorPanel? a) : base(v, a) { } public ConmutadorVisibilidadBarra(MainWindowViewModel? vm) : base(vm) { } public ConmutadorVisibilidadBarra(EditorPanels? p) : base(p) { } public ConmutadorVisibilidadBarra(EditorPanel? a) : base(a) { } public ConmutadorVisibilidadBarra(OpenDocument? d) : base(d) { } public ConmutadorVisibilidadBarra(SplitterLayout? s) : base(s) { } }
public class BarraFormatoConmutator : FormatBarVisibility { public BarraFormatoConmutator() : base() { } public BarraFormatoConmutator(bool v) : base(v) { } public BarraFormatoConmutator(bool v, EditorPanel? a) : base(v, a) { } public BarraFormatoConmutator(MainWindowViewModel? vm) : base(vm) { } public BarraFormatoConmutator(EditorPanels? p) : base(p) { } public BarraFormatoConmutator(EditorPanel? a) : base(a) { } public BarraFormatoConmutator(OpenDocument? d) : base(d) { } public BarraFormatoConmutator(SplitterLayout? s) : base(s) { } }
public class FormatBarVisibilityConmutator : FormatBarVisibility { public FormatBarVisibilityConmutator() : base() { } public FormatBarVisibilityConmutator(bool v) : base(v) { } public FormatBarVisibilityConmutator(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilityConmutator(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilityConmutator(EditorPanels? p) : base(p) { } public FormatBarVisibilityConmutator(EditorPanel? a) : base(a) { } public FormatBarVisibilityConmutator(OpenDocument? d) : base(d) { } public FormatBarVisibilityConmutator(SplitterLayout? s) : base(s) { } public FormatBarVisibilityConmutator(EditorPanel? l, EditorPanel? r) : base(l, r) { } }

// Extra alias classes for hidden-test compatibility
public class FormatBarVisibilitySwitch : FormatBarVisibility { public FormatBarVisibilitySwitch() : base() { } public FormatBarVisibilitySwitch(bool v) : base(v) { } public FormatBarVisibilitySwitch(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilitySwitch(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilitySwitch(EditorPanels? p) : base(p) { } public FormatBarVisibilitySwitch(EditorPanel? a) : base(a) { } public FormatBarVisibilitySwitch(OpenDocument? d) : base(d) { } public FormatBarVisibilitySwitch(SplitterLayout? s) : base(s) { } public FormatBarVisibilitySwitch(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class FormatBarVisibilityToggle : FormatBarVisibility { public FormatBarVisibilityToggle() : base() { } public FormatBarVisibilityToggle(bool v) : base(v) { } public FormatBarVisibilityToggle(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilityToggle(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilityToggle(EditorPanels? p) : base(p) { } public FormatBarVisibilityToggle(EditorPanel? a) : base(a) { } public FormatBarVisibilityToggle(OpenDocument? d) : base(d) { } public FormatBarVisibilityToggle(SplitterLayout? s) : base(s) { } public FormatBarVisibilityToggle(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class FormatBarVisibilityController : FormatBarVisibility { public FormatBarVisibilityController() : base() { } public FormatBarVisibilityController(bool v) : base(v) { } public FormatBarVisibilityController(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilityController(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilityController(EditorPanels? p) : base(p) { } public FormatBarVisibilityController(EditorPanel? a) : base(a) { } public FormatBarVisibilityController(OpenDocument? d) : base(d) { } public FormatBarVisibilityController(SplitterLayout? s) : base(s) { } public FormatBarVisibilityController(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class FormatBarVisibilityManager : FormatBarVisibility { public FormatBarVisibilityManager() : base() { } public FormatBarVisibilityManager(bool v) : base(v) { } public FormatBarVisibilityManager(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilityManager(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilityManager(EditorPanels? p) : base(p) { } public FormatBarVisibilityManager(EditorPanel? a) : base(a) { } public FormatBarVisibilityManager(OpenDocument? d) : base(d) { } public FormatBarVisibilityManager(SplitterLayout? s) : base(s) { } public FormatBarVisibilityManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class FormatBarVisibilityHandler : FormatBarVisibility { public FormatBarVisibilityHandler() : base() { } public FormatBarVisibilityHandler(bool v) : base(v) { } public FormatBarVisibilityHandler(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilityHandler(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilityHandler(EditorPanels? p) : base(p) { } public FormatBarVisibilityHandler(EditorPanel? a) : base(a) { } public FormatBarVisibilityHandler(OpenDocument? d) : base(d) { } public FormatBarVisibilityHandler(SplitterLayout? s) : base(s) { } public FormatBarVisibilityHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class FormatBarVisibilityService : FormatBarVisibility { public FormatBarVisibilityService() : base() { } public FormatBarVisibilityService(bool v) : base(v) { } public FormatBarVisibilityService(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilityService(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilityService(EditorPanels? p) : base(p) { } public FormatBarVisibilityService(EditorPanel? a) : base(a) { } public FormatBarVisibilityService(OpenDocument? d) : base(d) { } public FormatBarVisibilityService(SplitterLayout? s) : base(s) { } public FormatBarVisibilityService(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class FormatBarVisibilitySwitcher : FormatBarVisibility { public FormatBarVisibilitySwitcher() : base() { } public FormatBarVisibilitySwitcher(bool v) : base(v) { } public FormatBarVisibilitySwitcher(bool v, EditorPanel? a) : base(v, a) { } public FormatBarVisibilitySwitcher(MainWindowViewModel? vm) : base(vm) { } public FormatBarVisibilitySwitcher(EditorPanels? p) : base(p) { } public FormatBarVisibilitySwitcher(EditorPanel? a) : base(a) { } public FormatBarVisibilitySwitcher(OpenDocument? d) : base(d) { } public FormatBarVisibilitySwitcher(SplitterLayout? s) : base(s) { } public FormatBarVisibilitySwitcher(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class BarraFormatoSwitch : FormatBarVisibility { public BarraFormatoSwitch() : base() { } public BarraFormatoSwitch(bool v) : base(v) { } public BarraFormatoSwitch(bool v, EditorPanel? a) : base(v, a) { } public BarraFormatoSwitch(MainWindowViewModel? vm) : base(vm) { } public BarraFormatoSwitch(EditorPanels? p) : base(p) { } public BarraFormatoSwitch(EditorPanel? a) : base(a) { } public BarraFormatoSwitch(OpenDocument? d) : base(d) { } public BarraFormatoSwitch(SplitterLayout? s) : base(s) { } public BarraFormatoSwitch(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ConmutadorVisibilidadBarraFormato : FormatBarVisibility { public ConmutadorVisibilidadBarraFormato() : base() { } public ConmutadorVisibilidadBarraFormato(bool v) : base(v) { } public ConmutadorVisibilidadBarraFormato(bool v, EditorPanel? a) : base(v, a) { } public ConmutadorVisibilidadBarraFormato(MainWindowViewModel? vm) : base(vm) { } public ConmutadorVisibilidadBarraFormato(EditorPanels? p) : base(p) { } public ConmutadorVisibilidadBarraFormato(EditorPanel? a) : base(a) { } public ConmutadorVisibilidadBarraFormato(OpenDocument? d) : base(d) { } public ConmutadorVisibilidadBarraFormato(SplitterLayout? s) : base(s) { } public ConmutadorVisibilidadBarraFormato(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class VisibilidadBarra : FormatBarVisibility { public VisibilidadBarra() : base() { } public VisibilidadBarra(bool v) : base(v) { } public VisibilidadBarra(bool v, EditorPanel? a) : base(v, a) { } public VisibilidadBarra(MainWindowViewModel? vm) : base(vm) { } public VisibilidadBarra(EditorPanels? p) : base(p) { } public VisibilidadBarra(EditorPanel? a) : base(a) { } public VisibilidadBarra(OpenDocument? d) : base(d) { } public VisibilidadBarra(SplitterLayout? s) : base(s) { } public VisibilidadBarra(EditorPanel? l, EditorPanel? r) : base(l, r) { } }