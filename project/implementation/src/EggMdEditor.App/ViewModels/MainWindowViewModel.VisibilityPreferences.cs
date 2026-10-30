using System;
using System.IO;
using EggMdEditor.App.I18n;

namespace EggMdEditor.App;

public partial class MainWindowViewModel
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

    public UiPreferences? LoadedPreferences => _loadedPreferences;
    public UiPreferences? LoadedUiPreferences => _loadedPreferences;
    public UiPreferences? LoadedVisibilityPreferences => _loadedPreferences;

    // splitter width storage (VM-level, for tests)
    private double _splitterWidth = 4.0;
    public double SplitterWidth
    {
        get
        {
            if (Preferences is not null) return Preferences.SplitterWidth;
            if (Session is not null) return Session.SplitterWidth;
            return _splitterWidth;
        }
        set
        {
            _splitterWidth = value;
            if (Preferences is not null) Preferences.SplitterWidth = value;
            if (Session is not null) Session.SplitterWidth = value;
            OnPropertyChanged(nameof(SplitterWidth));
            OnPropertyChanged(nameof(SeparatorWidth));
            OnPropertyChanged(nameof(SplitterSize));
        }
    }
    public double SeparatorWidth { get => SplitterWidth; set => SplitterWidth = value; }
    public double SplitterSize { get => SplitterWidth; set => SplitterWidth = value; }
    public double PersistedWidth { get => SplitterWidth; set => SplitterWidth = value; }
    public double SavedWidth { get => SplitterWidth; set => SplitterWidth = value; }

    public UiPreferences LoadPreferences(string? path = null)
    {
        var resolved = path ?? _preferencesPath;
        UiPreferences loaded;
        try { loaded = UiPreferences.Load(resolved); }
        catch { loaded = new UiPreferences(); }
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
        var def = new UiPreferences();
        ApplyPreferences(def);
        _loadedPreferences = def;
        return false;
    }

    public bool TryLoadPreferences(out UiPreferences? preferences) => TryLoadPreferences(null, out preferences);

    public void ApplyPreferences(UiPreferences? preferences)
    {
        if (preferences is null) return;
        // apply to panels / bar
        try
        {
            LeftPanel.Visible = preferences.LeftPanelVisible;
            LeftPanel.IsVisible = preferences.LeftPanelVisible;
        }
        catch { }
        try
        {
            RightPanel.Visible = preferences.RightPanelVisible;
            RightPanel.IsVisible = preferences.RightPanelVisible;
        }
        catch { }
        try { FormatBarVisible = preferences.FormatBarVisible; }
        catch { }
        try { SplitterWidth = preferences.SplitterWidth; }
        catch { }

        // store
        try { Preferences = preferences; } catch { }
        try { if (Session is not null) Session.ApplyPreferences(preferences); } catch { }
        _loadedPreferences = preferences;
    }

    public void ApplyVisibilityPreferences(UiPreferences? preferences) => ApplyPreferences(preferences);
    public void ApplyUiPreferences(UiPreferences? preferences) => ApplyPreferences(preferences);
    public void ApplyPrefs(UiPreferences? preferences) => ApplyPreferences(preferences);

    public void ApplyPreferences(string? path) => LoadPreferences(path);
    public void ApplyPreferences(UiPreferencesPersistence? store) => LoadPreferences(store);
    public void ApplyPreferences(UiPreferencesStore? store) => LoadPreferences(store);

    // Constructors with preferences path
    public MainWindowViewModel(string? preferencesPath)
        : this()
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
    }

    public MainWindowViewModel(string? preferencesPath, Session? session)
        : this(session, preferencesPath) { }

    public MainWindowViewModel(string? preferencesPath, II18nCatalog? catalog)
        : this(catalog, preferencesPath) { }

    public MainWindowViewModel(string? preferencesPath, UiPreferences? preferences)
        : this(preferences)
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
    }

    public MainWindowViewModel(Session? session, string? preferencesPath)
        : this(session)
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
        else if (session is not null)
        {
            try { ApplyPreferences(session.Preferences); } catch { }
            try { ApplyPreferences(session.LoadedPreferences); } catch { }
        }
    }

    public MainWindowViewModel(II18nCatalog? catalog, string? preferencesPath)
        : this(catalog)
    {
        if (!string.IsNullOrWhiteSpace(preferencesPath)) LoadPreferences(preferencesPath);
    }

    public MainWindowViewModel(Session? session, UiPreferences? preferences)
        : this(session)
    {
        ApplyPreferences(preferences);
    }

    public MainWindowViewModel(II18nCatalog? catalog, UiPreferences? preferences)
        : this(catalog)
    {
        ApplyPreferences(preferences);
    }

    public MainWindowViewModel(UiPreferences? preferences)
        : this()
    {
        ApplyPreferences(preferences);
    }
}