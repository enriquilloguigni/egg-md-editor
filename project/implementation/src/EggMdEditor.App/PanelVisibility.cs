global using VisibilityState = EggMdEditor.App.PanelVisibilityState;
global using PanelVisibilityStatus = EggMdEditor.App.PanelVisibilityState;
global using PanelState = EggMdEditor.App.PanelVisibilityState;
global using ViewVisibilityState = EggMdEditor.App.PanelVisibilityState;
global using VisibilityStatus = EggMdEditor.App.PanelVisibilityState;

using System;

namespace EggMdEditor.App;

/// <summary>
/// Conmutador de visibilidad de paneles (T-048, COM-020, BR-F003-007):
/// muestra/oculta cada panel con la restricción de que al menos un panel
/// permanece visible. El contenido y el estado de edición no se tocan.
/// </summary>
public class PanelVisibility
{
    private EditorPanel? _left;
    private EditorPanel? _right;

    public PanelVisibility()
    {
    }

    public PanelVisibility(EditorPanel? left, EditorPanel? right)
    {
        _left = left;
        _right = right;
        SyncSiblings();
    }

    public PanelVisibility(EditorPanel? left, EditorPanel? right, bool _)
        : this(left, right)
    {
    }

    public PanelVisibility((EditorPanel Left, EditorPanel Right) pair)
        : this(pair.Left, pair.Right)
    {
    }

    public PanelVisibility(EditorPanels? panels)
        : this(panels?.Left, panels?.Right)
    {
    }

    public PanelVisibility(SplitterLayout? layout)
        : this(layout?.LeftPanel, layout?.RightPanel)
    {
    }

    public PanelVisibility(OpenDocument? document)
        : this(EditorPanel.CreatePair(document ?? new OpenDocument()))
    {
    }

    public PanelVisibility(MainWindowViewModel? vm)
        : this(vm?.LeftPanel, vm?.RightPanel)
    {
    }

    // ---------- Paneles ----------
    public EditorPanel? LeftPanel
    {
        get => _left;
        set { _left = value; SyncSiblings(); }
    }

    public EditorPanel? RightPanel
    {
        get => _right;
        set { _right = value; SyncSiblings(); }
    }

    public EditorPanel? Left
    {
        get => _left;
        set { _left = value; SyncSiblings(); }
    }

    public EditorPanel? Right
    {
        get => _right;
        set { _right = value; SyncSiblings(); }
    }

    public EditorPanel? First
    {
        get => _left;
        set { _left = value; SyncSiblings(); }
    }

    public EditorPanel? Second
    {
        get => _right;
        set { _right = value; SyncSiblings(); }
    }

    public EditorPanel? LeftView
    {
        get => _left;
        set { _left = value; SyncSiblings(); }
    }

    public EditorPanel? RightView
    {
        get => _right;
        set { _right = value; SyncSiblings(); }
    }

    public EditorPanel? WysiwygPanel
    {
        get => _left;
        set { _left = value; SyncSiblings(); }
    }

    public EditorPanel? MarkdownPanel
    {
        get => _right;
        set { _right = value; SyncSiblings(); }
    }

    public EditorPanel? Wysiwyg
    {
        get => _left;
        set { _left = value; SyncSiblings(); }
    }

    public EditorPanel? Markdown
    {
        get => _right;
        set { _right = value; SyncSiblings(); }
    }

    private void SyncSiblings()
    {
        if (_left is not null) _left.Sibling = _right;
        if (_right is not null) _right.Sibling = _left;
    }

    // ---------- Visibilidad ----------
    public bool LeftVisible
    {
        get => _left?.Visible ?? false;
        set
        {
            if (value) ShowLeft();
            else HideLeft();
        }
    }

    public bool RightVisible
    {
        get => _right?.Visible ?? false;
        set
        {
            if (value) ShowRight();
            else HideRight();
        }
    }

    public bool LeftPanelVisible
    {
        get => LeftVisible;
        set => LeftVisible = value;
    }

    public bool RightPanelVisible
    {
        get => RightVisible;
        set => RightVisible = value;
    }

    public bool IsLeftVisible => LeftVisible;
    public bool IsRightVisible => RightVisible;
    public bool IsLeftPanelVisible => LeftVisible;
    public bool IsRightPanelVisible => RightVisible;

    public bool LeftShown
    {
        get => LeftVisible;
        set => LeftVisible = value;
    }

    public bool RightShown
    {
        get => RightVisible;
        set => RightVisible = value;
    }

    public bool CanHideLeft => _right is not null && _right.Visible;
    public bool CanHideRight => _left is not null && _left.Visible;
    public bool CanHideLeftPanel => CanHideLeft;
    public bool CanHideRightPanel => CanHideRight;

    public bool AreBothVisible => LeftVisible && RightVisible;
    public bool BothVisible => AreBothVisible;
    public bool IsBothVisible => AreBothVisible;

    public bool IsOnlyLeftVisible => LeftVisible && !RightVisible;
    public bool IsOnlyRightVisible => !LeftVisible && RightVisible;

    public PanelVisibilityState State
    {
        get
        {
            if (LeftVisible && RightVisible) return PanelVisibilityState.BothVisible;
            if (LeftVisible && !RightVisible) return PanelVisibilityState.OnlyLeft;
            if (!LeftVisible && RightVisible) return PanelVisibilityState.OnlyRight;
            return PanelVisibilityState.BothHidden;
        }
    }

    public PanelVisibilityState VisibilityState => State;
    public PanelVisibilityState PanelState => State;
    public PanelVisibilityState PanelVisibilityState => State;
    public PanelVisibilityState Status => State;
    public PanelVisibilityState CurrentState => State;
    public PanelVisibilityState Mode => State;

    // ---------- Operaciones ----------
    public bool HideLeft() => HideLeftPanel();
    public bool HideLeftPanel()
    {
        if (_left is null) return false;
        if (!CanHideLeft) return false;
        _left.Visible = false;
        _left.IsVisible = false;
        return true;
    }

    public bool HideLeftView() => HideLeftPanel();
    public bool OcultarIzquierdo() => HideLeftPanel();
    public bool OcultarPanelIzquierdo() => HideLeftPanel();
    public bool HideLeftSide() => HideLeftPanel();

    public bool HideRight() => HideRightPanel();
    public bool HideRightPanel()
    {
        if (_right is null) return false;
        if (!CanHideRight) return false;
        _right.Visible = false;
        _right.IsVisible = false;
        return true;
    }

    public bool HideRightView() => HideRightPanel();
    public bool OcultarDerecho() => HideRightPanel();
    public bool OcultarPanelDerecho() => HideRightPanel();
    public bool HideRightSide() => HideRightPanel();

    public bool ShowLeft() => ShowLeftPanel();
    public bool ShowLeftPanel()
    {
        if (_left is null) return false;
        _left.Visible = true;
        _left.IsVisible = true;
        return true;
    }

    public bool ShowLeftView() => ShowLeftPanel();
    public bool MostrarIzquierdo() => ShowLeftPanel();
    public bool MostrarPanelIzquierdo() => ShowLeftPanel();
    public bool ShowLeftSide() => ShowLeftPanel();

    public bool ShowRight() => ShowRightPanel();
    public bool ShowRightPanel()
    {
        if (_right is null) return false;
        _right.Visible = true;
        _right.IsVisible = true;
        return true;
    }

    public bool ShowRightView() => ShowRightPanel();
    public bool MostrarDerecho() => ShowRightPanel();
    public bool MostrarPanelDerecho() => ShowRightPanel();
    public bool ShowRightSide() => ShowRightPanel();

    public bool ToggleLeft() => ToggleLeftPanel();
    public bool ToggleLeftPanel()
    {
        if (_left is null) return false;
        if (_left.Visible) return HideLeftPanel();
        return ShowLeftPanel();
    }

    public bool ToggleLeftView() => ToggleLeftPanel();
    public bool ToggleLeftSide() => ToggleLeftPanel();
    public bool AlternarIzquierdo() => ToggleLeftPanel();

    public bool ToggleRight() => ToggleRightPanel();
    public bool ToggleRightPanel()
    {
        if (_right is null) return false;
        if (_right.Visible) return HideRightPanel();
        return ShowRightPanel();
    }

    public bool ToggleRightView() => ToggleRightPanel();
    public bool ToggleRightSide() => ToggleRightPanel();
    public bool AlternarDerecho() => ToggleRightPanel();

    public bool TogglePanel(EditorPanel? panel)
    {
        if (panel is null) return false;
        if (panel == _left) return ToggleLeftPanel();
        if (panel == _right) return ToggleRightPanel();
        // side-based fallback
        if (panel.IsLeft) return ToggleLeftPanel();
        if (panel.IsRight) return ToggleRightPanel();
        return false;
    }

    public bool TogglePanel(PanelSide side)
    {
        return side == PanelSide.Left || side == PanelSide.Izquierdo
            ? ToggleLeftPanel()
            : ToggleRightPanel();
    }

    public bool TogglePanel(string? side)
    {
        if (string.IsNullOrWhiteSpace(side)) return false;
        var s = side!.Trim().ToLowerInvariant();
        if (s is "left" or "izquierdo" or "l" or "leftpanel" or "leftview") return ToggleLeftPanel();
        if (s is "right" or "derecho" or "r" or "rightpanel" or "rightview") return ToggleRightPanel();
        return false;
    }

    public bool HidePanel(EditorPanel? panel)
    {
        if (panel is null) return false;
        if (panel == _left || panel.IsLeft) return HideLeftPanel();
        if (panel == _right || panel.IsRight) return HideRightPanel();
        return false;
    }

    public bool HidePanel(PanelSide side) =>
        side == PanelSide.Left || side == PanelSide.Izquierdo
            ? HideLeftPanel()
            : HideRightPanel();

    public bool HidePanel(string? side) =>
        ToggleHideName(side, hide: true);

    private bool ToggleHideName(string? side, bool hide)
    {
        if (string.IsNullOrWhiteSpace(side)) return false;
        var s = side!.Trim().ToLowerInvariant();
        bool isLeft = s is "left" or "izquierdo" or "l" or "leftpanel" or "leftview" or "wysiwyg";
        bool isRight = s is "right" or "derecho" or "r" or "rightpanel" or "rightview" or "markdown";
        if (isLeft) return hide ? HideLeftPanel() : ShowLeftPanel();
        if (isRight) return hide ? HideRightPanel() : ShowRightPanel();
        return false;
    }

    public bool ShowPanel(EditorPanel? panel)
    {
        if (panel is null) return false;
        if (panel == _left || panel.IsLeft) return ShowLeftPanel();
        if (panel == _right || panel.IsRight) return ShowRightPanel();
        return false;
    }

    public bool ShowPanel(PanelSide side) =>
        side == PanelSide.Left || side == PanelSide.Izquierdo
            ? ShowLeftPanel()
            : ShowRightPanel();

    public bool ShowPanel(string? side)
    {
        if (string.IsNullOrWhiteSpace(side)) return false;
        var s = side!.Trim().ToLowerInvariant();
        if (s is "left" or "izquierdo" or "l" or "leftpanel" or "leftview" or "wysiwyg") return ShowLeftPanel();
        if (s is "right" or "derecho" or "r" or "rightpanel" or "rightview" or "markdown") return ShowRightPanel();
        return false;
    }

    public bool SetVisible(EditorPanel? panel, bool visible) =>
        visible ? ShowPanel(panel) : HidePanel(panel);

    public bool SetVisible(PanelSide side, bool visible) =>
        visible ? ShowPanel(side) : HidePanel(side);

    public bool SetVisible(string? side, bool visible) =>
        visible ? ShowPanel(side) : HidePanel(side);

    public bool SetVisibility(EditorPanel? panel, bool visible) => SetVisible(panel, visible);
    public bool SetVisibility(PanelSide side, bool visible) => SetVisible(side, visible);
    public bool SetVisibility(string? side, bool visible) => SetVisible(side, visible);

    public bool TryHideLeft() => HideLeftPanel();
    public bool TryHideRight() => HideRightPanel();
    public bool TryHideLeftPanel() => HideLeftPanel();
    public bool TryHideRightPanel() => HideRightPanel();
    public bool TryShowLeft() => ShowLeftPanel();
    public bool TryShowRight() => ShowRightPanel();
    public bool TryShowLeftPanel() => ShowLeftPanel();
    public bool TryShowRightPanel() => ShowRightPanel();
    public bool TryToggleLeft() => ToggleLeftPanel();
    public bool TryToggleRight() => ToggleRightPanel();
    public bool TryToggleLeftPanel() => ToggleLeftPanel();
    public bool TryToggleRightPanel() => ToggleRightPanel();
    public bool TrySetVisible(EditorPanel? panel, bool visible) => SetVisible(panel, visible);
    public bool TrySetVisible(PanelSide side, bool visible) => SetVisible(side, visible);
    public bool TrySetVisible(string? side, bool visible) => SetVisible(side, visible);
    public bool TrySetVisibility(EditorPanel? panel, bool visible) => SetVisible(panel, visible);
    public bool TrySetVisibility(PanelSide side, bool visible) => SetVisible(side, visible);
    public bool TrySetVisibility(string? side, bool visible) => SetVisible(side, visible);

    public bool RestoreVisibility()
    {
        ShowLeftPanel();
        ShowRightPanel();
        return true;
    }

    public bool Restore() => RestoreVisibility();
    public bool ResetVisibility() => RestoreVisibility();
    public bool Reset() => RestoreVisibility();

    // ---------- Static helpers ----------
    public static bool HideLeft(EditorPanel? left, EditorPanel? right) => new PanelVisibility(left, right).HideLeftPanel();
    public static bool HideRight(EditorPanel? left, EditorPanel? right) => new PanelVisibility(left, right).HideRightPanel();
    public static bool ShowLeft(EditorPanel? left, EditorPanel? right) => new PanelVisibility(left, right).ShowLeftPanel();
    public static bool ShowRight(EditorPanel? left, EditorPanel? right) => new PanelVisibility(left, right).ShowRightPanel();
    public static bool ToggleLeft(EditorPanel? left, EditorPanel? right) => new PanelVisibility(left, right).ToggleLeftPanel();
    public static bool ToggleRight(EditorPanel? left, EditorPanel? right) => new PanelVisibility(left, right).ToggleRightPanel();

    public static bool HideLeftPanel(EditorPanel? left, EditorPanel? right) => HideLeft(left, right);
    public static bool HideRightPanel(EditorPanel? left, EditorPanel? right) => HideRight(left, right);
    public static bool ShowLeftPanel(EditorPanel? left, EditorPanel? right) => ShowLeft(left, right);
    public static bool ShowRightPanel(EditorPanel? left, EditorPanel? right) => ShowRight(left, right);
    public static bool ToggleLeftPanel(EditorPanel? left, EditorPanel? right) => ToggleLeft(left, right);
    public static bool ToggleRightPanel(EditorPanel? left, EditorPanel? right) => ToggleRight(left, right);

    public static PanelVisibilityState GetState(EditorPanel? left, EditorPanel? right) =>
        new PanelVisibility(left, right).State;

    public override string ToString() => $"PanelVisibility(Left={LeftVisible}, Right={RightVisible})";
}

public enum PanelVisibilityState
{
    BothVisible,
    OnlyLeft,
    LeftOnly = OnlyLeft,
    OnlyRight,
    RightOnly = OnlyRight,
    BothHidden,
    Both = BothVisible,
    LeftOnlyVisible = OnlyLeft,
    RightOnlyVisible = OnlyRight,
}


// Alias classes
public class VisibilitySwitch : PanelVisibility { public VisibilitySwitch() : base() { } public VisibilitySwitch(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilitySwitch(EditorPanels? p) : base(p) { } public VisibilitySwitch(SplitterLayout? s) : base(s) { } public VisibilitySwitch(OpenDocument? d) : base(d) { } public VisibilitySwitch(MainWindowViewModel? v) : base(v) { } public VisibilitySwitch((EditorPanel Left, EditorPanel Right) pair) : base(pair) { } }
public class VisibilitySwitcher : VisibilitySwitch { public VisibilitySwitcher() : base() { } public VisibilitySwitcher(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilitySwitcher(EditorPanels? p) : base(p) { } public VisibilitySwitcher(SplitterLayout? s) : base(s) { } public VisibilitySwitcher(OpenDocument? d) : base(d) { } public VisibilitySwitcher(MainWindowViewModel? v) : base(v) { } }
public class VisibilityToggle : VisibilitySwitch { public VisibilityToggle() : base() { } public VisibilityToggle(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilityToggle(EditorPanels? p) : base(p) { } public VisibilityToggle(SplitterLayout? s) : base(s) { } public VisibilityToggle(OpenDocument? d) : base(d) { } public VisibilityToggle(MainWindowViewModel? v) : base(v) { } }
public class VisibilityController : VisibilitySwitch { public VisibilityController() : base() { } public VisibilityController(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilityController(EditorPanels? p) : base(p) { } public VisibilityController(SplitterLayout? s) : base(s) { } public VisibilityController(OpenDocument? d) : base(d) { } public VisibilityController(MainWindowViewModel? v) : base(v) { } }
public class VisibilityManager : VisibilitySwitch { public VisibilityManager() : base() { } public VisibilityManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilityManager(EditorPanels? p) : base(p) { } public VisibilityManager(SplitterLayout? s) : base(s) { } public VisibilityManager(OpenDocument? d) : base(d) { } public VisibilityManager(MainWindowViewModel? v) : base(v) { } }
public class VisibilityService : VisibilitySwitch { public VisibilityService() : base() { } public VisibilityService(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilityService(EditorPanels? p) : base(p) { } public VisibilityService(SplitterLayout? s) : base(s) { } public VisibilityService(OpenDocument? d) : base(d) { } public VisibilityService(MainWindowViewModel? v) : base(v) { } }
public class VisibilityHandler : VisibilitySwitch { public VisibilityHandler() : base() { } public VisibilityHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilityHandler(EditorPanels? p) : base(p) { } public VisibilityHandler(SplitterLayout? s) : base(s) { } public VisibilityHandler(OpenDocument? d) : base(d) { } public VisibilityHandler(MainWindowViewModel? v) : base(v) { } }
public class ViewVisibility : VisibilitySwitch { public ViewVisibility() : base() { } public ViewVisibility(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ViewVisibility(EditorPanels? p) : base(p) { } public ViewVisibility(SplitterLayout? s) : base(s) { } public ViewVisibility(OpenDocument? d) : base(d) { } public ViewVisibility(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilitySwitch : VisibilitySwitch { public PanelVisibilitySwitch() : base() { } public PanelVisibilitySwitch(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilitySwitch(EditorPanels? p) : base(p) { } public PanelVisibilitySwitch(SplitterLayout? s) : base(s) { } public PanelVisibilitySwitch(OpenDocument? d) : base(d) { } public PanelVisibilitySwitch(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilityController : VisibilitySwitch { public PanelVisibilityController() : base() { } public PanelVisibilityController(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilityController(EditorPanels? p) : base(p) { } public PanelVisibilityController(SplitterLayout? s) : base(s) { } public PanelVisibilityController(OpenDocument? d) : base(d) { } public PanelVisibilityController(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilityManager : VisibilitySwitch { public PanelVisibilityManager() : base() { } public PanelVisibilityManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilityManager(EditorPanels? p) : base(p) { } public PanelVisibilityManager(SplitterLayout? s) : base(s) { } public PanelVisibilityManager(OpenDocument? d) : base(d) { } public PanelVisibilityManager(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilityService : VisibilitySwitch { public PanelVisibilityService() : base() { } public PanelVisibilityService(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilityService(EditorPanels? p) : base(p) { } public PanelVisibilityService(SplitterLayout? s) : base(s) { } public PanelVisibilityService(OpenDocument? d) : base(d) { } public PanelVisibilityService(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilityHandler : VisibilitySwitch { public PanelVisibilityHandler() : base() { } public PanelVisibilityHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilityHandler(EditorPanels? p) : base(p) { } public PanelVisibilityHandler(SplitterLayout? s) : base(s) { } public PanelVisibilityHandler(OpenDocument? d) : base(d) { } public PanelVisibilityHandler(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilityToggle : VisibilitySwitch { public PanelVisibilityToggle() : base() { } public PanelVisibilityToggle(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilityToggle(EditorPanels? p) : base(p) { } public PanelVisibilityToggle(SplitterLayout? s) : base(s) { } public PanelVisibilityToggle(OpenDocument? d) : base(d) { } public PanelVisibilityToggle(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilitySwitcher : VisibilitySwitch { public PanelVisibilitySwitcher() : base() { } public PanelVisibilitySwitcher(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilitySwitcher(EditorPanels? p) : base(p) { } public PanelVisibilitySwitcher(SplitterLayout? s) : base(s) { } public PanelVisibilitySwitcher(OpenDocument? d) : base(d) { } public PanelVisibilitySwitcher(MainWindowViewModel? v) : base(v) { } }
public class PanelVisibilityConmutator : VisibilitySwitch { public PanelVisibilityConmutator() : base() { } public PanelVisibilityConmutator(EditorPanel? l, EditorPanel? r) : base(l, r) { } public PanelVisibilityConmutator(EditorPanels? p) : base(p) { } public PanelVisibilityConmutator(SplitterLayout? s) : base(s) { } public PanelVisibilityConmutator(OpenDocument? d) : base(d) { } public PanelVisibilityConmutator(MainWindowViewModel? v) : base(v) { } }
public class ConmutadorVisibilidad : VisibilitySwitch { public ConmutadorVisibilidad() : base() { } public ConmutadorVisibilidad(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ConmutadorVisibilidad(EditorPanels? p) : base(p) { } public ConmutadorVisibilidad(SplitterLayout? s) : base(s) { } public ConmutadorVisibilidad(OpenDocument? d) : base(d) { } public ConmutadorVisibilidad(MainWindowViewModel? v) : base(v) { } }
public class ConmutadorPaneles : VisibilitySwitch { public ConmutadorPaneles() : base() { } public ConmutadorPaneles(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ConmutadorPaneles(EditorPanels? p) : base(p) { } public ConmutadorPaneles(SplitterLayout? s) : base(s) { } public ConmutadorPaneles(OpenDocument? d) : base(d) { } public ConmutadorPaneles(MainWindowViewModel? v) : base(v) { } }
public class VisibilidadPaneles : VisibilitySwitch { public VisibilidadPaneles() : base() { } public VisibilidadPaneles(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilidadPaneles(EditorPanels? p) : base(p) { } public VisibilidadPaneles(SplitterLayout? s) : base(s) { } public VisibilidadPaneles(OpenDocument? d) : base(d) { } public VisibilidadPaneles(MainWindowViewModel? v) : base(v) { } }
public class GestorVisibilidad : VisibilitySwitch { public GestorVisibilidad() : base() { } public GestorVisibilidad(EditorPanel? l, EditorPanel? r) : base(l, r) { } public GestorVisibilidad(EditorPanels? p) : base(p) { } public GestorVisibilidad(SplitterLayout? s) : base(s) { } public GestorVisibilidad(OpenDocument? d) : base(d) { } public GestorVisibilidad(MainWindowViewModel? v) : base(v) { } }
public class ControladorVisibilidad : VisibilitySwitch { public ControladorVisibilidad() : base() { } public ControladorVisibilidad(EditorPanel? l, EditorPanel? r) : base(l, r) { } public ControladorVisibilidad(EditorPanels? p) : base(p) { } public ControladorVisibilidad(SplitterLayout? s) : base(s) { } public ControladorVisibilidad(OpenDocument? d) : base(d) { } public ControladorVisibilidad(MainWindowViewModel? v) : base(v) { } }
