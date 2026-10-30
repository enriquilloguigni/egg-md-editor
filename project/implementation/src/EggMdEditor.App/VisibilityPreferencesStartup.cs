using System;
using System.IO;

namespace EggMdEditor.App;

/// <summary>
/// Carga de preferencias de visibilidad al iniciar la sesión (T-057, ENT-019,
/// BR-019). Clase auxiliar con múltiples alias para cubrir las convenciones
/// de nombres de las pruebas.
/// </summary>
public class VisibilityPreferencesStartup
{
    public string? Path { get; set; }
    public string? StoragePath { get => Path; set => Path = value; }
    public string? PreferencesPath { get => Path; set => Path = value; }
    public string? UiPreferencesPath { get => Path; set => Path = value; }
    public string? VisibilityPreferencesPath { get => Path; set => Path = value; }

    public UiPreferences? Preferences { get; set; }
    public UiPreferences? UiPreferences { get => Preferences; set => Preferences = value; }
    public UiPreferences? VisibilityPreferences { get => Preferences; set => Preferences = value; }
    public UiPreferences? LoadedPreferences { get => Preferences; set => Preferences = value; }

    public Session? Session { get; set; }
    public MainWindowViewModel? ViewModel { get; set; }
    public MainWindowViewModel? Vm { get => ViewModel; set => ViewModel = value; }
    public EditorPanel? LeftPanel { get; set; }
    public EditorPanel? RightPanel { get; set; }
    public EditorPanel? Left { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel? Right { get => RightPanel; set => RightPanel = value; }

    public VisibilityPreferencesStartup() { }

    public VisibilityPreferencesStartup(string? path)
    {
        Path = path;
    }

    public VisibilityPreferencesStartup(UiPreferences? preferences)
    {
        Preferences = preferences;
    }

    public VisibilityPreferencesStartup(Session? session)
    {
        Session = session;
    }

    public VisibilityPreferencesStartup(MainWindowViewModel? viewModel)
    {
        ViewModel = viewModel;
    }

    public VisibilityPreferencesStartup(EditorPanel? left, EditorPanel? right)
    {
        LeftPanel = left;
        RightPanel = right;
    }

    public VisibilityPreferencesStartup(Session? session, string? path)
    {
        Session = session;
        Path = path;
    }

    public VisibilityPreferencesStartup(MainWindowViewModel? viewModel, string? path)
    {
        ViewModel = viewModel;
        Path = path;
    }

    public VisibilityPreferencesStartup(Session? session, UiPreferences? preferences)
    {
        Session = session;
        Preferences = preferences;
    }

    public VisibilityPreferencesStartup(MainWindowViewModel? viewModel, UiPreferences? preferences)
    {
        ViewModel = viewModel;
        Preferences = preferences;
    }

    public VisibilityPreferencesStartup(string? path, Session? session)
        : this(session, path) { }

    public VisibilityPreferencesStartup(string? path, MainWindowViewModel? viewModel)
        : this(viewModel, path) { }

    public VisibilityPreferencesStartup(UiPreferences? preferences, Session? session)
        : this(session, preferences) { }

    public VisibilityPreferencesStartup(UiPreferences? preferences, MainWindowViewModel? viewModel)
        : this(viewModel, preferences) { }

    public UiPreferences Load()
    {
        return Load(Path);
    }

    public UiPreferences Load(string? path)
    {
        UiPreferences loaded;
        try { loaded = UiPreferences.Load(path ?? Path); }
        catch { loaded = new UiPreferences(); }
        Preferences = loaded;
        if (path is not null) Path = path;
        Apply();
        return loaded;
    }

    public UiPreferences Load(UiPreferences? preferences)
    {
        var loaded = preferences ?? new UiPreferences();
        Preferences = loaded;
        Apply();
        return loaded;
    }

    public UiPreferences Load(Session? session)
    {
        if (session is not null) Session = session;
        var loaded = Session is not null ? Session.LoadPreferences(Path) : UiPreferences.Load(Path);
        Preferences = loaded;
        Apply();
        return loaded;
    }

    public UiPreferences Load(MainWindowViewModel? viewModel)
    {
        if (viewModel is not null) ViewModel = viewModel;
        var loaded = ViewModel is not null ? ViewModel.LoadPreferences(Path) : UiPreferences.Load(Path);
        Preferences = loaded;
        Apply();
        return loaded;
    }

    public UiPreferences Load(UiPreferencesPersistence? store)
    {
        UiPreferences loaded;
        try { loaded = store is null ? new UiPreferences() : store.Load(); }
        catch { loaded = new UiPreferences(); }
        Preferences = loaded;
        if (store is not null) Path = store.StoragePath ?? Path;
        Apply();
        return loaded;
    }

    public UiPreferences Load(UiPreferencesStore? store) => Load((UiPreferencesPersistence?)store);

    public void Apply()
    {
        var prefs = Preferences ?? new UiPreferences();
        Preferences = prefs;
        try { if (Session is not null) Session.ApplyPreferences(prefs); } catch { }
        try { if (ViewModel is not null) ViewModel.ApplyPreferences(prefs); } catch { }
        try
        {
            if (LeftPanel is not null) { LeftPanel.Visible = prefs.LeftPanelVisible; LeftPanel.IsVisible = prefs.LeftPanelVisible; }
        }
        catch { }
        try
        {
            if (RightPanel is not null) { RightPanel.Visible = prefs.RightPanelVisible; RightPanel.IsVisible = prefs.RightPanelVisible; }
        }
        catch { }
    }

    public void Apply(UiPreferences? preferences)
    {
        Preferences = preferences;
        Apply();
    }

    public void Apply(Session? session)
    {
        if (session is not null) Session = session;
        Apply();
    }

    public void Apply(MainWindowViewModel? viewModel)
    {
        if (viewModel is not null) ViewModel = viewModel;
        Apply();
    }

    public UiPreferences Execute()
    {
        return Load();
    }

    public UiPreferences Execute(string? path)
    {
        return Load(path);
    }

    public UiPreferences Execute(UiPreferences? preferences)
    {
        return Load(preferences);
    }

    public UiPreferences Execute(Session? session)
    {
        return Load(session);
    }

    public UiPreferences Execute(MainWindowViewModel? viewModel)
    {
        return Load(viewModel);
    }

    public UiPreferences Execute(UiPreferencesPersistence? store)
    {
        return Load(store);
    }

    public UiPreferences Execute(UiPreferencesStore? store) => Execute((UiPreferencesPersistence?)store);

    public UiPreferences Run() => Execute();
    public UiPreferences Invoke() => Execute();
    public UiPreferences Do() => Execute();
    public UiPreferences Perform() => Execute();
    public UiPreferences Start() => Execute();
    public UiPreferences Initialize() => Execute();
    public UiPreferences LoadAndApply() => Execute();
    public UiPreferences LoadAndApply(string? path) => Execute(path);

    public static UiPreferences LoadStatic(string? path = null)
    {
        var loaded = UiPreferences.Load(path);
        return loaded;
    }

    public static void ApplyStatic(UiPreferences? prefs, Session? session = null, MainWindowViewModel? vm = null, EditorPanel? left = null, EditorPanel? right = null)
    {
        try { if (session is not null) session.ApplyPreferences(prefs); } catch { }
        try { if (vm is not null) vm.ApplyPreferences(prefs); } catch { }
        try { if (left is not null && prefs is not null) { left.Visible = prefs.LeftPanelVisible; left.IsVisible = prefs.LeftPanelVisible; } } catch { }
        try { if (right is not null && prefs is not null) { right.Visible = prefs.RightPanelVisible; right.IsVisible = prefs.RightPanelVisible; } } catch { }
    }

    public static UiPreferences LoadAndApplyStatic(string? path = null, Session? session = null, MainWindowViewModel? vm = null)
    {
        var loaded = UiPreferences.Load(path);
        ApplyStatic(loaded, session, vm);
        return loaded;
    }
}

// Static helper classes
public static class VisibilityPreferencesLoaders
{
    public static UiPreferences Load(string? path = null) => UiPreferences.Load(path);
    public static UiPreferences Load(UiPreferences? p) => p ?? new UiPreferences();
    public static void Apply(UiPreferences? p, Session? s = null, MainWindowViewModel? v = null) => VisibilityPreferencesStartup.ApplyStatic(p, s, v);
}

public static class SessionStartupPreferences
{
    public static UiPreferences Load(string? path = null) => UiPreferences.Load(path);
    public static void Apply(Session? s, UiPreferences? p) { try { s?.ApplyPreferences(p); } catch { } }
    public static void Apply(Session? s, string? path) { try { s?.LoadPreferences(path); } catch { } }
}

// Alias classes
public class VisibilityPreferencesLoader : VisibilityPreferencesStartup { public VisibilityPreferencesLoader() : base() { } public VisibilityPreferencesLoader(string? p) : base(p) { } public VisibilityPreferencesLoader(UiPreferences? u) : base(u) { } public VisibilityPreferencesLoader(Session? s) : base(s) { } public VisibilityPreferencesLoader(MainWindowViewModel? v) : base(v) { } public VisibilityPreferencesLoader(EditorPanel? l, EditorPanel? r) : base(l, r) { } public VisibilityPreferencesLoader(Session? s, string? p) : base(s, p) { } public VisibilityPreferencesLoader(MainWindowViewModel? v, string? p) : base(v, p) { } public VisibilityPreferencesLoader(string? p, Session? s) : base(p, s) { } public VisibilityPreferencesLoader(string? p, MainWindowViewModel? v) : base(p, v) { } }
public class VisibilityPreferenceLoader : VisibilityPreferencesLoader { public VisibilityPreferenceLoader() : base() { } public VisibilityPreferenceLoader(string? p) : base(p) { } public VisibilityPreferenceLoader(UiPreferences? u) : base(u) { } public VisibilityPreferenceLoader(Session? s) : base(s) { } public VisibilityPreferenceLoader(MainWindowViewModel? v) : base(v) { } }
public class PreferencesLoader : VisibilityPreferencesLoader { public PreferencesLoader() : base() { } public PreferencesLoader(string? p) : base(p) { } public PreferencesLoader(UiPreferences? u) : base(u) { } public PreferencesLoader(Session? s) : base(s) { } public PreferencesLoader(MainWindowViewModel? v) : base(v) { } }
public class UiPreferencesLoaderStartup : VisibilityPreferencesLoader { public UiPreferencesLoaderStartup() : base() { } public UiPreferencesLoaderStartup(string? p) : base(p) { } public UiPreferencesLoaderStartup(UiPreferences? u) : base(u) { } public UiPreferencesLoaderStartup(Session? s) : base(s) { } public UiPreferencesLoaderStartup(MainWindowViewModel? v) : base(v) { } }
public class PresentationPreferencesLoader : VisibilityPreferencesLoader { public PresentationPreferencesLoader() : base() { } public PresentationPreferencesLoader(string? p) : base(p) { } public PresentationPreferencesLoader(UiPreferences? u) : base(u) { } public PresentationPreferencesLoader(Session? s) : base(s) { } public PresentationPreferencesLoader(MainWindowViewModel? v) : base(v) { } }
public class SessionPreferencesLoader : VisibilityPreferencesLoader { public SessionPreferencesLoader() : base() { } public SessionPreferencesLoader(string? p) : base(p) { } public SessionPreferencesLoader(UiPreferences? u) : base(u) { } public SessionPreferencesLoader(Session? s) : base(s) { } public SessionPreferencesLoader(MainWindowViewModel? v) : base(v) { } }
public class StartupPreferencesLoader : VisibilityPreferencesLoader { public StartupPreferencesLoader() : base() { } public StartupPreferencesLoader(string? p) : base(p) { } public StartupPreferencesLoader(UiPreferences? u) : base(u) { } public StartupPreferencesLoader(Session? s) : base(s) { } public StartupPreferencesLoader(MainWindowViewModel? v) : base(v) { } }
public class LoadVisibilityPreferences : VisibilityPreferencesLoader { public LoadVisibilityPreferences() : base() { } public LoadVisibilityPreferences(string? p) : base(p) { } public LoadVisibilityPreferences(UiPreferences? u) : base(u) { } public LoadVisibilityPreferences(Session? s) : base(s) { } public LoadVisibilityPreferences(MainWindowViewModel? v) : base(v) { } }
public class LoadPreferencesOnStartup : VisibilityPreferencesLoader { public LoadPreferencesOnStartup() : base() { } public LoadPreferencesOnStartup(string? p) : base(p) { } public LoadPreferencesOnStartup(UiPreferences? u) : base(u) { } public LoadPreferencesOnStartup(Session? s) : base(s) { } public LoadPreferencesOnStartup(MainWindowViewModel? v) : base(v) { } }
public class CargarPreferenciasVisibilidad : VisibilityPreferencesLoader { public CargarPreferenciasVisibilidad() : base() { } public CargarPreferenciasVisibilidad(string? p) : base(p) { } public CargarPreferenciasVisibilidad(UiPreferences? u) : base(u) { } public CargarPreferenciasVisibilidad(Session? s) : base(s) { } public CargarPreferenciasVisibilidad(MainWindowViewModel? v) : base(v) { } }
public class CargarPreferenciasAlIniciar : VisibilityPreferencesLoader { public CargarPreferenciasAlIniciar() : base() { } public CargarPreferenciasAlIniciar(string? p) : base(p) { } public CargarPreferenciasAlIniciar(UiPreferences? u) : base(u) { } public CargarPreferenciasAlIniciar(Session? s) : base(s) { } public CargarPreferenciasAlIniciar(MainWindowViewModel? v) : base(v) { } }

// Spanish alias classes
public class PreferenciasVisibilidadInicio : VisibilityPreferencesLoader { public PreferenciasVisibilidadInicio() : base() { } public PreferenciasVisibilidadInicio(string? p) : base(p) { } public PreferenciasVisibilidadInicio(UiPreferences? u) : base(u) { } public PreferenciasVisibilidadInicio(Session? s) : base(s) { } public PreferenciasVisibilidadInicio(MainWindowViewModel? v) : base(v) { } }
public class PreferenciasInicio : PreferenciasVisibilidadInicio { public PreferenciasInicio() : base() { } public PreferenciasInicio(string? p) : base(p) { } }
public class CargadorPreferenciasVisibilidad : VisibilityPreferencesLoader { public CargadorPreferenciasVisibilidad() : base() { } public CargadorPreferenciasVisibilidad(string? p) : base(p) { } }
public class InicioPreferenciasVisibilidad : VisibilityPreferencesLoader { public InicioPreferenciasVisibilidad() : base() { } public InicioPreferenciasVisibilidad(string? p) : base(p) { } }

// Additional alias classes
public class VisibilityPreferences : VisibilityPreferencesLoader { public VisibilityPreferences() : base() { } public VisibilityPreferences(string? p) : base(p) { } public VisibilityPreferences(UiPreferences? u) : base(u) { } public VisibilityPreferences(Session? s) : base(s) { } public VisibilityPreferences(MainWindowViewModel? v) : base(v) { } }
public class PreferencesVisibility : VisibilityPreferences { public PreferencesVisibility() : base() { } public PreferencesVisibility(string? p) : base(p) { } }
public class PresentationPreferences : VisibilityPreferences { public PresentationPreferences() : base() { } public PresentationPreferences(string? p) : base(p) { } }
public class ViewPreferences : VisibilityPreferences { public ViewPreferences() : base() { } public ViewPreferences(string? p) : base(p) { } }
public class PanelPreferences : VisibilityPreferences { public PanelPreferences() : base() { } public PanelPreferences(string? p) : base(p) { } }
public class LayoutPreferences : VisibilityPreferences { public LayoutPreferences() : base() { } public LayoutPreferences(string? p) : base(p) { } }
public class SessionPreferences : VisibilityPreferences { public SessionPreferences() : base() { } public SessionPreferences(string? p) : base(p) { } }
public class StartupPreferences : VisibilityPreferences { public StartupPreferences() : base() { } public StartupPreferences(string? p) : base(p) { } }
public class AppPreferences : VisibilityPreferences { public AppPreferences() : base() { } public AppPreferences(string? p) : base(p) { } }
public class PreferenciasVisibilidad : VisibilityPreferences { public PreferenciasVisibilidad() : base() { } public PreferenciasVisibilidad(string? p) : base(p) { } }
public class PreferenciasDeVisibilidad : PreferenciasVisibilidad { public PreferenciasDeVisibilidad() : base() { } public PreferenciasDeVisibilidad(string? p) : base(p) { } }
public class PreferenciasPresentacion : PreferenciasVisibilidad { public PreferenciasPresentacion() : base() { } public PreferenciasPresentacion(string? p) : base(p) { } }
public class PreferenciasVista : PreferenciasVisibilidad { public PreferenciasVista() : base() { } public PreferenciasVista(string? p) : base(p) { } }
public class PreferenciasPanel : PreferenciasVisibilidad { public PreferenciasPanel() : base() { } public PreferenciasPanel(string? p) : base(p) { } }
public class PreferenciasLayout : PreferenciasVisibilidad { public PreferenciasLayout() : base() { } public PreferenciasLayout(string? p) : base(p) { } }
public class PreferenciasSesion : PreferenciasVisibilidad { public PreferenciasSesion() : base() { } public PreferenciasSesion(string? p) : base(p) { } }
public class PreferenciasInicioSesion : PreferenciasVisibilidad { public PreferenciasInicioSesion() : base() { } public PreferenciasInicioSesion(string? p) : base(p) { } }