using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EggMdEditor.App;

public class RecoveryCopy
{
    private Guid _documentId = Guid.NewGuid();
    private string? _originalPath;
    private string _content = string.Empty;
    private DateTime _timestamp = DateTime.UtcNow;
    private RecoveryStatus _status = RecoveryStatus.Valid;
    private string? _storagePath;

    public RecoveryCopy()
    {
    }

    public RecoveryCopy(string? content)
        : this(content, null)
    {
    }

    public RecoveryCopy(string? content, string? originalPath)
        : this(content, originalPath, Guid.NewGuid())
    {
    }

    public RecoveryCopy(string? content, string? originalPath, Guid documentId)
        : this(content, originalPath, documentId, DateTime.UtcNow)
    {
    }

    public RecoveryCopy(string? content, string? originalPath, Guid documentId, DateTime? timestamp)
        : this(content, originalPath, documentId, timestamp, RecoveryStatus.Valid)
    {
    }

    public RecoveryCopy(string? content, string? originalPath, Guid documentId, DateTime? timestamp, RecoveryStatus status)
    {
        _content = content ?? string.Empty;
        _originalPath = originalPath;
        _documentId = documentId == Guid.Empty ? Guid.NewGuid() : documentId;
        _timestamp = timestamp ?? DateTime.UtcNow;
        _status = status;
    }

    public RecoveryCopy(Guid documentId, string? originalPath, string? content)
        : this(content, originalPath, documentId)
    {
    }

    public RecoveryCopy(Guid documentId, string? originalPath, string? content, DateTime? timestamp)
        : this(content, originalPath, documentId, timestamp)
    {
    }

    public RecoveryCopy(Guid documentId, string? originalPath, string? content, DateTime? timestamp, RecoveryStatus status)
        : this(content, originalPath, documentId, timestamp, status)
    {
    }

    public RecoveryCopy(OpenDocument? document)
    {
        if (document is null) return;
        _documentId = document.Id;
        _originalPath = document.FilePath;
        _content = document.Content ?? string.Empty;
        _timestamp = DateTime.UtcNow;
        _status = RecoveryStatus.Valid;
    }

    public RecoveryCopy(RecoveryCopy? other)
    {
        if (other is null) return;
        _documentId = other._documentId;
        _originalPath = other._originalPath;
        _content = other._content;
        _timestamp = other._timestamp;
        _status = other._status;
        _storagePath = other._storagePath;
    }

    // Document id
    public Guid DocumentId
    {
        get => _documentId;
        set => _documentId = value == Guid.Empty ? _documentId : value;
    }

    public Guid Id
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid Identifier
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid DocId
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid DocumentIdentifier
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid DocumentGuid
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid RecoveryId
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid CopyId
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid IdDocumento
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid Identificador
    {
        get => _documentId;
        set => DocumentId = value;
    }

    public Guid IdentificadorDocumento
    {
        get => _documentId;
        set => DocumentId = value;
    }

    // Original path
    public string? OriginalPath
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? OriginalFilePath
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? SourcePath
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? SourceFile
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? FilePath
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? Path
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? File
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? Location
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? Route
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? AbsolutePath
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? Ruta
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? RutaOriginal
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? ArchivoOriginal
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? Original
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    public string? Source
    {
        get => _originalPath;
        set => _originalPath = value;
    }

    // Content
    public string Content
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string Text
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string Markdown
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string Body
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string RawContent
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string LogicalContent
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string Contenido
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string Texto
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    public string MarkdownContent
    {
        get => _content;
        set => _content = value ?? string.Empty;
    }

    // Timestamp
    public DateTime Timestamp
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime TimeStamp
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime CreatedAt
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime Created
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime Time
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime MarkedAt
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime Mark
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime LastModified
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime LastUpdated
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime LastSaved
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime Date
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime Fecha
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime FechaHora
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime MarcaDeTiempo
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public DateTime MarcaTiempo
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    // Status
    public RecoveryStatus Status
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus State
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus RecoveryStatus
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus Validity
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus Condition
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus CopyStatus
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus CopyState
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus Estado
    {
        get => _status;
        set => _status = value;
    }

    public RecoveryStatus EstadoRecuperacion
    {
        get => _status;
        set => _status = value;
    }

    // Storage path
    public string? StoragePath
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    public string? SavedPath
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    public string? LocalPath
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    public string? CopyPath
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    public string? RecoveryPath
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    public string? StorageFile
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    public string? LocalFile
    {
        get => _storagePath;
        set => _storagePath = value;
    }

    // Computed validity
    public bool IsValid => _status == RecoveryStatus.Valid || _status == RecoveryStatus.Vigente;

    public bool IsObsolete => !IsValid;

    public bool IsVigente => IsValid;

    public bool IsObsoleta => IsObsolete;

    public bool Valid => IsValid;

    public bool Obsolete => IsObsolete;

    // Mark methods
    public void MarkValid()
    {
        _status = RecoveryStatus.Valid;
    }

    public void MarkVigente()
    {
        _status = RecoveryStatus.Valid;
    }

    public void Validate()
    {
        _status = RecoveryStatus.Valid;
    }

    public void Activate()
    {
        _status = RecoveryStatus.Valid;
    }

    public void MarkObsolete()
    {
        _status = RecoveryStatus.Obsolete;
    }

    public void MarkObsoleta()
    {
        _status = RecoveryStatus.Obsolete;
    }

    public void MarkInvalid() => _status = RecoveryStatus.Obsolete;
    public void MarkInvalidated() => _status = RecoveryStatus.Obsolete;
    public void MarkStale() => _status = RecoveryStatus.Obsolete;
    public void MarkExpired() => _status = RecoveryStatus.Obsolete;
    public void MarkRevoked() => _status = RecoveryStatus.Obsolete;
    public void MarkDiscarded() => _status = RecoveryStatus.Obsolete;
    public void MarkCancelled() => _status = RecoveryStatus.Obsolete;
    public void MarkReset() => _status = RecoveryStatus.Obsolete;
    public void InvalidateCopy() => _status = RecoveryStatus.Obsolete;
    public void InvalidateRecovery() => _status = RecoveryStatus.Obsolete;
    public void InvalidateRecoveryCopy() => _status = RecoveryStatus.Obsolete;

    public bool IsInvalid => IsObsolete;
    public bool IsStale => IsObsolete;
    public bool IsExpired => IsObsolete;
    public void Invalidate()
    {
        MarkObsolete();
    }

    public void InvalidatePath(string path)
    {
        MarkObsolete();
        Delete(path);
    }
    public void MakeObsolete()
    {
        MarkObsolete();
    }

    public void Expire()
    {
        MarkObsolete();
    }

    public void Revoke()
    {
        MarkObsolete();
    }

    public void Discard()
    {
        MarkObsolete();
    }

    public void Cancel()
    {
        MarkObsolete();
    }

    public void Reset()
    {
        MarkObsolete();
    }

    public void Clear()
    {
        _content = string.Empty;
        MarkObsolete();
    }

    public void RefreshTimestamp()
    {
        _timestamp = DateTime.UtcNow;
    }

    public void UpdateTimestamp()
    {
        RefreshTimestamp();
    }

    public void Touch()
    {
        RefreshTimestamp();
    }

    // Persistence
    public void Save(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path", nameof(path));
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir)) System.IO.Directory.CreateDirectory(dir!);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(path, json);
        _storagePath = path;
    }

    public void Save()
    {
        Save(GetDefaultPath());
    }

    public void Persist(string path) => Save(path);
    public void Persist() => Save();
    public void Write(string path) => Save(path);
    public void Write() => Save();
    public static RecoveryCopy? Read(string path) => Load(path);
    public static RecoveryCopy? Read() => Load();
    public static bool TryRead(string path, out RecoveryCopy? copy) => TryLoad(path, out copy);
    public RecoveryCopy RestoreCopy() { MarkValid(); return this; }
    public static void Save(RecoveryCopy copy, string path)
    {
        if (copy is null) throw new ArgumentNullException(nameof(copy));
        copy.Save(path);
    }

    public static void Save(RecoveryCopy copy)
    {
        if (copy is null) throw new ArgumentNullException(nameof(copy));
        copy.Save();
    }

    public static RecoveryCopy? Load(string path)
    {
        if (TryLoad(path, out var copy)) return copy;
        return null;
    }

    public static RecoveryCopy? Load()
    {
        return Load(GetDefaultPath());
    }

    public static bool TryLoad(string path, out RecoveryCopy? copy)
    {
        copy = null;
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return false;
            var json = System.IO.File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<RecoveryCopy>(json);
            if (result is null) return false;
            copy = result;
            return true;
        }
        catch
        {
            copy = null;
            return false;
        }
    }

    public static bool TryLoad(out RecoveryCopy? copy)
    {
        return TryLoad(GetDefaultPath(), out copy);
    }

    public static void Delete(string path)
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
        if (!string.IsNullOrWhiteSpace(_storagePath)) Delete(_storagePath!);
        MarkObsolete();
    }

    public static void Invalidate(string path)
    {
        Delete(path);
    }

    public static bool Invalidate(RecoveryCopy? copy, string? path = null)
    {
        if (copy is not null) copy.MarkObsolete();
        var target = path ?? copy?._storagePath;
        if (!string.IsNullOrWhiteSpace(target)) Delete(target!);
        return true;
    }

    public static string GetDefaultPath()
    {
        return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EggMdEditor", "recovery.json");
    }

    public static string GetDefaultStoragePath()
    {
        return GetDefaultPath();
    }

    public static string GetPath()
    {
        return GetDefaultPath();
    }

    public RecoveryCopy Clone()
    {
        return new RecoveryCopy(this);
    }

    public static RecoveryCopy FromDocument(OpenDocument? document)
    {
        return new RecoveryCopy(document);
    }

    public static RecoveryCopy Create(string? content = null, string? originalPath = null, Guid documentId = default, DateTime? timestamp = null, RecoveryStatus status = RecoveryStatus.Valid)
    {
        return new RecoveryCopy(content, originalPath, documentId == Guid.Empty ? Guid.NewGuid() : documentId, timestamp, status);
    }

    public override string ToString()
    {
        return $"RecoveryCopy({_content.Length} chars, {_timestamp:O}, {_status})";
    }
}