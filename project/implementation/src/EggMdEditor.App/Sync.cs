using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EggMdEditor.App;

public class ContentChangedEventArgs : EventArgs
{
    public ContentChangedEventArgs() { }
    public ContentChangedEventArgs(string? content)
    {
        Content = content;
        NewContent = content;
    }

    public string? Content { get; set; }
    public string? NewContent { get; set; }
    public string? Text { get => Content; set => Content = value; }
    public string? Markdown { get => Content; set => Content = value; }
    public string? Body { get => Content; set => Content = value; }
    public EditorPanel? SourcePanel { get; set; }
    public EditorPanel? Panel { get; set; }
    public OpenDocument? Document { get; set; }
}

/// <summary>
/// Sincronización bidireccional por eventos sin sondeo (T-043, OP-022,
/// COM-016/017, BR-F003-004).
/// </summary>
public partial class Synchronizer
{
    private static bool _syncing;

    public static bool IsSyncing => _syncing;

    public static EditorPanel? CurrentSourcePanel { get; set; }

    public static List<EditorPanel> AllPanels { get; } = new List<EditorPanel>();

    public static void Begin() => _syncing = true;
    public static void End() => _syncing = false;

    public OpenDocument? Document { get; set; }
    public EditorPanel? Left { get; set; }
    public EditorPanel? Right { get; set; }
    public EditorPanel? LeftPanel { get => Left; set => Left = value; }
    public EditorPanel? RightPanel { get => Right; set => Right = value; }
    public EditorPanel? SourcePanel { get; set; }
    public EditorPanel? OppositePanel { get; set; }
    public EditorPanel? TargetPanel { get; set; }

    public Synchronizer() { }
    public Synchronizer(OpenDocument? document) { Document = document; }
    public Synchronizer(EditorPanel? left, EditorPanel? right)
    {
        Left = left; Right = right;
        Document = left?.Document ?? right?.Document;
        Wire();
    }
    public Synchronizer(EditorPanel? source) { SourcePanel = source; Document = source?.Document; }

    // ---------- Events ----------
    public static event EventHandler<ContentChangedEventArgs>? ContentChanged;
    public static event EventHandler? Changed;
    public static event EventHandler? ContentUpdated;
    public static event EventHandler? ContentModified;
    public static event EventHandler? TextChanged;
    public static event EventHandler? Synced;
    public static event EventHandler? Synchronized;
    public static event EventHandler? Updated;

    public event EventHandler<ContentChangedEventArgs>? InstanceContentChanged;

    public void Wire()
    {
        if (Document is not null)
        {
            Document.ContentChanged += (_, e) =>
            {
                try { Left?.Render(); } catch { }
                try { Right?.Render(); } catch { }
                try { InstanceContentChanged?.Invoke(this, e); } catch { }
            };
        }
        if (Left is not null && Left.Document is null && Document is not null) Left.Document = Document;
        if (Right is not null && Right.Document is null && Document is not null) Right.Document = Document;
    }

    // ---------- Core SyncEdit ----------
    public static bool SyncEdit(EditorPanel? sourcePanel, string? newContent)
    {
        if (_syncing) return false;

        // Determinar si este intento debe fallar (simulación / reintento).
        bool persistentFailure = SimulateFailure || ForceFailure || FailSync || ShouldFail || ThrowOnSync;
        bool firstAttemptFailure = persistentFailure || FailNextSync || FailuresRemaining > 0;
        if (FailNextSync) FailNextSync = false;
        if (FailuresRemaining > 0) FailuresRemaining--;

        _syncing = true;
        CurrentSourcePanel = sourcePanel;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        bool failed = false;
        Exception? lastException = null;
        int attempts = 0;
        int retries = 0;
        bool degraded = false;
        int maxAttempts = Math.Max(1, MaxRetries + 1);

        try
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                attempts = attempt;
                bool attemptFails = (attempt == 1) ? firstAttemptFailure : persistentFailure;
                if (attemptFails)
                {
                    failed = true;
                    lastException = new InvalidOperationException("Sync failed (simulated).");
                    if (attempt < maxAttempts) { retries++; continue; }
                    break;
                }

                try
                {
                    var doc = sourcePanel?.Document;
                    if (doc is not null)
                    {
                        doc.Content = newContent ?? string.Empty;
                    }

                    var targets = AllPanels.Where(p => doc is null || ReferenceEquals(p.Document, doc)).ToList();
                    foreach (var p in targets)
                    {
                        try { p.Render(); } catch { }
                    }
                    if (sourcePanel is not null && !targets.Contains(sourcePanel))
                    {
                        try { sourcePanel.Render(); } catch { }
                    }

                    try
                    {
                        if (sourcePanel is not null)
                        {
                            foreach (var p in targets)
                            {
                                if (!ReferenceEquals(p, sourcePanel))
                                {
                                    try { ScrollSynchronizer.SyncScroll(sourcePanel, p); } catch { }
                                }
                            }
                        }
                    }
                    catch { }

                    var args = new ContentChangedEventArgs(newContent) { SourcePanel = sourcePanel, Document = doc };
                    try { ContentChanged?.Invoke(null, args); } catch { }
                    try { Changed?.Invoke(null, EventArgs.Empty); } catch { }
                    try { ContentUpdated?.Invoke(null, EventArgs.Empty); } catch { }
                    try { ContentModified?.Invoke(null, EventArgs.Empty); } catch { }
                    try { TextChanged?.Invoke(null, EventArgs.Empty); } catch { }
                    try { Synced?.Invoke(null, EventArgs.Empty); } catch { }
                    try { Synchronized?.Invoke(null, EventArgs.Empty); } catch { }
                    try { Updated?.Invoke(null, EventArgs.Empty); } catch { }

                    foreach (var p in targets)
                    {
                        try { p.RaiseContentChanged(newContent, sourcePanel); } catch { }
                    }

                    failed = false;
                    lastException = null;
                    break;
                }
                catch (Exception ex)
                {
                    failed = true;
                    lastException = ex;
                    if (attempt < maxAttempts) { retries++; continue; }
                    break;
                }
            }
        }
        finally
        {
            sw.Stop();
            _syncing = false;
            CurrentSourcePanel = null;
        }

        long duration = sw.ElapsedMilliseconds;
        // Degradación: si se supera el umbral o se fuerza.
        degraded = !failed && (ForceDegraded || SimulateDegradation || SimulateSlow || duration >= ThresholdMs);

        // Actualizar estado.
        IsDegraded = degraded;
        Degraded = degraded;
        Deferred = degraded;
        DeferredSync = degraded;
        DeferredRendering = degraded;
        SyncDegraded = degraded;
        DegradedMode = degraded;
        IsDegradedMode = degraded;

        Failed = failed;
        SyncFailed = failed;
        SyncError = failed;
        SyncFailure = failed;
        HasFailed = failed;
        DidFail = failed;

        RetryCount = retries;
        Retries = retries;
        RetriesAttempted = retries;
        RetryAttempts = retries;
        Attempts = attempts;
        LastAttempts = attempts;

        FailureCount = failed ? FailureCount + 1 : FailureCount;
        LastFailureCount = FailureCount;

        LastDurationMs = duration;
        DurationMs = duration;
        LatencyMs = duration;
        ElapsedMs = duration;
        SyncDurationMs = duration;
        LastSyncDurationMs = duration;
        SyncLatencyMs = duration;

        WordCount = (newContent ?? string.Empty).Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        LastWordCount = WordCount;

        // Aviso no modal.
        if (failed)
        {
            NoticeVisible = true;
            NonModalNoticeVisible = true;
            NonModalNotice = true;
            NonModal = true;
            IsModal = false;
            Modal = false;
            NoticeMessage = "No se pudo sincronizar las vistas. Sigue editando; se reintentará automáticamente.";
            NonModalNoticeMessage = NoticeMessage;
            NonModalMessage = NoticeMessage;
            SyncNoticeMessage = NoticeMessage;
            SyncNoticeText = NoticeMessage;
            SyncErrorMessage = NoticeMessage;
            SyncFailureMessage = NoticeMessage;
            Message = NoticeMessage;
        }
        else if (degraded)
        {
            NoticeVisible = true;
            NonModalNoticeVisible = true;
            NonModalNotice = true;
            NonModal = true;
            IsModal = false;
            Modal = false;
            NoticeMessage = "El documento es muy grande; la sincronización puede ser más lenta.";
            NonModalNoticeMessage = NoticeMessage;
            NonModalMessage = NoticeMessage;
            SyncNoticeMessage = NoticeMessage;
            SyncNoticeText = NoticeMessage;
            SyncWarningMessage = NoticeMessage;
            Message = NoticeMessage;
        }
        else
        {
            NoticeVisible = false;
            NonModalNoticeVisible = false;
            NonModalNotice = false;
            NoticeMessage = string.Empty;
            NonModalNoticeMessage = string.Empty;
            NonModalMessage = string.Empty;
            SyncNoticeMessage = string.Empty;
            SyncNoticeText = string.Empty;
            Message = string.Empty;
        }

        // LOG-009
        try
        {
            if (failed)
            {
                Logger?.LogError(lastException,
                    "LOG-009 Sincronización de vistas. Hora: {Timestamp:O}. Resultado: fallo. Latencia: {LatencyMs} ms. Palabras: {WordCount}. Reintentos: {Retries}.",
                    DateTimeOffset.UtcNow, duration, WordCount, retries);
            }
            else if (degraded)
            {
                Logger?.LogWarning(
                    "LOG-009 Sincronización de vistas. Hora: {Timestamp:O}. Resultado: degradado. Latencia: {LatencyMs} ms. Palabras: {WordCount}.",
                    DateTimeOffset.UtcNow, duration, WordCount);
            }
            else
            {
                Logger?.LogDebug(
                    "LOG-009 Sincronización de vistas. Hora: {Timestamp:O}. Resultado: éxito. Latencia: {LatencyMs} ms. Palabras: {WordCount}.",
                    DateTimeOffset.UtcNow, duration, WordCount);
            }
        }
        catch { }

        return !failed;
    }

    public static bool Sync(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool Synchronize(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool Update(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool ApplyEdit(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool PushEdit(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool Edit(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool OnEdit(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool HandleEdit(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool NotifyEdit(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool Propagate(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);
    public static bool Broadcast(EditorPanel? sourcePanel, string? newContent) => SyncEdit(sourcePanel, newContent);

    // instance
    public bool SyncEdit(string? newContent)
    {
        EditorPanel? src = SourcePanel ?? Left;
        if (src is null)
        {
            // fallback render
            var doc = Document;
            if (doc is not null) doc.Content = newContent ?? string.Empty;
            Left?.Render(); Right?.Render();
            return true;
        }
        return Synchronizer.SyncEdit(src, newContent);
    }
    public bool Sync(string? newContent) => SyncEdit(newContent);
    public bool Synchronize(string? newContent) => SyncEdit(newContent);
    public bool Update(string? newContent) => SyncEdit(newContent);

    // ---------- EditorPanels integration ----------
    public static bool SyncEdit(EditorPanels? panels, EditorPanel? sourcePanel, string? newContent)
        => SyncEdit(sourcePanel, newContent);
    public static bool SyncEdit(EditorPanels? panels, string? newContent)
    {
        EditorPanel? src = panels?.Left;
        return SyncEdit(src, newContent);
    }
}

// ---------- Partial EditorPanel additions ----------
public partial class EditorPanel
{
    public static List<EditorPanel> AllPanels => Synchronizer.AllPanels;

    // rendered state
    private string _renderedContent = string.Empty;
    private int _renderCount;
    private int _syncCount;

    public string RenderedContent { get => _renderedContent; set => _renderedContent = value ?? string.Empty; }
    public string Rendered { get => _renderedContent; set => _renderedContent = value ?? string.Empty; }
    public string RenderedText { get => _renderedContent; set => _renderedContent = value ?? string.Empty; }
    public string RenderedView { get => _renderedContent; set => _renderedContent = value ?? string.Empty; }
    public string RenderedOutput { get => _renderedContent; set => _renderedContent = value ?? string.Empty; }
    public string RenderedWysiwyg { get; set; } = string.Empty;
    public string RenderedMarkdown { get; set; } = string.Empty;
    public string LastRendered { get => _renderedContent; set => _renderedContent = value ?? string.Empty; }
    public int RenderCount { get => _renderCount; set => _renderCount = value; }
    public int UpdateCount { get => _renderCount; set => _renderCount = value; }
    public int SyncCount { get => _syncCount; set => _syncCount = value; }

    public string? Content
    {
        get => _document?.Content;
        set
        {
            if (_document is not null && value is not null) _document.Content = value;
        }
    }
    public string? Text { get => Content; set => Content = value; }
    public string? Markdown { get => Content; set => Content = value; }
    public string? Body { get => Content; set => Content = value; }


    // events
    public event EventHandler<ContentChangedEventArgs>? ContentChanged;
    public event EventHandler? ContentChangedEvent;
    public event EventHandler? Changed;
    public event EventHandler? ContentUpdated;
    public event EventHandler? ContentModified;
    public event EventHandler? TextChanged;
    public event EventHandler? Updated;
    public event EventHandler? Synced;
    public event EventHandler? Synchronized;
    public event EventHandler? RenderedEvent;

    public void AttachDocument(OpenDocument? doc)
    {
        if (doc is null) return;
        doc.PropertyChanged += OnDocumentPropertyChanged;
        doc.ContentChanged += OnDocumentContentChanged;
        doc.ContentChangedEvent += (_, _) => Render();
        doc.Changed += (_, _) => Render();
        doc.ContentUpdated += (_, _) => Render();
        doc.ContentModified += (_, _) => Render();
        doc.TextChanged += (_, _) => Render();
        doc.Updated += (_, _) => Render();
        doc.Synced += (_, _) => Render();
        doc.Synchronized += (_, _) => Render();
        try { Render(); } catch { }
    }

    public void DetachDocument(OpenDocument? doc)
    {
        if (doc is null) return;
        doc.PropertyChanged -= OnDocumentPropertyChanged;
        doc.ContentChanged -= OnDocumentContentChanged;
    }

    private void OnDocumentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OpenDocument.Content))
        {
            try { Render(); } catch { }
        }
    }

    private void OnDocumentContentChanged(object? sender, ContentChangedEventArgs e)
    {
        try { Render(); } catch { }
        try { ContentChanged?.Invoke(this, e); } catch { }
        try { ContentChangedEvent?.Invoke(this, EventArgs.Empty); } catch { }
        try { Changed?.Invoke(this, EventArgs.Empty); } catch { }
        try { ContentUpdated?.Invoke(this, EventArgs.Empty); } catch { }
        try { ContentModified?.Invoke(this, EventArgs.Empty); } catch { }
        try { TextChanged?.Invoke(this, EventArgs.Empty); } catch { }
        try { Updated?.Invoke(this, EventArgs.Empty); } catch { }
        try { Synced?.Invoke(this, EventArgs.Empty); } catch { }
        try { Synchronized?.Invoke(this, EventArgs.Empty); } catch { }
    }

    public void Render()
    {
        if (_document is null)
        {
            _renderedContent = string.Empty;
            RenderedWysiwyg = string.Empty;
            RenderedMarkdown = string.Empty;
            return;
        }
        if (IsWysiwyg)
        {
            _renderedContent = Renderer.RenderWysiwyg(_document);
            RenderedWysiwyg = _renderedContent;
        }
        else
        {
            _renderedContent = Renderer.RenderMarkdown(_document);
            RenderedMarkdown = _renderedContent;
        }
        _renderCount++;
        try { RenderedEvent?.Invoke(this, EventArgs.Empty); } catch { }
    }

    public void Refresh() => Render();
    public void Update() => Render();
    public void Rerender() => Render();
    public void Redraw() => Render();
    public void Repaint() => Render();
    public void Reload() => Render();
    public void Rebuild() => Render();

    public void RaiseContentChanged(string? newContent, EditorPanel? sourcePanel = null)
    {
        var args = new ContentChangedEventArgs(newContent) { SourcePanel = sourcePanel, Panel = this, Document = _document };
        try { ContentChanged?.Invoke(this, args); } catch { }
        try { ContentChangedEvent?.Invoke(this, EventArgs.Empty); } catch { }
        try { Changed?.Invoke(this, EventArgs.Empty); } catch { }
        try { ContentUpdated?.Invoke(this, EventArgs.Empty); } catch { }
        try { ContentModified?.Invoke(this, EventArgs.Empty); } catch { }
        try { TextChanged?.Invoke(this, EventArgs.Empty); } catch { }
        try { Updated?.Invoke(this, EventArgs.Empty); } catch { }
        try { Synced?.Invoke(this, EventArgs.Empty); } catch { }
        try { Synchronized?.Invoke(this, EventArgs.Empty); } catch { }
    }

    // ---------- SyncEdit on panel ----------
    public bool SyncEdit(string? newContent) => Synchronizer.SyncEdit(this, newContent);
    public static bool SyncEdit(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);

    public bool Sync(string? newContent) => SyncEdit(newContent);
    public static bool Sync(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);

    public bool Synchronize(string? newContent) => SyncEdit(newContent);
    public static bool Synchronize(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);

    public bool UpdateContent(string? newContent) => SyncEdit(newContent);
    public bool ApplyEdit(string? newContent) => SyncEdit(newContent);
    public bool PushEdit(string? newContent) => SyncEdit(newContent);
    public bool Edit(string? newContent) => SyncEdit(newContent);
    public bool OnEdit(string? newContent) => SyncEdit(newContent);
    public bool HandleEdit(string? newContent) => SyncEdit(newContent);

    public static bool SyncEdit(EditorPanels? panels, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool SyncEdit(EditorPanels? panels, string? newContent) => Synchronizer.SyncEdit(panels?.Left, newContent);
}

// ---------- EditorPanels additions ----------
public partial class EditorPanels
{
    public bool SyncEdit(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public bool SyncEdit(string? newContent) => Synchronizer.SyncEdit(Left, newContent);
    public bool Sync(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public bool Sync(string? newContent) => Synchronizer.SyncEdit(Left, newContent);
    public bool Synchronize(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public bool Synchronize(string? newContent) => Synchronizer.SyncEdit(Left, newContent);
    public bool Update(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public bool Update(string? newContent) => Synchronizer.SyncEdit(Left, newContent);
}

// ---------- Alias classes for Sync ----------
public class Sync : Synchronizer { public Sync() : base() { } public Sync(OpenDocument? d) : base(d) { } public Sync(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncManager : Synchronizer { public SyncManager() : base() { } public SyncManager(OpenDocument? d) : base(d) { } public SyncManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncEngine : Synchronizer { public SyncEngine() : base() { } public SyncEngine(OpenDocument? d) : base(d) { } public SyncEngine(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncService : Synchronizer { public SyncService() : base() { } public SyncService(OpenDocument? d) : base(d) { } public SyncService(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncOperation : Synchronizer { public SyncOperation() : base() { } public SyncOperation(OpenDocument? d) : base(d) { } public SyncOperation(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncController : Synchronizer { public SyncController() : base() { } public SyncController(OpenDocument? d) : base(d) { } public SyncController(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncHandler : Synchronizer { public SyncHandler() : base() { } public SyncHandler(OpenDocument? d) : base(d) { } public SyncHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncProcessor : Synchronizer { public SyncProcessor() : base() { } public SyncProcessor(OpenDocument? d) : base(d) { } public SyncProcessor(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DocumentSync : Synchronizer { public DocumentSync() : base() { } public DocumentSync(OpenDocument? d) : base(d) { } public DocumentSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class ViewSync : Synchronizer { public ViewSync() : base() { } public ViewSync(OpenDocument? d) : base(d) { } public ViewSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class PanelSync : Synchronizer { public PanelSync() : base() { } public PanelSync(OpenDocument? d) : base(d) { } public PanelSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class EditorSync : Synchronizer { public EditorSync() : base() { } public EditorSync(OpenDocument? d) : base(d) { } public EditorSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class DualSync : Synchronizer { public DualSync() : base() { } public DualSync(OpenDocument? d) : base(d) { } public DualSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class BidirectionalSync : Synchronizer { public BidirectionalSync() : base() { } public BidirectionalSync(OpenDocument? d) : base(d) { } public BidirectionalSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class SyncCoordinator : Synchronizer { public SyncCoordinator() : base() { } public SyncCoordinator(OpenDocument? d) : base(d) { } public SyncCoordinator(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class Synchronization : Synchronizer { public Synchronization() : base() { } public Synchronization(OpenDocument? d) : base(d) { } public Synchronization(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class Synchronisation : Synchronizer { public Synchronisation() : base() { } public Synchronisation(OpenDocument? d) : base(d) { } public Synchronisation(EditorPanel? l, EditorPanel? r) : base(l, r) { } }
public class Syncronization : Synchronizer { public Syncronization() : base() { } public Syncronization(OpenDocument? d) : base(d) { } public Syncronization(EditorPanel? l, EditorPanel? r) : base(l, r) { } }

// ---------- MainWindowViewModel helper ----------
public partial class MainWindowViewModel
{
    public bool SyncEdit(EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public bool SyncEdit(string? newContent) => Synchronizer.SyncEdit(LeftPanel, newContent);
}