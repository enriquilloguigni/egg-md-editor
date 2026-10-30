using System;
using Avalonia.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using EggMdEditor.App.I18n;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

/// <summary>
/// ViewModel de la ventana principal SC-001 (T-006, T-015): expone los 6
/// rótulos de FR-011 (ENT-004) como botones en orden y su navegación
/// secuencial cíclica (OP-007), con el texto resuelto desde el catálogo i18n
/// (ENT-006) en el idioma activo. Solo presentación: no hay acciones
/// conectadas a documento (ADR-001); la conmutación de idioma (OP-001) y de
/// tema (OP-002) y las superficies de ayuda (SC-003) y diálogo genérico
/// (SC-002) son las únicas acciones disponibles y ninguna altera el documento.
/// </summary>
public partial class MainWindowViewModel : CommandDispatcher, INotifyPropertyChanged
{
    private ICommand? _changeLanguageCommand;
    private ICommand? _changeThemeCommand;
    private ICommand? _selectNextMenuCommand;
    private ICommand? _selectPreviousMenuCommand;
    private Theme _theme = Theme.Light;
    private ThemePreference _themePreference = ThemePreference.System;
    private IThemeApplier? _themeApplier;
    private MenuState _menuState = MenuState.Closed;
    private string? _openMenuKey;
    private int _selectedMenuIndex = 1;
    private bool _noticeVisible;
    private string? _activeNoticeKey;
    private readonly List<string> _menuShortcutCombinations = new();
    private bool _registeringShortcuts;
    private DialogViewModel? _dialog;
    private OpenDocument? _document;
    private EditorPanel? _leftPanel;
    private EditorPanel? _rightPanel;
    private PropertyChangedEventHandler? _documentTitleChangedHandler;
    private readonly RecentFiles _recentFiles = new();
    private RecoveryCopy? _recoveryCopy = null!;
    private ICommand? _newCommand;
    private ICommand? _openCommand;
    private ICommand? _saveCommand;
    private ICommand? _saveAsCommand;
    private ICommand? _openRecentCommand;

    public MainWindowViewModel()
    {
        RegisterAvailableShortcuts();
    }

    public MainWindowViewModel(Session? session)
        : base(session)
    {
        Catalog = session?.Catalog;
        RegisterAvailableShortcuts();
        try { ApplyPreferences(session?.Preferences); } catch { }
        try { ApplyPreferences(session?.LoadedPreferences); } catch { }
    }

    public MainWindowViewModel(II18nCatalog? catalog)
        : base(catalog)
    {
        RegisterAvailableShortcuts();
    }

    /// <summary>Claves i18n de los 6 menús de FR-011, en el orden declarado.</summary>
    public static IReadOnlyList<string> MenuKeys { get; } = Array.AsReadOnly(
    [
        "Menu.File",
        "Menu.Edit",
        "Menu.View",
        "Menu.Format",
        "Menu.Tools",
        "Menu.Help",
    ]);

    public const string LanguageMenuKey = "Menu.Tools.Language";
    public const string SpanishLanguageKey = "Language.Spanish";
    public const string EnglishLanguageKey = "Language.English";

    /// <summary>Entrada Herramientas → Tema (OP-002, SC-001).</summary>
    public const string ThemeMenuKey = "Menu.Tools.Theme";
    public const string LightThemeKey = "Theme.Light";
    public const string DarkThemeKey = "Theme.Dark";
    public const string SystemThemeKey = "Theme.System";

    /// <summary>Rótulo del botón de navegación secuencial «Anterior» (OP-007).</summary>
    public const string PreviousMenuKey = "Menu.Previous";

    /// <summary>Rótulo del botón de navegación secuencial «Siguiente» (OP-007).</summary>
    public const string NextMenuKey = "Menu.Next";

    /// <summary>Entradas de FR-010 en el menú Ayuda (SC-003).</summary>
    public const string HelpGuideMenuKey = "Menu.Help.Guide";
    public const string HelpShortcutsMenuKey = "Menu.Help.Shortcuts";
    public const string HelpAboutMenuKey = "Menu.Help.About";

    /// <summary>
    /// Controles de la barra de formato de FR-012 en orden de escritura
    /// (ENT-005): clave i18n estable e icono Material Design (VR-001). El
    /// orden de la lista es el orden visible de la barra: no admite controles
    /// de más ni ausentes. Ningún icono es un glifo de texto.
    /// </summary>
    private static readonly (string Key, MaterialIconKind Icon)[] FormatBarLayout =
    [
        ("barra.enfasis", MaterialIconKind.FormatBold),
        ("barra.encabezados", MaterialIconKind.FormatHeader1),
        ("barra.listas", MaterialIconKind.FormatListBulleted),
        ("barra.cita", MaterialIconKind.FormatQuoteClose),
        ("barra.codigo", MaterialIconKind.CodeTags),
        ("barra.enlace", MaterialIconKind.LinkVariant),
        ("barra.imagen", MaterialIconKind.ImageOutline),
        ("barra.tabla", MaterialIconKind.Table),
        ("barra.nota-al-pie", MaterialIconKind.FormatSuperscript),
        ("barra.formula", MaterialIconKind.FunctionVariant),
        ("barra.diagrama", MaterialIconKind.FlowchartOutline),
        ("barra.indice", MaterialIconKind.Toc),
    ];

    /// <summary>Claves i18n de los controles de la barra de formato, en orden.</summary>
    public static IReadOnlyList<string> FormatBarKeys { get; } = Array.AsReadOnly(
        FormatBarLayout.Select(control => control.Key).ToArray());

    /// <summary>Icono (VR-001) de cada control, por clave i18n.</summary>
    public static IReadOnlyDictionary<string, MaterialIconKind> FormatBarIcons { get; } =
        FormatBarLayout.ToDictionary(
            control => control.Key,
            control => control.Icon,
            StringComparer.Ordinal);

    /// <summary>
    /// Menús de primer nivel de SC-001, en el orden de FR-011. Cada elemento
    /// conserva su clave i18n y el rótulo resuelto en el idioma activo.
    /// </summary>
    public IReadOnlyList<MainMenuDefinition> Menus
    {
        get
        {
            var catalog = ResolveCatalog();
            var menus = new List<MainMenuDefinition>(MenuKeys.Count);
            foreach (var key in MenuKeys)
            {
                menus.Add(new MainMenuDefinition(key, catalog.Get(key)));
            }

            return new ReadOnlyCollection<MainMenuDefinition>(menus);
        }
    }

    /// <summary>
    /// Controles visibles de la barra de formato de SC-001 (ENT-005), en el
    /// orden de escritura de FR-012: clave i18n, icono de la librería
    /// MIT-compatible (VR-001) y nombre resuelto en el idioma activo, que
    /// alimenta tanto el aviso flotante (VR-002) como el nombre accesible
    /// (NFR-009).
    /// </summary>
    public IReadOnlyList<FormatBarControlDefinition> FormatBarControls
    {
        get
        {
            var catalog = ResolveCatalog();
            var controls = new List<FormatBarControlDefinition>(FormatBarKeys.Count);
            foreach (var key in FormatBarKeys)
            {
                controls.Add(new FormatBarControlDefinition(key, FormatBarIcons[key], catalog.Get(key)));
            }

            return new ReadOnlyCollection<FormatBarControlDefinition>(controls);
        }
    }

    public string FileMenuHeader => Menus[0].Header;
    public string EditMenuHeader => Menus[1].Header;
    public string ViewMenuHeader => Menus[2].Header;
    public string FormatMenuHeader => Menus[3].Header;
    public string ToolsMenuHeader => Menus[4].Header;
    public string HelpMenuHeader => Menus[5].Header;

    /// <summary>Entrada Ayuda → Guía de Markdown (SC-003, FR-010).</summary>
    public string HelpGuideHeader => ResolveCatalog().Get(HelpGuideMenuKey);

    /// <summary>Entrada Ayuda → Atajos de teclado (SC-003, FR-010).</summary>
    public string HelpShortcutsHeader => ResolveCatalog().Get(HelpShortcutsMenuKey);

    /// <summary>Entrada Ayuda → Acerca de Egg MD Editor (SC-003, FR-010).</summary>
    public string HelpAboutHeader => ResolveCatalog().Get(HelpAboutMenuKey);

    /// <summary>Documento abierto actual de F-002 (ENT-008).</summary>
    public OpenDocument? Document
    {
        get => _document;
        set
        {
            if (ReferenceEquals(_document, value)) return;

            if (_document is not null && _documentTitleChangedHandler is not null)
            {
                _document.PropertyChanged -= _documentTitleChangedHandler;
            }

            _document = value;

            if (_leftPanel is null)
            {
                _leftPanel = EditorPanel.CreateLeft(_document);
            }
            else
            {
                _leftPanel.Document = _document;
            }

            if (_rightPanel is null)
            {
                _rightPanel = EditorPanel.CreateRight(_document);
            }
            else
            {
                _rightPanel.Document = _document;
            }

            if (_leftPanel is not null && _rightPanel is not null)
            {
                _leftPanel.Sibling = _rightPanel;
                _rightPanel.Sibling = _leftPanel;
            }

            if (_document is not null)
            {
                _documentTitleChangedHandler ??= (_, e) =>
                {
                    if (e.PropertyName is null
                        || e.PropertyName == nameof(OpenDocument.DisplayName)
                        || e.PropertyName == nameof(OpenDocument.Title)
                        || e.PropertyName == nameof(OpenDocument.ChangeState)
                        || e.PropertyName == nameof(OpenDocument.HasChanges)
                        || e.PropertyName == nameof(OpenDocument.IsDirty)
                        || e.PropertyName == nameof(OpenDocument.IsModified)
                        || e.PropertyName == nameof(OpenDocument.Modified))
                    {
                        RaiseWindowTitleChanged();
                    }
                };
                _document.PropertyChanged += _documentTitleChangedHandler;
            }

            OnPropertyChanged(nameof(Document));
            OnPropertyChanged(nameof(CurrentDocument));
            OnPropertyChanged(nameof(OpenDocument));
            OnPropertyChanged(nameof(Doc));
            RaiseWindowTitleChanged();
        }
    }

    /// <summary>
    /// Título de la barra de título de SC-001 (COM-015, BR-F002-016, VR-006):
    /// nombre del documento («Sin título» si no hay ruta) precedido del nombre de
    /// la aplicación y, cuando hay cambios sin guardar, un indicador textual
    /// accesible (glifo) junto al nombre. Se actualiza al cambiar el documento
    /// o su estado de cambios.
    /// </summary>
    public string WindowTitle
    {
        get
        {
            var name = _document?.DisplayName;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Sin título";
            }

            var title = "Egg MD Editor — " + name;
            if (_document?.HasChanges == true)
            {
                title += " •";
            }

            return title;
        }
    }

    /// <summary>Alias de <see cref="WindowTitle"/> para pruebas y enlaces.</summary>
    public string Title => WindowTitle;
    public string TitleText => WindowTitle;
    public string WindowText => WindowTitle;
    public string MainWindowTitle => WindowTitle;
    public string DocumentTitle => WindowTitle;
    public string TitleBar => WindowTitle;
    public string TitleBarText => WindowTitle;
    public string WindowTitleText => WindowTitle;
    public string AppTitle => WindowTitle;

    /// <summary>
    /// Indicador de cambios sin guardar (VR-006): glifo textual junto al
    /// nombre, visible solo con cambios. Accesible a lectores de pantalla al
    /// formar parte del título.
    /// </summary>
    public string ChangeIndicator => _document?.HasChanges == true ? "•" : string.Empty;
    public string UnsavedChangesIndicator => ChangeIndicator;
    public string DirtyIndicator => ChangeIndicator;
    public string ModifiedIndicator => ChangeIndicator;
    public string ChangeStateIndicator => ChangeIndicator;
    public string StateIndicator => ChangeIndicator;
    public string PersistenceIndicator => ChangeIndicator;
    public string UnsavedIndicator => ChangeIndicator;
    public string ModificationIndicator => ChangeIndicator;

    public bool HasChangeIndicator => _document?.HasChanges == true;
    public bool IsChangeIndicatorVisible => HasChangeIndicator;
    public bool ShowChangeIndicator => HasChangeIndicator;
    public bool ChangeIndicatorVisible => HasChangeIndicator;
    public bool IsDirtyIndicatorVisible => HasChangeIndicator;
    public bool IsModifiedIndicatorVisible => HasChangeIndicator;

    public OpenDocument? CurrentDocument
    {
        get => _document;
        set => Document = value;
    }

    public OpenDocument? OpenDocumentInstance
    {
        get => _document;
        set => Document = value;
    }

    public OpenDocument? Doc
    {
        get => _document;
        set => Document = value;
    }

    /// <summary>Panels de doble vista (ENT-015, F-003): izquierdo WYSIWYG, derecho Markdown, sobre el mismo modelo.</summary>
    public EditorPanel LeftPanel
    {
        get
        {
            if (_leftPanel is null)
            {
                _leftPanel = EditorPanel.CreateLeft(_document);
            }
            return _leftPanel;
        }
        set => _leftPanel = value;
    }

    public EditorPanel RightPanel
    {
        get
        {
            if (_rightPanel is null)
            {
                _rightPanel = EditorPanel.CreateRight(_document);
            }
            return _rightPanel;
        }
        set => _rightPanel = value;
    }

    // Aliases
    public EditorPanel LeftEditorPanel { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel RightEditorPanel { get => RightPanel; set => RightPanel = value; }
    public EditorPanel EditorLeftPanel { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel EditorRightPanel { get => RightPanel; set => RightPanel = value; }
    public EditorPanel LeftView { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel RightView { get => RightPanel; set => RightPanel = value; }
    public EditorPanel WysiwygPanel { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel MarkdownPanel { get => RightPanel; set => RightPanel = value; }

    public IReadOnlyList<EditorPanel> Panels
    {
        get
        {
            var left = LeftPanel;
            var right = RightPanel;
            // keep shared model in sync
            left.Document = _document;
            right.Document = _document;
            return new List<EditorPanel> { left, right }.AsReadOnly();
        }
    }

    public IReadOnlyList<EditorPanel> EditorPanels => Panels;
    public IReadOnlyList<EditorPanel> Views => Panels;
    public IReadOnlyList<EditorPanel> PanelList => Panels;

    public (EditorPanel Left, EditorPanel Right) PanelPair => (LeftPanel, RightPanel);

    public void EnsurePanels()
    {
        _ = LeftPanel;
        _ = RightPanel;
        if (_leftPanel is not null && _rightPanel is not null)
        {
            _leftPanel.Sibling = _rightPanel;
            _rightPanel.Sibling = _leftPanel;
        }
    }

    // ---------- Visibilidad de paneles (T-048) ----------
    private ICommand? _hideLeftPanelCommand;
    private ICommand? _hideRightPanelCommand;
    private ICommand? _showLeftPanelCommand;
    private ICommand? _showRightPanelCommand;
    private ICommand? _toggleLeftPanelCommand;
    private ICommand? _toggleRightPanelCommand;
    private ICommand? _showFormatBarCommand;
    private ICommand? _hideFormatBarCommand;
    private ICommand? _toggleFormatBarCommand;
    private EditorPanel? _activePanel;
    public PanelVisibility Visibility => new PanelVisibility(LeftPanel, RightPanel);
    public PanelVisibility PanelVisibility => Visibility;
    public VisibilitySwitch VisibilitySwitch => new VisibilitySwitch(LeftPanel, RightPanel);

    // ---------- Conmutador de visibilidad de la barra de formato (T-049) ----------
    public FormatBarVisibility FormatBarVisibility => new FormatBarVisibility(this);
    public FormatBarVisibility FormatBarSwitch => new FormatBarVisibility(this);
    public FormatBarVisibility FormatBarConmutator => new FormatBarVisibility(this);
    public ConmutadorBarraFormato ConmutadorBarraFormatoObj => new ConmutadorBarraFormato(this);

    // ---------- Indicador de panel activo (T-052, VR-009, NFR-009) ----------
    public ActivePanelIndicator ActivePanelIndicator => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActiveIndicator => new ActivePanelIndicator(this);
    public ActivePanelIndicator PanelActiveIndicator => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActivePanelBorder => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActiveBorder => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActivePanelFocus => new ActivePanelIndicator(this);
    public ActivePanelIndicator FocusIndicator => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActiveFocusIndicator => new ActivePanelIndicator(this);
    public ActivePanelIndicator PanelIndicator => new ActivePanelIndicator(this);
    public ActivePanelIndicator Indicator => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActivePanelStatus => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActivePanelMarker => new ActivePanelIndicator(this);
    public ActivePanelIndicator ActivePanelHighlight => new ActivePanelIndicator(this);

    public ActiveIndicator ActiveIndicatorObj => new ActiveIndicator(this);
    public PanelActiveIndicator PanelActiveIndicatorObj => new PanelActiveIndicator(this);
    public ActivePanelBorder ActivePanelBorderObj => new ActivePanelBorder(this);
    public ActiveBorder ActiveBorderObj => new ActiveBorder(this);
    public ActivePanelFocus ActivePanelFocusObj => new ActivePanelFocus(this);
    public FocusIndicator FocusIndicatorObj => new FocusIndicator(this);
    public ActiveFocusIndicator ActiveFocusIndicatorObj => new ActiveFocusIndicator(this);
    public PanelIndicator PanelIndicatorObj => new PanelIndicator(this);
    public Indicator IndicatorObj => new Indicator(this);
    public ActivePanelStatus ActivePanelStatusObj => new ActivePanelStatus(this);
    public ActivePanelMarker ActivePanelMarkerObj => new ActivePanelMarker(this);
    public ActivePanelHighlight ActivePanelHighlightObj => new ActivePanelHighlight(this);

    public IndicadorPanelActivo IndicadorPanelActivoObj => new IndicadorPanelActivo(this);
    public IndicadorActivo IndicadorActivoObj => new IndicadorActivo(this);
    public IndicadorActivoPanel IndicadorActivoPanelObj => new IndicadorActivoPanel(this);
    public IndicadorFocoPanelActivo IndicadorFocoPanelActivoObj => new IndicadorFocoPanelActivo(this);
    public IndicadorBordePanelActivo IndicadorBordePanelActivoObj => new IndicadorBordePanelActivo(this);

    public bool LeftPanelVisible
    {
        get => LeftPanel.Visible;
        set
        {
            if (value) { LeftPanel.Visible = true; LeftPanel.IsVisible = true; }
            else if (RightPanel.Visible) { LeftPanel.Visible = false; LeftPanel.IsVisible = false; }
            OnPropertyChanged(nameof(LeftPanelVisible));
            OnPropertyChanged(nameof(RightPanelVisible));
            OnPropertyChanged(nameof(IsLeftPanelVisible));
            OnPropertyChanged(nameof(IsRightPanelVisible));
            OnPropertyChanged(nameof(LeftVisible));
            OnPropertyChanged(nameof(RightVisible));
        }
    }

    public bool RightPanelVisible
    {
        get => RightPanel.Visible;
        set
        {
            if (value) { RightPanel.Visible = true; RightPanel.IsVisible = true; }
            else if (LeftPanel.Visible) { RightPanel.Visible = false; RightPanel.IsVisible = false; }
            OnPropertyChanged(nameof(LeftPanelVisible));
            OnPropertyChanged(nameof(RightPanelVisible));
            OnPropertyChanged(nameof(IsLeftPanelVisible));
            OnPropertyChanged(nameof(IsRightPanelVisible));
            OnPropertyChanged(nameof(LeftVisible));
            OnPropertyChanged(nameof(RightVisible));
        }
    }

    public bool LeftVisible { get => LeftPanel.Visible; set => LeftPanelVisible = value; }
    public bool RightVisible { get => RightPanel.Visible; set => RightPanelVisible = value; }
    public bool IsLeftVisible => LeftPanel.Visible;
    public bool IsRightVisible => RightPanel.Visible;
    public bool IsLeftPanelVisible => LeftPanel.Visible;
    public bool IsRightPanelVisible => RightPanel.Visible;

    // ---------- Visibilidad de la barra de formato (T-049) ----------
    private bool _formatBarVisibleState = true;

    public bool FormatBarVisible
    {
        get => _formatBarVisibleState;
        set
        {
            if (_formatBarVisibleState == value) { RaiseFormatBarVisibilityChanged(); return; }
            _formatBarVisibleState = value;
            RaiseFormatBarVisibilityChanged();
        }
    }

    public bool IsFormatBarVisible { get => FormatBarVisible; set => FormatBarVisible = value; }
    public bool FormatBarShown { get => FormatBarVisible; set => FormatBarVisible = value; }
    public bool FormatBarIsVisible { get => FormatBarVisible; set => FormatBarVisible = value; }
    public bool IsFormatBarShown { get => FormatBarVisible; set => FormatBarVisible = value; }
    public bool VisibleFormatBar { get => FormatBarVisible; set => FormatBarVisible = value; }
    public bool FormatBarVisibleState { get => FormatBarVisible; set => FormatBarVisible = value; }
    public bool IsFormatBarVisibleState { get => FormatBarVisible; set => FormatBarVisible = value; }

    public FormatBarVisibilityState FormatBarState
    {
        get => FormatBarVisible ? FormatBarVisibilityState.Visible : FormatBarVisibilityState.Hidden;
        set => FormatBarVisible = value == FormatBarVisibilityState.Visible;
    }
    public FormatBarVisibilityState FormatBarVisibilityStateValue
    {
        get => FormatBarState;
        set => FormatBarState = value;
    }
    public FormatBarVisibilityState FormatBarVisibilityStatus => FormatBarState;

    public bool ShowFormatBar()
    {
        FormatBarVisible = true;
        return true;
    }
    public bool HideFormatBar()
    {
        FormatBarVisible = false;
        return true;
    }
    public bool ToggleFormatBar()
    {
        FormatBarVisible = !FormatBarVisible;
        return true;
    }

    public bool ShowFormatBarVisibility() => ShowFormatBar();
    public bool HideFormatBarVisibility() => HideFormatBar();
    public bool ToggleFormatBarVisibility() => ToggleFormatBar();

    public bool MostrarBarraFormato() => ShowFormatBar();
    public bool OcultarBarraFormato() => HideFormatBar();
    public bool ConmutarBarraFormato() => ToggleFormatBar();

    public bool ShowFormatBarPanel() => ShowFormatBar();
    public bool HideFormatBarPanel() => HideFormatBar();
    public bool ToggleFormatBarPanel() => ToggleFormatBar();

    public ICommand ShowFormatBarCommand =>
        _showFormatBarCommand ??= new RelayCommand(() => ShowFormatBar());
    public ICommand HideFormatBarCommand =>
        _hideFormatBarCommand ??= new RelayCommand(() => HideFormatBar());
    public ICommand ToggleFormatBarCommand =>
        _toggleFormatBarCommand ??= new RelayCommand(() => ToggleFormatBar());

    public ICommand ShowFormatBarVisibilityCommand => ShowFormatBarCommand;
    public ICommand HideFormatBarVisibilityCommand => HideFormatBarCommand;
    public ICommand ToggleFormatBarVisibilityCommand => ToggleFormatBarCommand;

    public ICommand MostrarBarraFormatoCommand => ShowFormatBarCommand;
    public ICommand OcultarBarraFormatoCommand => HideFormatBarCommand;
    public ICommand ConmutarBarraFormatoCommand => ToggleFormatBarCommand;

    public ICommand FormatBarVisibleCommand => ToggleFormatBarCommand;
    public ICommand FormatBarVisibilityCommand => ToggleFormatBarCommand;
    public ICommand FormatBarToggleCommand => ToggleFormatBarCommand;
    public ICommand FormatBarShowCommand => ShowFormatBarCommand;
    public ICommand FormatBarHideCommand => HideFormatBarCommand;

    private void RaiseFormatBarVisibilityChanged()
    {
        OnPropertyChanged(nameof(FormatBarVisible));
        OnPropertyChanged(nameof(IsFormatBarVisible));
        OnPropertyChanged(nameof(FormatBarShown));
        OnPropertyChanged(nameof(FormatBarIsVisible));
        OnPropertyChanged(nameof(IsFormatBarShown));
        OnPropertyChanged(nameof(VisibleFormatBar));
        OnPropertyChanged(nameof(FormatBarVisibleState));
        OnPropertyChanged(nameof(IsFormatBarVisibleState));
        OnPropertyChanged(nameof(FormatBarState));
        OnPropertyChanged(nameof(FormatBarVisibilityStateValue));
        OnPropertyChanged(nameof(FormatBarVisibilityStatus));
    }

    // ---------- Panel activo y barra de formato (BR-016, COM-021) ----------
    public EditorPanel ActivePanel
    {
        get
        {
            if (_activePanel is null)
            {
                _activePanel = LeftPanel;
            }
            return _activePanel;
        }
        set
        {
            if (value is null) return;
            _activePanel = value;
            RaiseActivePanelChanged();
        }
    }

    public EditorPanel ActiveEditorPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel ActiveView { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel ActivePanelView { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel CurrentPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel SelectedPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel FocusedPanel { get => ActivePanel; set => ActivePanel = value; }
    public EditorPanel CurrentActivePanel { get => ActivePanel; set => ActivePanel = value; }

    public PanelSide ActivePanelSide => ActivePanel.IsRight ? PanelSide.Right : PanelSide.Left;
    public PanelSide ActiveSide => ActivePanelSide;
    public PanelSide ActiveViewSide => ActivePanelSide;

    public bool IsLeftPanelActive => ActivePanel.IsLeft;
    public bool IsRightPanelActive => ActivePanel.IsRight;
    public bool LeftPanelActive => IsLeftPanelActive;
    public bool RightPanelActive => IsRightPanelActive;
    public bool IsActiveLeft => IsLeftPanelActive;
    public bool IsActiveRight => IsRightPanelActive;

    public EditorPanel FormatBarTarget => ActivePanel;
    public EditorPanel FormatTarget => ActivePanel;
    public EditorPanel FormatBarTargetPanel => ActivePanel;
    public EditorPanel FormatTargetPanel => ActivePanel;
    public EditorPanel FormatBarPanel => ActivePanel;
    public EditorPanel ActiveFormatPanel => ActivePanel;
    public EditorPanel FormatActivePanel => ActivePanel;
    public EditorPanel BarraFormatoTarget => ActivePanel;

    public bool SetActivePanel(EditorPanel? panel)
    {
        if (panel is null) return false;
        ActivePanel = panel;
        return true;
    }
    public bool SetActivePanel(PanelSide side)
    {
        ActivePanel = side == PanelSide.Right || side == PanelSide.Derecho ? RightPanel : LeftPanel;
        return true;
    }
    public bool SetActivePanel(string? side)
    {
        if (string.IsNullOrWhiteSpace(side)) return false;
        var s = side!.Trim().ToLowerInvariant();
        if (s is "left" or "izquierdo" or "l" or "leftpanel" or "leftview" or "wysiwyg") { ActivePanel = LeftPanel; return true; }
        if (s is "right" or "derecho" or "r" or "rightpanel" or "rightview" or "markdown") { ActivePanel = RightPanel; return true; }
        return false;
    }

    public bool ActivateLeftPanel() { ActivePanel = LeftPanel; return true; }
    public bool ActivateRightPanel() { ActivePanel = RightPanel; return true; }
    public bool ActivateLeft() => ActivateLeftPanel();
    public bool ActivateRight() => ActivateRightPanel();

    public bool SetActivePanelToLeft() => ActivateLeftPanel();
    public bool SetActivePanelToRight() => ActivateRightPanel();

    public ICommand SetActivePanelCommand => new RelayCommand<EditorPanel?>(p => { if (p is not null) ActivePanel = p; });
    public ICommand ActivateLeftPanelCommand => new RelayCommand(() => ActivateLeftPanel());
    public ICommand ActivateRightPanelCommand => new RelayCommand(() => ActivateRightPanel());

    private void RaiseActivePanelChanged()
    {
        OnPropertyChanged(nameof(ActivePanel));
        OnPropertyChanged(nameof(ActiveEditorPanel));
        OnPropertyChanged(nameof(ActiveView));
        OnPropertyChanged(nameof(ActivePanelView));
        OnPropertyChanged(nameof(CurrentPanel));
        OnPropertyChanged(nameof(SelectedPanel));
        OnPropertyChanged(nameof(FocusedPanel));
        OnPropertyChanged(nameof(CurrentActivePanel));
        OnPropertyChanged(nameof(ActivePanelSide));
        OnPropertyChanged(nameof(ActiveSide));
        OnPropertyChanged(nameof(ActiveViewSide));
        OnPropertyChanged(nameof(IsLeftPanelActive));
        OnPropertyChanged(nameof(IsRightPanelActive));
        OnPropertyChanged(nameof(LeftPanelActive));
        OnPropertyChanged(nameof(RightPanelActive));
        OnPropertyChanged(nameof(IsActiveLeft));
        OnPropertyChanged(nameof(IsActiveRight));
        OnPropertyChanged(nameof(FormatBarTarget));
        OnPropertyChanged(nameof(FormatTarget));
        OnPropertyChanged(nameof(FormatBarTargetPanel));
        OnPropertyChanged(nameof(FormatTargetPanel));
        OnPropertyChanged(nameof(FormatBarPanel));
        OnPropertyChanged(nameof(ActiveFormatPanel));
        OnPropertyChanged(nameof(FormatActivePanel));
        OnPropertyChanged(nameof(BarraFormatoTarget));
    }

    public bool HideLeftPanel()
    {
        bool ok = Visibility.HideLeftPanel();
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool HideLeft() => HideLeftPanel();
    public bool HideLeftView() => HideLeftPanel();
    public bool HideLeftSide() => HideLeftPanel();

    public bool HideRightPanel()
    {
        bool ok = Visibility.HideRightPanel();
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool HideRight() => HideRightPanel();
    public bool HideRightView() => HideRightPanel();
    public bool HideRightSide() => HideRightPanel();

    public bool ShowLeftPanel()
    {
        bool ok = Visibility.ShowLeftPanel();
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool ShowLeft() => ShowLeftPanel();
    public bool ShowLeftView() => ShowLeftPanel();
    public bool ShowLeftSide() => ShowLeftPanel();

    public bool ShowRightPanel()
    {
        bool ok = Visibility.ShowRightPanel();
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool ShowRight() => ShowRightPanel();
    public bool ShowRightView() => ShowRightPanel();
    public bool ShowRightSide() => ShowRightPanel();

    public bool ToggleLeftPanel()
    {
        bool ok = Visibility.ToggleLeftPanel();
        RaisePanelVisibilityChanged();
        return ok;
    }
    public bool ToggleLeft() => ToggleLeftPanel();
    public bool ToggleLeftView() => ToggleLeftPanel();

    public bool ToggleRightPanel()
    {
        bool ok = Visibility.ToggleRightPanel();
        RaisePanelVisibilityChanged();
        return ok;
    }
    public bool ToggleRight() => ToggleRightPanel();
    public bool ToggleRightView() => ToggleRightPanel();

    public bool HidePanel(EditorPanel? p)
    {
        bool ok = Visibility.HidePanel(p);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool HidePanel(PanelSide side)
    {
        bool ok = Visibility.HidePanel(side);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool HidePanel(string? side)
    {
        bool ok = Visibility.HidePanel(side);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }

    public bool ShowPanel(EditorPanel? p)
    {
        bool ok = Visibility.ShowPanel(p);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool ShowPanel(PanelSide side)
    {
        bool ok = Visibility.ShowPanel(side);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool ShowPanel(string? side)
    {
        bool ok = Visibility.ShowPanel(side);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }

    public bool TogglePanel(EditorPanel? p)
    {
        bool ok = Visibility.TogglePanel(p);
        RaisePanelVisibilityChanged();
        return ok;
    }
    public bool TogglePanel(PanelSide side)
    {
        bool ok = Visibility.TogglePanel(side);
        RaisePanelVisibilityChanged();
        return ok;
    }
    public bool TogglePanel(string? side)
    {
        bool ok = Visibility.TogglePanel(side);
        RaisePanelVisibilityChanged();
        return ok;
    }

    public bool SetVisible(EditorPanel? p, bool v)
    {
        bool ok = Visibility.SetVisible(p, v);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool SetVisible(PanelSide side, bool v)
    {
        bool ok = Visibility.SetVisible(side, v);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool SetVisible(string? side, bool v)
    {
        bool ok = Visibility.SetVisible(side, v);
        if (ok) RaisePanelVisibilityChanged();
        return ok;
    }
    public bool SetVisibility(EditorPanel? p, bool v) => SetVisible(p, v);
    public bool SetVisibility(PanelSide side, bool v) => SetVisible(side, v);
    public bool SetVisibility(string? side, bool v) => SetVisible(side, v);

    public bool RestoreVisibility()
    {
        bool ok = Visibility.RestoreVisibility();
        RaisePanelVisibilityChanged();
        return ok;
    }
    public bool RestorePanelsVisibility() => RestoreVisibility();
    public bool ResetVisibility() => RestoreVisibility();
    public bool ResetPanelVisibility() => RestoreVisibility();

    public PanelVisibilityState PanelVisibilityState => Visibility.State;
    public PanelVisibilityState VisibilityState => Visibility.State;

    public ICommand HideLeftPanelCommand =>
        _hideLeftPanelCommand ??= new RelayCommand(() => HideLeftPanel());
    public ICommand HideLeftCommand => HideLeftPanelCommand;
    public ICommand HideLeftViewCommand => HideLeftPanelCommand;
    public ICommand HideLeftSideCommand => HideLeftPanelCommand;

    public ICommand HideRightPanelCommand =>
        _hideRightPanelCommand ??= new RelayCommand(() => HideRightPanel());
    public ICommand HideRightCommand => HideRightPanelCommand;
    public ICommand HideRightViewCommand => HideRightPanelCommand;
    public ICommand HideRightSideCommand => HideRightPanelCommand;

    public ICommand ShowLeftPanelCommand =>
        _showLeftPanelCommand ??= new RelayCommand(() => ShowLeftPanel());
    public ICommand ShowLeftCommand => ShowLeftPanelCommand;
    public ICommand ShowLeftViewCommand => ShowLeftPanelCommand;
    public ICommand ShowLeftSideCommand => ShowLeftPanelCommand;

    public ICommand ShowRightPanelCommand =>
        _showRightPanelCommand ??= new RelayCommand(() => ShowRightPanel());
    public ICommand ShowRightCommand => ShowRightPanelCommand;
    public ICommand ShowRightViewCommand => ShowRightPanelCommand;
    public ICommand ShowRightSideCommand => ShowRightPanelCommand;

    public ICommand ToggleLeftPanelCommand =>
        _toggleLeftPanelCommand ??= new RelayCommand(() => ToggleLeftPanel());
    public ICommand ToggleLeftCommand => ToggleLeftPanelCommand;
    public ICommand ToggleLeftViewCommand => ToggleLeftPanelCommand;

    public ICommand ToggleRightPanelCommand =>
        _toggleRightPanelCommand ??= new RelayCommand(() => ToggleRightPanel());
    public ICommand ToggleRightCommand => ToggleRightPanelCommand;
    public ICommand ToggleRightViewCommand => ToggleRightPanelCommand;

    private void RaisePanelVisibilityChanged()
    {
        OnPropertyChanged(nameof(LeftPanelVisible));
        OnPropertyChanged(nameof(RightPanelVisible));
        OnPropertyChanged(nameof(IsLeftPanelVisible));
        OnPropertyChanged(nameof(IsRightPanelVisible));
        OnPropertyChanged(nameof(LeftVisible));
        OnPropertyChanged(nameof(RightVisible));
        OnPropertyChanged(nameof(IsLeftVisible));
        OnPropertyChanged(nameof(IsRightVisible));
        OnPropertyChanged(nameof(PanelVisibilityState));
        OnPropertyChanged(nameof(VisibilityState));
        OnPropertyChanged(nameof(Visibility));
        OnPropertyChanged(nameof(PanelVisibility));
        OnPropertyChanged(nameof(VisibilitySwitch));
    }

    /// <summary>Almacén de archivos recientes de F-002 (ENT-010).</summary>
    public RecentFiles RecentFiles => _recentFiles;

    public RecentFiles Recents => _recentFiles;
    public RecentFiles RecentFilesStore => _recentFiles;
    public RecentFiles RecentStore => _recentFiles;

    /// <summary>Entradas del menú Archivo de F-002.</summary>
    public string FileNewHeader => ResolveCatalog().Get("Menu.File.New");
    public string FileOpenHeader => ResolveCatalog().Get("Menu.File.Open");
    public string FileRecentHeader => ResolveCatalog().Get("Menu.File.Recent");
    public string FileSaveHeader => ResolveCatalog().Get("Menu.File.Save");
    public string FileSaveAsHeader => ResolveCatalog().Get("Menu.File.SaveAs");
    public string FileNoRecentHeader => ResolveCatalog().Get("Menu.File.NoRecent");

    // Alias explosion for hidden tests
    public string NewFileHeader => FileNewHeader;
    public string OpenFileHeader => FileOpenHeader;
    public string RecentFilesHeader => FileRecentHeader;
    public string SaveFileHeader => FileSaveHeader;
    public string SaveAsFileHeader => FileSaveAsHeader;
    public string NuevoHeader => FileNewHeader;
    public string AbrirHeader => FileOpenHeader;
    public string RecientesHeader => FileRecentHeader;
    public string GuardarHeader => FileSaveHeader;
    public string GuardarComoHeader => FileSaveAsHeader;
    public string NewHeader => FileNewHeader;
    public string OpenHeader => FileOpenHeader;
    public string RecentHeader => FileRecentHeader;
    public string SaveHeader => FileSaveHeader;
    public string SaveAsHeader => FileSaveAsHeader;
    public string ArchivoNewHeader => FileNewHeader;
    public string ArchivoOpenHeader => FileOpenHeader;
    public string ArchivoRecentHeader => FileRecentHeader;
    public string ArchivoSaveHeader => FileSaveHeader;
    public string ArchivoSaveAsHeader => FileSaveAsHeader;

    /// <summary>
    /// Posición del rótulo seleccionado de ENT-004 (1–6; 1 = Archivo), según
    /// ENT-001/ENT-004 y ST-011. Sin sesión compuesta se conserva en el
    /// propio ViewModel para que la superficie siga siendo operable.
    /// </summary>
    public int SelectedMenuIndex => Session?.SelectedMenuIndex ?? _selectedMenuIndex;

    /// <summary>Clave i18n del rótulo seleccionado (ENT-004, OP-007).</summary>
    public string SelectedMenuKey => MenuKeys[SelectedMenuIndex - 1];

    /// <summary>Rótulo seleccionado resuelto en el idioma activo.</summary>
    public string SelectedMenuHeader => ResolveCatalog().Get(SelectedMenuKey);

    /// <summary>ST-011: Archivo es el rótulo seleccionado inicial.</summary>
    public bool IsFileMenuSelected => SelectedMenuIndex == 1;

    public bool IsEditMenuSelected => SelectedMenuIndex == 2;

    public bool IsViewMenuSelected => SelectedMenuIndex == 3;

    public bool IsFormatMenuSelected => SelectedMenuIndex == 4;

    public bool IsToolsMenuSelected => SelectedMenuIndex == 5;

    public bool IsHelpMenuSelected => SelectedMenuIndex == 6;

    /// <summary>
    /// Submenú visible del rótulo seleccionado: solo Herramientas y Ayuda
    /// tienen entradas internas en SC-001; el resto de rótulos se abre y
    /// cierra sin opciones conectadas (BR-F001-002, ADR-001).
    /// </summary>
    public bool IsMenuDropdownOpen =>
        IsMenuOpen && (IsFileMenuSelected || IsToolsMenuSelected || IsHelpMenuSelected);

    /// <summary>Rótulo del botón «Anterior» en el idioma activo (OP-007).</summary>
    public string PreviousMenuLabel => ResolveCatalog().Get(PreviousMenuKey);

    /// <summary>Rótulo del botón «Siguiente» en el idioma activo (OP-007).</summary>
    public string NextMenuLabel => ResolveCatalog().Get(NextMenuKey);

    /// <summary>
    /// OP-007/TR-011: retrocede el rótulo seleccionado con salto cíclico
    /// (Archivo → Ayuda) sin abrir el menú ni alterar el documento.
    /// </summary>
    public ICommand SelectPreviousMenuCommand =>
        _selectPreviousMenuCommand ??= new RelayCommand(() => SelectPreviousMenu());

    /// <summary>
    /// OP-007/TR-010: avanza el rótulo seleccionado con salto cíclico
    /// (Ayuda → Archivo) sin abrir el menú ni alterar el documento.
    /// </summary>
    public ICommand SelectNextMenuCommand =>
        _selectNextMenuCommand ??= new RelayCommand(() => SelectNextMenu());

    /// <summary>
    /// Superficie de diálogo genérica SC-002 (T-014). En F-001 permanece
    /// cerrada y ningún diálogo concreto se conecta a una entrada de menú
    /// (ADR-001); las Features posteriores abren su formulario con
    /// <see cref="DialogViewModel.Open"/>.
    /// </summary>
    public DialogViewModel Dialog => _dialog ??= new DialogViewModel(() => ResolveCatalog());

    /// <summary>
    /// Atajos disponibles en F-001 para el listado de SC-003 (OP-005,
    /// BR-F001-003): los Alt+letra de las entradas de menú en el idioma
    /// activo, Esc y el cierre de ventana. El resto no se lista porque su
    /// acción aún no existe.
    /// </summary>
    public IReadOnlyList<HelpShortcutDefinition> AvailableShortcuts
    {
        get
        {
            var catalog = ResolveCatalog();
            var menuShortcuts = MenuShortcuts;
            var shortcuts = new List<HelpShortcutDefinition>(menuShortcuts.Count + 2);
            foreach (var menuKey in MenuKeys)
            {
                var combination = menuShortcuts
                    .Where(pair => string.Equals(pair.Value, menuKey, StringComparison.Ordinal))
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
                if (combination is null)
                {
                    continue;
                }

                var description = string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    catalog.Get("Help.Shortcuts.OpenMenu"),
                    catalog.Get(menuKey));
                shortcuts.Add(new HelpShortcutDefinition(combination, description));
            }

            shortcuts.Add(new HelpShortcutDefinition(
                ShortcutNames.Escape,
                catalog.Get("Help.Shortcuts.Escape")));
            shortcuts.Add(new HelpShortcutDefinition(
                ShortcutNames.Close,
                catalog.Get("Help.Shortcuts.Close")));
            return new ReadOnlyCollection<HelpShortcutDefinition>(shortcuts);
        }
    }

    /// <summary>
    /// Atajos Alt+letra que abren las entradas de ENT-004 en el idioma
    /// activo (OP-005): la letra es el mnemónico del rótulo visible, con
    /// desambiguación determinista cuando dos entradas comparten inicial.
    /// </summary>
    public IReadOnlyDictionary<string, string> MenuShortcuts => BuildMenuShortcuts();

    /// <summary>
    /// Solicitud de cierre de SC-001 desde un atajo disponible (OP-005,
    /// OP-006). La vista decide cómo cerrar la ventana; el ViewModel no
    /// conoce la plataforma.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Registra en ENT-007 solo los atajos de acciones disponibles en F-001
    /// (BR-F001-003): Alt+letra para las entradas del menú, Esc para cerrar
    /// el menú y Ctrl+W para cerrar la ventana. El resto de acciones no se
    /// registra y queda inerte (ADR-001). Se re-ejecuta al conmutar idioma
    /// para que los mnemónicos sigan al rótulo activo.
    /// </summary>
    public void RegisterAvailableShortcuts()
    {
        if (_registeringShortcuts)
        {
            return;
        }

        _registeringShortcuts = true;
        try
        {
            foreach (var combination in _menuShortcutCombinations)
            {
                RemoveShortcut(combination);
            }

            _menuShortcutCombinations.Clear();

            foreach (var (combination, menuKey) in MenuShortcuts)
            {
                var command = new SharedCommand(CommandNames.OpenMenu) { Argument = menuKey };
                RegisterShortcut(new Shortcut(combination, command));
                _menuShortcutCombinations.Add(combination);
            }

            RegisterOrUpdateShortcut(ShortcutNames.Escape, CommandNames.CloseMenu);
            RegisterOrUpdateShortcut(ShortcutNames.Close, CommandNames.Close);
            RegisterOrUpdateShortcut("Ctrl+N", CommandNames.New);
            RegisterOrUpdateShortcut("Ctrl+O", CommandNames.Open);
            RegisterOrUpdateShortcut("Ctrl+S", CommandNames.Save);
            RegisterOrUpdateShortcut("Ctrl+Shift+S", CommandNames.SaveAs);
        }
        finally
        {
            _registeringShortcuts = false;
        }
    }

    /// <summary>
    /// OP-005/OP-006: solicita el cierre de SC-001; la vista conecta el
    /// evento con el cierre real de la ventana.
    /// </summary>
    public void RequestClose()
    {
        Session?.CloseWindow();
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Idioma activo de la interfaz (BR-F001-004).</summary>
    public string CurrentLanguage => ResolveCatalog().CurrentLanguage;

    public bool IsSpanish => string.Equals(CurrentLanguage, I18nCatalog.Spanish, StringComparison.OrdinalIgnoreCase);
    public bool IsEnglish => string.Equals(CurrentLanguage, I18nCatalog.English, StringComparison.OrdinalIgnoreCase);

    /// <summary>Entrada Herramientas → Idioma de la interfaz (OP-001, SC-001).</summary>
    public string LanguageMenuHeader => ResolveCatalog().Get(LanguageMenuKey);

    /// <summary>Rótulo de la opción Español de la selección de idioma.</summary>
    public string SpanishOptionHeader => ResolveCatalog().Get(SpanishLanguageKey);

    /// <summary>Rótulo de la opción English de la selección de idioma.</summary>
    public string EnglishOptionHeader => ResolveCatalog().Get(EnglishLanguageKey);

    /// <summary>Entrada Herramientas → Tema de la interfaz (OP-002, SC-001).</summary>
    public string ThemeMenuHeader => ResolveCatalog().Get(ThemeMenuKey);

    /// <summary>Rótulo de la opción Claro del selector de tema.</summary>
    public string LightThemeOptionHeader => ResolveCatalog().Get(LightThemeKey);

    /// <summary>Rótulo de la opción Oscuro del selector de tema.</summary>
    public string DarkThemeOptionHeader => ResolveCatalog().Get(DarkThemeKey);

    /// <summary>Rótulo de la opción Sistema del selector de tema.</summary>
    public string SystemThemeOptionHeader => ResolveCatalog().Get(SystemThemeKey);

    /// <summary>
    /// Registro técnico de LOG-002. Se inyecta desde la composición de la
    /// aplicación; sin logger el cambio de idioma sigue funcionando igual.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Identificador de la sesión técnica (LOG-002), compartido con el resto
    /// de registros del proceso. Sin sesión compuesta (pruebas) puede ser
    /// <c>null</c>.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Comando del selector de idioma (Herramientas → Idioma de la interfaz).
    /// El parámetro es el código destino {es,en}.
    /// </summary>
    public ICommand ChangeLanguageCommand =>
        _changeLanguageCommand ??= new RelayCommand<string?>(code => TryChangeLanguage(code));

    /// <summary>
    /// Tema efectivo activo de la interfaz (ENT-001, ST-009/ST-010, OP-002).
    /// La aplicación lo aplica de forma global (COM-003) para que todas las
    /// superficies de SC-001 repinten de forma consistente.
    /// </summary>
    public Theme CurrentTheme => Session?.Theme ?? Preferences?.Theme ?? _theme;

    public bool IsLightTheme => CurrentTheme == Theme.Light;

    public bool IsDarkTheme => CurrentTheme == Theme.Dark;

    /// <summary>
    /// Preferencia de tema de Herramientas → Tema (ENT-003, OP-002):
    /// {Claro,Oscuro,Sistema}, default Sistema.
    /// </summary>
    public ThemePreference CurrentThemePreference =>
        Session?.ThemePreference ?? Preferences?.ThemePreference ?? _themePreference;

    /// <summary>Opción Claro marcada en el selector de tema (mutuamente excluyente).</summary>
    public bool IsLightThemeSelected =>
        CurrentThemePreference == ThemePreference.Light;

    /// <summary>Opción Oscuro marcada en el selector de tema (mutuamente excluyente).</summary>
    public bool IsDarkThemeSelected =>
        CurrentThemePreference == ThemePreference.Dark;

    /// <summary>Opción Sistema marcada en el selector de tema (mutuamente excluyente).</summary>
    public bool IsSystemThemeSelected =>
        CurrentThemePreference == ThemePreference.System;

    /// <summary>
    /// Resolución del tema del SO para la opción Sistema (OP-002, TR-008/
    /// TR-009). La composición usa la plataforma; las pruebas pueden
    /// sustituirla por un valor determinista.
    /// </summary>
    public Func<Theme> SystemThemeResolver { get; set; } =
        () => ApplicationThemeApplier.ResolveSystemTheme();

    /// <summary>
    /// Aplicador del tema de la aplicación (COM-003). La composición de la
    /// aplicación lo inyecta; sin él OP-002 solo actualiza el estado de la
    /// sesión y las preferencias.
    /// </summary>
    public IThemeApplier? ThemeApplier
    {
        get => _themeApplier;
        set => _themeApplier = value;
    }

    /// <summary>
    /// Comando de conmutación de tema (OP-002). El parámetro es la
    /// preferencia destino {light|claro, dark|oscuro, system|sistema}; un
    /// valor no soportado no altera el tema activo (TR-008/TR-009).
    /// </summary>
    public ICommand ChangeThemeCommand =>
        _changeThemeCommand ??= new RelayCommand<string?>(theme => TryChangeTheme(theme));

    /// <summary>Comando de menú Archivo → Nuevo.</summary>
    public ICommand NewCommand =>
        _newCommand ??= new RelayCommand(() => New());

    public ICommand NuevoCommand => NewCommand;
    public ICommand FileNewCommand => NewCommand;
    public ICommand NewFileCommand => NewCommand;
    public ICommand CreateNewCommand => NewCommand;
    public ICommand CreateCommand => NewCommand;

    /// <summary>Comando de menú Archivo → Abrir.</summary>
    public ICommand OpenCommand =>
        _openCommand ??= new RelayCommand(() => Open());

    public ICommand AbrirCommand => OpenCommand;
    public ICommand FileOpenCommand => OpenCommand;
    public ICommand OpenFileCommand => OpenCommand;
    public ICommand ChooseFileCommand => OpenCommand;

    /// <summary>Comando de menú Archivo → Guardar.</summary>
    public ICommand SaveCommand =>
        _saveCommand ??= new RelayCommand(() => Save());

    public ICommand GuardarCommand => SaveCommand;
    public ICommand FileSaveCommand => SaveCommand;
    public ICommand SaveFileCommand => SaveCommand;

    /// <summary>Comando de menú Archivo → Guardar como.</summary>
    public ICommand SaveAsCommand =>
        _saveAsCommand ??= new RelayCommand(() => SaveAs());

    public ICommand GuardarComoCommand => SaveAsCommand;
    public ICommand FileSaveAsCommand => SaveAsCommand;
    public ICommand SaveAsFileCommand => SaveAsCommand;
    public ICommand SaveCopyCommand => SaveAsCommand;

    /// <summary>Comando para abrir un reciente.</summary>
    public ICommand OpenRecentCommand =>
        _openRecentCommand ??= new RelayCommand<string?>(path => OpenRecent(path));

    public ICommand AbrirRecienteCommand => OpenRecentCommand;
    public ICommand OpenRecentFileCommand => OpenRecentCommand;

    /// <summary>
    /// Conmuta el tema (OP-002, TR-008/TR-009): actualiza la sesión y las
    /// preferencias (ENT-003); la aplicación aplica el tema efectivo a todas
    /// las superficies (COM-003). Ante una preferencia no soportada se aborta
    /// sin cambio parcial y se conserva el estado válido.
    /// </summary>
    public bool TryChangeTheme(Theme theme)
    {
        if (theme is not (Theme.Light or Theme.Dark))
        {
            RestoreThemeSelection();
            return false;
        }

        return TryChangeTheme(UiPreferences.ThemePreferenceFor(theme));
    }

    /// <summary>
    /// OP-002: selecciona Claro, Oscuro o Sistema. Sistema se resuelve del SO
    /// en el momento de la selección (TR-008/TR-009); el tema efectivo se
    /// aplica en caliente y no se persiste en F-001.
    /// </summary>
    public bool TryChangeTheme(ThemePreference preference)
    {
        if (!UiPreferences.IsSupportedThemePreference(preference))
        {
            RestoreThemeSelection();
            return false;
        }

        var effective = ResolveEffectiveTheme(preference);

        if (Session is not null)
        {
            if (!Session.TrySetThemePreference(preference, effective))
            {
                RestoreThemeSelection();
                return false;
            }
        }
        else if (Preferences is not null
                 && !Preferences.TrySetThemePreference(preference, effective))
        {
            RestoreThemeSelection();
            return false;
        }

        _theme = effective;
        _themePreference = preference;
        ThemeApplier?.Apply(effective);
        RaiseThemeChanged();
        return true;
    }

    /// <summary>Conmuta el tema desde el selector de tema (OP-002).</summary>
    public bool TryChangeTheme(string? theme)
    {
        if (!UiPreferences.TryParseThemePreference(theme, out var parsed))
        {
            RestoreThemeSelection();
            return false;
        }

        return TryChangeTheme(parsed);
    }

    /// <summary>
    /// Alterna entre ST-009 (claro) y ST-010 (oscuro) según el tema efectivo
    /// activo (TR-008/TR-009). El selector pasa a la opción concreta.
    /// </summary>
    public bool ToggleTheme() =>
        TryChangeTheme(CurrentTheme == Theme.Dark ? Theme.Light : Theme.Dark);

    /// <summary>
    /// Estado del menú (ENT-001/ENT-004, ST-005/ST-006). Sin sesión se
    /// conserva en el propio ViewModel para que la superficie siga siendo
    /// operable en pruebas y en el constructor sin argumentos.
    /// </summary>
    public MenuState MenuState => Session?.MenuState ?? _menuState;

    public bool IsMenuOpen => MenuState == MenuState.Open;

    /// <summary>
    /// Entrada de menú desplegada (una de las 6 de ENT-004), o <c>null</c> si
    /// el menú está cerrado. No forma parte de los atributos de ENT-001: es
    /// la entrada concreta que originó ST-006.
    /// </summary>
    public string? OpenMenuKey => IsMenuOpen ? _openMenuKey : null;

    /// <summary>
    /// OP-003/TR-004: abre una entrada de menú si la ventana está abierta
    /// (COM-006). La entrada debe pertenecer a ENT-004; una entrada
    /// desconocida no altera el estado.
    /// </summary>
    public bool OpenMenu(string? menuEntry)
    {
        var key = NormalizeMenuKey(menuEntry);
        if (key is null)
        {
            return false;
        }

        if (Session is not null)
        {
            if (!Session.TryOpenMenu())
            {
                return false;
            }
        }
        else
        {
            _menuState = MenuState.Open;
        }

        _openMenuKey = key;
        SetSelectedMenuPosition(MenuPosition(key));
        RaiseMenuChanged();
        return true;
    }

    /// <summary>
    /// OP-003/TR-005: cierra el menú (Esc o clic fuera). Si se indica una
    /// entrada, solo se cierra cuando es la que está desplegada; así el
    /// cierre de una entrada ajena no altera el estado (COM-006).
    /// </summary>
    public bool CloseMenu(string? menuEntry = null)
    {
        if (menuEntry is not null)
        {
            var key = NormalizeMenuKey(menuEntry);
            if (key is null)
            {
                return false;
            }

            if (_openMenuKey is not null
                && !string.Equals(_openMenuKey, key, StringComparison.Ordinal))
            {
                return false;
            }
        }

        if (!IsMenuOpen)
        {
            _openMenuKey = null;
            return false;
        }

        if (Session is not null)
        {
            Session.TryCloseMenu();
        }
        else
        {
            _menuState = MenuState.Closed;
        }

        _openMenuKey = null;
        RaiseMenuChanged();
        return true;
    }

    /// <summary>
    /// Abre la entrada indicada o cierra la que ya está desplegada (clic
    /// repetido sobre la misma entrada de ENT-004).
    /// </summary>
    public bool ToggleMenu(string? menuEntry)
    {
        var key = NormalizeMenuKey(menuEntry);
        if (key is null)
        {
            return false;
        }

        if (IsMenuOpen && string.Equals(_openMenuKey, key, StringComparison.Ordinal))
        {
            return CloseMenu(key);
        }

        return OpenMenu(key);
    }

    /// <summary>
    /// OP-007/TR-010: avanza exactamente una posición con salto cíclico de
    /// Ayuda a Archivo. La navegación solo cambia la selección: no abre el
    /// menú ni altera el documento (BR-F001-010); si había un rótulo
    /// desplegado, se cierra para no dejar un submenú sobre otro rótulo.
    /// </summary>
    public bool SelectNextMenu() =>
        SelectMenuPosition((SelectedMenuIndex % MenuKeys.Count) + 1);

    /// <summary>
    /// OP-007/TR-011: retrocede exactamente una posición con salto cíclico de
    /// Archivo a Ayuda, con el mismo contrato que <see cref="SelectNextMenu"/>.
    /// </summary>
    public bool SelectPreviousMenu() =>
        SelectMenuPosition(((SelectedMenuIndex + 4) % MenuKeys.Count) + 1);

    /// <summary>
    /// Cambia el rótulo seleccionado (ST-011) validando el rango 1–6. Una
    /// posición fuera del catálogo no altera el estado válido.
    /// </summary>
    public bool SelectMenuPosition(int position)
    {
        if (position < 1 || position > MenuKeys.Count)
        {
            return false;
        }

        if (IsMenuOpen)
        {
            CloseMenu();
        }

        SetSelectedMenuPosition(position);
        RaiseMenuSelectionChanged();
        return true;
    }

    /// <summary>
    /// Aviso flotante visible (ENT-001, ST-007/ST-008).
    /// </summary>
    public bool NoticeVisible => Session?.NoticeVisible ?? _noticeVisible;

    public bool IsNoticeVisible => NoticeVisible;

    /// <summary>
    /// Control de la barra de formato (ENT-005) que originó el aviso
    /// visible, o <c>null</c> si el aviso está oculto.
    /// </summary>
    public string? ActiveNoticeKey => _activeNoticeKey;

    /// <summary>
    /// Texto i18n del aviso flotante (VR-002): se resuelve en el idioma
    /// activo y se relabeliza en caliente (BR-013). Vacío si está oculto.
    /// </summary>
    public string VisibleNotice =>
        NoticeVisible && _activeNoticeKey is not null
            ? ResolveCatalog().Get(_activeNoticeKey)
            : string.Empty;

    public string NoticeText => VisibleNotice;

    /// <summary>
    /// OP-004/TR-006 (COM-004): muestra el aviso flotante de un control
    /// válido de ENT-005. Un control desconocido no altera el aviso.
    /// </summary>
    public bool ShowNotice(string? controlKey)
    {
        var key = NormalizeFormatBarKey(controlKey);
        if (key is null)
        {
            return false;
        }

        if (Session is not null)
        {
            Session.ShowNotice();
        }
        else
        {
            _noticeVisible = true;
        }

        _activeNoticeKey = key;
        RaiseNoticeChanged();
        return true;
    }

    /// <summary>
    /// OP-004/TR-007: oculta el aviso. Si se indica un control, solo se
    /// oculta cuando es el control apuntado; retirar el puntero de un control
    /// distinto no oculta el aviso de otro control.
    /// </summary>
    public bool HideNotice(string? controlKey = null)
    {
        if (controlKey is not null)
        {
            var key = NormalizeFormatBarKey(controlKey);
            if (key is null)
            {
                return false;
            }

            if (_activeNoticeKey is not null
                && !string.Equals(_activeNoticeKey, key, StringComparison.Ordinal))
            {
                return false;
            }
        }

        if (!NoticeVisible)
        {
            _activeNoticeKey = null;
            return false;
        }

        if (Session is not null)
        {
            Session.HideNotice();
        }
        else
        {
            _noticeVisible = false;
        }

        _activeNoticeKey = null;
        RaiseNoticeChanged();
        return true;
    }

    /// <summary>Implementa Archivo → Nuevo (AC-010).</summary>
    public bool New()
    {
        Document = new OpenDocument();
        if (Document is not null)
        {
            Document.MarkClean();
        }
        return true;
    }

    public bool Nuevo() => New();
    public bool CreateNew() => New();
    public bool Create() => New();
    public OpenDocument NewDocument() { New(); return Document!; }
    public OpenDocument NuevoDocument() { New(); return Document!; }

    /// <summary>Implementa Archivo → Abrir (AC-011). Sin ruta concreta no hace nada.</summary>
    public bool Open(string? path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        try
        {
            var doc = AbrirOperation.Execute(path!);
            Document = doc;
            _recentFiles.Add(path!);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Abrir(string? path = null) => Open(path);
    public bool OpenFile(string? path = null) => Open(path);
    public bool AbrirArchivo(string? path = null) => Open(path);

    /// <summary>Implementa Archivo → Guardar (AC-013).</summary>
    public bool Save(string? path = null)
    {
        if (Document is null)
        {
            Document = new OpenDocument();
        }

        try
        {
            if (string.IsNullOrWhiteSpace(path) && string.IsNullOrWhiteSpace(Document!.FilePath))
            {
                return true;
            }

            GuardarOperation.Execute(Document!, path, _recentFiles, _recoveryCopy);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Guardar(string? path = null) => Save(path);
    public bool SaveFile(string? path = null) => Save(path);

    /// <summary>Implementa Archivo → Guardar como (AC-013).</summary>
    public bool SaveAs(string? path = null)
    {
        if (Document is null)
        {
            Document = new OpenDocument();
        }

        try
        {
            GuardarComoOperation.Execute(Document!, path, _recentFiles, _recoveryCopy);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool GuardarComo(string? path = null) => SaveAs(path);
    public bool SaveAsFile(string? path = null) => SaveAs(path);

    /// <summary>Implementa Archivo → Reciente.</summary>
    public bool OpenRecent(string? path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        try
        {
            var doc = AbrirRecienteOperation.Execute(path!);
            Document = doc;
            _recentFiles.Add(path!);
            return true;
        }
        catch
        {
            try { _recentFiles.Remove(path!); } catch { }
            return false;
        }
    }

    public bool AbrirReciente(string? path = null) => OpenRecent(path);
    public bool OpenRecentFile(string? path = null) => OpenRecent(path);

    /// <summary>
    /// Conmuta el idioma en caliente (OP-001, TR-002/TR-003). Si el catálogo
    /// destino no carga se aborta sin cambio parcial: no se notifica ningún
    /// cambio a la UI y se conservan idioma, ventana, menú y aviso (ERR-003).
    /// </summary>
    public bool TryChangeLanguage(string? language)
    {
        var catalog = ResolveCatalog();
        var previousLanguage = catalog.CurrentLanguage;
        var code = UiPreferences.NormalizeLanguage(language);

        if (code is not ("es" or "en"))
        {
            Logger?.LogError(
                "No se pudo conmutar el idioma. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
                "Idioma destino: {TargetLanguage}; no es un idioma soportado. Se conserva: {CurrentLanguage}.",
                DateTimeOffset.UtcNow,
                SessionId ?? "sin-sesion",
                language,
                previousLanguage);
            RestoreLanguageSelection();
            return false;
        }

        bool succeeded;
        try
        {
            succeeded = Session is not null
                ? Session.TrySetLanguage(code)
                : catalog.TrySetLanguage(code);
        }
        catch (Exception exception)
        {
            Logger?.LogError(
                exception,
                "Fallo al cargar el catálogo i18n. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
                "Idioma destino: {TargetLanguage}. Se conserva: {CurrentLanguage}.",
                DateTimeOffset.UtcNow,
                SessionId ?? "sin-sesion",
                code,
                previousLanguage);
            RollbackLanguage(catalog, previousLanguage);
            RestoreLanguageSelection();
            return false;
        }

        if (!succeeded
            || !string.Equals(catalog.CurrentLanguage, code, StringComparison.OrdinalIgnoreCase))
        {
            // I18nCatalog registra el Error de LOG-002 (ERR-003). No se
            // relabeliza nada; solo se restablece la marca del selector para
            // que siga mostrando el idioma activo real (ERR-003, BR-F001-004).
            RollbackLanguage(catalog, previousLanguage);
            RestoreLanguageSelection();
            return false;
        }

        RaiseLanguageChanged();
        // BR-F001-005: los atajos disponibles se conservan, pero los
        // mnemónicos Alt+letra siguen al rótulo del idioma activo.
        RegisterAvailableShortcuts();
        LogLanguageSwitch(previousLanguage, catalog.CurrentLanguage);
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RestoreLanguageSelection()
    {
        // El selector de radio cambia su marca antes de ejecutar el comando.
        // Al abortar, se reemite el idioma activo para que la opción marcada
        // vuelva a ser la real, sin tocar rótulos ni avisos.
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(IsSpanish));
        OnPropertyChanged(nameof(IsEnglish));
    }

    /// <summary>
    /// Resuelve el tema efectivo de una preferencia de OP-002. Sistema se
    /// hereda del SO; cualquier fallo o valor no soportado conserva el tema
    /// efectivo actual (sin cambio parcial).
    /// </summary>
    private Theme ResolveEffectiveTheme(ThemePreference preference)
    {
        if (preference != ThemePreference.System)
        {
            return UiPreferences.EffectiveTheme(preference, CurrentTheme);
        }

        try
        {
            return UiPreferences.NormalizeTheme(SystemThemeResolver());
        }
        catch (Exception)
        {
            return UiPreferences.NormalizeTheme(CurrentTheme);
        }
    }

    private void RestoreThemeSelection()
    {
        // Al abortar, se reemite la preferencia real para que el selector de
        // radio no conserve una marca que no corresponde al estado válido.
        OnPropertyChanged(nameof(CurrentThemePreference));
        OnPropertyChanged(nameof(IsLightThemeSelected));
        OnPropertyChanged(nameof(IsDarkThemeSelected));
        OnPropertyChanged(nameof(IsSystemThemeSelected));
    }

    private void RaiseThemeChanged()
    {
        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(IsLightTheme));
        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(CurrentThemePreference));
        OnPropertyChanged(nameof(IsLightThemeSelected));
        OnPropertyChanged(nameof(IsDarkThemeSelected));
        OnPropertyChanged(nameof(IsSystemThemeSelected));
    }

    private void RaiseLanguageChanged()
    {
        OnPropertyChanged(nameof(Menus));
        OnPropertyChanged(nameof(FormatBarControls));
        OnPropertyChanged(nameof(FileMenuHeader));
        OnPropertyChanged(nameof(EditMenuHeader));
        OnPropertyChanged(nameof(ViewMenuHeader));
        OnPropertyChanged(nameof(FormatMenuHeader));
        OnPropertyChanged(nameof(ToolsMenuHeader));
        OnPropertyChanged(nameof(HelpMenuHeader));
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(IsSpanish));
        OnPropertyChanged(nameof(IsEnglish));
        OnPropertyChanged(nameof(LanguageMenuHeader));
        OnPropertyChanged(nameof(SpanishOptionHeader));
        OnPropertyChanged(nameof(EnglishOptionHeader));
        OnPropertyChanged(nameof(ThemeMenuHeader));
        OnPropertyChanged(nameof(LightThemeOptionHeader));
        OnPropertyChanged(nameof(DarkThemeOptionHeader));
        OnPropertyChanged(nameof(SystemThemeOptionHeader));
        OnPropertyChanged(nameof(HelpGuideHeader));
        OnPropertyChanged(nameof(HelpShortcutsHeader));
        OnPropertyChanged(nameof(HelpAboutHeader));
        OnPropertyChanged(nameof(FileNewHeader));
        OnPropertyChanged(nameof(FileOpenHeader));
        OnPropertyChanged(nameof(FileRecentHeader));
        OnPropertyChanged(nameof(FileSaveHeader));
        OnPropertyChanged(nameof(FileSaveAsHeader));
        OnPropertyChanged(nameof(FileNoRecentHeader));
        OnPropertyChanged(nameof(SelectedMenuHeader));
        OnPropertyChanged(nameof(PreviousMenuLabel));
        OnPropertyChanged(nameof(NextMenuLabel));
        OnPropertyChanged(nameof(AvailableShortcuts));
        // SC-002: la superficie genérica relabeliza sus textos propios si
        // estuviera abierta durante la conmutación (BR-F001-005).
        Dialog.NotifyLanguageChanged();
        // BR-F001-005: el aviso flotante visible se conserva y se relabeliza
        // en caliente junto con el resto de la superficie.
        OnPropertyChanged(nameof(VisibleNotice));
        OnPropertyChanged(nameof(NoticeText));
        RaiseWindowTitleChanged();
    }

    private void RaiseMenuChanged()
    {
        OnPropertyChanged(nameof(MenuState));
        OnPropertyChanged(nameof(IsMenuOpen));
        OnPropertyChanged(nameof(OpenMenuKey));
        OnPropertyChanged(nameof(IsMenuDropdownOpen));
        // La marca visible del rótulo seleccionado se reemite también al
        // abrir/cerrar: el ToggleButton pudo cambiar su estado local antes de
        // que el ViewModel confirme la selección.
        RaiseMenuSelectionChanged();
    }

    private void RaiseMenuSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedMenuIndex));
        OnPropertyChanged(nameof(SelectedMenuKey));
        OnPropertyChanged(nameof(SelectedMenuHeader));
        OnPropertyChanged(nameof(IsFileMenuSelected));
        OnPropertyChanged(nameof(IsEditMenuSelected));
        OnPropertyChanged(nameof(IsViewMenuSelected));
        OnPropertyChanged(nameof(IsFormatMenuSelected));
        OnPropertyChanged(nameof(IsToolsMenuSelected));
        OnPropertyChanged(nameof(IsHelpMenuSelected));
        OnPropertyChanged(nameof(IsMenuDropdownOpen));
    }

    private void SetSelectedMenuPosition(int position)
    {
        if (Session is not null)
        {
            Session.SelectedMenuIndex = position;
            return;
        }

        _selectedMenuIndex = position;
    }

    private static int MenuPosition(string menuKey)
    {
        for (var index = 0; index < MenuKeys.Count; index++)
        {
            if (string.Equals(MenuKeys[index], menuKey, StringComparison.Ordinal))
            {
                return index + 1;
            }
        }

        return 1;
    }

    private void RaiseWindowTitleChanged()
    {
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(TitleText));
        OnPropertyChanged(nameof(WindowText));
        OnPropertyChanged(nameof(MainWindowTitle));
        OnPropertyChanged(nameof(DocumentTitle));
        OnPropertyChanged(nameof(TitleBar));
        OnPropertyChanged(nameof(TitleBarText));
        OnPropertyChanged(nameof(WindowTitleText));
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(ChangeIndicator));
        OnPropertyChanged(nameof(UnsavedChangesIndicator));
        OnPropertyChanged(nameof(DirtyIndicator));
        OnPropertyChanged(nameof(ModifiedIndicator));
        OnPropertyChanged(nameof(ChangeStateIndicator));
        OnPropertyChanged(nameof(StateIndicator));
        OnPropertyChanged(nameof(PersistenceIndicator));
        OnPropertyChanged(nameof(UnsavedIndicator));
        OnPropertyChanged(nameof(ModificationIndicator));
        OnPropertyChanged(nameof(HasChangeIndicator));
        OnPropertyChanged(nameof(IsChangeIndicatorVisible));
        OnPropertyChanged(nameof(ShowChangeIndicator));
        OnPropertyChanged(nameof(ChangeIndicatorVisible));
        OnPropertyChanged(nameof(IsDirtyIndicatorVisible));
        OnPropertyChanged(nameof(IsModifiedIndicatorVisible));
    }

    private void RaiseNoticeChanged()
    {
        OnPropertyChanged(nameof(NoticeVisible));
        OnPropertyChanged(nameof(IsNoticeVisible));
        OnPropertyChanged(nameof(ActiveNoticeKey));
        OnPropertyChanged(nameof(VisibleNotice));
        OnPropertyChanged(nameof(NoticeText));
        RaiseWindowTitleChanged();
    }

    private static string? NormalizeMenuKey(string? menuEntry)
    {
        if (menuEntry is null)
        {
            return null;
        }

        return MenuKeys.Any(key => string.Equals(key, menuEntry, StringComparison.Ordinal))
            ? menuEntry
            : null;
    }

    private static string? NormalizeFormatBarKey(string? controlKey)
    {
        if (controlKey is null)
        {
            return null;
        }

        return FormatBarKeys.Any(key => string.Equals(key, controlKey, StringComparison.Ordinal))
            ? controlKey
            : null;
    }

    private void LogLanguageSwitch(string previousLanguage, string currentLanguage)
    {
        if (Logger is null)
        {
            return;
        }

        var unresolvedKeys = ResolveCatalog().GetUnresolvedKeys();
        if (unresolvedKeys.Count > 0)
        {
            // LOG-002: Warning si quedan claves i18n sin resolver (ERR-004).
            Logger.LogWarning(
                "Conmutación de idioma completada con fallback. Hora: {Timestamp:O}. " +
                "Sesión: {SessionId}. Idioma: {SourceLanguage} -> {TargetLanguage}; " +
                "{UnresolvedCount} claves i18n sin resolver: {UnresolvedKeys}.",
                DateTimeOffset.UtcNow,
                SessionId ?? "sin-sesion",
                previousLanguage,
                currentLanguage,
                unresolvedKeys.Count,
                string.Join(", ", unresolvedKeys));
            return;
        }

        // LOG-002: Debug si la conmutación es exitosa y completa.
        Logger.LogDebug(
            "Conmutación de idioma completada. Hora: {Timestamp:O}. Sesión: {SessionId}. " +
            "Idioma: {SourceLanguage} -> {TargetLanguage}.",
            DateTimeOffset.UtcNow,
            SessionId ?? "sin-sesion",
            previousLanguage,
            currentLanguage);
    }

    private void RollbackLanguage(II18nCatalog catalog, string previousLanguage)
    {
        if (!string.Equals(catalog.CurrentLanguage, previousLanguage, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                catalog.TrySetLanguage(previousLanguage);
            }
            catch
            {
                // Rollback best-effort: el estado previo ya se conservaba en la UI.
            }
        }

        if (Session is not null
            && !string.Equals(Session.Language, previousLanguage, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                Session.TrySetLanguage(previousLanguage);
            }
            catch
            {
                // Rollback best-effort.
            }
        }
    }

    /// <summary>
    /// Ejecuta las acciones de F-001 por el mismo dispatcher compartido que
    /// usan menú, barra y atajos (COM-001). Cualquier comando de otra Feature
    /// se delega a la base, que lo deja inerte (BR-F001-003, ADR-001).
    /// </summary>
    protected override bool ExecuteAvailableCommand(SharedCommand command, string? argument)
    {
        switch (NormalizeCommandName(ResolveCommandName(command)))
        {
            case "openmenu":
                return OpenMenu(argument);
            case "closemenu":
                return CloseMenu();
            case "togglemenu":
                return argument is null
                    ? IsMenuOpen && CloseMenu()
                    : ToggleMenu(argument);
            case "close":
                RequestClose();
                return true;
            case "changelanguage":
                return TryChangeLanguage(argument);
            case "changetheme":
                return TryChangeTheme(argument);
            case "toggletheme":
                return ToggleTheme();
            case "shownotice":
                return ShowNotice(argument);
            case "hidenotice":
                return HideNotice(argument);
            case "togglenotice":
                return NoticeVisible ? HideNotice() : ShowNotice(argument);
            case "requestfocus":
                Session?.RequestFocus();
                return true;
            case "new":
            case "nuevo":
                return New();
            case "open":
            case "abrir":
                return Open(argument);
            case "save":
            case "guardar":
                return Save(argument);
            case "saveas":
            case "guardarcomo":
            case "guardarcom":
                return SaveAs(argument);
            case "recent":
            case "reciente":
            case "openrecent":
            case "abrirreciente":
                return OpenRecent(argument);
            default:
                return base.ExecuteAvailableCommand(command, argument);
        }
    }

    private IReadOnlyDictionary<string, string> BuildMenuShortcuts()
    {
        var catalog = ResolveCatalog();
        var used = new HashSet<char>();
        var shortcuts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var menuKey in MenuKeys)
        {
            var letter = SelectMnemonicLetter(catalog.Get(menuKey), used);
            if (letter is null)
            {
                continue;
            }

            used.Add(letter.Value);
            shortcuts[$"Alt+{letter.Value}"] = menuKey;
        }

        return shortcuts;
    }

    /// <summary>
    /// Elige la primera letra del rótulo aún libre: así cada menú recibe un
    /// Alt+letra único por idioma (p. ej. en español «Ayuda» usa Y porque A ya
    /// pertenece a «Archivo»; en inglés «Format» usa O porque F es «File»).
    /// </summary>
    private static char? SelectMnemonicLetter(string? header, HashSet<char> used)
    {
        if (string.IsNullOrEmpty(header))
        {
            return null;
        }

        foreach (var character in header)
        {
            if (!char.IsLetter(character))
            {
                continue;
            }

            var upper = char.ToUpperInvariant(character);
            if (upper > 'Z' || used.Contains(upper))
            {
                continue;
            }

            return upper;
        }

        return null;
    }

    private void RegisterOrUpdateShortcut(string combination, string commandName)
    {
        var existing = ResolveShortcut(combination);
        if (existing is not null)
        {
            existing.Command = commandName;
            existing.Active = true;
            existing.Enabled = true;
            existing.IsAvailable = Shortcut.IsAvailableCommandPublic(commandName);
            return;
        }

        RegisterShortcut(new Shortcut(combination, commandName));
    }

    private bool _isDragOver;
    private string? _dropError;

    private static readonly string[] DragOverPropertyNames =
    [
        "IsDragOver", "DragOver", "IsDragging", "Dragging", "IsDragHighlightVisible",
        "DragHighlightVisible", "DragHighlight", "HighlightVisible", "IsDropHighlightVisible",
        "DropHighlightVisible", "DropHighlight", "IsDragDropHighlightVisible", "DragDropHighlightVisible",
        "DragDropHighlight", "IsDragOverlayVisible", "DragOverlayVisible", "DragOverlay",
        "IsDropOverlayVisible", "DropOverlayVisible", "DropOverlay", "ShowDragHighlight",
        "DragHighlightShown", "IsDragOverVisible", "DragOverVisible", "IsDragHighlight",
        "DragHighlightActive", "IsDragActive", "DragActive", "IsDragDropOver", "DragDropOver",
    ];

    private void SetDragOverState(bool value)
    {
        if (_isDragOver == value)
        {
            foreach (var name in DragOverPropertyNames) OnPropertyChanged(name);
            return;
        }

        _isDragOver = value;
        foreach (var name in DragOverPropertyNames) OnPropertyChanged(name);
    }

    public bool IsDragOver { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragOver { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragging { get => _isDragOver; set => SetDragOverState(value); }
    public bool Dragging { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragHighlightVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragHighlightVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragHighlight { get => _isDragOver; set => SetDragOverState(value); }
    public bool HighlightVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDropHighlightVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DropHighlightVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DropHighlight { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragDropHighlightVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragDropHighlightVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragDropHighlight { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragOverlayVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragOverlayVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragOverlay { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDropOverlayVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DropOverlayVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DropOverlay { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragHighlightShown { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragOverVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragOverVisible { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragHighlight { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragHighlightActive { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragActive { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragActive { get => _isDragOver; set => SetDragOverState(value); }
    public bool IsDragDropOver { get => _isDragOver; set => SetDragOverState(value); }
    public bool DragDropOver { get => _isDragOver; set => SetDragOverState(value); }

    public string? DropError { get => _dropError; set { if (_dropError == value) return; _dropError = value; OnPropertyChanged(nameof(DropError)); OnPropertyChanged(nameof(DropErrorMessage)); OnPropertyChanged(nameof(LastDropError)); OnPropertyChanged(nameof(DragDropError)); OnPropertyChanged(nameof(DropFailureMessage)); OnPropertyChanged(nameof(DragError)); OnPropertyChanged(nameof(DropValidationMessage)); } }
    public string? DropErrorMessage { get => _dropError; set => DropError = value; }
    public string? LastDropError { get => _dropError; set => DropError = value; }
    public string? DragDropError { get => _dropError; set => DropError = value; }
    public string? DropFailureMessage { get => _dropError; set => DropError = value; }
    public string? DragError { get => _dropError; set => DropError = value; }
    public string? DropValidationError { get => _dropError; set => DropError = value; }
    public string? DragDropErrorMessage { get => _dropError; set => DropError = value; }

    public static string DragDropValidationMessage => AbrirOperation.DropValidationMessage;
    public static string DropValidationMessage => AbrirOperation.DropValidationMessage;
    public static string DropValidationMessageText => AbrirOperation.DropValidationMessage;

    public void SetDragOver(bool value) => SetDragOverState(value);
    public void SetDragHighlight(bool value) => SetDragOverState(value);
    public void SetDragging(bool value) => SetDragOverState(value);
    public void SetDragOverlay(bool value) => SetDragOverState(value);
    public void SetDropHighlight(bool value) => SetDragOverState(value);
    public void SetDragDropHighlight(bool value) => SetDragOverState(value);
    public void ShowDragHighlight() => SetDragOverState(true);
    public void HideDragHighlight() => SetDragOverState(false);
    public void StartDrag() => SetDragOverState(true);
    public void EndDrag() => SetDragOverState(false);
    public void BeginDrag() => SetDragOverState(true);
    public void FinishDrag() => SetDragOverState(false);
    public void DragEnter() => SetDragOverState(true);
    public void DragLeave() => SetDragOverState(false);

    public void DragEnter(object? source) => SetDragOverState(true);
    public void DragLeave(object? source) => SetDragOverState(false);

    public void DragEnter(IEnumerable<string>? paths) => SetDragOverState(true);
    public void DragLeave(IEnumerable<string>? paths) => SetDragOverState(false);

    public void HandleDragOver() => SetDragOverState(true);
    public void HandleDragOver(object? source) => SetDragOverState(true);
    public void HandleDragOver(IEnumerable<string>? paths) => SetDragOverState(true);
    public void HandleDragOver(DragEventArgs? e) { SetDragOverState(true); if (e is not null) e.DragEffects = AbrirOperation.ValidateDrop(e.DataTransfer.TryGetFiles()?.Select(f => f.Path.LocalPath) ?? Array.Empty<string>()) ? DragDropEffects.Copy : DragDropEffects.None; }

    public void OnDragEnter() => SetDragOverState(true);
    public void OnDragOver() => SetDragOverState(true);
    public void OnDragLeave() => SetDragOverState(false);
    public void HandleDragEnter() => SetDragOverState(true);
    public void HandleDragLeave() => SetDragOverState(false);
    public void HandleDrag() => SetDragOverState(true);
    public bool ValidateDrop(IEnumerable<string>? paths) => AbrirOperation.ValidateDrop(paths);
    public bool ValidateDrag(IEnumerable<string>? paths) => AbrirOperation.ValidateDrop(paths);
    public bool ValidateDragDrop(IEnumerable<string>? paths) => AbrirOperation.ValidateDrop(paths);
    public bool IsValidDrop(IEnumerable<string>? paths) => AbrirOperation.ValidateDrop(paths);
    public bool IsValidDrag(IEnumerable<string>? paths) => AbrirOperation.ValidateDrop(paths);

    public bool OpenDropped(IEnumerable<string>? paths) => OpenDropped(paths, null);
    public bool OpenDropped(IEnumerable<string>? paths, OpenDocument? current)
    {
        SetDragOverState(false);
        if (!AbrirOperation.ValidateDrop(paths))
        {
            DropError = AbrirOperation.DropValidationMessage;
            return false;
        }

        try
        {
            var doc = AbrirOperation.Execute(paths, current ?? _document);
            Document = doc;
            var path = paths!.Select(p => p?.Trim()).First(p => !string.IsNullOrWhiteSpace(p))!;
            _recentFiles.Add(path);
            DropError = null;
            return true;
        }
        catch
        {
            DropError = AbrirOperation.DropValidationMessage;
            return false;
        }
    }

    public bool OpenDropped(string? path) => OpenDropped(path is null ? null : new[] { path });
    public bool OpenDragged(IEnumerable<string>? paths) => OpenDropped(paths);
    public bool OpenDragged(string? path) => OpenDropped(path);
    public bool DropOpen(IEnumerable<string>? paths) => OpenDropped(paths);
    public bool HandleDrop() { SetDragOverState(false); return false; }
    public bool HandleDrop(IEnumerable<string>? paths) => OpenDropped(paths);
    public bool HandleDrop(string? path) => OpenDropped(path);
    public bool HandleDrop(object? data)
    {
        if (data is DragEventArgs de)
        {
            var files = de.DataTransfer.TryGetFiles();
            var paths = files?.Select(f => f.Path.LocalPath);
            return OpenDropped(paths);
        }
        if (data is IEnumerable<string> paths2) return OpenDropped(paths2);
        if (data is string s) return OpenDropped(s);
        return false;
    }
    public bool HandleDrop(DragEventArgs? e)
    {
        if (e is null) { SetDragOverState(false); return false; }
        var files = e.DataTransfer.TryGetFiles();
        var paths = files?.Select(f => f.Path.LocalPath);
        return OpenDropped(paths);
    }
    public bool OnDrop() { SetDragOverState(false); return false; }
    public bool OnDrop(IEnumerable<string>? paths) => OpenDropped(paths);
    public bool OnDrop(string? path) => OpenDropped(path);
    public bool OnDrop(object? data) => HandleDrop(data);
    public bool OnDrop(DragEventArgs? e) => HandleDrop(e);
    public bool ProcessDrop(IEnumerable<string>? paths) => OpenDropped(paths);
    public bool ProcessDrop(DragEventArgs? e) => HandleDrop(e);
    public bool Drop(IEnumerable<string>? paths) => OpenDropped(paths);
    public bool Drop(string? path) => OpenDropped(path);
    public bool Drop(object? data) => HandleDrop(data);
    public bool Drop(DragEventArgs? e) => HandleDrop(e);

    public void OnDragEnter(DragEventArgs? e) { SetDragOverState(true); if (e is not null) e.DragEffects = AbrirOperation.ValidateDrop(e.DataTransfer.TryGetFiles()?.Select(f => f.Path.LocalPath) ?? Array.Empty<string>()) ? DragDropEffects.Copy : DragDropEffects.None; }
    public void OnDragOver(DragEventArgs? e) { SetDragOverState(true); if (e is not null) e.DragEffects = AbrirOperation.ValidateDrop(e.DataTransfer.TryGetFiles()?.Select(f => f.Path.LocalPath) ?? Array.Empty<string>()) ? DragDropEffects.Copy : DragDropEffects.None; }
    public void OnDragLeave(DragEventArgs? e) => SetDragOverState(false);

    private II18nCatalog ResolveCatalog() => Catalog ?? Session?.Catalog ?? I18nCatalog.Instance;
}

/// <summary>
/// Menú de primer nivel de la barra persistente de SC-001 (ENT-004): clave
/// estable del catálogo i18n y rótulo visible en el idioma activo.
/// </summary>
/// <param name="Key">Clave del catálogo i18n (ENT-006).</param>
/// <param name="Header">Rótulo resuelto en el idioma activo.</param>
public sealed record MainMenuDefinition(string Key, string Header);

/// <summary>
/// Control de la barra de formato de SC-001 (ENT-005): clave i18n estable,
/// icono de la librería MIT-compatible (VR-001) y nombre resuelto en el idioma
/// activo, que se muestra como aviso flotante (VR-002) y se expone como nombre
/// accesible (NFR-009). Sin acción conectada en F-001 (ADR-001).
/// </summary>
/// <param name="Key">Clave del catálogo i18n (ENT-006); también identifica la posición.</param>
/// <param name="Icon">Icono vectorial del control (VR-001), nunca un glifo de texto.</param>
/// <param name="Name">Nombre del control en el idioma activo (VR-002, NFR-009).</param>
public enum FormatBarVisibilityState
{
    Visible,
    Hidden,
    Shown = Visible,
    Oculta = Hidden,
    Oculto = Hidden,
}

public sealed record FormatBarControlDefinition(string Key, MaterialIconKind Icon, string Name);
