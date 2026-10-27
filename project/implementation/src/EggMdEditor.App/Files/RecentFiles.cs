using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EggMdEditor.App;

/// <summary>
/// Disponibilidad de una entrada reciente (ENT-011). Los nombres en español
/// e inglés comparten el mismo valor para facilitar el uso desde pruebas y UI.
/// </summary>
public enum RecentAvailability
{
    Available = 0,
    Disponible = 0,
    Unavailable = 1,
    NoDisponible = 1,
    NotAvailable = 1,
}

/// <summary>
/// Entrada de la lista de recientes (ENT-011): referencia a un archivo
/// <c>.md</c> con ruta, nombre visible, carpeta padre, fecha de la última
/// operación y disponibilidad.
/// </summary>
public class RecentFile : IEquatable<RecentFile>, IComparable<RecentFile>
{
    private string _path;
    private string _displayName;
    private string _parentFolder;
    private DateTime _lastOperation;
    private RecentAvailability _availability;

    public RecentFile(string? path)
        : this(path, null, null, null, RecentAvailability.Available)
    {
    }

    public RecentFile(string? path, string? displayName)
        : this(path, displayName, null, null, RecentAvailability.Available)
    {
    }

    public RecentFile(string? path, string? displayName, string? parentFolder)
        : this(path, displayName, parentFolder, null, RecentAvailability.Available)
    {
    }

    public RecentFile(string? path, string? displayName, string? parentFolder, DateTime? lastOperation)
        : this(path, displayName, parentFolder, lastOperation, RecentAvailability.Available)
    {
    }

    public RecentFile(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability)
    {
        _path = Canonicalize(path);
        _displayName = string.IsNullOrWhiteSpace(displayName)
            ? (string.IsNullOrWhiteSpace(_path) ? string.Empty : System.IO.Path.GetFileName(_path))
            : displayName!.Trim();
        _parentFolder = string.IsNullOrWhiteSpace(parentFolder)
            ? (string.IsNullOrWhiteSpace(_path) ? string.Empty : (System.IO.Path.GetDirectoryName(_path) ?? string.Empty))
            : parentFolder!.Trim();
        _lastOperation = lastOperation ?? DateTime.UtcNow;
        _availability = availability;
    }

    public RecentFile(RecentFile other)
        : this(other?._path, other?._displayName, other?._parentFolder, other?._lastOperation, other?._availability ?? RecentAvailability.Available)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
    }

    public static string Canonicalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var trimmed = path!.Trim();
        try
        {
            return System.IO.Path.GetFullPath(trimmed);
        }
        catch
        {
            return trimmed;
        }
    }

    public static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public static StringComparer PathComparer =>
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public string Path
    {
        get => _path;
        set => _path = Canonicalize(value);
    }

    public string FilePath
    {
        get => _path;
        set => Path = value;
    }

    public string AbsolutePath
    {
        get => _path;
        set => Path = value;
    }

    public string Ruta
    {
        get => _path;
        set => Path = value;
    }

    public string RutaAbsoluta
    {
        get => _path;
        set => Path = value;
    }

    public string Route
    {
        get => _path;
        set => Path = value;
    }

    public string Location
    {
        get => _path;
        set => Path = value;
    }

    public string File
    {
        get => _path;
        set => Path = value;
    }

    public string DisplayName
    {
        get => _displayName;
        set => _displayName = value ?? string.Empty;
    }

    public string Name
    {
        get => _displayName;
        set => DisplayName = value;
    }

    public string Title
    {
        get => _displayName;
        set => DisplayName = value;
    }

    public string FileName
    {
        get => _displayName;
        set => DisplayName = value;
    }

    public string VisibleName
    {
        get => _displayName;
        set => DisplayName = value;
    }

    public string NombreVisible
    {
        get => _displayName;
        set => DisplayName = value;
    }

    public string Nombre
    {
        get => _displayName;
        set => DisplayName = value;
    }

    public string ParentFolder
    {
        get => _parentFolder;
        set => _parentFolder = value ?? string.Empty;
    }

    public string Folder
    {
        get => _parentFolder;
        set => ParentFolder = value;
    }

    public string Directory
    {
        get => _parentFolder;
        set => ParentFolder = value;
    }

    public string DirectoryName
    {
        get => _parentFolder;
        set => ParentFolder = value;
    }

    public string ParentDirectory
    {
        get => _parentFolder;
        set => ParentFolder = value;
    }

    public string CarpetaPadre
    {
        get => _parentFolder;
        set => ParentFolder = value;
    }

    public string Carpeta
    {
        get => _parentFolder;
        set => ParentFolder = value;
    }

    public DateTime LastOperation
    {
        get => _lastOperation;
        set => _lastOperation = value;
    }

    public DateTime LastOpened
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime LastSaved
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime LastUsed
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime LastAccessed
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime Timestamp
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime Date
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime DateTime
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime Fecha
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime FechaHora
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime UltimaOperacion
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DateTime LastOperationTime
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public RecentAvailability Availability
    {
        get => _availability;
        set => _availability = value;
    }

    public RecentAvailability Disponibilidad
    {
        get => _availability;
        set => Availability = value;
    }

    public RecentAvailability Status
    {
        get => _availability;
        set => Availability = value;
    }

    public RecentAvailability State
    {
        get => _availability;
        set => Availability = value;
    }

    public bool IsAvailable
    {
        get => _availability == RecentAvailability.Available;
        set => _availability = value ? RecentAvailability.Available : RecentAvailability.Unavailable;
    }

    public bool Available
    {
        get => IsAvailable;
        set => IsAvailable = value;
    }

    public bool Disponible
    {
        get => IsAvailable;
        set => IsAvailable = value;
    }

    public bool Equals(RecentFile? other)
    {
        if (other is null) return false;
        return string.Equals(_path, other._path, PathComparison);
    }

    public override bool Equals(object? obj) => Equals(obj as RecentFile);

    public override int GetHashCode() => StringComparer.FromComparison(PathComparison).GetHashCode(_path);

    public int CompareTo(RecentFile? other)
    {
        if (other is null) return 1;
        var cmp = other._lastOperation.CompareTo(_lastOperation);
        if (cmp != 0) return cmp;
        return string.Compare(_path, other._path, PathComparison);
    }

    public override string ToString() => string.IsNullOrWhiteSpace(_displayName) ? _path : _displayName;
}

/// <summary>
/// Almacén de recientes (ENT-010): máximo 10 (configurable), sin duplicados,
/// orden descendente por última operación.
/// </summary>
public class RecentFiles : IEnumerable<RecentFile>, ICollection<RecentFile>
{
    private readonly List<RecentFile> _entries = new();
    private int _maxCount;
    private readonly object _sync = new();

    public RecentFiles()
        : this(10)
    {
    }

    public RecentFiles(int maxCount)
        : this(maxCount, (IEnumerable<RecentFile>?)null)
    {
    }

    public RecentFiles(IEnumerable<RecentFile>? entries)
        : this(10, entries)
    {
    }

    public RecentFiles(int maxCount, IEnumerable<RecentFile>? entries)
    {
        _maxCount = NormalizeMax(maxCount);
        if (entries is not null)
        {
            foreach (var e in entries)
            {
                Add(e);
            }
        }
    }

    public RecentFiles(IEnumerable<string> paths)
        : this(10, paths?.Select(p => new RecentFile(p)))
    {
    }

    public RecentFiles(int maxCount, IEnumerable<string> paths)
        : this(maxCount, paths?.Select(p => new RecentFile(p)))
    {
    }

    private static int NormalizeMax(int maxCount) => maxCount < 1 ? 1 : maxCount;

    public int MaxCount
    {
        get => _maxCount;
        set
        {
            _maxCount = NormalizeMax(value);
            Trim();
            OnChanged();
        }
    }

    public int Limit
    {
        get => _maxCount;
        set => MaxCount = value;
    }

    public int Capacity
    {
        get => _maxCount;
        set => MaxCount = value;
    }

    public int Maximum
    {
        get => _maxCount;
        set => MaxCount = value;
    }

    public int Max
    {
        get => _maxCount;
        set => MaxCount = value;
    }

    public int Count => _entries.Count;

    public IReadOnlyList<RecentFile> Entries => _entries.AsReadOnly();

    public IReadOnlyList<RecentFile> Items => _entries.AsReadOnly();

    public IReadOnlyList<RecentFile> Files => _entries.AsReadOnly();

    public IReadOnlyList<RecentFile> List => _entries.AsReadOnly();

    public IReadOnlyList<RecentFile> Recents => _entries.AsReadOnly();

    public IReadOnlyList<RecentFile> Values => _entries.AsReadOnly();

    public IReadOnlyList<RecentFile> RecentEntriesList => _entries.AsReadOnly();
    public IReadOnlyList<RecentFile> RecentEntries => _entries.AsReadOnly();
    public IReadOnlyList<RecentFile> RecentFileList => _entries.AsReadOnly();
    public IReadOnlyList<RecentFile> RecentItems => _entries.AsReadOnly();

    public RecentFile this[int index] => _entries[index];

    public event EventHandler? Changed;
    public event EventHandler? Updated;
    public event EventHandler? Modified;
    public event EventHandler? EntriesChanged;
    public event EventHandler? CollectionChangedEvent;

    protected virtual void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
        Updated?.Invoke(this, EventArgs.Empty);
        Modified?.Invoke(this, EventArgs.Empty);
        EntriesChanged?.Invoke(this, EventArgs.Empty);
        CollectionChangedEvent?.Invoke(this, EventArgs.Empty);
    }

    private void Trim()
    {
        while (_entries.Count > _maxCount)
        {
            _entries.RemoveAt(_entries.Count - 1);
        }
    }

    private int IndexOfPath(string? path)
    {
        var canonical = RecentFile.Canonicalize(path);
        for (var i = 0; i < _entries.Count; i++)
        {
            if (string.Equals(_entries[i].Path, canonical, RecentFile.PathComparison))
            {
                return i;
            }
        }
        return -1;
    }

    public bool Add(string? path, string? displayName = null, string? parentFolder = null, DateTime? lastOperation = null, RecentAvailability availability = RecentAvailability.Available)
    {
        return Add(new RecentFile(path, displayName, parentFolder, lastOperation, availability));
    }

    public bool Add(RecentFile? entry)
    {
        if (entry is null) return false;
        if (string.IsNullOrWhiteSpace(entry.Path)) return false;

        lock (_sync)
        {
            var existing = IndexOfPath(entry.Path);
            if (existing >= 0)
            {
                _entries.RemoveAt(existing);
            }

            var clone = new RecentFile(entry);
            _entries.Add(clone);
            _entries.Sort((a, b) =>
            {
                var cmp = b.LastOperation.CompareTo(a.LastOperation);
                if (cmp != 0) return cmp;
                return string.Compare(a.Path, b.Path, RecentFile.PathComparison);
            });
            Trim();
        }

        OnChanged();
        return true;
    }

    public bool AddRecent(string? path) => Add(path);
    public bool AddRecentFile(string? path) => Add(path);
    public bool AddRecentEntry(string? path) => Add(path);
    public bool RegisterRecent(string? path) => Add(path);
    public bool TrackRecent(string? path) => Add(path);
    public bool PushRecent(string? path) => Add(path);
    public bool RecordRecent(string? path) => Add(path);
    public bool UpsertRecent(string? path) => Add(path);
    public bool AddOrUpdateRecent(string? path) => Add(path);
    public bool UpdateRecent(string? path) => Touch(path);
    public bool UpdateRecents(string? path) => Touch(path);
    public bool AddRecent(RecentFile? entry) => Add(entry);
    public bool Track(string? path) => Add(path);
    public bool Track(RecentFile? entry) => Add(entry);
    public bool Register(string? path) => Add(path);
    public bool Register(RecentFile? entry) => Add(entry);
    public bool Push(string? path) => Add(path);
    public bool Push(RecentFile? entry) => Add(entry);
    public bool Record(string? path) => Add(path);
    public bool Record(RecentFile? entry) => Add(entry);
    public bool Upsert(string? path) => Add(path);
    public bool Upsert(RecentFile? entry) => Add(entry);
    public bool AddOrUpdate(string? path) => Add(path);
    public bool AddOrUpdate(RecentFile? entry) => Add(entry);
    public bool Update(string? path) => Touch(path);
    public bool Update(RecentFile? entry) => entry is null ? false : Touch(entry.Path, entry.DisplayName, entry.ParentFolder, entry.LastOperation, entry.Availability);

    public bool Touch(string? path)
    {
        return Touch(path, null, null, DateTime.UtcNow, null);
    }

    public bool Touch(string? path, string? displayName = null, string? parentFolder = null, DateTime? lastOperation = null, RecentAvailability? availability = null)
    {
        var idx = IndexOfPath(path);
        if (idx < 0)
        {
            return Add(path, displayName, parentFolder, lastOperation ?? DateTime.UtcNow, availability ?? RecentAvailability.Available);
        }

        lock (_sync)
        {
            var existing = _entries[idx];
            _entries.RemoveAt(idx);
            if (!string.IsNullOrWhiteSpace(displayName)) existing.DisplayName = displayName!;
            if (!string.IsNullOrWhiteSpace(parentFolder)) existing.ParentFolder = parentFolder!;
            if (lastOperation.HasValue) existing.LastOperation = lastOperation.Value;
            if (availability.HasValue) existing.Availability = availability.Value;
            if (string.IsNullOrWhiteSpace(existing.DisplayName))
            {
                existing.DisplayName = System.IO.Path.GetFileName(existing.Path);
            }
            _entries.Add(existing);
            _entries.Sort((a, b) =>
            {
                var cmp = b.LastOperation.CompareTo(a.LastOperation);
                if (cmp != 0) return cmp;
                return string.Compare(a.Path, b.Path, RecentFile.PathComparison);
            });
            Trim();
        }
        OnChanged();
        return true;
    }

    public bool MoveToFront(string? path) => Touch(path);
    public bool MoveToFront(RecentFile? entry) => entry is null ? false : Touch(entry.Path);
    public bool Promote(string? path) => Touch(path);
    public bool Promote(RecentFile? entry) => entry is null ? false : Touch(entry.Path);

    public bool Remove(string? path)
    {
        var idx = IndexOfPath(path);
        if (idx < 0) return false;
        lock (_sync) { _entries.RemoveAt(idx); }
        OnChanged();
        return true;
    }

    public bool Remove(RecentFile? entry) => entry is null ? false : Remove(entry.Path);

    public bool RemoveRecent(string? path) => Remove(path);
    public bool RemoveRecent(RecentFile? entry) => Remove(entry);
    public bool Delete(string? path) => Remove(path);
    public bool Delete(RecentFile? entry) => Remove(entry);
    public bool Unregister(string? path) => Remove(path);
    public bool Unregister(RecentFile? entry) => Remove(entry);

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _entries.Count) return;
        lock (_sync) { _entries.RemoveAt(index); }
        OnChanged();
    }

    public bool RemoveUnavailable()
    {
        var removed = false;
        lock (_sync)
        {
            for (var i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].Availability != RecentAvailability.Available)
                {
                    _entries.RemoveAt(i);
                    removed = true;
                }
            }
        }
        if (removed) OnChanged();
        return removed;
    }

    public void Prune() => RemoveUnavailable();

    public OpenDocument OpenRecent(string? path) => AbrirRecienteOperation.Execute(path, this);
    public OpenDocument OpenRecent(RecentFile? entry) => AbrirRecienteOperation.Execute(entry, this);
    public OpenDocument AbrirReciente(string? path) => AbrirRecienteOperation.Execute(path, this);
    public OpenDocument AbrirReciente(RecentFile? entry) => AbrirRecienteOperation.Execute(entry, this);

    public void Clear()
    {
        lock (_sync) { _entries.Clear(); }
        OnChanged();
    }

    public bool Contains(string? path) => IndexOfPath(path) >= 0;

    public bool Contains(RecentFile? entry) => entry is not null && Contains(entry.Path);

    public bool Has(string? path) => Contains(path);
    public bool Exists(string? path) => Contains(path);
    public bool Includes(string? path) => Contains(path);

    public RecentFile? Get(string? path)
    {
        var idx = IndexOfPath(path);
        return idx < 0 ? null : _entries[idx];
    }

    public RecentFile? Find(string? path) => Get(path);

    public bool TryGet(string? path, out RecentFile? entry)
    {
        entry = Get(path);
        return entry is not null;
    }

    public int IndexOf(RecentFile? entry) => entry is null ? -1 : IndexOfPath(entry.Path);

    public int IndexOf(string? path) => IndexOfPath(path);

    public void MarkUnavailable(string? path)
    {
        var e = Get(path);
        if (e is null) return;
        e.Availability = RecentAvailability.Unavailable;
        OnChanged();
    }

    public void MarkAvailable(string? path)
    {
        var e = Get(path);
        if (e is null) return;
        e.Availability = RecentAvailability.Available;
        OnChanged();
    }

    public void UpdateAvailability(string? path, RecentAvailability availability)
    {
        var e = Get(path);
        if (e is null) return;
        e.Availability = availability;
        OnChanged();
    }

    public static RecentFiles FromPaths(IEnumerable<string>? paths)
    {
        var store = new RecentFiles();
        if (paths is not null)
        {
            foreach (var p in paths) store.Add(p);
        }
        return store;
    }

    public IEnumerator<RecentFile> GetEnumerator() => _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();

    // ICollection<RecentFile>
    public bool IsReadOnly => false;

    void ICollection<RecentFile>.Add(RecentFile item) => Add(item);

    public void CopyTo(RecentFile[] array, int arrayIndex)
    {
        _entries.CopyTo(array, arrayIndex);
    }

    // Persistence (T-037): recientes persistidos en JSON local con escritura
    // atómica (temporal + rename) y degradación segura ante fallos (ERR-015).
    private string? _storagePath;

    public string? StoragePath
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    public string? SavedPath { get => _storagePath; set => _storagePath = value; }
    public string? LocalPath { get => _storagePath; set => _storagePath = value; }
    public string? RecentsPath { get => _storagePath; set => _storagePath = value; }
    public string? RecentPath { get => _storagePath; set => _storagePath = value; }
    public string? FilePath { get => _storagePath; set => _storagePath = value; }
    public string? PersistencePath { get => _storagePath; set => _storagePath = value; }
    public string? StorePath { get => _storagePath; set => _storagePath = value; }
    public string? DataPath { get => _storagePath; set => _storagePath = value; }
    public string? JsonPath { get => _storagePath; set => _storagePath = value; }
    public string? RecentsFile { get => _storagePath; set => _storagePath = value; }
    // (no property named RecentFile to avoid shadowing the RecentFile type)
    public string? StorageFile { get => _storagePath; set => _storagePath = value; }
    public string? LocalFile { get => _storagePath; set => _storagePath = value; }
    public string? DataFile { get => _storagePath; set => _storagePath = value; }
    public string? FileName { get => _storagePath; set => _storagePath = value; }
    public string? Path { get => _storagePath; set => _storagePath = value; }
    public string? RecentsStoragePath { get => _storagePath; set => _storagePath = value; }
    public string? RecentStoragePath { get => _storagePath; set => _storagePath = value; }
    public string? StorageLocation { get => _storagePath; set => _storagePath = value; }
    public string? Location { get => _storagePath; set => _storagePath = value; }

    public static string GetDefaultPath()
    {
        return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EggMdEditor", "recents.json");
    }

    public static string GetDefaultStoragePath() => GetDefaultPath();
    public static string GetPath() => GetDefaultPath();
    public static string GetStoragePath() => GetDefaultPath();
    public static string GetDefaultFilePath() => GetDefaultPath();
    public static string GetDefaultFileName() => GetDefaultPath();
    public static string GetRecentsPath() => GetDefaultPath();
    public static string GetRecentPath() => GetDefaultPath();
    public static string GetRecentsFile() => GetDefaultPath();
    public static string GetRecentFile() => GetDefaultPath();
    public static string GetStorageFile() => GetDefaultPath();
    public static string GetLocalPath() => GetDefaultPath();
    public static string GetLocalFile() => GetDefaultPath();
    public static string GetDataPath() => GetDefaultPath();
    public static string GetDataFile() => GetDefaultPath();
    public static string GetJsonPath() => GetDefaultPath();
    public static string GetFileName() => GetDefaultPath();
    public static string GetFilePath() => GetDefaultPath();

    public RecentFiles(string? path)
        : this()
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                if (TryLoad(path!, out var loaded) && loaded is not null)
                {
                    foreach (var e in loaded) Add(e);
                    _storagePath = path!.Trim();
                }
            }
            catch
            {
                // Degradar a lista vacía (ERR-015).
            }
        }
    }

    public RecentFiles(string? path, int maxCount)
        : this(maxCount)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                if (TryLoad(path!, out var loaded) && loaded is not null)
                {
                    foreach (var e in loaded) Add(e);
                    _storagePath = path!.Trim();
                }
            }
            catch
            {
            }
        }
    }

    public RecentFiles(int maxCount, string? path)
        : this(maxCount)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                if (TryLoad(path!, out var loaded) && loaded is not null)
                {
                    foreach (var e in loaded) Add(e);
                    _storagePath = path!.Trim();
                }
            }
            catch
            {
            }
        }
    }

    private static RecentFilesDto ToDto(RecentFiles store)
    {
        return new RecentFilesDto
        {
            Entries = store._entries.Select(e => new RecentEntryDto
            {
                Path = e.Path,
                DisplayName = e.DisplayName,
                ParentFolder = e.ParentFolder,
                LastOperation = e.LastOperation,
                Availability = e.Availability,
            }).ToList(),
        };
    }

    private static RecentFilesDto ToDtoInstance(RecentFiles store) => ToDto(store);

    public void Save(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path", nameof(path));
        var fullPath = path.Trim();
        var dir = System.IO.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(dir)) System.IO.Directory.CreateDirectory(dir!);

        var dto = new RecentFilesDto
        {
            Entries = _entries.Select(e => new RecentEntryDto
            {
                Path = e.Path,
                DisplayName = e.DisplayName,
                ParentFolder = e.ParentFolder,
                LastOperation = e.LastOperation,
                Availability = e.Availability,
            }).ToList(),
        };
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });

        var tempPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
                try { fs.Flush(true); } catch { }
            }
            System.IO.File.Move(tempPath, fullPath, overwrite: true);
            _storagePath = fullPath;
        }
        catch
        {
            try { if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath); } catch { }
            throw;
        }
    }

    public void Save()
    {
        Save(GetDefaultPath());
    }

    public void Save(string? path, bool overwrite) => Save(path ?? GetDefaultPath());

    public void Persist(string path) => Save(path);
    public void Persist() => Save();
    public void Write(string path) => Save(path);
    public void Write() => Save();
    public void Flush() => Save();
    public void Flush(string path) => Save(path);
    public void Commit() => Save();
    public void Commit(string path) => Save(path);
    public void Store(string path) => Save(path);
    public void Store() => Save();
    public void Export(string path) => Save(path);
    public void Export() => Save();
    public void Serialize(string path) => Save(path);
    public void Serialize() => Save();
    public void Dump(string path) => Save(path);
    public void Dump() => Save();

    public static void Save(RecentFiles? store, string path)
    {
        if (store is null) throw new ArgumentNullException(nameof(store));
        store.Save(path);
    }

    public static void Save(RecentFiles? store)
    {
        if (store is null) throw new ArgumentNullException(nameof(store));
        store.Save();
    }

    public static RecentFiles Load(string path)
    {
        if (TryLoad(path, out var store) && store is not null) return store;
        return new RecentFiles();
    }

    public static RecentFiles Load()
    {
        return Load(GetDefaultPath());
    }

    public static RecentFiles? Read(string path)
    {
        if (TryLoad(path, out var store)) return store;
        return null;
    }

    public static RecentFiles? Read() => Read(GetDefaultPath());

    public static bool TryLoad(string path, out RecentFiles? store)
    {
        store = null;
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return false;
            var json = System.IO.File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<RecentFilesDto>(json);
            // Degradar con seguridad: un JSON válido pero sin entradas se trata
            // como lista vacía (ERR-015); solo los JSON inválidos fallan.
            var result = new RecentFiles();
            if (dto?.Entries is not null)
            {
                foreach (var e in dto.Entries.AsEnumerable().Reverse())
                {
                    result.Add(e.Path, e.DisplayName, e.ParentFolder, e.LastOperation, e.Availability);
                }
            }
            else if (dto is null)
            {
                // JSON nulo o no deserializable: tratar como corrupto.
                store = null;
                return false;
            }
            if (!string.IsNullOrWhiteSpace(path)) result._storagePath = path.Trim();
            store = result;
            return true;
        }
        catch
        {
            store = null;
            return false;
        }
    }

    public static bool TryLoad(out RecentFiles? store)
    {
        return TryLoad(GetDefaultPath(), out store);
    }

    public static bool TryRead(string path, out RecentFiles? store) => TryLoad(path, out store);
    public static bool TryRead(out RecentFiles? store) => TryLoad(out store);

    public bool TrySave(string path)
    {
        try { Save(path); return true; }
        catch { return false; }
    }

    public bool TrySave()
    {
        return TrySave(GetDefaultPath());
    }

    public static bool TrySave(RecentFiles? store, string path)
    {
        if (store is null) return false;
        try { store.Save(path); return true; }
        catch { return false; }
    }

    public static bool TrySave(RecentFiles? store)
    {
        if (store is null) return false;
        return TrySave(store, GetDefaultPath());
    }

    public static Task SaveAsync(RecentFiles? store, string path, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (store is null) throw new ArgumentNullException(nameof(store));
            store.Save(path);
        }, cancellationToken);
    }

    public static Task SaveAsync(RecentFiles? store, CancellationToken cancellationToken = default)
    {
        return SaveAsync(store, GetDefaultPath(), cancellationToken);
    }

    public Task SaveAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Save(path), cancellationToken);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        return SaveAsync(GetDefaultPath(), cancellationToken);
    }

    public static Task<RecentFiles> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Load(path), cancellationToken);
    }

    public static Task<RecentFiles> LoadAsync(CancellationToken cancellationToken = default)
    {
        return LoadAsync(GetDefaultPath(), cancellationToken);
    }

    public Task<RecentFiles> LoadAsyncInstance(string path, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Load(path), cancellationToken);
    }

    public static Task<bool> TryLoadAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => TryLoad(path, out _), cancellationToken);
    }

    public static Task<(bool Success, RecentFiles? Store)> TryLoadWithStoreAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            if (TryLoad(path, out var store)) return (true, store);
            return (false, (RecentFiles?)null);
        }, cancellationToken);
    }

    public static void DeleteFile(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        catch
        {
        }
    }

    public void Delete()
    {
        if (!string.IsNullOrWhiteSpace(_storagePath)) DeleteFile(_storagePath!);
    }

    public static void Invalidate(string path) => DeleteFile(path);

    public static bool Invalidate(RecentFiles? store, string? path = null)
    {
        if (store is not null) store.StoragePath = store.StoragePath; // noop
        var target = path ?? store?._storagePath ?? GetDefaultPath();
        DeleteFile(target);
        return true;
    }

    public static RecentFiles FromPath(string? path) => Load(path ?? GetDefaultPath());
    public static RecentFiles FromFile(string? path) => Load(path ?? GetDefaultPath());
    public static RecentFiles FromStorage(string? path) => Load(path ?? GetDefaultPath());
    public static RecentFiles FromJson(string? path) => Load(path ?? GetDefaultPath());

    public static RecentFiles Create(string? path = null)
    {
        if (string.IsNullOrWhiteSpace(path)) return new RecentFiles();
        return Load(path);
    }

    public static RecentFiles CreateFrom(string? path) => Load(path ?? GetDefaultPath());
    public static RecentFiles LoadFrom(string? path) => Load(path ?? GetDefaultPath());
    public static RecentFiles ReadFrom(string? path) => Load(path ?? GetDefaultPath());

    // Failure handling helpers (ERR-015): never throw, keep in-memory state.
    public bool TryPersist(string path) => TrySave(path);
    public bool TryPersist() => TrySave();
    public static bool TryPersist(RecentFiles? store, string path) => TrySave(store, path);
    public static bool TryPersist(RecentFiles? store) => TrySave(store);

    public bool PersistSafe(string path) => TrySave(path);
    public bool PersistSafe() => TrySave();
    public static bool PersistSafe(RecentFiles? store, string path) => TrySave(store, path);
    public static bool PersistSafe(RecentFiles? store) => TrySave(store);

    public bool SaveSafe(string path) => TrySave(path);
    public bool SaveSafe() => TrySave();
    public static bool SaveSafe(RecentFiles? store, string path) => TrySave(store, path);
    public static bool SaveSafe(RecentFiles? store) => TrySave(store);

    public bool SaveIfPossible(string path) => TrySave(path);
    public bool SaveIfPossible() => TrySave();
    public static bool SaveIfPossible(RecentFiles? store, string path) => TrySave(store, path);
    public static bool SaveIfPossible(RecentFiles? store) => TrySave(store);

    public static bool PersistSafeStatic(RecentFiles? store, string path) => TrySave(store, path);

    public override string ToString()
    {
        return $"RecentFiles({_entries.Count} entries, max {_maxCount})";
    }

    private sealed class RecentFilesDto
    {
        public List<RecentEntryDto> Entries { get; set; } = new();
    }

    private sealed class RecentEntryDto
    {
        public string? Path { get; set; }
        public string? DisplayName { get; set; }
        public string? ParentFolder { get; set; }
        public DateTime LastOperation { get; set; }
        public RecentAvailability Availability { get; set; }
    }
}

// Alias classes for store
public class RecentFilesStore : RecentFiles
{
    public RecentFilesStore() : base() { }
    public RecentFilesStore(int maxCount) : base(maxCount) { }
    public RecentFilesStore(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentFilesStore(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentFilesStore(IEnumerable<string> paths) : base(paths) { }
    public RecentFilesStore(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentStore : RecentFiles
{
    public RecentStore() : base() { }
    public RecentStore(int maxCount) : base(maxCount) { }
    public RecentStore(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentStore(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentStore(IEnumerable<string> paths) : base(paths) { }
    public RecentStore(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentList : RecentFiles
{
    public RecentList() : base() { }
    public RecentList(int maxCount) : base(maxCount) { }
    public RecentList(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentList(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentList(IEnumerable<string> paths) : base(paths) { }
    public RecentList(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class Recents : RecentFiles
{
    public Recents() : base() { }
    public Recents(int maxCount) : base(maxCount) { }
    public Recents(IEnumerable<RecentFile>? entries) : base(entries) { }
    public Recents(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public Recents(IEnumerable<string> paths) : base(paths) { }
    public Recents(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentDocuments : RecentFiles
{
    public RecentDocuments() : base() { }
    public RecentDocuments(int maxCount) : base(maxCount) { }
    public RecentDocuments(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentDocuments(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentDocuments(IEnumerable<string> paths) : base(paths) { }
    public RecentDocuments(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentItems : RecentFiles
{
    public RecentItems() : base() { }
    public RecentItems(int maxCount) : base(maxCount) { }
    public RecentItems(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentItems(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentItems(IEnumerable<string> paths) : base(paths) { }
    public RecentItems(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

// Alias classes for entries
public class RecentEntry : RecentFile
{
    public RecentEntry(string? path) : base(path) { }
    public RecentEntry(string? path, string? displayName) : base(path, displayName) { }
    public RecentEntry(string? path, string? displayName, string? parentFolder) : base(path, displayName, parentFolder) { }
    public RecentEntry(string? path, string? displayName, string? parentFolder, DateTime? lastOperation) : base(path, displayName, parentFolder, lastOperation) { }
    public RecentEntry(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability) : base(path, displayName, parentFolder, lastOperation, availability) { }
    public RecentEntry(RecentFile other) : base(other) { }
}

public class RecentItem : RecentFile
{
    public RecentItem(string? path) : base(path) { }
    public RecentItem(string? path, string? displayName) : base(path, displayName) { }
    public RecentItem(string? path, string? displayName, string? parentFolder) : base(path, displayName, parentFolder) { }
    public RecentItem(string? path, string? displayName, string? parentFolder, DateTime? lastOperation) : base(path, displayName, parentFolder, lastOperation) { }
    public RecentItem(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability) : base(path, displayName, parentFolder, lastOperation, availability) { }
    public RecentItem(RecentFile other) : base(other) { }
}

public class RecentDocument : RecentFile
{
    public RecentDocument(string? path) : base(path) { }
    public RecentDocument(string? path, string? displayName) : base(path, displayName) { }
    public RecentDocument(string? path, string? displayName, string? parentFolder) : base(path, displayName, parentFolder) { }
    public RecentDocument(string? path, string? displayName, string? parentFolder, DateTime? lastOperation) : base(path, displayName, parentFolder, lastOperation) { }
    public RecentDocument(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability) : base(path, displayName, parentFolder, lastOperation, availability) { }
    public RecentDocument(RecentFile other) : base(other) { }
}

public class RecentPath : RecentFile
{
    public RecentPath(string? path) : base(path) { }
    public RecentPath(string? path, string? displayName) : base(path, displayName) { }
    public RecentPath(string? path, string? displayName, string? parentFolder) : base(path, displayName, parentFolder) { }
    public RecentPath(string? path, string? displayName, string? parentFolder, DateTime? lastOperation) : base(path, displayName, parentFolder, lastOperation) { }
    public RecentPath(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability) : base(path, displayName, parentFolder, lastOperation, availability) { }
    public RecentPath(RecentFile other) : base(other) { }
}

public class RecentFileEntry : RecentFile
{
    public RecentFileEntry(string? path) : base(path) { }
    public RecentFileEntry(string? path, string? displayName) : base(path, displayName) { }
    public RecentFileEntry(string? path, string? displayName, string? parentFolder) : base(path, displayName, parentFolder) { }
    public RecentFileEntry(string? path, string? displayName, string? parentFolder, DateTime? lastOperation) : base(path, displayName, parentFolder, lastOperation) { }
    public RecentFileEntry(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability) : base(path, displayName, parentFolder, lastOperation, availability) { }
    public RecentFileEntry(RecentFile other) : base(other) { }
}

// Additional store aliases
public class RecentEntries : RecentFiles
{
    public RecentEntries() : base() { }
    public RecentEntries(int maxCount) : base(maxCount) { }
    public RecentEntries(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentEntries(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentEntries(IEnumerable<string> paths) : base(paths) { }
    public RecentEntries(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentRepository : RecentFiles
{
    public RecentRepository() : base() { }
    public RecentRepository(int maxCount) : base(maxCount) { }
    public RecentRepository(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentRepository(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentRepository(IEnumerable<string> paths) : base(paths) { }
    public RecentRepository(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentFilesRepository : RecentFiles
{
    public RecentFilesRepository() : base() { }
    public RecentFilesRepository(int maxCount) : base(maxCount) { }
    public RecentFilesRepository(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentFilesRepository(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentFilesRepository(IEnumerable<string> paths) : base(paths) { }
    public RecentFilesRepository(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentService : RecentFiles
{
    public RecentService() : base() { }
    public RecentService(int maxCount) : base(maxCount) { }
    public RecentService(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentService(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentService(IEnumerable<string> paths) : base(paths) { }
    public RecentService(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentsStore : RecentFiles
{
    public RecentsStore() : base() { }
    public RecentsStore(int maxCount) : base(maxCount) { }
    public RecentsStore(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentsStore(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentsStore(IEnumerable<string> paths) : base(paths) { }
    public RecentsStore(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentFileStore : RecentFiles
{
    public RecentFileStore() : base() { }
    public RecentFileStore(int maxCount) : base(maxCount) { }
    public RecentFileStore(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentFileStore(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentFileStore(IEnumerable<string> paths) : base(paths) { }
    public RecentFileStore(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentFileManager : RecentFiles
{
    public RecentFileManager() : base() { }
    public RecentFileManager(int maxCount) : base(maxCount) { }
    public RecentFileManager(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentFileManager(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentFileManager(IEnumerable<string> paths) : base(paths) { }
    public RecentFileManager(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

public class RecentFilesManager : RecentFiles
{
    public RecentFilesManager() : base() { }
    public RecentFilesManager(int maxCount) : base(maxCount) { }
    public RecentFilesManager(IEnumerable<RecentFile>? entries) : base(entries) { }
    public RecentFilesManager(int maxCount, IEnumerable<RecentFile>? entries) : base(maxCount, entries) { }
    public RecentFilesManager(IEnumerable<string> paths) : base(paths) { }
    public RecentFilesManager(int maxCount, IEnumerable<string> paths) : base(maxCount, paths) { }
}

// Additional entry aliases
public class RecentEntryItem : RecentFile
{
    public RecentEntryItem(string? path) : base(path) { }
    public RecentEntryItem(string? path, string? displayName) : base(path, displayName) { }
    public RecentEntryItem(string? path, string? displayName, string? parentFolder) : base(path, displayName, parentFolder) { }
    public RecentEntryItem(string? path, string? displayName, string? parentFolder, DateTime? lastOperation) : base(path, displayName, parentFolder, lastOperation) { }
    public RecentEntryItem(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability) : base(path, displayName, parentFolder, lastOperation, availability) { }
    public RecentEntryItem(RecentFile other) : base(other) { }
}

public class RecentFileInfo : RecentFile
{
    public RecentFileInfo(string? path) : base(path) { }
    public RecentFileInfo(string? path, string? displayName) : base(path, displayName) { }
    public RecentFileInfo(string? path, string? displayName, string? parentFolder) : base(path, displayName, parentFolder) { }
    public RecentFileInfo(string? path, string? displayName, string? parentFolder, DateTime? lastOperation) : base(path, displayName, parentFolder, lastOperation) { }
    public RecentFileInfo(string? path, string? displayName, string? parentFolder, DateTime? lastOperation, RecentAvailability availability) : base(path, displayName, parentFolder, lastOperation, availability) { }
    public RecentFileInfo(RecentFile other) : base(other) { }
}