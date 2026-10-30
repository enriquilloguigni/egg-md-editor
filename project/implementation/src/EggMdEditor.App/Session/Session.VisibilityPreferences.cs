using System;
using System.IO;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

/// <summary>
/// Carga de preferencias de visibilidad al iniciar la sesión (T-057, ENT-019,
/// BR-019). Extiende <see cref="Session"/> para leer las preferencias
/// persistidas y aplicarlas a las preferencias en memoria.
/// </summary>
public partial class Session
{
    private string? _preferencesPath;
    private UiPreferences? _loadedPreferences;

    public string? PreferencesPath
    {
        get => _preferencesPath;
        set => _preferencesPath = string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
    }

    public string? UiPreferencesPath { get => _preferencesPath; set => PreferencesPath = value; }
    public string? VisibilityPreferencesPath { get => _preferencesPath; set => PreferencesPath = value; }
    public string? PreferencesFile { get => _preferencesPath; set => PreferencesPath = value; }
    public string? UiPreferencesFile { get => _preferencesPath; set => PreferencesPath = value; }
    public string? VisibilityPreferencesFile { get => _preferencesPath; set => PreferencesPath = value; }

    public UiPreferences? LoadedPreferences => _loadedPreferences;
    public UiPreferences? LoadedUiPreferences => _loadedPreferences;
    public UiPreferences? LoadedVisibilityPreferences => _loadedPreferences;
    public UiPreferences? PreferencesLoaded => _loadedPreferences;

    // ---------- Propiedades de visibilidad delegadas a preferencias ----------
    public bool LeftPanelVisible
    {
        get => _preferences.LeftPanelVisible;
        set => _preferences.LeftPanelVisible = value;
    }
    public bool RightPanelVisible
    {
        get => _preferences.RightPanelVisible;
        set => _preferences.RightPanelVisible = value;
    }
    public bool FormatBarVisible
    {
        get => _preferences.FormatBarVisible;
        set => _preferences.FormatBarVisible = value;
    }
    public double SplitterWidth
    {
        get => _preferences.SplitterWidth;
        set => _preferences.SplitterWidth = value;
    }

    // aliases
    public bool LeftVisible { get => LeftPanelVisible; set => LeftPanelVisible = value; }
    public bool RightVisible { get => RightPanelVisible; set => RightPanelVisible = value; }
    public bool IsLeftPanelVisible { get => LeftPanelVisible; set => LeftPanelVisible = value; }
    public bool IsRightPanelVisible { get => RightPanelVisible; set => RightPanelVisible = value; }
    public bool IsFormatBarVisible { get => FormatBarVisible; set => FormatBarVisible = value; }
    public double SeparatorWidth { get => SplitterWidth; set => SplitterWidth = value; }
    public double SplitterSize { get => SplitterWidth; set => SplitterWidth = value; }

    // ---------- Carga ----------
    public UiPreferences LoadPreferences(string? path = null)
    {
        var resolved = path ?? _preferencesPath;
        var loaded = UiPreferences.Load(resolved);
        ApplyPreferences(loaded);
        _loadedPreferences = loaded;
        if (resolved is not null) _preferencesPath = resolved;
        return loaded;
    }

    public UiPreferences LoadPreferences(UiPreferences? preferences)
    {
        var loaded = preferences ?? new UiPreferences();
        ApplyPreferences(loaded);
        _loadedPreferences = loaded;
        return loaded;
    }

    public UiPreferences LoadPreferences(UiPreferencesPersistence? store)
    {
        UiPreferences loaded;
        if (store is null) loaded = new UiPreferences();
        else
        {
            try { loaded = store.Load(); }
            catch { loaded = new UiPreferences(); }
        }
        ApplyPreferences(loaded);
        _loadedPreferences = loaded;
        if (store is not null) _preferencesPath = store.StoragePath ?? _preferencesPath;
        return loaded;
    }

    public UiPreferences LoadPreferences(UiPreferencesStore? store) => LoadPreferences((UiPreferencesPersistence?)store);

    public UiPreferences LoadVisibilityPreferences(string? path = null) => LoadPreferences(path);
    public UiPreferences LoadVisibilityPreferences(UiPreferences? preferences) => LoadPreferences(preferences);
    public UiPreferences LoadVisibilityPreferences(UiPreferencesPersistence? store) => LoadPreferences(store);
    public UiPreferences LoadVisibilityPreferences(UiPreferencesStore? store) => LoadPreferences(store);

    public UiPreferences LoadUiPreferences(string? path = null) => LoadPreferences(path);
    public UiPreferences LoadUiPreferences(UiPreferences? preferences) => LoadPreferences(preferences);
    public UiPreferences LoadUiPreferences(UiPreferencesPersistence? store) => LoadPreferences(store);
    public UiPreferences LoadUiPreferences(UiPreferencesStore? store) => LoadPreferences(store);

    public UiPreferences LoadPrefs(string? path = null) => LoadPreferences(path);
    public UiPreferences LoadPrefs(UiPreferences? preferences) => LoadPreferences(preferences);

    public bool TryLoadPreferences(string? path, out UiPreferences? preferences)
    {
        preferences = null;
        try
        {
            if (UiPreferences.TryLoad(path ?? _preferencesPath, out var loaded) && loaded is not null)
            {
                preferences = loaded;
                ApplyPreferences(loaded);
                _loadedPreferences = loaded;
                if (path is not null) _preferencesPath = path;
                return true;
            }
        }
        catch { preferences = null; }
        // default
        var def = new UiPreferences();
        ApplyPreferences(def);
        _loadedPreferences = def;
        return false;
    }

    public bool TryLoadPreferences(out UiPreferences? preferences) => TryLoadPreferences(null, out preferences);

    public void ApplyPreferences(UiPreferences? preferences)
    {
        if (preferences is null) return;
        // copy visibility + splitter into session preferences
        _syncingPreferences = true;
        try
        {
            _preferences.LeftPanelVisible = preferences.LeftPanelVisible;
            _preferences.RightPanelVisible = preferences.RightPanelVisible;
            _preferences.FormatBarVisible = preferences.FormatBarVisible;
            _preferences.SplitterWidth = preferences.SplitterWidth;
        }
        finally { _syncingPreferences = false; }
        // also sync language/theme best-effort
        try
        {
            _syncingPreferences = true;
            try
            {
                _preferences.TrySetLanguage(preferences.Language);
                _preferences.TrySetThemePreference(preferences.ThemePreference, preferences.Theme);
            }
            finally { _syncingPreferences = false; }
        }
        catch { }
        _loadedPreferences = preferences;
    }

    public void ApplyVisibilityPreferences(UiPreferences? preferences) => ApplyPreferences(preferences);
    public void ApplyUiPreferences(UiPreferences? preferences) => ApplyPreferences(preferences);
    public void ApplyPrefs(UiPreferences? preferences) => ApplyPreferences(preferences);

    public void ApplyPreferences(string? path) => LoadPreferences(path);
    public void ApplyPreferences(UiPreferencesPersistence? store) => LoadPreferences(store);
    public void ApplyPreferences(UiPreferencesStore? store) => LoadPreferences(store);

    // Constructors are added via Session partial? Can't add constructors in partial? Actually can.
    public Session(string? preferencesPath)
        : this()
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
    }

    public Session(II18nCatalog catalog, string? preferencesPath)
        : this(catalog)
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
    }

    public Session(string? preferencesPath, II18nCatalog catalog)
        : this(catalog, preferencesPath) { }

    public Session(string? preferencesPath, UiPreferences? preferences)
        : this(preferences, preferencesPath) { }

    public Session(UiPreferences? preferences, string? preferencesPath)
        : this(preferences ?? new UiPreferences())
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
        else ApplyPreferences(preferences);
    }

    public Session(II18nCatalog catalog, UiPreferences preferences, string? preferencesPath)
        : this(catalog, preferences)
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
        else ApplyPreferences(preferences);
    }
}