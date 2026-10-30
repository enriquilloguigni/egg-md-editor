using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace EggMdEditor.App;

/// <summary>
/// Preferencias de visibilidad y ancho del separador (ENT-019) y su
/// persistencia en el directorio de configuración del usuario (A-008, BR-019,
/// FR-013). Se añade a <see cref="UiPreferences"/> (ENT-003) para mantener
/// una única preferencia por usuario/sesión.
/// </summary>
public partial class UiPreferences
{
    private bool _leftPanelVisible = true;
    private bool _rightPanelVisible = true;
    private bool _formatBarVisible = true;
    private double _splitterWidth = 4.0;
    private DateTime _lastUpdated = DateTime.UtcNow;

    // ---------- Panel izquierdo ----------
    public bool LeftPanelVisible
    {
        get => _leftPanelVisible;
        set { if (_leftPanelVisible != value) { _leftPanelVisible = value; Touch(); OnPropertyChanged(nameof(LeftPanelVisible)); } }
    }
    public bool LeftVisible { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool IsLeftPanelVisible { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool IsLeftVisible { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool LeftShown { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool LeftPanelShown { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool ShowLeftPanel { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool PanelLeftVisible { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool LeftVisibility { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool LeftPanelVisibility { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool IsLeftPanelShown { get => _leftPanelVisible; set => LeftPanelVisible = value; }
    public bool LeftPanelIsVisible { get => _leftPanelVisible; set => LeftPanelVisible = value; }

    // ---------- Panel derecho ----------
    public bool RightPanelVisible
    {
        get => _rightPanelVisible;
        set { if (_rightPanelVisible != value) { _rightPanelVisible = value; Touch(); OnPropertyChanged(nameof(RightPanelVisible)); } }
    }
    public bool RightVisible { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool IsRightPanelVisible { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool IsRightVisible { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool RightShown { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool RightPanelShown { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool ShowRightPanel { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool PanelRightVisible { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool RightVisibility { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool RightPanelVisibility { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool IsRightPanelShown { get => _rightPanelVisible; set => RightPanelVisible = value; }
    public bool RightPanelIsVisible { get => _rightPanelVisible; set => RightPanelVisible = value; }

    // ---------- Barra de formato ----------
    public bool FormatBarVisible
    {
        get => _formatBarVisible;
        set { if (_formatBarVisible != value) { _formatBarVisible = value; Touch(); OnPropertyChanged(nameof(FormatBarVisible)); } }
    }
    public bool FormatBarShown { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool IsFormatBarVisible { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool IsFormatBarShown { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool BarVisible { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool BarShown { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool IsBarVisible { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool IsBarShown { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool FormatBarVisibility { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool BarVisibility { get => _formatBarVisible; set => FormatBarVisible = value; }
    public bool FormatBarIsVisible { get => _formatBarVisible; set => FormatBarVisible = value; }

    // ---------- Ancho del separador ----------
    public double SplitterWidth
    {
        get => _splitterWidth;
        set { _splitterWidth = value; Touch(); OnPropertyChanged(nameof(SplitterWidth)); }
    }
    public double SeparatorWidth { get => _splitterWidth; set => SplitterWidth = value; }
    public double SplitterSize { get => _splitterWidth; set => SplitterWidth = value; }
    public double SeparatorSize { get => _splitterWidth; set => SplitterWidth = value; }
    public double Width { get => _splitterWidth; set => SplitterWidth = value; }
    public double SplitterPersistedWidth { get => _splitterWidth; set => SplitterWidth = value; }
    public double PersistedWidth { get => _splitterWidth; set => SplitterWidth = value; }
    public double SavedWidth { get => _splitterWidth; set => SplitterWidth = value; }
    public double SeparatorPersistedWidth { get => _splitterWidth; set => SplitterWidth = value; }
    public double SplitterThickness { get => _splitterWidth; set => SplitterWidth = value; }
    public double SeparatorThickness { get => _splitterWidth; set => SplitterWidth = value; }
    public double AnchoSeparador { get => _splitterWidth; set => SplitterWidth = value; }
    public double AnchoSplitter { get => _splitterWidth; set => SplitterWidth = value; }
    public double SeparatorAncho { get => _splitterWidth; set => SplitterWidth = value; }

    // ---------- Última actualización ----------
    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set { _lastUpdated = value; OnPropertyChanged(nameof(LastUpdated)); }
    }
    public DateTime LastUpdate { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime UpdatedAt { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime Updated { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime Timestamp { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime LastModified { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime LastChanged { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime ModifiedAt { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime FechaActualizacion { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime UltimaActualizacion { get => _lastUpdated; set => LastUpdated = value; }
    public DateTime LastUpdateTime { get => _lastUpdated; set => LastUpdated = value; }

    private void Touch()
    {
        _lastUpdated = DateTime.UtcNow;
        OnPropertyChanged(nameof(LastUpdated));
    }

    // ---------- Persistencia (A-008) ----------
    private static string _defaultPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EggMdEditor", "ui-preferences.json");

    public static string GetDefaultPath() => _defaultPath;
    public static string GetDefaultStoragePath() => _defaultPath;
    public static string GetPath() => _defaultPath;
    public static string GetStoragePath() => _defaultPath;
    public static string GetDefaultFilePath() => _defaultPath;
    public static string GetDefaultFileName() => _defaultPath;
    public static string GetPreferencesPath() => _defaultPath;
    public static string GetPreferencePath() => _defaultPath;
    public static string GetPreferencesFile() => _defaultPath;
    public static string GetPreferenceFile() => _defaultPath;
    public static string GetJsonPath() => _defaultPath;
    public static string GetFile() => _defaultPath;
    public static string GetFilePath() => _defaultPath;
    public static string GetFileName() => _defaultPath;
    public static string FilePathDefault => _defaultPath;
    public static string DefaultPath => _defaultPath;
    public static string DefaultFilePath => _defaultPath;
    public static string StoragePathDefault => _defaultPath;

    public static void SetDefaultPath(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path)) _defaultPath = path!.Trim();
    }

    // Instance storage path (optional)
    private string? _storagePath;
    public string? StoragePath { get => _storagePath; set => _storagePath = value; }
    public string? Path { get => _storagePath; set => _storagePath = value; }
    public string? FilePath { get => _storagePath; set => _storagePath = value; }
    public string? LocalPath { get => _storagePath; set => _storagePath = value; }
    public string? DataPath { get => _storagePath; set => _storagePath = value; }
    public string? JsonPath { get => _storagePath; set => _storagePath = value; }
    public string? FileName { get => _storagePath; set => _storagePath = value; }
    public string? StorageFile { get => _storagePath; set => _storagePath = value; }
    public string? DataFile { get => _storagePath; set => _storagePath = value; }
    public string? LocalFile { get => _storagePath; set => _storagePath = value; }
    public string? PreferencesPath { get => _storagePath; set => _storagePath = value; }
    public string? PreferencePath { get => _storagePath; set => _storagePath = value; }
    public string? PreferencesFile { get => _storagePath; set => _storagePath = value; }
    public string? PreferenceFile { get => _storagePath; set => _storagePath = value; }

    private static string ResolvePath(string? path) =>
        string.IsNullOrWhiteSpace(path) ? _defaultPath : path!.Trim();

    public static UiPreferences Load()
    {
        return Load((string?)null);
    }

    public static UiPreferences Load(string? path)
    {
        path = ResolvePath(path);
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return new UiPreferences();
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return new UiPreferences();
            var loaded = JsonSerializer.Deserialize<UiPreferences>(json);
            return loaded ?? new UiPreferences();
        }
        catch
        {
            return new UiPreferences();
        }
    }

    public static bool TryLoad(string? path, out UiPreferences? preferences)
    {
        preferences = null;
        try
        {
            path = ResolvePath(path);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return false;
            var loaded = JsonSerializer.Deserialize<UiPreferences>(json);
            if (loaded is null) return false;
            preferences = loaded;
            return true;
        }
        catch
        {
            preferences = null;
            return false;
        }
    }

    public static bool TryLoad(out UiPreferences? preferences) => TryLoad(null, out preferences);

    public static UiPreferences Read(string? path = null) => Load(path);
    public static UiPreferences? TryRead(string? path = null)
    {
        if (TryLoad(path, out var p)) return p;
        return null;
    }

    public static UiPreferences FromPath(string? path) => Load(path);
    public static UiPreferences FromFile(string? path) => Load(path);
    public static UiPreferences FromJson(string? path) => Load(path);
    public static UiPreferences FromStorage(string? path) => Load(path);
    public static UiPreferences Create(string? path = null) => string.IsNullOrWhiteSpace(path) ? new UiPreferences() : Load(path);
    public static UiPreferences CreateFrom(string? path) => Load(path);
    public static UiPreferences LoadFrom(string? path) => Load(path);
    public static UiPreferences ReadFrom(string? path) => Load(path);

    public bool Save()
    {
        return Save(this, (string?)null);
    }

    public bool Save(string? path)
    {
        return Save(this, path);
    }

    public static bool Save(UiPreferences? preferences, string? path = null)
    {
        if (preferences is null) throw new ArgumentNullException(nameof(preferences));
        path = ResolvePath(path ?? preferences._storagePath);
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir)) System.IO.Directory.CreateDirectory(dir!);

        var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true });
        var tempPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
                try { fs.Flush(true); } catch { }
            }
            System.IO.File.Move(tempPath, path, overwrite: true);
            preferences._storagePath = path;
            return true;
        }
        catch
        {
            try { if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath); } catch { }
            throw;
        }
    }

    public static bool Save(UiPreferences? preferences) => Save(preferences, (string?)null);

    public bool SaveTo(string? path = null)
    {
        try { Save(this, path); return true; } catch { return false; }
    }
    public bool TrySave(string? path = null)
    {
        try { Save(this, path); return true; } catch { return false; }
    }
    public bool TrySave() => TrySave((string?)null);

    public static bool TrySave(UiPreferences? preferences, string? path = null)
    {
        if (preferences is null) return false;
        try { Save(preferences, path); return true; } catch { return false; }
    }
    public static bool TrySave(UiPreferences? preferences) => TrySave(preferences, null);

    // Instance Load using storage path
    public UiPreferences LoadInstance(string? path = null)
    {
        var loaded = Load(path ?? _storagePath);
        // copy into this? We'll return loaded; method name kept for compatibility.
        return loaded;
    }

    public static Task SaveAsync(UiPreferences? preferences, string? path = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Save(preferences, path), cancellationToken);
    }
    public static Task<UiPreferences> LoadAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Load(path), cancellationToken);
    }
}
