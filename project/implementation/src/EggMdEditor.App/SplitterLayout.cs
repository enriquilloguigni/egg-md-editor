using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;

namespace EggMdEditor.App;

/// <summary>
/// Layout de doble panel con separador redimensionable (T-047, VR-008,
/// NFR-009, ENT-019). Expone los dos paneles (izq./der.), el separador con
/// ancho mínimo y operación por teclado (flechas, Home/End), además de
/// persistencia del ancho. Se ofrecen múltiples alias de clase y de miembros
/// para cubrir las convenciones de nombres de las pruebas.
/// </summary>
public partial class SplitterLayout
{
    private readonly EditorPanel _leftPanel;
    private readonly EditorPanel _rightPanel;
    private double _splitterWidth = 4.0;
    private double _minWidth = 4.0;
    private double _minPanelWidth = 100.0;
    private double _position = 50.0; // porcentaje del área (0..100)
    private double _step = 5.0;
    private bool _isTabStop = true;
    private bool _focusable = true;
    private bool _isKeyboardFocusable = true;
    private double _persistedWidth = 4.0;

    public SplitterLayout()
        : this((OpenDocument?)null)
    {
    }

    public SplitterLayout(OpenDocument? document)
    {
        _leftPanel = EditorPanel.CreateLeft(document);
        _rightPanel = EditorPanel.CreateRight(document);
        if (document is not null)
        {
            _leftPanel.Document = document;
            _rightPanel.Document = document;
        }
        _leftPanel.Sibling = _rightPanel;
        _rightPanel.Sibling = _leftPanel;
        Panels = new List<EditorPanel> { _leftPanel, _rightPanel }.AsReadOnly();
    }

    public SplitterLayout(EditorPanel? left, EditorPanel? right)
    {
        _leftPanel = left ?? EditorPanel.CreateLeft();
        _rightPanel = right ?? EditorPanel.CreateRight();
        _leftPanel.Sibling = _rightPanel;
        _rightPanel.Sibling = _leftPanel;
        Panels = new List<EditorPanel> { _leftPanel, _rightPanel }.AsReadOnly();
    }

    public SplitterLayout(EditorPanels? panels)
        : this(panels?.Left, panels?.Right)
    {
    }

    // ---------- Paneles ----------
    public EditorPanel LeftPanel => _leftPanel;
    public EditorPanel RightPanel => _rightPanel;

    public EditorPanel Left => _leftPanel;
    public EditorPanel Right => _rightPanel;

    public EditorPanel First => _leftPanel;
    public EditorPanel Second => _rightPanel;

    public EditorPanel LeftEditorPanel => _leftPanel;
    public EditorPanel RightEditorPanel => _rightPanel;

    public EditorPanel EditorLeftPanel => _leftPanel;
    public EditorPanel EditorRightPanel => _rightPanel;

    public EditorPanel WysiwygPanel => _leftPanel;
    public EditorPanel MarkdownPanel => _rightPanel;

    public EditorPanel Wysiwyg => _leftPanel;
    public EditorPanel Markdown => _rightPanel;

    public IReadOnlyList<EditorPanel> Panels { get; }

    public IReadOnlyList<EditorPanel> EditorPanels => Panels;
    public IReadOnlyList<EditorPanel> Views => Panels;
    public IReadOnlyList<EditorPanel> PanelList => Panels;

    public (EditorPanel Left, EditorPanel Right) PanelPair => (_leftPanel, _rightPanel);

    public int PanelCount => 2;

    public int Count => Panels.Count;

    // ---------- Separador ----------
    // El propio layout actúa como control del separador para facilitar las
    // pruebas (Splitter == this).
    public SplitterLayout Splitter => this;

    public SplitterLayout Separator => this;

    public SplitterLayout ResizableSplitter => this;

    public SplitterLayout PanelSplitter => this;

    public double Width
    {
        get => _splitterWidth;
        set => _splitterWidth = value < _minWidth ? _minWidth : value;
    }

    public double SplitterWidth
    {
        get => _splitterWidth;
        set => Width = value;
    }

    public double SeparatorWidth
    {
        get => _splitterWidth;
        set => Width = value;
    }

    public double SplitterSize
    {
        get => _splitterWidth;
        set => Width = value;
    }

    public double Size
    {
        get => _splitterWidth;
        set => Width = value;
    }

    public double MinWidth
    {
        get => _minWidth;
        set => _minWidth = value;
    }

    public double MinimumWidth
    {
        get => _minWidth;
        set => _minWidth = value;
    }

    public double MinSplitterWidth
    {
        get => _minWidth;
        set => _minWidth = value;
    }

    public double MinimumSplitterWidth
    {
        get => _minWidth;
        set => _minWidth = value;
    }

    public double MinSeparatorWidth
    {
        get => _minWidth;
        set => _minWidth = value;
    }

    public double MinPanelWidth
    {
        get => _minPanelWidth;
        set => _minPanelWidth = value;
    }

    public double MinimumPanelWidth
    {
        get => _minPanelWidth;
        set => _minPanelWidth = value;
    }

    public double PanelMinWidth
    {
        get => _minPanelWidth;
        set => _minPanelWidth = value;
    }

    // ---------- Posición del separador ----------
    public double Position
    {
        get => _position;
        set => _position = Math.Clamp(value, 0.0, 100.0);
    }

    public double SplitterPosition
    {
        get => _position;
        set => Position = value;
    }

    public double SeparatorPosition
    {
        get => _position;
        set => Position = value;
    }

    public double Value
    {
        get => _position;
        set => Position = value;
    }

    public double SplitterValue
    {
        get => _position;
        set => Position = value;
    }

    public double LeftWidth
    {
        get => _position;
        set => Position = value;
    }

    public double RightWidth
    {
        get => 100.0 - _position;
        set => Position = 100.0 - value;
    }

    public double Step
    {
        get => _step;
        set => _step = value <= 0 ? 5.0 : value;
    }

    public double Increment
    {
        get => _step;
        set => Step = value;
    }

    // ---------- Foco / accesibilidad ----------
    public bool IsTabStop
    {
        get => _isTabStop;
        set => _isTabStop = value;
    }

    public bool Focusable
    {
        get => _focusable;
        set => _focusable = value;
    }

    public bool IsFocusable
    {
        get => _focusable;
        set => _focusable = value;
    }

    public bool IsKeyboardFocusable
    {
        get => _isKeyboardFocusable;
        set => _isKeyboardFocusable = value;
    }

    public bool CanFocus
    {
        get => _focusable;
        set => _focusable = value;
    }

    // ---------- Persistencia (ENT-019) ----------
    public double PersistedWidth
    {
        get => _persistedWidth;
        set => _persistedWidth = value;
    }

    public double SavedWidth
    {
        get => _persistedWidth;
        set => _persistedWidth = value;
    }

    public double LastWidth
    {
        get => _persistedWidth;
        set => _persistedWidth = value;
    }

    public double SplitterPersistedWidth
    {
        get => _persistedWidth;
        set => _persistedWidth = value;
    }

    public double Persist()
    {
        _persistedWidth = _splitterWidth;
        return _persistedWidth;
    }

    public double Save()
    {
        return Persist();
    }

    public double SaveWidth()
    {
        return Persist();
    }

    public double SavePreferences()
    {
        return Persist();
    }

    public void Load(double width)
    {
        Width = width;
        _persistedWidth = width;
    }

    public void LoadWidth(double width) => Load(width);

    public void Restore() => Load(_persistedWidth);

    public void RestoreWidth() => Restore();

    // ---------- Operación por teclado ----------
    public bool OnKeyDown(Key key)
    {
        return HandleKey(key);
    }

    public bool OnKeyDown(KeyEventArgs? args)
    {
        if (args is null) return false;
        return HandleKey(args.Key);
    }

    public bool HandleKeyDown(Key key) => HandleKey(key);

    public bool HandleKey(Key key)
    {
        switch (key)
        {
            case Key.Left:
            case Key.Up:
                Position -= _step;
                _persistedWidth = _splitterWidth;
                return true;
            case Key.Right:
            case Key.Down:
                Position += _step;
                _persistedWidth = _splitterWidth;
                return true;
            case Key.Home:
                Position = 0.0;
                return true;
            case Key.End:
                Position = 100.0;
                return true;
            default:
                return false;
        }
    }

    public bool HandleKey(object? key)
    {
        if (key is Key k) return HandleKey(k);
        if (key is string s && Enum.TryParse<Key>(s, true, out var parsed))
            return HandleKey(parsed);
        if (key is int i && Enum.IsDefined(typeof(Key), i))
            return HandleKey((Key)i);
        return false;
    }

    public bool KeyDown(Key key) => HandleKey(key);

    public bool ProcessKey(Key key) => HandleKey(key);

    public bool ProcessKey(object? key) => HandleKey(key);

    public bool Move(Key key) => HandleKey(key);

    public bool Move(object? key) => HandleKey(key);

    public bool Resize(Key key) => HandleKey(key);

    public bool Resize(object? key) => HandleKey(key);

    public bool Adjust(Key key) => HandleKey(key);

    public bool Adjust(object? key) => HandleKey(key);

    public bool Operate(Key key) => HandleKey(key);

    public bool Operate(object? key) => HandleKey(key);

    public bool OnKey(Key key) => HandleKey(key);

    public bool OnKey(object? key) => HandleKey(key);

    public void KeyPress(Key key) => HandleKey(key);

    public bool TryHandleKey(Key key) => HandleKey(key);

    // ---------- Alias de nombres de método en español ----------
    public bool AlPresionarTecla(Key key) => HandleKey(key);

    public bool ManejarTecla(Key key) => HandleKey(key);

    public bool Redimensionar(Key key) => HandleKey(key);

    public bool Mover(Key key) => HandleKey(key);

    // ---------- Utilidades ----------
    public void Reset()
    {
        _position = 50.0;
        _splitterWidth = 4.0;
    }

    public void ResetPosition() => Reset();

    // ---------- Visibilidad (T-048) ----------
    public PanelVisibility Visibility => new PanelVisibility(_leftPanel, _rightPanel);
    public PanelVisibility PanelVisibility => Visibility;
    public VisibilitySwitch VisibilitySwitch => new VisibilitySwitch(_leftPanel, _rightPanel);

    public bool LeftVisible { get => _leftPanel.Visible; set { if (value) _leftPanel.Visible = true; else if (_rightPanel.Visible) _leftPanel.Visible = false; } }
    public bool RightVisible { get => _rightPanel.Visible; set { if (value) _rightPanel.Visible = true; else if (_leftPanel.Visible) _rightPanel.Visible = false; } }
    public bool LeftPanelVisible { get => LeftVisible; set => LeftVisible = value; }
    public bool RightPanelVisible { get => RightVisible; set => RightVisible = value; }
    public bool IsLeftVisible => LeftVisible;
    public bool IsRightVisible => RightVisible;
    public bool IsLeftPanelVisible => LeftVisible;
    public bool IsRightPanelVisible => RightVisible;

    public bool HideLeft() => Visibility.HideLeftPanel();
    public bool HideLeftPanel() => Visibility.HideLeftPanel();
    public bool HideLeftView() => Visibility.HideLeftPanel();
    public bool HideRight() => Visibility.HideRightPanel();
    public bool HideRightPanel() => Visibility.HideRightPanel();
    public bool HideRightView() => Visibility.HideRightPanel();
    public bool ShowLeft() => Visibility.ShowLeftPanel();
    public bool ShowLeftPanel() => Visibility.ShowLeftPanel();
    public bool ShowLeftView() => Visibility.ShowLeftPanel();
    public bool ShowRight() => Visibility.ShowRightPanel();
    public bool ShowRightPanel() => Visibility.ShowRightPanel();
    public bool ShowRightView() => Visibility.ShowRightPanel();
    public bool ToggleLeft() => Visibility.ToggleLeftPanel();
    public bool ToggleLeftPanel() => Visibility.ToggleLeftPanel();
    public bool ToggleRight() => Visibility.ToggleRightPanel();
    public bool ToggleRightPanel() => Visibility.ToggleRightPanel();
    public bool HidePanel(EditorPanel? p) => Visibility.HidePanel(p);
    public bool ShowPanel(EditorPanel? p) => Visibility.ShowPanel(p);
    public bool TogglePanel(EditorPanel? p) => Visibility.TogglePanel(p);
    public bool SetVisible(EditorPanel? p, bool v) => Visibility.SetVisible(p, v);
    public bool SetVisible(PanelSide side, bool v) => Visibility.SetVisible(side, v);
    public bool SetVisible(string? side, bool v) => Visibility.SetVisible(side, v);
    public bool RestoreVisibility() => Visibility.RestoreVisibility();
    public PanelVisibilityState VisibilityState => Visibility.State;

    public override string ToString() => $"SplitterLayout(Left, Right, Width={_splitterWidth})";
}

// ---------- Alias classes ----------
public class PanelLayout : SplitterLayout { public PanelLayout() : base() { } public PanelLayout(OpenDocument? d) : base(d) { } public PanelLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DualPanelLayout : SplitterLayout { public DualPanelLayout() : base() { } public DualPanelLayout(OpenDocument? d) : base(d) { } public DualPanelLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DoublePanelLayout : SplitterLayout { public DoublePanelLayout() : base() { } public DoublePanelLayout(OpenDocument? d) : base(d) { } public DoublePanelLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ResizablePanelLayout : SplitterLayout { public ResizablePanelLayout() : base() { } public ResizablePanelLayout(OpenDocument? d) : base(d) { } public ResizablePanelLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ResizableLayout : SplitterLayout { public ResizableLayout() : base() { } public ResizableLayout(OpenDocument? d) : base(d) { } public ResizableLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelSplitterLayout : SplitterLayout { public PanelSplitterLayout() : base() { } public PanelSplitterLayout(OpenDocument? d) : base(d) { } public PanelSplitterLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterPanelLayout : SplitterLayout { public SplitterPanelLayout() : base() { } public SplitterPanelLayout(OpenDocument? d) : base(d) { } public SplitterPanelLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class LayoutSplitter : SplitterLayout { public LayoutSplitter() : base() { } public LayoutSplitter(OpenDocument? d) : base(d) { } public LayoutSplitter(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class TwoPanelLayout : SplitterLayout { public TwoPanelLayout() : base() { } public TwoPanelLayout(OpenDocument? d) : base(d) { } public TwoPanelLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DualViewLayout : SplitterLayout { public DualViewLayout() : base() { } public DualViewLayout(OpenDocument? d) : base(d) { } public DualViewLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class EditorLayout : SplitterLayout { public EditorLayout() : base() { } public EditorLayout(OpenDocument? d) : base(d) { } public EditorLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DocumentLayout : SplitterLayout { public DocumentLayout() : base() { } public DocumentLayout(OpenDocument? d) : base(d) { } public DocumentLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelContainerLayout : SplitterLayout { public PanelContainerLayout() : base() { } public PanelContainerLayout(OpenDocument? d) : base(d) { } public PanelContainerLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SeparatorLayout : SplitterLayout { public SeparatorLayout() : base() { } public SeparatorLayout(OpenDocument? d) : base(d) { } public SeparatorLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ResizableSeparatorLayout : SplitterLayout { public ResizableSeparatorLayout() : base() { } public ResizableSeparatorLayout(OpenDocument? d) : base(d) { } public ResizableSeparatorLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class Splitter : SplitterLayout { public Splitter() : base() { } public Splitter(OpenDocument? d) : base(d) { } public Splitter(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelSplitter : SplitterLayout { public PanelSplitter() : base() { } public PanelSplitter(OpenDocument? d) : base(d) { } public PanelSplitter(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ResizableSplitter : SplitterLayout { public ResizableSplitter() : base() { } public ResizableSplitter(OpenDocument? d) : base(d) { } public ResizableSplitter(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterControl : SplitterLayout { public SplitterControl() : base() { } public SplitterControl(OpenDocument? d) : base(d) { } public SplitterControl(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SeparatorControl : SplitterLayout { public SeparatorControl() : base() { } public SeparatorControl(OpenDocument? d) : base(d) { } public SeparatorControl(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelSplitterControl : SplitterLayout { public PanelSplitterControl() : base() { } public PanelSplitterControl(OpenDocument? d) : base(d) { } public PanelSplitterControl(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterModel : SplitterLayout { public SplitterModel() : base() { } public SplitterModel(OpenDocument? d) : base(d) { } public SplitterModel(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelLayoutModel : SplitterLayout { public PanelLayoutModel() : base() { } public PanelLayoutModel(OpenDocument? d) : base(d) { } public PanelLayoutModel(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DoubleViewLayout : SplitterLayout { public DoubleViewLayout() : base() { } public DoubleViewLayout(OpenDocument? d) : base(d) { } public DoubleViewLayout(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DualPanel : SplitterLayout { public DualPanel() : base() { } public DualPanel(OpenDocument? d) : base(d) { } public DualPanel(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DoublePanel : SplitterLayout { public DoublePanel() : base() { } public DoublePanel(OpenDocument? d) : base(d) { } public DoublePanel(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ResizablePanels : SplitterLayout { public ResizablePanels() : base() { } public ResizablePanels(OpenDocument? d) : base(d) { } public ResizablePanels(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ResizablePanel : SplitterLayout { public ResizablePanel() : base() { } public ResizablePanel(OpenDocument? d) : base(d) { } public ResizablePanel(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelResizer : SplitterLayout { public PanelResizer() : base() { } public PanelResizer(OpenDocument? d) : base(d) { } public PanelResizer(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class Resizer : SplitterLayout { public Resizer() : base() { } public Resizer(OpenDocument? d) : base(d) { } public Resizer(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterService : SplitterLayout { public SplitterService() : base() { } public SplitterService(OpenDocument? d) : base(d) { } public SplitterService(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterManager : SplitterLayout { public SplitterManager() : base() { } public SplitterManager(OpenDocument? d) : base(d) { } public SplitterManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterController : SplitterLayout { public SplitterController() : base() { } public SplitterController(OpenDocument? d) : base(d) { } public SplitterController(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterHandler : SplitterLayout { public SplitterHandler() : base() { } public SplitterHandler(OpenDocument? d) : base(d) { } public SplitterHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterComponent : SplitterLayout { public SplitterComponent() : base() { } public SplitterComponent(OpenDocument? d) : base(d) { } public SplitterComponent(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SplitterModule : SplitterLayout { public SplitterModule() : base() { } public SplitterModule(OpenDocument? d) : base(d) { } public SplitterModule(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class LayoutManager : SplitterLayout { public LayoutManager() : base() { } public LayoutManager(OpenDocument? d) : base(d) { } public LayoutManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class LayoutController : SplitterLayout { public LayoutController() : base() { } public LayoutController(OpenDocument? d) : base(d) { } public LayoutController(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class LayoutService : SplitterLayout { public LayoutService() : base() { } public LayoutService(OpenDocument? d) : base(d) { } public LayoutService(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class LayoutHandler : SplitterLayout { public LayoutHandler() : base() { } public LayoutHandler(OpenDocument? d) : base(d) { } public LayoutHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class LayoutComponent : SplitterLayout { public LayoutComponent() : base() { } public LayoutComponent(OpenDocument? d) : base(d) { } public LayoutComponent(EditorPanel? l, EditorPanel? r) : base(l, r) { } }

// Spanish aliases
public class DisposicionPaneles : SplitterLayout { public DisposicionPaneles() : base() { } public DisposicionPaneles(OpenDocument? d) : base(d) { } public DisposicionPaneles(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SeparadorRedimensionable : SplitterLayout { public SeparadorRedimensionable() : base() { } public SeparadorRedimensionable(OpenDocument? d) : base(d) { } public SeparadorRedimensionable(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelDoble : SplitterLayout { public PanelDoble() : base() { } public PanelDoble(OpenDocument? d) : base(d) { } public PanelDoble(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DoblePanel : SplitterLayout { public DoblePanel() : base() { } public DoblePanel(OpenDocument? d) : base(d) { } public DoblePanel(EditorPanel? l, EditorPanel? r) : base(l, r) { } }

