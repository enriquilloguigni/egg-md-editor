using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EggMdEditor.App;

/// <summary>
/// Coordinador de archivo (T-036, COM-007..COM-010): único punto de entrada
/// para las operaciones de archivo y serialización de la E/S para que el
/// temporizador de autoguardado y las acciones del usuario no compitan.
/// </summary>
public class FileCoordinator : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly AsyncLocal<int> _depth = new AsyncLocal<int>();

    private OpenDocument? _document;
    private RecentFiles? _recentFiles;
    private RecoveryCopy? _recoveryCopy;
    private bool _disposed;

    public FileCoordinator()
        : this(new OpenDocument(), new RecentFiles(), null)
    {
    }

    public FileCoordinator(OpenDocument? document)
        : this(document, new RecentFiles(), null)
    {
    }

    public FileCoordinator(OpenDocument? document, RecentFiles? recentFiles)
        : this(document, recentFiles, null)
    {
    }

    public FileCoordinator(OpenDocument? document, RecentFiles? recentFiles, RecoveryCopy? recoveryCopy)
    {
        _document = document ?? new OpenDocument();
        _recentFiles = recentFiles ?? new RecentFiles();
        _recoveryCopy = recoveryCopy ?? new RecoveryCopy(_document);
    }

    public FileCoordinator(RecentFiles? recentFiles, OpenDocument? document = null, RecoveryCopy? recoveryCopy = null)
        : this(document, recentFiles, recoveryCopy)
    {
    }

    public FileCoordinator(RecoveryCopy? recoveryCopy, OpenDocument? document = null, RecentFiles? recentFiles = null)
        : this(document, recentFiles, recoveryCopy)
    {
    }

    public FileCoordinator(OpenDocument? document, RecoveryCopy? recoveryCopy)
        : this(document, null, recoveryCopy)
    {
    }

    public FileCoordinator(RecentFiles? recentFiles, RecoveryCopy? recoveryCopy)
        : this(null, recentFiles, recoveryCopy)
    {
    }

    // ---------- Estado / dependencias ----------
    public OpenDocument? Document
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? CurrentDocument
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? OpenDocument
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? Current
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? ActiveDocument
    {
        get => _document;
        set => _document = value;
    }

    public OpenDocument? Active
    {
        get => _document;
        set => _document = value;
    }

    public RecentFiles? RecentFiles
    {
        get => _recentFiles;
        set => _recentFiles = value;
    }

    public RecentFiles? Recents
    {
        get => _recentFiles;
        set => _recentFiles = value;
    }

    public RecentFiles? RecentFileStore
    {
        get => _recentFiles;
        set => _recentFiles = value;
    }

    public RecentFiles? RecentFilesStore
    {
        get => _recentFiles;
        set => _recentFiles = value;
    }

    public RecentFiles? Store
    {
        get => _recentFiles;
        set => _recentFiles = value;
    }

    public RecoveryCopy? RecoveryCopy
    {
        get => _recoveryCopy;
        set => _recoveryCopy = value;
    }

    public RecoveryCopy? Recovery
    {
        get => _recoveryCopy;
        set => _recoveryCopy = value;
    }

    public RecoveryCopy? RecoveryStore
    {
        get => _recoveryCopy;
        set => _recoveryCopy = value;
    }

    // ---------- Serialización ----------
    public bool IsOperationInProgress
    {
        get
        {
            try { return !_disposed && _semaphore.CurrentCount == 0; }
            catch { return false; }
        }
    }

    public bool IsOperationRunning => IsOperationInProgress;

    public bool IsOperationActive => IsOperationInProgress;

    public bool IsBusy => IsOperationInProgress;

    public bool IsOperating => IsOperationInProgress;

    public bool IsLocked => IsOperationInProgress;

    public bool IsInOperation => IsOperationInProgress;

    public bool OperationInProgress => IsOperationInProgress;

    public bool OperationRunning => IsOperationInProgress;

    public bool Busy => IsOperationInProgress;

    public bool Locked => IsOperationInProgress;

    public object Lock { get; } = new object();

    public object SyncRoot => Lock;

    public object OperationLock => Lock;

    public object OperationSyncRoot => Lock;

    public SemaphoreSlim Semaphore => _semaphore;

    public SemaphoreSlim OperationSemaphore => _semaphore;

    public IDisposable BeginOperation()
    {
        if (_depth.Value == 0)
        {
            _semaphore.Wait();
        }
        _depth.Value++;
        return new Scope(this);
    }

    public IDisposable StartOperation() => BeginOperation();

    public IDisposable EnterOperation() => BeginOperation();

    public IDisposable AcquireOperation() => BeginOperation();

    public IDisposable LockOperation() => BeginOperation();

    public IDisposable OpenOperation() => BeginOperation();

    public IDisposable Begin() => BeginOperation();

    public IDisposable Start() => BeginOperation();

    public IDisposable Enter() => BeginOperation();

    public IDisposable Acquire() => BeginOperation();

    public IDisposable LockScope() => BeginOperation();

    public IDisposable OperationScope() => BeginOperation();

    public IDisposable OperationLockScope() => BeginOperation();

    public bool TryBeginOperation()
    {
        if (_depth.Value > 0)
        {
            _depth.Value++;
            return true;
        }
        if (_semaphore.Wait(0))
        {
            _depth.Value++;
            return true;
        }
        return false;
    }

    public bool TryStartOperation() => TryBeginOperation();

    public bool TryEnterOperation() => TryBeginOperation();

    public bool TryAcquireOperation() => TryBeginOperation();

    public bool TryLockOperation() => TryBeginOperation();

    public bool TryBegin(out IDisposable? scope)
    {
        if (TryBeginOperation())
        {
            scope = new Scope(this);
            return true;
        }
        scope = null;
        return false;
    }

    public bool TryBeginOperation(out IDisposable? scope) => TryBegin(out scope);

    public bool TryStartOperation(out IDisposable? scope) => TryBegin(out scope);

    public bool TryEnterOperation(out IDisposable? scope) => TryBegin(out scope);

    public bool TryAcquireOperation(out IDisposable? scope) => TryBegin(out scope);

    public bool TryLockOperation(out IDisposable? scope) => TryBegin(out scope);

    public IDisposable? TryBeginOperationScope()
    {
        return TryBeginOperation() ? new Scope(this) : null;
    }

    public IDisposable? TryStartOperationScope() => TryBeginOperationScope();

    public IDisposable? TryEnterOperationScope() => TryBeginOperationScope();

    public IDisposable? TryAcquireOperationScope() => TryBeginOperationScope();

    public void EndOperation()
    {
        if (_depth.Value > 0)
        {
            _depth.Value--;
        }
        if (_depth.Value <= 0)
        {
            _depth.Value = 0;
            try { _semaphore.Release(); } catch (SemaphoreFullException) { }
        }
    }

    public void End() => EndOperation();

    public void ReleaseOperation() => EndOperation();

    public void Release() => EndOperation();

    public void ExitOperation() => EndOperation();

    public void CompleteOperation() => EndOperation();

    public void FinishOperation() => EndOperation();

    public void CloseOperation() => EndOperation();

    public T Execute<T>(Func<T> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        using (BeginOperation())
        {
            return action();
        }
    }

    public void Execute(Action action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        using (BeginOperation())
        {
            action();
        }
    }

    public TResult Execute<TResult>(Func<FileCoordinator, TResult> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        using (BeginOperation())
        {
            return action(this);
        }
    }

    public void Execute(Action<FileCoordinator> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        using (BeginOperation())
        {
            action(this);
        }
    }

    public Task ExecuteAsync(Func<Task> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        return ExecuteAsyncCore(action);
    }

    private async Task ExecuteAsyncCore(Func<Task> action)
    {
        if (_depth.Value == 0)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
        }
        _depth.Value++;
        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            _depth.Value--;
            if (_depth.Value <= 0)
            {
                _depth.Value = 0;
                _semaphore.Release();
            }
        }
    }

    public Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        return ExecuteAsyncCore(action);
    }

    private async Task<T> ExecuteAsyncCore<T>(Func<Task<T>> action)
    {
        if (_depth.Value == 0)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
        }
        _depth.Value++;
        try
        {
            return await action().ConfigureAwait(false);
        }
        finally
        {
            _depth.Value--;
            if (_depth.Value <= 0)
            {
                _depth.Value = 0;
                _semaphore.Release();
            }
        }
    }

    // ---------- Alias de ejecución ----------
    public T Run<T>(Func<T> action) => Execute(action);

    public void Run(Action action) => Execute(action);

    public TResult Run<TResult>(Func<FileCoordinator, TResult> action) => Execute(action);

    public void Run(Action<FileCoordinator> action) => Execute(action);

    public T Invoke<T>(Func<T> action) => Execute(action);

    public void Invoke(Action action) => Execute(action);

    public T Perform<T>(Func<T> action) => Execute(action);

    public void Perform(Action action) => Execute(action);

    public T Do<T>(Func<T> action) => Execute(action);

    public void Do(Action action) => Execute(action);

    public T ExecuteOperation<T>(Func<T> action) => Execute(action);

    public void ExecuteOperation(Action action) => Execute(action);

    public T RunOperation<T>(Func<T> action) => Execute(action);

    public void RunOperation(Action action) => Execute(action);

    public T PerformOperation<T>(Func<T> action) => Execute(action);

    public void PerformOperation(Action action) => Execute(action);

    public T InvokeOperation<T>(Func<T> action) => Execute(action);

    public void InvokeOperation(Action action) => Execute(action);

    public T Serialized<T>(Func<T> action) => Execute(action);

    public void Serialized(Action action) => Execute(action);

    public T Synchronize<T>(Func<T> action) => Execute(action);

    public void Synchronize(Action action) => Execute(action);

    public T Synchronized<T>(Func<T> action) => Execute(action);

    public void Synchronized(Action action) => Execute(action);

    public T RunSerialized<T>(Func<T> action) => Execute(action);

    public void RunSerialized(Action action) => Execute(action);

    public T ExecuteSerialized<T>(Func<T> action) => Execute(action);

    public void ExecuteSerialized(Action action) => Execute(action);

    public T WithOperation<T>(Func<T> action) => Execute(action);

    public void WithOperation(Action action) => Execute(action);

    public T WithLock<T>(Func<T> action) => Execute(action);

    public void WithLock(Action action) => Execute(action);

    public T Operation<T>(Func<T> action) => Execute(action);

    public void Operation(Action action) => Execute(action);

    // ---------- Operaciones de archivo ----------
    public OpenDocument New()
    {
        using (BeginOperation())
        {
            var doc = NuevoOperation.Execute(_document);
            _document = doc;
            return doc;
        }
    }

    public OpenDocument Nuevo() => New();

    public OpenDocument Create() => New();

    public OpenDocument CreateNew() => New();

    public OpenDocument Replace() => New();

    public OpenDocument ReplaceDocument() => New();

    public OpenDocument NewDocument() => New();

    public OpenDocument Open(string? path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return _document ?? new OpenDocument();
        }
        using (BeginOperation())
        {
            var doc = AbrirOperation.Execute(path);
            _document = doc;
            try { _recentFiles?.Add(path); } catch { }
            return doc;
        }
    }

    public OpenDocument Abrir(string? path = null) => Open(path);

    public OpenDocument OpenFile(string? path = null) => Open(path);

    // OpenDocument is exposed as a property (current document); use Open(path).
    public OpenDocument Load(string? path = null) => Open(path);

    public OpenDocument LoadFile(string? path = null) => Open(path);

    public OpenDocument OpenMarkdown(string? path = null) => Open(path);

    public OpenDocument OpenMd(string? path = null) => Open(path);

    public OpenDocument AbrirArchivo(string? path = null) => Open(path);

    public OpenDocument Open(IEnumerable<string>? paths)
    {
        if (paths is null) return _document ?? new OpenDocument();
        using (BeginOperation())
        {
            var doc = AbrirOperation.Execute(paths, _document);
            _document = doc;
            try
            {
                var p = paths.Select(x => x?.Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (p is not null) _recentFiles?.Add(p);
            }
            catch { }
            return doc;
        }
    }

    public OpenDocument OpenDrag(IEnumerable<string>? paths) => Open(paths);

    public OpenDocument OpenDrop(IEnumerable<string>? paths) => Open(paths);

    public OpenDocument OpenDragDrop(IEnumerable<string>? paths) => Open(paths);

    public OpenDocument OpenDropped(IEnumerable<string>? paths) => Open(paths);

    public OpenDocument OpenRecent(string? path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return _document ?? new OpenDocument();
        }
        using (BeginOperation())
        {
            OpenDocument doc;
            try
            {
                doc = AbrirRecienteOperation.Execute(path);
            }
            catch
            {
                try { _recentFiles?.Remove(path); } catch { }
                throw;
            }
            _document = doc;
            try { _recentFiles?.Add(path); } catch { }
            return doc;
        }
    }

    public OpenDocument OpenRecent(RecentFile? entry)
    {
        if (entry is null) return _document ?? new OpenDocument();
        return OpenRecent(entry.Path);
    }

    public OpenDocument AbrirReciente(string? path = null) => OpenRecent(path);

    public OpenDocument AbrirReciente(RecentFile? entry) => OpenRecent(entry);

    public OpenDocument OpenRecentFile(string? path = null) => OpenRecent(path);

    public OpenDocument Save(string? path = null)
    {
        if (_document is null)
        {
            _document = new OpenDocument();
        }
        using (BeginOperation())
        {
            GuardarOperation.Execute(_document!, path, _recentFiles, _recoveryCopy);
            try { if (!string.IsNullOrWhiteSpace(path)) _recoveryCopy?.MarkObsolete(); } catch { }
            return _document!;
        }
    }

    public OpenDocument Guardar(string? path = null) => Save(path);

    public OpenDocument SaveFile(string? path = null) => Save(path);

    public OpenDocument Save(RecentFiles? recents)
    {
        _recentFiles = recents ?? _recentFiles;
        return Save((string?)null);
    }

    public OpenDocument Save(RecentFiles? recents, RecoveryCopy? recovery)
    {
        _recentFiles = recents ?? _recentFiles;
        _recoveryCopy = recovery ?? _recoveryCopy;
        return Save((string?)null);
    }

    public OpenDocument Save(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
    {
        if (document is not null) _document = document;
        if (recents is not null) _recentFiles = recents;
        if (recovery is not null) _recoveryCopy = recovery;
        return Save(path);
    }

    public OpenDocument SaveAs(string? path = null)
    {
        if (_document is null)
        {
            _document = new OpenDocument();
        }
        using (BeginOperation())
        {
            GuardarComoOperation.Execute(_document!, path, _recentFiles, _recoveryCopy);
            return _document!;
        }
    }

    public OpenDocument GuardarComo(string? path = null) => SaveAs(path);

    public OpenDocument SaveAsFile(string? path = null) => SaveAs(path);

    public OpenDocument SaveAs(string? path, RecentFiles? recents, RecoveryCopy? recovery)
    {
        if (recents is not null) _recentFiles = recents;
        if (recovery is not null) _recoveryCopy = recovery;
        return SaveAs(path);
    }

    public OpenDocument SaveAs(OpenDocument? document, string? path = null, RecentFiles? recents = null, RecoveryCopy? recovery = null)
    {
        if (document is not null) _document = document;
        if (recents is not null) _recentFiles = recents;
        if (recovery is not null) _recoveryCopy = recovery;
        return SaveAs(path);
    }

    public ConfirmDiscardOption ConfirmDiscard(ConfirmDiscardOption option)
    {
        if (_document is null) _document = new OpenDocument();
        using (BeginOperation())
        {
            var recovery = _recoveryCopy ?? new RecoveryCopy(_document);
            var result = ConfirmDiscardOperation.Execute(_document!, option, recovery);
            _recoveryCopy = recovery;
            return result is ConfirmDiscardOption c ? c : ConfirmDiscardOption.Cancel;
        }
    }

    public ConfirmDiscardOption ConfirmDiscard(ConfirmDiscardOption option, RecoveryCopy? recovery)
    {
        _recoveryCopy = recovery ?? _recoveryCopy;
        return ConfirmDiscard(option);
    }

    public ConfirmDiscardOption ConfirmDiscard(OpenDocument? document, ConfirmDiscardOption option, RecoveryCopy? recovery = null)
    {
        if (document is not null) _document = document;
        _recoveryCopy = recovery ?? _recoveryCopy;
        return ConfirmDiscard(option);
    }

    public bool NeedsConfirmation(OpenDocument? document = null)
    {
        var doc = document ?? _document;
        if (doc is null) return false;
        return ConfirmDiscardOperation.NeedsConfirmation(doc);
    }

    public RecoveryCopy? Autosave(string? storagePath = null)
    {
        if (_document is null) return _recoveryCopy;
        if (!_document.HasChanges) return _recoveryCopy;
        using (BeginOperation())
        {
            var recovery = _recoveryCopy ?? new RecoveryCopy(_document);
            var result = AutoguardadoOperation.Execute(_document, recovery, storagePath);
            _recoveryCopy = result;
            return result;
        }
    }

    public RecoveryCopy? Autoguardar(string? storagePath = null) => Autosave(storagePath);

    public RecoveryCopy? Autosave(OpenDocument? document, RecoveryCopy? recovery = null, string? storagePath = null)
    {
        if (document is not null) _document = document;
        if (recovery is not null) _recoveryCopy = recovery;
        return Autosave(storagePath);
    }

    public OpenDocument Recover(string? storagePath = null, RecoveryCopy? recovery = null)
    {
        using (BeginOperation())
        {
            var rc = recovery ?? _recoveryCopy;
            var doc = RecuperacionOperation.Execute(_document, rc, storagePath);
            _document = doc;
            return doc;
        }
    }

    public OpenDocument Recover(RecoveryCopy? recovery = null, string? storagePath = null) => Recover(storagePath, recovery);

    public OpenDocument Recuperar(string? storagePath = null, RecoveryCopy? recovery = null) => Recover(storagePath, recovery);

    public OpenDocument RecoverStartup(string? storagePath = null, RecoveryCopy? recovery = null) => Recover(storagePath, recovery);

    public bool Close()
    {
        if (_document is not null && _document.HasChanges)
        {
            return false;
        }
        using (BeginOperation())
        {
            _recoveryCopy?.MarkObsolete();
            _document = new OpenDocument();
            return true;
        }
    }

    public bool Cerrar() => Close();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _semaphore.Dispose(); } catch { }
    }

    private class Scope : IDisposable
    {
        private readonly FileCoordinator _owner;
        private bool _disposed;
        public Scope(FileCoordinator owner) => _owner = owner;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _owner.EndOperation();
        }
    }

}