using System;
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
public class MainWindowViewModel : CommandDispatcher, INotifyPropertyChanged
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

    public MainWindowViewModel()
    {
        RegisterAvailableShortcuts();
    }

    public MainWindowViewModel(Session? session)
        : base(session)
    {
        Catalog = session?.Catalog;
        RegisterAvailableShortcuts();
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
        IsMenuOpen && (IsToolsMenuSelected || IsHelpMenuSelected);

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

    private void RaiseNoticeChanged()
    {
        OnPropertyChanged(nameof(NoticeVisible));
        OnPropertyChanged(nameof(IsNoticeVisible));
        OnPropertyChanged(nameof(ActiveNoticeKey));
        OnPropertyChanged(nameof(VisibleNotice));
        OnPropertyChanged(nameof(NoticeText));
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
public sealed record FormatBarControlDefinition(string Key, MaterialIconKind Icon, string Name);
