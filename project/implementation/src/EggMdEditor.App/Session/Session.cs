using System;
using System.ComponentModel;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

/// <summary>
/// Sesión de aplicación en memoria (ENT-001): idioma, tema, estado de menú y aviso.
/// Singleton por proceso; se libera al cerrar.
/// </summary>
public class Session : INotifyPropertyChanged, ISession
{
    private static readonly Lazy<Session> _instance = new(() => new Session());

    private string _language;
    private Theme _theme;
    private MenuState _menuState;
    private bool _noticeVisible;
    private bool _windowOpen;
    private II18nCatalog _catalog;
    private readonly UiPreferences _preferences;

    public Session()
        : this(I18nCatalog.Instance, "es", Theme.Light, MenuState.Closed, false, true)
    {
    }

    public Session(II18nCatalog catalog)
        : this(catalog, catalog?.CurrentLanguage ?? "es", Theme.Light, MenuState.Closed, false, true)
    {
    }

    public Session(UiPreferences preferences)
        : this(I18nCatalog.Instance, preferences?.Language ?? "es", preferences?.Theme ?? Theme.Light, MenuState.Closed, false, true)
    {
        if (preferences is not null)
        {
            _preferences.Language = preferences.Language;
            _preferences.Theme = preferences.Theme;
        }
    }

    public Session(II18nCatalog catalog, UiPreferences preferences)
        : this(catalog, preferences?.Language ?? catalog?.CurrentLanguage ?? "es", preferences?.Theme ?? Theme.Light, MenuState.Closed, false, true)
    {
        if (preferences is not null)
        {
            _preferences.Language = preferences.Language;
            _preferences.Theme = preferences.Theme;
        }
    }

    public Session(II18nCatalog catalog, string language, Theme theme, MenuState menuState, bool noticeVisible, bool windowOpen = true)
    {
        _catalog = catalog ?? I18nCatalog.Instance;
        _language = UiPreferences.NormalizeLanguage(language);
        _theme = theme;
        _menuState = menuState;
        _noticeVisible = noticeVisible;
        _windowOpen = windowOpen;
        _preferences = new UiPreferences(_language, _theme);

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

    public UiPreferences Preferences => _preferences;

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
            _menuState = value;
            OnPropertyChanged(nameof(MenuState));
            OnPropertyChanged(nameof(CurrentMenuState));
            OnPropertyChanged(nameof(EstadoMenu));
            OnPropertyChanged(nameof(IsMenuOpen));
            OnPropertyChanged(nameof(IsMenuOpen));
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
            _preferences.TrySetLanguage(code);
            return true;
        }

        _language = code;
        _preferences.TrySetLanguage(code);
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(ActiveLanguage));
        OnPropertyChanged(nameof(LanguageCode));
        OnPropertyChanged(nameof(Idioma));
        return true;
    }

    public bool TrySetLanguage(Language language) => TrySetLanguage(ToCode(language));

    public void SetLanguage(string language)
    {
        if (!TrySetLanguage(language))
        {
            throw new InvalidOperationException($"No se pudo cambiar el idioma a '{language}'.");
        }
    }

    public void SetLanguage(Language language) => SetLanguage(ToCode(language));

    public void ChangeLanguage(string language) => SetLanguage(language);
    public void ChangeLanguage(Language language) => SetLanguage(language);

    public void SwitchLanguage(string language) => SetLanguage(language);
    public void SwitchLanguage(Language language) => SetLanguage(language);

    public bool ChangeLanguageSafe(string language) => TrySetLanguage(language);

    // Theme operations
    public bool TrySetTheme(Theme theme)
    {
        if (_theme == theme)
        {
            _preferences.TrySetTheme(theme);
            return true;
        }

        _theme = theme;
        _preferences.TrySetTheme(theme);
        OnPropertyChanged(nameof(Theme));
        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(ActiveTheme));
        OnPropertyChanged(nameof(Tema));
        return true;
    }

    public bool TrySetTheme(string? theme)
    {
        if (!UiPreferences.TryParseTheme(theme, out var parsed))
        {
            return false;
        }

        return TrySetTheme(parsed);
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

    public bool OpenMenuEntry() { OpenMenu(); return true; }
    public bool CloseMenuEntry() { CloseMenu(); return true; }

    public bool SetMenuState(MenuState state)
    {
        MenuState = state;
        return true;
    }

    public bool TrySetMenuState(MenuState state)
    {
        MenuState = state;
        return true;
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

    public bool SetAviso(bool visible) => SetNotice(visible);
    public void MostrarAviso() => ShowNotice();
    public void OcultarAviso() => HideNotice();

    // Window operations
    public void CloseWindow()
    {
        _windowOpen = false;
        OnPropertyChanged(nameof(WindowOpen));
        OnPropertyChanged(nameof(IsWindowOpen));
    }

    public void RequestFocus() { /* no-op in memory */ }

    public Session Clone()
    {
        return new Session(_catalog, _language, _theme, _menuState, _noticeVisible, _windowOpen);
    }

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
    MenuState MenuState { get; set; }
    bool IsMenuOpen { get; set; }
    bool NoticeVisible { get; set; }
    bool IsNoticeVisible { get; set; }
    UiPreferences Preferences { get; }
    II18nCatalog Catalog { get; set; }

    bool TrySetLanguage(string? language);
    bool TrySetLanguage(Language language);
    void SetLanguage(string language);
    void SetLanguage(Language language);
    void ChangeLanguage(string language);
    void SwitchLanguage(string language);

    bool TrySetTheme(Theme theme);
    bool TrySetTheme(string? theme);
    void SetTheme(Theme theme);
    void SetTheme(string theme);
    void ToggleTheme();

    void OpenMenu();
    void CloseMenu();
    void ToggleMenu();
    bool SetMenuState(MenuState state);

    void ShowNotice();
    void HideNotice();
    void ToggleNotice();

    void CloseWindow();
}
