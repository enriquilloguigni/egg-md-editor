using System.ComponentModel;
using System.IO;

namespace EggMdEditor.App;

public class OpenDocument : INotifyPropertyChanged, IDisposable
{
    private Guid _id = Guid.NewGuid();
    private string? _filePath;
    private string _displayName = "Sin título";
    private string _content = string.Empty;
    private bool _isChangingContent;
    private DocumentEncoding _encoding = DocumentEncoding.Utf8;
    private LineEnding _lineEnding = OperatingSystem.IsWindows() ? LineEnding.CRLF : LineEnding.LF;
    private DocumentChangeState _changeState = DocumentChangeState.Clean;
    private DocumentOrigin _origin = DocumentOrigin.New;
    private DocumentLastOperation _lastOperation = DocumentLastOperation.None;
    private MetadataBlock _metadata = new MetadataBlock();
    private bool _disposed;

    public OpenDocument()
    {
    }

    public OpenDocument(string? content)
        : this(content, null, DocumentOrigin.New, DocumentLastOperation.None)
    {
    }

    public OpenDocument(string? content, string? filePath)
        : this(content, filePath, DocumentOrigin.New, DocumentLastOperation.None)
    {
    }

    public OpenDocument(string? content, string? filePath, DocumentOrigin origin, DocumentLastOperation lastOperation)
    {
        _content = content ?? string.Empty;
        FilePath = filePath;
        _origin = origin;
        _lastOperation = lastOperation;
        _metadata = new MetadataBlock();
    }

    public OpenDocument(OpenDocument other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        _id = Guid.NewGuid();
        _filePath = other._filePath;
        _displayName = other._displayName;
        _content = other._content;
        _encoding = other._encoding;
        _lineEnding = other._lineEnding;
        _changeState = other._changeState;
        _origin = other._origin;
        _lastOperation = other._lastOperation;
        _metadata = new MetadataBlock(other._metadata.IsPresent, other._metadata.Content, other._metadata.Delimiters)
        {
            Position = other._metadata.Position,
        };
    }

    public Guid Id
    {
        get => _id;
        init => _id = value;
    }

    public Guid Identifier
    {
        get => _id;
        set => _id = value;
    }

    public string? FilePath
    {
        get => _filePath;
        set
        {
            if (string.Equals(_filePath, value, StringComparison.Ordinal)) return;
            _filePath = value;
            _displayName = string.IsNullOrWhiteSpace(value) ? "Sin título" : System.IO.Path.GetFileName(value);
            OnPropertyChanged(nameof(FilePath));
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(AbsolutePath));
            OnPropertyChanged(nameof(Route));
            OnPropertyChanged(nameof(Location));
            OnPropertyChanged(nameof(File));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(VisibleName));
        }
    }

    public string? Path
    {
        get => _filePath;
        set => FilePath = value;
    }

    public string? AbsolutePath
    {
        get => _filePath;
        set => FilePath = value;
    }

    public string? Route
    {
        get => _filePath;
        set => FilePath = value;
    }

    public string? Location
    {
        get => _filePath;
        set => FilePath = value;
    }

    public string? File
    {
        get => _filePath;
        set => FilePath = value;
    }

    public string DisplayName
    {
        get => _displayName;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "Sin título" : value!;
            if (string.Equals(_displayName, normalized, StringComparison.Ordinal)) return;
            _displayName = normalized;
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(VisibleName));
        }
    }

    public string Title
    {
        get => _displayName;
        set => DisplayName = value;
    }

    public string Name
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

    public string Content
    {
        get => _content;
        set
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(_content, normalized, StringComparison.Ordinal)) return;
            _content = normalized;
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(Markdown));
            OnPropertyChanged(nameof(Body));
            OnPropertyChanged(nameof(RawContent));
            OnPropertyChanged(nameof(LogicalContent));
            if (!_isChangingContent)
            {
                _isChangingContent = true;
                try
                {
                    var args = new ContentChangedEventArgs(normalized)
                    {
                        SourcePanel = Synchronizer.CurrentSourcePanel,
                        Panel = Synchronizer.CurrentSourcePanel,
                        Document = this,
                    };
                    ContentChanged?.Invoke(this, args);
                    ContentChangedEvent?.Invoke(this, EventArgs.Empty);
                    Changed?.Invoke(this, EventArgs.Empty);
                    ContentUpdated?.Invoke(this, EventArgs.Empty);
                    ContentModified?.Invoke(this, EventArgs.Empty);
                    TextChanged?.Invoke(this, EventArgs.Empty);
                    MarkdownChanged?.Invoke(this, EventArgs.Empty);
                    BodyChanged?.Invoke(this, EventArgs.Empty);
                    Synced?.Invoke(this, EventArgs.Empty);
                    Synchronized?.Invoke(this, EventArgs.Empty);
                    Updated?.Invoke(this, EventArgs.Empty);
                    ModifiedEvent?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    _isChangingContent = false;
                }
            }
        }
    }

    public string Text
    {
        get => _content;
        set => Content = value;
    }

    public string Markdown
    {
        get => _content;
        set => Content = value;
    }

    public string Body
    {
        get => _content;
        set => Content = value;
    }

    public string RawContent
    {
        get => _content;
        set => Content = value;
    }

    public string LogicalContent
    {
        get => _content;
        set => Content = value;
    }

    public DocumentEncoding Encoding
    {
        get => _encoding;
        set
        {
            if (_encoding == value) return;
            _encoding = value;
            OnPropertyChanged(nameof(Encoding));
            OnPropertyChanged(nameof(DocumentEncoding));
            OnPropertyChanged(nameof(Charset));
        }
    }

    public DocumentEncoding DocumentEncoding
    {
        get => _encoding;
        set => Encoding = value;
    }

    public DocumentEncoding Charset
    {
        get => _encoding;
        set => Encoding = value;
    }

    public LineEnding LineEnding
    {
        get => _lineEnding;
        set
        {
            if (_lineEnding == value) return;
            _lineEnding = value;
            OnPropertyChanged(nameof(LineEnding));
            OnPropertyChanged(nameof(LineEndings));
            OnPropertyChanged(nameof(NewLine));
            OnPropertyChanged(nameof(Eol));
        }
    }

    public LineEnding LineEndings
    {
        get => _lineEnding;
        set => LineEnding = value;
    }

    public LineEnding NewLine
    {
        get => _lineEnding;
        set => LineEnding = value;
    }

    public LineEnding Eol
    {
        get => _lineEnding;
        set => LineEnding = value;
    }

    public DocumentChangeState ChangeState
    {
        get => _changeState;
        set
        {
            if (_changeState == value) return;
            _changeState = value;
            OnPropertyChanged(nameof(ChangeState));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(ModificationState));
            OnPropertyChanged(nameof(DirtyState));
            OnPropertyChanged(nameof(HasChanges));
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(Modified));
        }
    }

    public DocumentChangeState State
    {
        get => _changeState;
        set => ChangeState = value;
    }

    public DocumentChangeState Status
    {
        get => _changeState;
        set => ChangeState = value;
    }

    public DocumentChangeState ModificationState
    {
        get => _changeState;
        set => ChangeState = value;
    }

    public DocumentChangeState DirtyState
    {
        get => _changeState;
        set => ChangeState = value;
    }

    public bool HasChanges => _changeState == DocumentChangeState.Modified;
    public bool IsDirty => HasChanges;
    public bool IsModified => HasChanges;
    public bool Modified => HasChanges;

    public DocumentOrigin Origin
    {
        get => _origin;
        set
        {
            if (_origin == value) return;
            _origin = value;
            OnPropertyChanged(nameof(Origin));
            OnPropertyChanged(nameof(Source));
            OnPropertyChanged(nameof(DocumentOrigin));
        }
    }

    public DocumentOrigin Source
    {
        get => _origin;
        set => Origin = value;
    }

    public DocumentOrigin DocumentOrigin
    {
        get => _origin;
        set => Origin = value;
    }

    public DocumentLastOperation LastOperation
    {
        get => _lastOperation;
        set
        {
            if (_lastOperation == value) return;
            _lastOperation = value;
            OnPropertyChanged(nameof(LastOperation));
            OnPropertyChanged(nameof(LastAction));
            OnPropertyChanged(nameof(LastFileOperation));
        }
    }

    public DocumentLastOperation LastAction
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public DocumentLastOperation LastFileOperation
    {
        get => _lastOperation;
        set => LastOperation = value;
    }

    public MetadataBlock Metadata
    {
        get => _metadata;
        set
        {
            var normalized = value ?? new MetadataBlock();
            if (ReferenceEquals(_metadata, normalized)) return;
            _metadata = normalized;
            OnPropertyChanged(nameof(Metadata));
            OnPropertyChanged(nameof(MetadataBlock));
            OnPropertyChanged(nameof(FrontMatter));
            OnPropertyChanged(nameof(Frontmatter));
            OnPropertyChanged(nameof(Block));
        }
    }

    public MetadataBlock MetadataBlock
    {
        get => _metadata;
        set => Metadata = value;
    }

    public MetadataBlock FrontMatter
    {
        get => _metadata;
        set => Metadata = value;
    }

    public MetadataBlock Frontmatter
    {
        get => _metadata;
        set => Metadata = value;
    }

    public MetadataBlock Block
    {
        get => _metadata;
        set => Metadata = value;
    }

    public Guid Identificador { get => _id; set => _id = value; }
    public string? Ruta { get => _filePath; set => FilePath = value; }
    public string NombreVisible { get => _displayName; set => DisplayName = value; }
    public string Contenido { get => _content; set => Content = value; }
    public DocumentEncoding Codificacion { get => _encoding; set => Encoding = value; }
    public LineEnding FinDeLinea { get => _lineEnding; set => LineEnding = value; }
    public DocumentChangeState EstadoCambios { get => _changeState; set => ChangeState = value; }
    public DocumentOrigin Origen { get => _origin; set => Origin = value; }
    public DocumentLastOperation UltimaOperacion { get => _lastOperation; set => LastOperation = value; }
    public MetadataBlock BloqueMetadatos { get => _metadata; set => Metadata = value; }

    public void MarkChanged() => ChangeState = DocumentChangeState.Modified;
    public void MarkClean() => ChangeState = DocumentChangeState.Clean;
    public void MarkModified() => ChangeState = DocumentChangeState.Modified;
    public void MarkDirty() => ChangeState = DocumentChangeState.Modified;

    public OpenDocument Save(string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
    {
        GuardarOperation.Execute(this, path, recents, recovery);
        return this;
    }
    public OpenDocument Save(RecentFiles? recents) { GuardarOperation.Execute(this, null, recents, null); return this; }
    public OpenDocument Save(RecentFiles? recents, RecoveryCopy? recovery) { GuardarOperation.Execute(this, null, recents, recovery); return this; }
    public OpenDocument Guardar(string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
    {
        GuardarOperation.Execute(this, path, recents, recovery);
        return this;
    }
    public OpenDocument SaveAs(string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
    {
        GuardarComoOperation.Execute(this, path, recents, recovery);
        return this;
    }
    public OpenDocument GuardarComo(string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
    {
        GuardarComoOperation.Execute(this, path, recents, recovery);
        return this;
    }

    // ---------- Operation Nuevo (T-025) ----------
    public static OpenDocument New() => new OpenDocument();
    public static OpenDocument Nuevo() => new OpenDocument();
    public static OpenDocument CreateNew() => new OpenDocument();
    public static OpenDocument Create() => new OpenDocument();
    public static OpenDocument Empty() => new OpenDocument();
    public static OpenDocument Blank() => new OpenDocument();

    // ---------- Operation Abrir (T-026) ----------
    public static OpenDocument Open(string? path) => AbrirOperation.Execute(path);

    // ---------- Operation Abrir reciente (T-027) ----------
    public static OpenDocument OpenRecent(string? path) => AbrirRecienteOperation.Execute(path);
    public static OpenDocument OpenRecent(RecentFile? entry) => AbrirRecienteOperation.Execute(entry);
    public static OpenDocument AbrirReciente(string? path) => AbrirRecienteOperation.Execute(path);
    public static OpenDocument AbrirReciente(RecentFile? entry) => AbrirRecienteOperation.Execute(entry);
    public static OpenDocument OpenRecentFile(string? path) => AbrirRecienteOperation.Execute(path);
    public static OpenDocument LoadRecent(string? path) => AbrirRecienteOperation.Execute(path);
    public static OpenDocument Abrir(string? path) => AbrirOperation.Execute(path);

    // ---------- Operation Guardar (T-028) ----------
    public static OpenDocument Save(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarOperation.Execute(document, path, recents, recovery);
    public static OpenDocument Save(OpenDocument? document, RecentFiles? recents) => GuardarOperation.Execute(document, null, recents, null);
    public static OpenDocument Save(OpenDocument? document, RecentFiles? recents, RecoveryCopy? recovery) => GuardarOperation.Execute(document, null, recents, recovery);
    public static OpenDocument Save(OpenDocument? document, string? path, RecentFiles? recents) => GuardarOperation.Execute(document, path, recents, null);
    public static OpenDocument Guardar(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarOperation.Execute(document, path, recents, recovery);
    public static OpenDocument SaveDocument(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarOperation.Execute(document, path, recents, recovery);
    public static OpenDocument SaveFile(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarOperation.Execute(document, path, recents, recovery);
    public static OpenDocument SaveCurrent(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarOperation.Execute(document, path, recents, recovery);

    // ---------- Operation Recuperación (T-031) ----------
    public OpenDocument Recover() => RecuperacionOperation.Execute(this);
    public OpenDocument Recuperar() => RecuperacionOperation.Execute(this);

    public static OpenDocument Recover(RecoveryCopy? recovery = null, string? storagePath = null)
        => RecuperacionOperation.Execute(recovery, storagePath);
    public static OpenDocument Recover(string? storagePath, RecoveryCopy? recovery = null)
        => RecuperacionOperation.Execute(storagePath, recovery);
    public static OpenDocument Recover(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => RecuperacionOperation.Execute(document, recovery, storagePath);

    public static OpenDocument Recuperar(RecoveryCopy? recovery = null, string? storagePath = null)
        => RecuperacionOperation.Execute(recovery, storagePath);
    public static OpenDocument Recuperar(string? storagePath, RecoveryCopy? recovery = null)
        => RecuperacionOperation.Execute(storagePath, recovery);
    public static OpenDocument Recuperar(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => RecuperacionOperation.Execute(document, recovery, storagePath);

    public static OpenDocument RecoverStartup(RecoveryCopy? recovery = null, string? storagePath = null)
        => RecuperacionOperation.Execute(recovery, storagePath);
    public static OpenDocument Restore(RecoveryCopy? recovery = null, string? storagePath = null)
        => RecuperacionOperation.Execute(recovery, storagePath);
    public static OpenDocument AutoRecover(RecoveryCopy? recovery = null, string? storagePath = null)
        => RecuperacionOperation.Execute(recovery, storagePath);
    // ---------- Operation Guardar como (T-028) ----------
    public RecoveryCopy Autosave(RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(this, recovery, storagePath);
    public RecoveryCopy Autoguardar(RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(this, recovery, storagePath);
    public RecoveryCopy SaveRecovery(RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(this, recovery, storagePath);

    public static RecoveryCopy Autosave(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(document, recovery, storagePath);
    public static RecoveryCopy Autoguardar(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(document, recovery, storagePath);
    public static RecoveryCopy SaveRecovery(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
        => AutoguardadoOperation.Execute(document, recovery, storagePath);

    // ---------- Operation Guardar como (T-028) ----------
    public static OpenDocument SaveAs(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarComoOperation.Execute(document, path, recents, recovery);
    public static OpenDocument GuardarComo(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarComoOperation.Execute(document, path, recents, recovery);
    public static OpenDocument SaveAsCopy(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarComoOperation.Execute(document, path, recents, recovery);
    public static OpenDocument SaveCopy(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarComoOperation.Execute(document, path, recents, recovery);
    public static OpenDocument SaveTo(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
        => GuardarComoOperation.Execute(document, path, recents, recovery);
    public static OpenDocument OpenFile(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument OpenDocumentFile(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument OpenMarkdown(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument OpenMd(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument Load(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument LoadFile(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument LoadDocument(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument LoadMarkdown(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument LoadMd(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument FromFile(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument FromPath(string? path) => AbrirOperation.Execute(path);
    public static OpenDocument ReadFile(string? path) => AbrirOperation.Execute(path);

    public OpenDocument ReplaceWithNew() => new OpenDocument();
    public OpenDocument ToNew() => new OpenDocument();
    public OpenDocument NuevoYReemplazar() => new OpenDocument();
    public OpenDocument AsNew() => new OpenDocument();

    public bool IsNewDocument => Origin == DocumentOrigin.New && string.IsNullOrWhiteSpace(FilePath) && !HasChanges && string.IsNullOrEmpty(Content);
    public bool CanReplace => !HasChanges;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<ContentChangedEventArgs>? ContentChanged;
    public event EventHandler? ContentChangedEvent;
    public event EventHandler? Changed;
    public event EventHandler? ContentUpdated;
    public event EventHandler? ContentModified;
    public event EventHandler? TextChanged;
    public event EventHandler? MarkdownChanged;
    public event EventHandler? BodyChanged;
    public event EventHandler? Synced;
    public event EventHandler? Synchronized;
    public event EventHandler? Updated;
    public event EventHandler? ModifiedEvent;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public static bool SyncEdit(OpenDocument? document, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool SyncEdit(OpenDocument? document, string? newContent) => document is null ? false : document.SyncEdit(newContent);
    public bool SyncEdit(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public bool SyncEdit(string? newContent)
    {
        if (Synchronizer.IsSyncing) return false;
        Synchronizer.Begin();
        Synchronizer.CurrentSourcePanel = null;
        try
        {
            Content = newContent ?? string.Empty;
            return true;
        }
        finally
        {
            Synchronizer.CurrentSourcePanel = null;
            Synchronizer.End();
        }
    }
    public bool Sync(string? newContent) => SyncEdit(newContent);
    public bool Synchronize(string? newContent) => SyncEdit(newContent);
    public bool UpdateContent(string? newContent) => SyncEdit(newContent);
    public bool SetContent(string? newContent) { Content = newContent ?? string.Empty; return true; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}

public class Document : OpenDocument
{
    public Document() : base() { }
    public Document(string? content) : base(content) { }
    public Document(string? content, string? filePath) : base(content, filePath) { }
    public Document(string? content, string? filePath, DocumentOrigin origin, DocumentLastOperation lastOperation)
        : base(content, filePath, origin, lastOperation) { }
}

public class MarkdownDocument : OpenDocument
{
    public MarkdownDocument() : base() { }
    public MarkdownDocument(string? content) : base(content) { }
    public MarkdownDocument(string? content, string? filePath) : base(content, filePath) { }
    public MarkdownDocument(string? content, string? filePath, DocumentOrigin origin, DocumentLastOperation lastOperation)
        : base(content, filePath, origin, lastOperation) { }
}

public class MdDocument : OpenDocument
{
    public MdDocument() : base() { }
    public MdDocument(string? content) : base(content) { }
    public MdDocument(string? content, string? filePath) : base(content, filePath) { }
    public MdDocument(string? content, string? filePath, DocumentOrigin origin, DocumentLastOperation lastOperation)
        : base(content, filePath, origin, lastOperation) { }
}

public class OpenMarkdownDocument : OpenDocument
{
    public OpenMarkdownDocument() : base() { }
    public OpenMarkdownDocument(string? content) : base(content) { }
    public OpenMarkdownDocument(string? content, string? filePath) : base(content, filePath) { }
    public OpenMarkdownDocument(string? content, string? filePath, DocumentOrigin origin, DocumentLastOperation lastOperation)
        : base(content, filePath, origin, lastOperation) { }
}