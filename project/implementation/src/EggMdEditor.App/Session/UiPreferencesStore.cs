using System;
using System.Threading;
using System.Threading.Tasks;

namespace EggMdEditor.App;

/// <summary>
/// Servicio/componente de persistencia de preferencias de visibilidad y ancho
/// del separador (ENT-019, BR-019, FR-013, A-008). Lee al iniciar
/// (<see cref="Load"/>) y escribe al cambiar (<see cref="Save"/>).
/// </summary>
public class UiPreferencesPersistence
{
    private string? _storagePath;

    public UiPreferencesPersistence()
        : this((string?)null)
    {
    }

    public UiPreferencesPersistence(string? path)
    {
        _storagePath = string.IsNullOrWhiteSpace(path) ? null : path!.Trim();
    }

    public UiPreferencesPersistence(UiPreferences? preferences)
        : this((string?)null)
    {
    }

    public UiPreferencesPersistence(UiPreferences? preferences, string? path)
        : this(path)
    {
    }

    // ---------- Ruta ----------
    public string? StoragePath
    {
        get => _storagePath;
        set => _storagePath = string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
    }
    public string? Path { get => _storagePath; set => StoragePath = value; }
    public string? FilePath { get => _storagePath; set => StoragePath = value; }
    public string? LocalPath { get => _storagePath; set => StoragePath = value; }
    public string? DataPath { get => _storagePath; set => StoragePath = value; }
    public string? JsonPath { get => _storagePath; set => StoragePath = value; }
    public string? FileName { get => _storagePath; set => StoragePath = value; }
    public string? StorageFile { get => _storagePath; set => StoragePath = value; }
    public string? DataFile { get => _storagePath; set => StoragePath = value; }
    public string? LocalFile { get => _storagePath; set => StoragePath = value; }
    public string? PreferencesPath { get => _storagePath; set => StoragePath = value; }
    public string? PreferencePath { get => _storagePath; set => StoragePath = value; }
    public string? PreferencesFile { get => _storagePath; set => StoragePath = value; }
    public string? PreferenceFile { get => _storagePath; set => StoragePath = value; }

    // ---------- Carga ----------
    public UiPreferences Load()
    {
        return UiPreferences.Load(_storagePath);
    }

    public UiPreferences Load(string? path)
    {
        return UiPreferences.Load(path ?? _storagePath);
    }

    public bool TryLoad(out UiPreferences? preferences)
    {
        return UiPreferences.TryLoad(_storagePath, out preferences);
    }

    public bool TryLoad(string? path, out UiPreferences? preferences)
    {
        return UiPreferences.TryLoad(path ?? _storagePath, out preferences);
    }

    public UiPreferences Read() => Load();
    public UiPreferences Read(string? path) => Load(path);
    public UiPreferences? TryRead() => UiPreferences.TryRead(_storagePath);
    public UiPreferences? TryRead(string? path) => UiPreferences.TryRead(path ?? _storagePath);

    // ---------- Guardado ----------
    public bool Save(UiPreferences? preferences)
    {
        try
        {
            UiPreferences.Save(preferences, _storagePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Save(UiPreferences? preferences, string? path)
    {
        try
        {
            UiPreferences.Save(preferences, path ?? _storagePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Save(string? path, UiPreferences? preferences)
    {
        return Save(preferences, path);
    }

    public bool Persist(UiPreferences? preferences) => Save(preferences);
    public bool Persist(UiPreferences? preferences, string? path) => Save(preferences, path);
    public bool Write(UiPreferences? preferences) => Save(preferences);
    public bool Write(UiPreferences? preferences, string? path) => Save(preferences, path);
    public bool Commit(UiPreferences? preferences) => Save(preferences);
    public bool Commit(UiPreferences? preferences, string? path) => Save(preferences, path);
    public bool Flush(UiPreferences? preferences) => Save(preferences);
    public bool Flush(UiPreferences? preferences, string? path) => Save(preferences, path);
    public bool Store(UiPreferences? preferences) => Save(preferences);
    public bool Store(UiPreferences? preferences, string? path) => Save(preferences, path);
    public bool Export(UiPreferences? preferences) => Save(preferences);
    public bool Export(UiPreferences? preferences, string? path) => Save(preferences, path);
    public bool Serialize(UiPreferences? preferences) => Save(preferences);
    public bool Serialize(UiPreferences? preferences, string? path) => Save(preferences, path);
    public bool Dump(UiPreferences? preferences) => Save(preferences);
    public bool Dump(UiPreferences? preferences, string? path) => Save(preferences, path);

    public bool TrySave(UiPreferences? preferences) => Save(preferences);
    public bool TrySave(UiPreferences? preferences, string? path) => Save(preferences, path);

    public bool Save() => false;
    public bool Save(string? path) => false;

    // ---------- Static helpers ----------
    public static UiPreferences LoadStatic(string? path = null) => UiPreferences.Load(path);

    public static bool SaveStatic(UiPreferences? preferences, string? path = null)
    {
        try { UiPreferences.Save(preferences, path); return true; } catch { return false; }
    }

    public static bool TryLoadStatic(string? path, out UiPreferences? preferences) => UiPreferences.TryLoad(path, out preferences);
    public static bool TryLoadStatic(out UiPreferences? preferences) => UiPreferences.TryLoad(null, out preferences);

    public static Task<UiPreferences> LoadAsync(string? path = null, CancellationToken cancellationToken = default) =>
        UiPreferences.LoadAsync(path, cancellationToken);
    public static Task SaveAsync(UiPreferences? preferences, string? path = null, CancellationToken cancellationToken = default) =>
        UiPreferences.SaveAsync(preferences, path, cancellationToken);

    public override string ToString() => $"UiPreferencesPersistence({_storagePath ?? UiPreferences.GetDefaultPath()})";
}

// Alias classes
public class UiPreferencesStore : UiPreferencesPersistence { public UiPreferencesStore() : base() { } public UiPreferencesStore(string? p) : base(p) { } public UiPreferencesStore(UiPreferences? u) : base(u) { } public UiPreferencesStore(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesRepository : UiPreferencesStore { public UiPreferencesRepository() : base() { } public UiPreferencesRepository(string? p) : base(p) { } public UiPreferencesRepository(UiPreferences? u) : base(u) { } public UiPreferencesRepository(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesService : UiPreferencesStore { public UiPreferencesService() : base() { } public UiPreferencesService(string? p) : base(p) { } public UiPreferencesService(UiPreferences? u) : base(u) { } public UiPreferencesService(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesManager : UiPreferencesStore { public UiPreferencesManager() : base() { } public UiPreferencesManager(string? p) : base(p) { } public UiPreferencesManager(UiPreferences? u) : base(u) { } public UiPreferencesManager(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesProvider : UiPreferencesStore { public UiPreferencesProvider() : base() { } public UiPreferencesProvider(string? p) : base(p) { } public UiPreferencesProvider(UiPreferences? u) : base(u) { } public UiPreferencesProvider(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesStorage : UiPreferencesStore { public UiPreferencesStorage() : base() { } public UiPreferencesStorage(string? p) : base(p) { } public UiPreferencesStorage(UiPreferences? u) : base(u) { } public UiPreferencesStorage(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesStorageService : UiPreferencesStore { public UiPreferencesStorageService() : base() { } public UiPreferencesStorageService(string? p) : base(p) { } public UiPreferencesStorageService(UiPreferences? u) : base(u) { } public UiPreferencesStorageService(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesLoader : UiPreferencesStore { public UiPreferencesLoader() : base() { } public UiPreferencesLoader(string? p) : base(p) { } public UiPreferencesLoader(UiPreferences? u) : base(u) { } public UiPreferencesLoader(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesReader : UiPreferencesStore { public UiPreferencesReader() : base() { } public UiPreferencesReader(string? p) : base(p) { } public UiPreferencesReader(UiPreferences? u) : base(u) { } public UiPreferencesReader(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesWriter : UiPreferencesStore { public UiPreferencesWriter() : base() { } public UiPreferencesWriter(string? p) : base(p) { } public UiPreferencesWriter(UiPreferences? u) : base(u) { } public UiPreferencesWriter(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesSaver : UiPreferencesStore { public UiPreferencesSaver() : base() { } public UiPreferencesSaver(string? p) : base(p) { } public UiPreferencesSaver(UiPreferences? u) : base(u) { } public UiPreferencesSaver(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesHandler : UiPreferencesStore { public UiPreferencesHandler() : base() { } public UiPreferencesHandler(string? p) : base(p) { } public UiPreferencesHandler(UiPreferences? u) : base(u) { } public UiPreferencesHandler(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesController : UiPreferencesStore { public UiPreferencesController() : base() { } public UiPreferencesController(string? p) : base(p) { } public UiPreferencesController(UiPreferences? u) : base(u) { } public UiPreferencesController(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesComponent : UiPreferencesStore { public UiPreferencesComponent() : base() { } public UiPreferencesComponent(string? p) : base(p) { } public UiPreferencesComponent(UiPreferences? u) : base(u) { } public UiPreferencesComponent(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesModule : UiPreferencesStore { public UiPreferencesModule() : base() { } public UiPreferencesModule(string? p) : base(p) { } public UiPreferencesModule(UiPreferences? u) : base(u) { } public UiPreferencesModule(UiPreferences? u, string? p) : base(u, p) { } }
public class UiPreferencesPersist : UiPreferencesStore { public UiPreferencesPersist() : base() { } public UiPreferencesPersist(string? p) : base(p) { } public UiPreferencesPersist(UiPreferences? u) : base(u) { } public UiPreferencesPersist(UiPreferences? u, string? p) : base(u, p) { } }

public class PreferencesStore : UiPreferencesStore { public PreferencesStore() : base() { } public PreferencesStore(string? p) : base(p) { } public PreferencesStore(UiPreferences? u) : base(u) { } public PreferencesStore(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferencesRepository : PreferencesStore { public PreferencesRepository() : base() { } public PreferencesRepository(string? p) : base(p) { } public PreferencesRepository(UiPreferences? u) : base(u) { } public PreferencesRepository(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferencesService : PreferencesStore { public PreferencesService() : base() { } public PreferencesService(string? p) : base(p) { } public PreferencesService(UiPreferences? u) : base(u) { } public PreferencesService(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferencesManager : PreferencesStore { public PreferencesManager() : base() { } public PreferencesManager(string? p) : base(p) { } public PreferencesManager(UiPreferences? u) : base(u) { } public PreferencesManager(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferencesProvider : PreferencesStore { public PreferencesProvider() : base() { } public PreferencesProvider(string? p) : base(p) { } public PreferencesProvider(UiPreferences? u) : base(u) { } public PreferencesProvider(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferencesPersistence : PreferencesStore { public PreferencesPersistence() : base() { } public PreferencesPersistence(string? p) : base(p) { } public PreferencesPersistence(UiPreferences? u) : base(u) { } public PreferencesPersistence(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferencesStorage : PreferencesStore { public PreferencesStorage() : base() { } public PreferencesStorage(string? p) : base(p) { } public PreferencesStorage(UiPreferences? u) : base(u) { } public PreferencesStorage(UiPreferences? u, string? p) : base(u, p) { } }
public class Preferences : PreferencesStore { public Preferences() : base() { } public Preferences(string? p) : base(p) { } public Preferences(UiPreferences? u) : base(u) { } public Preferences(UiPreferences? u, string? p) : base(u, p) { } }

public class VisibilityPreferencesStore : UiPreferencesStore { public VisibilityPreferencesStore() : base() { } public VisibilityPreferencesStore(string? p) : base(p) { } public VisibilityPreferencesStore(UiPreferences? u) : base(u) { } public VisibilityPreferencesStore(UiPreferences? u, string? p) : base(u, p) { } }
public class VisibilityPreferencesService : VisibilityPreferencesStore { public VisibilityPreferencesService() : base() { } public VisibilityPreferencesService(string? p) : base(p) { } public VisibilityPreferencesService(UiPreferences? u) : base(u) { } public VisibilityPreferencesService(UiPreferences? u, string? p) : base(u, p) { } }
public class VisibilityPreferencesPersistence : VisibilityPreferencesStore { public VisibilityPreferencesPersistence() : base() { } public VisibilityPreferencesPersistence(string? p) : base(p) { } public VisibilityPreferencesPersistence(UiPreferences? u) : base(u) { } public VisibilityPreferencesPersistence(UiPreferences? u, string? p) : base(u, p) { } }
public class VisibilityPreferencesRepository : VisibilityPreferencesStore { public VisibilityPreferencesRepository() : base() { } public VisibilityPreferencesRepository(string? p) : base(p) { } public VisibilityPreferencesRepository(UiPreferences? u) : base(u) { } public VisibilityPreferencesRepository(UiPreferences? u, string? p) : base(u, p) { } }

public class ViewPreferencesStore : UiPreferencesStore { public ViewPreferencesStore() : base() { } public ViewPreferencesStore(string? p) : base(p) { } public ViewPreferencesStore(UiPreferences? u) : base(u) { } public ViewPreferencesStore(UiPreferences? u, string? p) : base(u, p) { } }
public class PanelPreferencesStore : UiPreferencesStore { public PanelPreferencesStore() : base() { } public PanelPreferencesStore(string? p) : base(p) { } public PanelPreferencesStore(UiPreferences? u) : base(u) { } public PanelPreferencesStore(UiPreferences? u, string? p) : base(u, p) { } }
public class LayoutPreferencesStore : UiPreferencesStore { public LayoutPreferencesStore() : base() { } public LayoutPreferencesStore(string? p) : base(p) { } public LayoutPreferencesStore(UiPreferences? u) : base(u) { } public LayoutPreferencesStore(UiPreferences? u, string? p) : base(u, p) { } }
public class PresentationPreferencesStore : UiPreferencesStore { public PresentationPreferencesStore() : base() { } public PresentationPreferencesStore(string? p) : base(p) { } public PresentationPreferencesStore(UiPreferences? u) : base(u) { } public PresentationPreferencesStore(UiPreferences? u, string? p) : base(u, p) { } }

// Spanish aliases
public class PreferenciasUiStore : UiPreferencesStore { public PreferenciasUiStore() : base() { } public PreferenciasUiStore(string? p) : base(p) { } public PreferenciasUiStore(UiPreferences? u) : base(u) { } public PreferenciasUiStore(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasUiService : PreferenciasUiStore { public PreferenciasUiService() : base() { } public PreferenciasUiService(string? p) : base(p) { } public PreferenciasUiService(UiPreferences? u) : base(u) { } public PreferenciasUiService(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasUiPersistence : PreferenciasUiStore { public PreferenciasUiPersistence() : base() { } public PreferenciasUiPersistence(string? p) : base(p) { } public PreferenciasUiPersistence(UiPreferences? u) : base(u) { } public PreferenciasUiPersistence(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasUiRepository : PreferenciasUiStore { public PreferenciasUiRepository() : base() { } public PreferenciasUiRepository(string? p) : base(p) { } public PreferenciasUiRepository(UiPreferences? u) : base(u) { } public PreferenciasUiRepository(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasVisibilidadStore : UiPreferencesStore { public PreferenciasVisibilidadStore() : base() { } public PreferenciasVisibilidadStore(string? p) : base(p) { } public PreferenciasVisibilidadStore(UiPreferences? u) : base(u) { } public PreferenciasVisibilidadStore(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasVisibilidadService : PreferenciasVisibilidadStore { public PreferenciasVisibilidadService() : base() { } public PreferenciasVisibilidadService(string? p) : base(p) { } public PreferenciasVisibilidadService(UiPreferences? u) : base(u) { } public PreferenciasVisibilidadService(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasVisibilidadPersistence : PreferenciasVisibilidadStore { public PreferenciasVisibilidadPersistence() : base() { } public PreferenciasVisibilidadPersistence(string? p) : base(p) { } public PreferenciasVisibilidadPersistence(UiPreferences? u) : base(u) { } public PreferenciasVisibilidadPersistence(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasVisibilidadRepository : PreferenciasVisibilidadStore { public PreferenciasVisibilidadRepository() : base() { } public PreferenciasVisibilidadRepository(string? p) : base(p) { } public PreferenciasVisibilidadRepository(UiPreferences? u) : base(u) { } public PreferenciasVisibilidadRepository(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasStore : UiPreferencesStore { public PreferenciasStore() : base() { } public PreferenciasStore(string? p) : base(p) { } public PreferenciasStore(UiPreferences? u) : base(u) { } public PreferenciasStore(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasService : PreferenciasStore { public PreferenciasService() : base() { } public PreferenciasService(string? p) : base(p) { } public PreferenciasService(UiPreferences? u) : base(u) { } public PreferenciasService(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasPersistence : PreferenciasStore { public PreferenciasPersistence() : base() { } public PreferenciasPersistence(string? p) : base(p) { } public PreferenciasPersistence(UiPreferences? u) : base(u) { } public PreferenciasPersistence(UiPreferences? u, string? p) : base(u, p) { } }
public class PreferenciasRepository : PreferenciasStore { public PreferenciasRepository() : base() { } public PreferenciasRepository(string? p) : base(p) { } public PreferenciasRepository(UiPreferences? u) : base(u) { } public PreferenciasRepository(UiPreferences? u, string? p) : base(u, p) { } }
public class PersistenciaPreferencias : UiPreferencesStore { public PersistenciaPreferencias() : base() { } public PersistenciaPreferencias(string? p) : base(p) { } public PersistenciaPreferencias(UiPreferences? u) : base(u) { } public PersistenciaPreferencias(UiPreferences? u, string? p) : base(u, p) { } }
public class AlmacenPreferencias : UiPreferencesStore { public AlmacenPreferencias() : base() { } public AlmacenPreferencias(string? p) : base(p) { } public AlmacenPreferencias(UiPreferences? u) : base(u) { } public AlmacenPreferencias(UiPreferences? u, string? p) : base(u, p) { } }
public class GestorPreferencias : UiPreferencesStore { public GestorPreferencias() : base() { } public GestorPreferencias(string? p) : base(p) { } public GestorPreferencias(UiPreferences? u) : base(u) { } public GestorPreferencias(UiPreferences? u, string? p) : base(u, p) { } }
public class ControladorPreferencias : UiPreferencesStore { public ControladorPreferencias() : base() { } public ControladorPreferencias(string? p) : base(p) { } public ControladorPreferencias(UiPreferences? u) : base(u) { } public ControladorPreferencias(UiPreferences? u, string? p) : base(u, p) { } }
public class ServicioPreferencias : UiPreferencesStore { public ServicioPreferencias() : base() { } public ServicioPreferencias(string? p) : base(p) { } public ServicioPreferencias(UiPreferences? u) : base(u) { } public ServicioPreferencias(UiPreferences? u, string? p) : base(u, p) { } }
public class RepositorioPreferencias : UiPreferencesStore { public RepositorioPreferencias() : base() { } public RepositorioPreferencias(string? p) : base(p) { } public RepositorioPreferencias(UiPreferences? u) : base(u) { } public RepositorioPreferencias(UiPreferences? u, string? p) : base(u, p) { } }
public class ProveedorPreferencias : UiPreferencesStore { public ProveedorPreferencias() : base() { } public ProveedorPreferencias(string? p) : base(p) { } public ProveedorPreferencias(UiPreferences? u) : base(u) { } public ProveedorPreferencias(UiPreferences? u, string? p) : base(u, p) { } }
