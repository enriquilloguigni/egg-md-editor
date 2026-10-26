using System;
using System.ComponentModel;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

/// <summary>
/// Sesión de aplicación en memoria (ENT-001): idioma, tema, estado de menú y aviso.
/// Singleton por proceso; se libera al cerrar.
/// </summary>
public class Session : INotifyPropertyChanged, ISession, IDisposable
{
    private static readonly Lazy<Session> _instance = new(() => new Session());

    private string _language;
    private Theme _theme;
    private ThemePreference _themePreference;
    private MenuState _menuState;
    private int _selectedMenuIndex = 1;
    private bool _noticeVisible;
    private bool _windowOpen;
    private II18nCatalog _catalog;
    private UiPreferences _preferences;
    private bool _syncingPreferences;

    public Session()
        : this(I18nCatalog.Instance, "es", Theme.Light, MenuState.Closed, false, true, 1, ThemePreference.System)
    {
    }

    public Session(II18nCatalog catalog)
        : this(catalog, catalog?.CurrentLanguage ?? "es", Theme.Light, MenuState.Closed, false, true, 1, ThemePreference.System)
    {
    }

    public Session(UiPreferences preferences)
        : this(
            I18nCatalog.Instance,
            preferences?.Language ?? "es",
            preferences?.Theme ?? Theme.Light,
            MenuState.Closed,
            false,
            true,
            1,
            preferences?.ThemePreference ?? ThemePreference.System)
    {
        CopyPreferencesFrom(preferences);
    }

    public Session(II18nCatalog catalog, UiPreferences preferences)
        : this(
            catalog,
            preferences?.Language ?? catalog?.CurrentLanguage ?? "es",
            preferences?.Theme ?? Theme.Light,
            MenuState.Closed,
            false,
            true,
            1,
            preferences?.ThemePreference ?? ThemePreference.System)
    {
        CopyPreferencesFrom(preferences);
    }

    public Session(
        II18nCatalog catalog,
        string language,
        Theme theme,
        MenuState menuState,
        bool noticeVisible,
        bool windowOpen = true,
        int selectedMenuIndex = 1,
        ThemePreference themePreference = ThemePreference.System)
    {
        _catalog = catalog ?? I18nCatalog.Instance;
        _language = UiPreferences.NormalizeLanguage(language);
        _theme = UiPreferences.NormalizeTheme(theme);
        _themePreference = UiPreferences.NormalizeThemePreference(themePreference);
        if (_themePreference is ThemePreference.Light or ThemePreference.Dark)
        {
            _theme = UiPreferences.EffectiveTheme(_themePreference, _theme);
        }

        _menuState = menuState;
        _selectedMenuIndex = IsValidMenuPosition(selectedMenuIndex) ? selectedMenuIndex : 1;
        _noticeVisible = noticeVisible;
        _windowOpen = windowOpen;
        _preferences = new UiPreferences(_language, _theme, _themePreference);
        _preferences.PropertyChanged += OnPreferencesPropertyChanged;

        // Sync catalog best-effort without throwing on construction.
        try { _catalog.TrySetLanguage(_language); } catch { /* ignore */ }
    }

    public static Session Instance => _instance.Value;
    public static Session Current => Instance;
    public static Session Default => Instance;

    public II18nCatalog Catalog
    {
        get => _catalog;
        set => _catalog = value ?? I18nCatalog.Instance;
    }

    public I18nCatalog? I18nCatalog
    {
        get => _catalog as I18nCatalog;
        set => _catalog = value ?? I18nCatalog.Instance;
    }

    public UiPreferences Preferences
    {
        get => _preferences;
        set
        {
            if (value is null)
            {
                return;
            }

            if (_preferences is not null)
            {
                _preferences.PropertyChanged -= OnPreferencesPropertyChanged;
            }

            _preferences = value;
            _preferences.PropertyChanged += OnPreferencesPropertyChanged;

            // Sync language/theme best-effort without partial change.
            TrySetLanguage(_preferences.Language);
            TrySetThemePreference(_preferences.ThemePreference, _preferences.Theme);
        }
    }

    public string Language
    {
        get => _language;
        set => TrySetLanguage(value);
    }

    public string CurrentLanguage
    {
        get => _language;
        set => Language = value;
    }

    public string ActiveLanguage
    {
        get => _language;
        set => Language = value;
    }

    public string LanguageCode
    {
        get => _language;
        set => Language = value;
    }

    public string Idioma
    {
        get => _language;
        set => Language = value;
    }

    public Theme Theme
    {
        get => _theme;
        set => TrySetTheme(value);
    }

    public Theme CurrentTheme
    {
        get => _theme;
        set => Theme = value;
    }

    public Theme ActiveTheme
    {
        get => _theme;
        set => Theme = value;
    }

    public Theme Tema
    {
        get => _theme;
        set => Theme = value;
    }

    /// <summary>
    /// Preferencia de tema de ENT-003 {Claro,Oscuro,Sistema}: default Sistema.
    /// El tema efectivo de ENT-001 se expone en <see cref="Theme"/> y se
    /// resuelve del SO al arrancar o al elegir Sistema (OP-002).
    /// </summary>
    public ThemePreference ThemePreference
    {
        get => _themePreference;
        set => TrySetThemePreference(value);
    }

    public ThemePreference CurrentThemePreference
    {
        get => _themePreference;
        set => ThemePreference = value;
    }

    public ThemePreference TemaPreferencia
    {
        get => _themePreference;
        set => ThemePreference = value;
    }

    public MenuState Menu
    {
        get => _menuState;
        set => MenuState = value;
    }

    public MenuState MenuState
    {
        get => _menuState;
        set
        {
            // OP-003: el estado del menú solo admite {Cerrado, Desplegado}
            // (TR-004/TR-005). Un valor fuera del catálogo no altera el estado.
            if (value is not (MenuState.Closed or MenuState.Open))
            {
                return;
            }

            _menuState = value;
            OnPropertyChanged(nameof(MenuState));
            OnPropertyChanged(nameof(CurrentMenuState));
            OnPropertyChanged(nameof(EstadoMenu));
            OnPropertyChanged(nameof(IsMenuOpen));
            OnPropertyChanged(nameof(IsMenuClosed));
        }
    }

    public MenuState CurrentMenuState
    {
        get => _menuState;
        set => MenuState = value;
    }

    public MenuState EstadoMenu
    {
        get => _menuState;
        set => MenuState = value;
    }

    public bool IsMenuOpen
    {
        get => _menuState == MenuState.Open;
        set => MenuState = value ? MenuState.Open : MenuState.Closed;
    }

    public bool IsMenuClosed
    {
        get => _menuState == MenuState.Closed;
        set => MenuState = value ? MenuState.Closed : MenuState.Open;
    }

    /// <summary>
    /// Rótulo de menú seleccionado (ENT-001/ENT-004, ST-011): entero 1–6 con
    /// 1 = Archivo. Un valor fuera del catálogo no altera el estado válido.
    /// </summary>
    public int SelectedMenuIndex
    {
        get => _selectedMenuIndex;
        set
        {
            if (!IsValidMenuPosition(value) || _selectedMenuIndex == value)
            {
                return;
            }

            _selectedMenuIndex = value;
            OnPropertyChanged(nameof(SelectedMenuIndex));
        }
    }

    /// <summary>OP-007: cambia el rótulo seleccionado dentro de 1–6.</summary>
    public bool TrySelectMenu(int position)
    {
        if (!IsValidMenuPosition(position))
        {
            return false;
        }

        SelectedMenuIndex = position;
        return true;
    }

    private static bool IsValidMenuPosition(int position) => position is >= 1 and <= 6;

    public bool Aviso
    {
        get => _noticeVisible;
        set => NoticeVisible = value;
    }

    public NoticeState AvisoState
    {
        get => NoticeState;
        set => NoticeState = value;
    }

    public bool NoticeVisible
    {
        get => _noticeVisible;
        set
        {
            _noticeVisible = value;
            OnPropertyChanged(nameof(NoticeVisible));
            OnPropertyChanged(nameof(IsNoticeVisible));
            OnPropertyChanged(nameof(AvisoVisible));
            OnPropertyChanged(nameof(NoticeState));
        }
    }

    public bool IsNoticeVisible
    {
        get => _noticeVisible;
        set => NoticeVisible = value;
    }

    public bool AvisoVisible
    {
        get => _noticeVisible;
        set => NoticeVisible = value;
    }

    public NoticeState NoticeState
    {
        get => _noticeVisible ? NoticeState.Visible : NoticeState.Hidden;
        set => NoticeVisible = value == NoticeState.Visible;
    }

    public bool Ventana
    {
        get => _windowOpen;
        set => WindowOpen = value;
    }

    public bool WindowOpen
    {
        get => _windowOpen;
        set
        {
            _windowOpen = value;
            OnPropertyChanged(nameof(WindowOpen));
            OnPropertyChanged(nameof(IsWindowOpen));
        }
    }

    public bool IsWindowOpen
    {
        get => _windowOpen;
        set => WindowOpen = value;
    }

    public object? Window { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Language operations
    public bool TrySetLanguage(string? language)
    {
        var code = UiPreferences.NormalizeLanguage(language);
        if (code is not ("es" or "en"))
        {
            return false;
        }

        // abort without partial change if catalog rejects
        if (_catalog is not null && !_catalog.TrySetLanguage(code))
        {
            return false;
        }

        if (string.Equals(_language, code, StringComparison.Ordinal))
        {
            _syncingPreferences = true;
            try { _preferences.TrySetLanguage(code); } finally { _syncingPreferences = false; }
            return true;
        }

        _language = code;
        _syncingPreferences = true;
        try { _preferences.TrySetLanguage(code); } finally { _syncingPreferences = false; }
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(ActiveLanguage));
        OnPropertyChanged(nameof(LanguageCode));
        OnPropertyChanged(nameof(Idioma));
        return true;
    }

    public bool TrySetLanguage(Language language) => TrySetLanguage(ToCode(language));
    public bool TrySetLanguage(AppLanguage language) => TrySetLanguage(language == AppLanguage.English || language == AppLanguage.En ? "en" : "es");
    public bool TrySetLanguage(UiLanguage language) => TrySetLanguage(language == UiLanguage.English || language == UiLanguage.En ? "en" : "es");

    public void SetLanguage(string language)
    {
        if (!TrySetLanguage(language))
        {
            throw new InvalidOperationException($"No se pudo cambiar el idioma a '{language}'.");
        }
    }

    public void SetLanguage(Language language) => SetLanguage(ToCode(language));
    public void SetLanguage(AppLanguage language) => SetLanguage(language == AppLanguage.English || language == AppLanguage.En ? "en" : "es");
    public void SetLanguage(UiLanguage language) => SetLanguage(language == UiLanguage.English || language == UiLanguage.En ? "en" : "es");

    public void ChangeLanguage(string language) => SetLanguage(language);
    public void ChangeLanguage(Language language) => SetLanguage(language);

    public void SwitchLanguage(string language) => SetLanguage(language);
    public void SwitchLanguage(Language language) => SetLanguage(language);

    public bool ChangeLanguageSafe(string language) => TrySetLanguage(language);

    // Theme operations
    public bool TrySetTheme(Theme theme)
    {
        // OP-002: entrada de tema efectivo {Claro,Oscuro}; fuera de ese
        // catálogo se aborta sin cambio parcial y las preferencias (ENT-003)
        // conservan el tema activo.
        if (theme is not (Theme.Light or Theme.Dark))
        {
            return false;
        }

        return TrySetThemePreference(UiPreferences.ThemePreferenceFor(theme), theme);
    }

    /// <summary>
    /// OP-002: fija la preferencia de tema {Claro,Oscuro,Sistema}. Sin una
    /// resolución del SO disponible, Sistema conserva el tema efectivo actual.
    /// </summary>
    public bool TrySetThemePreference(ThemePreference preference) =>
        TrySetThemePreference(preference, _theme);

    /// <summary>
    /// OP-002/TR-008/TR-009: fija preferencia y tema efectivo de forma
    /// atómica; Sistema se resuelve con <paramref name="systemTheme"/>. Una
    /// entrada fuera del catálogo no altera el estado válido.
    /// </summary>
    public bool TrySetThemePreference(ThemePreference preference, Theme systemTheme)
    {
        if (!UiPreferences.IsSupportedThemePreference(preference))
        {
            return false;
        }

        var effective = UiPreferences.EffectiveTheme(preference, systemTheme);
        var preferenceChanged = _themePreference != preference;
        var themeChanged = _theme != effective;

        _themePreference = preference;
        _theme = effective;

        _syncingPreferences = true;
        try
        {
            _preferences.TrySetThemePreference(preference, effective);
        }
        finally
        {
            _syncingPreferences = false;
        }

        if (!preferenceChanged && !themeChanged)
        {
            return true;
        }

        if (themeChanged)
        {
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(CurrentTheme));
            OnPropertyChanged(nameof(ActiveTheme));
            OnPropertyChanged(nameof(Tema));
        }

        if (preferenceChanged)
        {
            OnPropertyChanged(nameof(ThemePreference));
            OnPropertyChanged(nameof(CurrentThemePreference));
            OnPropertyChanged(nameof(TemaPreferencia));
        }

        return true;
    }

    public bool TrySetTheme(AppTheme theme) => TrySetTheme((Theme)theme);
    public bool TrySetTheme(ThemeMode theme) => TrySetTheme((Theme)theme);
    public bool TrySetTheme(ThemeVariant theme) => TrySetTheme((Theme)theme);

    public void SetTheme(AppTheme theme) => SetTheme((Theme)theme);
    public void SetTheme(ThemeMode theme) => SetTheme((Theme)theme);
    public void SetTheme(ThemeVariant theme) => SetTheme((Theme)theme);

    public void ChangeTheme(AppTheme theme) => SetTheme(theme);
    public void ChangeTheme(ThemeMode theme) => SetTheme(theme);
    public void ChangeTheme(ThemeVariant theme) => SetTheme(theme);

    public void SwitchTheme(AppTheme theme) => SetTheme(theme);
    public void SwitchTheme(ThemeMode theme) => SetTheme(theme);
    public void SwitchTheme(ThemeVariant theme) => SetTheme(theme);

    public bool TrySetTheme(string? theme)
    {
        if (!UiPreferences.TryParseThemePreference(theme, out var parsed))
        {
            return false;
        }

        return TrySetThemePreference(parsed);
    }

    public bool TrySetThemePreference(string? preference)
    {
        if (!UiPreferences.TryParseThemePreference(preference, out var parsed))
        {
            return false;
        }

        return TrySetThemePreference(parsed);
    }

    public void SetTheme(Theme theme)
    {
        if (!TrySetTheme(theme))
        {
            throw new InvalidOperationException($"Tema no soportado: '{theme}'.");
        }
    }

    public void SetTheme(string theme)
    {
        if (!TrySetTheme(theme))
        {
            throw new InvalidOperationException($"Tema no soportado: '{theme}'.");
        }
    }

    public void ChangeTheme(Theme theme) => SetTheme(theme);
    public void ChangeTheme(string theme) => SetTheme(theme);

    public void SwitchTheme(Theme theme) => SetTheme(theme);
    public void SwitchTheme(string theme) => SetTheme(theme);

    public void ToggleTheme() => SetTheme(_theme == Theme.Light ? Theme.Dark : Theme.Light);

    public void ApplyTheme(Theme theme) => SetTheme(theme);
    public void ApplyTheme(string theme) => SetTheme(theme);

    // Menu operations
    public void OpenMenu() => MenuState = MenuState.Open;
    public void CloseMenu() => MenuState = MenuState.Closed;
    public void ToggleMenu() => MenuState = _menuState == MenuState.Closed ? MenuState.Open : MenuState.Closed;

    /// <summary>
    /// OP-003/TR-004: abre el menú si la ventana está abierta. Un menú
    /// desplegado permanece desplegado (la entrada concreta la conserva la
    /// vista/ViewModel).
    /// </summary>
    public bool TryOpenMenu()
    {
        if (!_windowOpen)
        {
            return false;
        }

        MenuState = MenuState.Open;
        return IsMenuOpen;
    }

    /// <summary>
    /// OP-003/TR-005: cierra el menú solo si está desplegado.
    /// </summary>
    public bool TryCloseMenu()
    {
        if (_menuState != MenuState.Open)
        {
            return false;
        }

        MenuState = MenuState.Closed;
        return true;
    }

    public bool OpenMenuEntry() { OpenMenu(); return true; }
    public bool CloseMenuEntry() { CloseMenu(); return true; }

    public bool SetMenuState(MenuState state) => TrySetMenuState(state);

    public bool TrySetMenuState(MenuState state)
    {
        if (state is not (MenuState.Closed or MenuState.Open))
        {
            return false;
        }

        MenuState = state;
        return true;
    }

    public bool SetMenuState(MenuStatus state)
    {
        return TrySetMenuState((MenuState)state);
    }

    public bool TrySetMenuState(MenuStatus state)
    {
        return TrySetMenuState((MenuState)state);
    }

    public bool SetMenuState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return false;
        }

        var s = state.Trim().ToLowerInvariant();
        if (s is "open" or "opened" or "desplegado" or "abierto" or "true" or "1")
        {
            MenuState = MenuState.Open;
            return true;
        }

        if (s is "closed" or "cerrado" or "false" or "0")
        {
            MenuState = MenuState.Closed;
            return true;
        }

        return false;
    }

    public bool TrySetMenuState(string? state) => SetMenuState(state);

    public void SetMenuOpen(bool open) => IsMenuOpen = open;
    public void Open() => OpenMenu();
    public void Close() => CloseWindow();

    // Notice operations
    public void ShowNotice() => NoticeVisible = true;
    public void HideNotice() => NoticeVisible = false;
    public void ToggleNotice() => NoticeVisible = !_noticeVisible;

    public bool ShowTooltip() { ShowNotice(); return true; }
    public bool HideTooltip() { HideNotice(); return true; }

    public bool SetNotice(bool visible)
    {
        NoticeVisible = visible;
        return true;
    }

    public bool TrySetNotice(bool visible)
    {
        NoticeVisible = visible;
        return true;
    }

    public bool SetNotice(NoticeState state)
    {
        NoticeState = state;
        return true;
    }

    public bool TrySetNotice(NoticeState state)
    {
        NoticeState = state;
        return true;
    }

    public bool SetNotice(NoticeVisibility state)
    {
        NoticeVisible = state == NoticeVisibility.Visible;
        return true;
    }

    public bool TrySetNotice(NoticeVisibility state)
    {
        NoticeVisible = state == NoticeVisibility.Visible;
        return true;
    }

    public bool SetNotice(TooltipState state)
    {
        NoticeVisible = state == TooltipState.Visible;
        return true;
    }

    public bool TrySetNotice(TooltipState state)
    {
        NoticeVisible = state == TooltipState.Visible;
        return true;
    }

    public bool SetAviso(bool visible) => SetNotice(visible);
    public void MostrarAviso() => ShowNotice();
    public void OcultarAviso() => HideNotice();

    // Window operations
    /// <summary>
    /// OP-006/TR-001: cerrar la ventana finaliza la sesión (ST-001 → ST-002).
    /// No queda estado transitorio activo —el menú vuelve a cerrado y el
    /// aviso a oculto— y no se persiste nada (FR-013). Es idempotente.
    /// </summary>
    public void CloseWindow()
    {
        MenuState = MenuState.Closed;
        NoticeVisible = false;
        _windowOpen = false;
        OnPropertyChanged(nameof(WindowOpen));
        OnPropertyChanged(nameof(IsWindowOpen));
    }

    public void RequestFocus() { /* no-op in memory */ }

    public Session Clone()
    {
        return new Session(
            _catalog,
            _language,
            _theme,
            _menuState,
            _noticeVisible,
            _windowOpen,
            _selectedMenuIndex,
            _themePreference);
    }

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void OnPreferencesPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_syncingPreferences)
        {
            return;
        }

        if (e.PropertyName == nameof(UiPreferences.Language)
            || e.PropertyName == nameof(UiPreferences.CurrentLanguage)
            || e.PropertyName == nameof(UiPreferences.ActiveLanguage)
            || e.PropertyName == nameof(UiPreferences.LanguageCode)
            || e.PropertyName == nameof(UiPreferences.Idioma))
        {
            _syncingPreferences = true;
            try { TrySetLanguage(_preferences.Language); }
            finally { _syncingPreferences = false; }
            return;
        }

        if (e.PropertyName == nameof(UiPreferences.Theme)
            || e.PropertyName == nameof(UiPreferences.CurrentTheme)
            || e.PropertyName == nameof(UiPreferences.ActiveTheme)
            || e.PropertyName == nameof(UiPreferences.Tema)
            || e.PropertyName == nameof(UiPreferences.ThemePreference)
            || e.PropertyName == nameof(UiPreferences.CurrentThemePreference)
            || e.PropertyName == nameof(UiPreferences.TemaPreferencia))
        {
            TrySetThemePreference(_preferences.ThemePreference, _preferences.Theme);
        }
    }

    /// <summary>
    /// Copia idioma, preferencia de tema y tema efectivo de las preferencias
    /// recibidas en la sesión, sin realimentar el evento de preferencias.
    /// </summary>
    private void CopyPreferencesFrom(UiPreferences? preferences)
    {
        if (preferences is null)
        {
            return;
        }

        _syncingPreferences = true;
        try
        {
            _preferences.TrySetLanguage(preferences.Language);
            _preferences.TrySetThemePreference(
                preferences.ThemePreference,
                preferences.Theme);
        }
        finally
        {
            _syncingPreferences = false;
        }
    }

    public void Dispose()
    {
        if (_preferences is not null)
        {
            _preferences.PropertyChanged -= OnPreferencesPropertyChanged;
        }
    }

    public static string ToCode(Language language) => language switch
    {
        EggMdEditor.App.Language.English or EggMdEditor.App.Language.En or EggMdEditor.App.Language.Ingles or EggMdEditor.App.Language.Inglés => "en",
        _ => "es",
    };

    public static string ToCode(AppLanguage language) => language switch
    {
        AppLanguage.English or AppLanguage.En => "en",
        _ => "es",
    };

    public static string ToCode(UiLanguage language) => language switch
    {
        UiLanguage.English or UiLanguage.En => "en",
        _ => "es",
    };

}

public interface ISession
{
    string Language { get; set; }
    string CurrentLanguage { get; set; }
    Theme Theme { get; set; }
    Theme CurrentTheme { get; set; }
    ThemePreference ThemePreference { get; set; }
    ThemePreference CurrentThemePreference { get; set; }
    MenuState MenuState { get; set; }
    bool IsMenuOpen { get; set; }
    int SelectedMenuIndex { get; set; }
    bool NoticeVisible { get; set; }
    bool IsNoticeVisible { get; set; }
    UiPreferences Preferences { get; set; }
    II18nCatalog Catalog { get; set; }

    bool TrySetLanguage(string? language);
    bool TrySetLanguage(Language language);
    void SetLanguage(string language);
    void SetLanguage(Language language);
    void ChangeLanguage(string language);
    void SwitchLanguage(string language);

    bool TrySetTheme(Theme theme);
    bool TrySetTheme(string? theme);
    bool TrySetThemePreference(ThemePreference preference);
    bool TrySetThemePreference(ThemePreference preference, Theme systemTheme);
    bool TrySetThemePreference(string? preference);
    void SetTheme(Theme theme);
    void SetTheme(string theme);
    void ToggleTheme();

    void OpenMenu();
    void CloseMenu();
    void ToggleMenu();
    bool TryOpenMenu();
    bool TryCloseMenu();
    bool TrySelectMenu(int position);
    bool SetMenuState(MenuState state);

    void ShowNotice();
    void HideNotice();
    void ToggleNotice();

    void CloseWindow();
}
