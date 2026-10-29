global using Side = EggMdEditor.App.PanelSide;
global using Lado = EggMdEditor.App.PanelSide;
global using PanelLado = EggMdEditor.App.PanelSide;
global using ViewSide = EggMdEditor.App.PanelSide;
global using EditorSide = EggMdEditor.App.PanelSide;
global using PanelPlacement = EggMdEditor.App.PanelSide;
global using PanelLocation = EggMdEditor.App.PanelSide;
global using PosicionPanel = EggMdEditor.App.PanelSide;

global using Mode = EggMdEditor.App.EditorMode;
global using PanelMode = EggMdEditor.App.EditorMode;
global using ViewMode = EggMdEditor.App.EditorMode;
global using EditorViewMode = EggMdEditor.App.EditorMode;
global using RenderMode = EggMdEditor.App.EditorMode;
global using Modo = EggMdEditor.App.EditorMode;

global using FocusStatusAlias = EggMdEditor.App.FocusState;
global using EstadoFocoAlias = EggMdEditor.App.FocusState;

global using PanelViewAlias = EggMdEditor.App.EditorPanel;
global using PanelAlias = EggMdEditor.App.EditorPanel;
global using ViewAlias = EggMdEditor.App.EditorPanel;
global using EditorViewAlias = EggMdEditor.App.EditorPanel;
global using ViewPanelAlias = EggMdEditor.App.EditorPanel;
global using PanelEditorAlias = EggMdEditor.App.EditorPanel;
global using EditorPanelViewAlias = EggMdEditor.App.EditorPanel;

using System;
using System.Collections.Generic;

namespace EggMdEditor.App;

/// <summary>
/// Vista de panel de edición (ENT-015): representa un panel (izquierdo
/// WYSIWYG o derecho Markdown) sobre el documento único compartido.
/// </summary>
public partial class EditorPanel
{
    private Guid _id = Guid.NewGuid();
    private PanelSide _side = PanelSide.Left;
    private EditorMode _mode = EditorMode.WYSIWYG;
    private bool _visible = true;
    private double _width = 50.0;
    private double _scrollPosition = 0.0;
    private FocusState _focus = FocusState.Inactive;
    private PassengerHighlight? _highlight;
    private OpenDocument? _document;

    public EditorPanel()
    {
    }

    public EditorPanel(PanelSide side, EditorMode mode, OpenDocument? document = null)
    {
        _side = side;
        _mode = mode;
        _document = document;
        Synchronizer.AllPanels.Add(this);
        AttachDocument(_document);
        if (side == PanelSide.Right && _mode == EditorMode.WYSIWYG)
        {
            // sin cambio forzado: el caller decide
        }
    }

    public EditorPanel(EditorMode mode, PanelSide side, OpenDocument? document = null)
        : this(side, mode, document)
    {
    }

    public EditorPanel(OpenDocument? document, PanelSide side, EditorMode mode)
        : this(side, mode, document)
    {
    }

    public EditorPanel(OpenDocument? document)
        : this(PanelSide.Left, EditorMode.WYSIWYG, document)
    {
    }

    public EditorPanel(OpenDocument? document, bool left)
        : this(left ? PanelSide.Left : PanelSide.Right,
               left ? EditorMode.WYSIWYG : EditorMode.Markdown,
               document)
    {
    }

    public EditorPanel(OpenDocument? document, PanelSide side)
        : this(side, side == PanelSide.Right ? EditorMode.Markdown : EditorMode.WYSIWYG, document)
    {
    }

    public EditorPanel(OpenDocument? document, EditorMode mode)
        : this(mode == EditorMode.Markdown ? PanelSide.Right : PanelSide.Left, mode, document)
    {
    }

    public EditorPanel(PanelSide side, OpenDocument? document)
        : this(side, side == PanelSide.Right ? EditorMode.Markdown : EditorMode.WYSIWYG, document)
    {
    }

    public EditorPanel(EditorMode mode, OpenDocument? document)
        : this(mode == EditorMode.Markdown ? PanelSide.Right : PanelSide.Left, mode, document)
    {
    }

    public EditorPanel(bool left, OpenDocument? document = null)
        : this(left ? PanelSide.Left : PanelSide.Right,
               left ? EditorMode.WYSIWYG : EditorMode.Markdown,
               document)
    {
    }

    // ---------- Identificador ----------
    public Guid Id
    {
        get => _id;
        set => _id = value;
    }

    public Guid Identifier
    {
        get => _id;
        set => _id = value;
    }

    public Guid Identificador
    {
        get => _id;
        set => _id = value;
    }

    // ---------- Lado ----------
    public PanelSide Side
    {
        get => _side;
        set => _side = value;
    }

    public PanelSide Lado
    {
        get => _side;
        set => _side = value;
    }

    public PanelSide PanelSide
    {
        get => _side;
        set => _side = value;
    }

    public bool IsLeft => _side == PanelSide.Left || _side == PanelSide.Izquierdo;
    public bool IsRight => _side == PanelSide.Right || _side == PanelSide.Derecho;
    public bool EsIzquierdo => IsLeft;
    public bool EsDerecho => IsRight;

    // ---------- Modo ----------
    public EditorMode Mode
    {
        get => _mode;
        set => _mode = value;
    }

    public EditorMode Modo
    {
        get => _mode;
        set => _mode = value;
    }

    public EditorMode EditorMode
    {
        get => _mode;
        set => _mode = value;
    }

    // alias enum-typed properties (cast-compatible)
    public PanelMode PanelMode
    {
        get => (PanelMode)(int)_mode;
        set => _mode = (EditorMode)(int)value;
    }

    public ViewMode ViewMode
    {
        get => (ViewMode)(int)_mode;
        set => _mode = (EditorMode)(int)value;
    }

    public bool IsWysiwyg => _mode == EditorMode.WYSIWYG || _mode == EditorMode.Wysiwyg;
    public bool IsMarkdown => _mode == EditorMode.Markdown;
    public bool EsWysiwyg => IsWysiwyg;
    public bool EsMarkdown => IsMarkdown;

    // ---------- Visibilidad ----------
    public bool Visible
    {
        get => _visible;
        set => _visible = value;
    }

    public bool IsVisible
    {
        get => _visible;
        set => _visible = value;
    }

    public bool Visibilidad
    {
        get => _visible;
        set => _visible = value;
    }

    public bool VisiblePanel
    {
        get => _visible;
        set => _visible = value;
    }

    // ---------- Ancho ----------
    public double Width
    {
        get => _width;
        set => _width = value;
    }

    public double Ancho
    {
        get => _width;
        set => _width = value;
    }

    public double WidthPercent
    {
        get => _width;
        set => _width = value;
    }

    public double Percent
    {
        get => _width;
        set => _width = value;
    }

    public double Size
    {
        get => _width;
        set => _width = value;
    }

    public double AnchoPorcentaje
    {
        get => _width;
        set => _width = value;
    }

    // ---------- Posición de desplazamiento ----------
    public double ScrollPosition
    {
        get => _scrollPosition;
        set => _scrollPosition = value;
    }

    public double Position
    {
        get => _scrollPosition;
        set => _scrollPosition = value;
    }

    public double ScrollOffset
    {
        get => _scrollPosition;
        set => _scrollPosition = value;
    }

    public double Offset
    {
        get => _scrollPosition;
        set => _scrollPosition = value;
    }

    public double PosicionDesplazamiento
    {
        get => _scrollPosition;
        set => _scrollPosition = value;
    }

    public double Scroll
    {
        get => _scrollPosition;
        set => _scrollPosition = value;
    }

    // ---------- Foco ----------
    public FocusState Focus
    {
        get => _focus;
        set => _focus = value;
    }

    public FocusState FocusState
    {
        get => _focus;
        set => _focus = value;
    }

    public FocusState FocusStatus
    {
        get => _focus;
        set => _focus = value;
    }

    public FocusState EstadoFoco
    {
        get => _focus;
        set => _focus = value;
    }

    public bool IsActive
    {
        get => _focus == FocusState.Active || _focus == FocusState.Activo;
        set => _focus = value ? FocusState.Active : FocusState.Inactive;
    }

    public bool IsFocused => IsActive;

    // ---------- Resaltado pasajero ----------
    public PassengerHighlight? Highlight
    {
        get => _highlight;
        set => _highlight = value;
    }

    public PassengerHighlight? Resaltado
    {
        get => _highlight;
        set => _highlight = value;
    }

    public PassengerHighlight? PassengerHighlight
    {
        get => _highlight;
        set => _highlight = value;
    }

    public PassengerHighlight? ResaltadoPasajero
    {
        get => _highlight;
        set => _highlight = value;
    }

    public object? HighlightReference
    {
        get => _highlight;
        set => _highlight = value as PassengerHighlight;
    }

    // ---------- Modelo compartido ----------
    public OpenDocument? Document
    {
        get => _document;
        set
        {
            if (ReferenceEquals(_document, value)) return;
            DetachDocument(_document);
            _document = value;
            AttachDocument(_document);
        }
    }

    public OpenDocument? Model
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? OpenDocument
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? SharedDocument
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? SharedModel
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? Doc
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? CurrentDocument
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? Documento
    {
        get => _document;
        set => _document = value;
    }

    // ---------- Factory helpers ----------
    public static EditorPanel CreateLeft(OpenDocument? document = null)
        => new EditorPanel(PanelSide.Left, EditorMode.WYSIWYG, document);

    public static EditorPanel CreateRight(OpenDocument? document = null)
        => new EditorPanel(PanelSide.Right, EditorMode.Markdown, document);

    public static EditorPanel Left(OpenDocument? document = null)
        => CreateLeft(document);

    public static EditorPanel Right(OpenDocument? document = null)
        => CreateRight(document);

    public static EditorPanel Create(PanelSide side, OpenDocument? document = null)
        => side == PanelSide.Right || side == PanelSide.Derecho
            ? CreateRight(document)
            : CreateLeft(document);

    public static (EditorPanel Left, EditorPanel Right) CreatePair(OpenDocument? document = null)
    {
        var doc = document ?? new OpenDocument();
        return (CreateLeft(doc), CreateRight(doc));
    }

    public static EditorPanel[] CreatePanels(OpenDocument? document = null)
    {
        var pair = CreatePair(document);
        return new[] { pair.Left, pair.Right };
    }

    public override string ToString() => $"EditorPanel({Side}, {Mode})";
}

// ---------- Enums ----------
public enum PanelSide
{
    Left,
    Right,
    Izquierdo = Left,
    Derecho = Right,
}

public enum EditorMode
{
    WYSIWYG,
    Markdown,
    Wysiwyg = WYSIWYG,
}

// alias enums (global using below)

// PanelMode and ViewMode are aliases of EditorMode (global using below).
public enum FocusState
{
    Inactive,
    Active,
    Inactivo = Inactive,
    Activo = Active,
}

public enum HighlightState
{
    Active,
    Expired,
    Activo = Active,
    Expirado = Expired,
}

// ---------- Highlight ----------
public partial class PassengerHighlight { }

public class ResaltadoPasajero : PassengerHighlight { public ResaltadoPasajero() : base() { } public ResaltadoPasajero(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public ResaltadoPasajero(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class Highlight : PassengerHighlight { public Highlight() : base() { } public Highlight(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public Highlight(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }
public class PassengerMark : PassengerHighlight { public PassengerMark() : base() { } public PassengerMark(EditorPanel? p, object? r = null, int d = 1500) : base(p, r, d) { } public PassengerMark(EditorPanel? p, object? r, int d, bool red) : base(p, r, d, red) { } }

// ---------- Aliases (classes) ----------
public class PanelView : EditorPanel
{
    public PanelView() : base() { }
    public PanelView(PanelSide side, EditorMode mode, OpenDocument? document = null) : base(side, mode, document) { }
    public PanelView(OpenDocument? document) : base(document) { }
}

public class Panel : EditorPanel
{
    public Panel() : base() { }
    public Panel(PanelSide side, EditorMode mode, OpenDocument? document = null) : base(side, mode, document) { }
    public Panel(OpenDocument? document) : base(document) { }
}

public class View : EditorPanel
{
    public View() : base() { }
    public View(PanelSide side, EditorMode mode, OpenDocument? document = null) : base(side, mode, document) { }
    public View(OpenDocument? document) : base(document) { }
}

public class EditorView : EditorPanel
{
    public EditorView() : base() { }
    public EditorView(PanelSide side, EditorMode mode, OpenDocument? document = null) : base(side, mode, document) { }
    public EditorView(OpenDocument? document) : base(document) { }
}

public class ViewPanel : EditorPanel
{
    public ViewPanel() : base() { }
    public ViewPanel(PanelSide side, EditorMode mode, OpenDocument? document = null) : base(side, mode, document) { }
    public ViewPanel(OpenDocument? document) : base(document) { }
}

public class PanelEditor : EditorPanel
{
    public PanelEditor() : base() { }
    public PanelEditor(PanelSide side, EditorMode mode, OpenDocument? document = null) : base(side, mode, document) { }
    public PanelEditor(OpenDocument? document) : base(document) { }
}

public class EditorPanelView : EditorPanel
{
    public EditorPanelView() : base() { }
    public EditorPanelView(PanelSide side, EditorMode mode, OpenDocument? document = null) : base(side, mode, document) { }
    public EditorPanelView(OpenDocument? document) : base(document) { }
}

// ---------- Container for the two panels over the same model ----------
public partial class EditorPanels
{
    public OpenDocument Document { get; }
    public EditorPanel Left { get; }
    public EditorPanel Right { get; }
    public EditorPanel LeftPanel { get => Left; }
    public EditorPanel RightPanel { get => Right; }
    public EditorPanel First { get => Left; }
    public EditorPanel Second { get => Right; }
    public EditorPanel Wysiwyg { get => Left; }
    public EditorPanel Markdown { get => Right; }
    public IReadOnlyList<EditorPanel> Panels { get; }

    public EditorPanels(OpenDocument? document = null)
    {
        Document = document ?? new OpenDocument();
        Left = EditorPanel.CreateLeft(Document);
        Right = EditorPanel.CreateRight(Document);
        Panels = new List<EditorPanel> { Left, Right }.AsReadOnly();
    }

    public EditorPanels() : this(new OpenDocument()) { }
}

public class PanelSet : EditorPanels { public PanelSet(OpenDocument? d = null) : base(d) { } public PanelSet() : base() { } }
public class DualPanels : EditorPanels { public DualPanels(OpenDocument? d = null) : base(d) { } public DualPanels() : base() { } }
public class PanelPair : EditorPanels { public PanelPair(OpenDocument? d = null) : base(d) { } public PanelPair() : base() { } }
public class DualView : EditorPanels { public DualView(OpenDocument? d = null) : base(d) { } public DualView() : base() { } }
public class DualEditor : EditorPanels { public DualEditor(OpenDocument? d = null) : base(d) { } public DualEditor() : base() { } }
public class TwoPanels : EditorPanels { public TwoPanels(OpenDocument? d = null) : base(d) { } public TwoPanels() : base() { } }
public class PanelsContainer : EditorPanels { public PanelsContainer(OpenDocument? d = null) : base(d) { } public PanelsContainer() : base() { } }

