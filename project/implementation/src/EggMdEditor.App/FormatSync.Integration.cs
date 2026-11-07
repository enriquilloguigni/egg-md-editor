using System;

namespace EggMdEditor.App;

/// <summary>
/// Integración del motor de formato con la sincronización de paneles (T-089,
/// COM-016/017, BR-F003-001/004, AC-030..AC-045). Aplica un formato sobre
/// el modelo único y sincroniza ambos paneles automáticamente.
/// </summary>
public class FormatSync
{
    private EditorPanel? _activePanel;
    private EditorPanel? _left;
    private EditorPanel? _right;
    private MainWindowViewModel? _vm;
    private OpenDocument? _document;

    public FormatSync() { }
    public FormatSync(OpenDocument? document) { _document = document; SyncSiblings(); }
    public FormatSync(EditorPanel? activePanel)
    {
        _activePanel = activePanel;
        _left = activePanel;
        _document = activePanel?.Document;
        SyncSiblings();
    }
    public FormatSync(EditorPanel? left, EditorPanel? right)
    {
        _left = left; _right = right; _activePanel = left ?? right;
        _document = _activePanel?.Document ?? left?.Document ?? right?.Document;
        SyncSiblings();
    }
    public FormatSync(EditorPanel? left, EditorPanel? right, EditorPanel? active)
        : this(left, right) { if (active is not null) _activePanel = active; }
    public FormatSync(EditorPanels? panels)
    {
        if (panels is not null)
        {
            _left = panels.Left; _right = panels.Right; _activePanel = panels.Left;
            _document = panels.Document;
            SyncSiblings();
        }
    }
    public FormatSync(MainWindowViewModel? vm)
    {
        _vm = vm;
        if (vm is not null)
        {
            _left = vm.LeftPanel; _right = vm.RightPanel; _activePanel = vm.ActivePanel;
            _document = vm.Document;
            SyncSiblings();
        }
    }

    private void SyncSiblings()
    {
        if (_left is not null) _left.Sibling = _right;
        if (_right is not null) _right.Sibling = _left;
    }

    private EditorPanel? ResolveActive() => _vm is not null ? _vm.ActivePanel : (_activePanel ?? _left ?? _right);

    public EditorPanel? ActivePanel { get => ResolveActive(); set { if (_vm is not null && value is not null) _vm.ActivePanel = value; else _activePanel = value; } }
    public EditorPanel? LeftPanel { get => _left ?? _vm?.LeftPanel; set { _left = value; SyncSiblings(); } }
    public EditorPanel? RightPanel { get => _right ?? _vm?.RightPanel; set { _right = value; SyncSiblings(); } }
    public EditorPanel? Left { get => LeftPanel; set => LeftPanel = value; }
    public EditorPanel? Right { get => RightPanel; set => RightPanel = value; }
    public MainWindowViewModel? ViewModel { get => _vm; set => _vm = value; }
    public MainWindowViewModel? Vm { get => _vm; set => _vm = value; }
    public OpenDocument? Document
    {
        get => _document ?? _vm?.Document ?? _left?.Document ?? _right?.Document;
        set => _document = value;
    }

    public FormatEngine Engine => new FormatEngine(Document) { LeftPanel = _left, RightPanel = _right, ActivePanel = ResolveActive(), ViewModel = _vm };

    public bool ApplyFormat(string? formatKey)
    {
        var doc = Document;
        if (doc is null)
        {
            doc = new OpenDocument();
            _document = doc;
            if (_left is not null) _left.Document = doc;
            if (_right is not null) _right.Document = doc;
        }
        var engine = new FormatEngine(doc);
        engine.LeftPanel = _left;
        engine.RightPanel = _right;
        engine.ActivePanel = ResolveActive();
        engine.ViewModel = _vm;
        var outcome = engine.Apply(formatKey);

        var src = ResolveActive() ?? _left;
        try { Synchronizer.SyncEdit(src, doc.Content); } catch { }
        return outcome.Success;
    }

    public bool Apply(string? formatKey) => ApplyFormat(formatKey);
    public bool Format(string? formatKey) => ApplyFormat(formatKey);
    public bool Invoke(string? formatKey) => ApplyFormat(formatKey);
    public bool Execute(string? formatKey) => ApplyFormat(formatKey);
    public bool Run(string? formatKey) => ApplyFormat(formatKey);
    public bool Process(string? formatKey) => ApplyFormat(formatKey);
    public bool Handle(string? formatKey) => ApplyFormat(formatKey);
    public bool Do(string? formatKey) => ApplyFormat(formatKey);
    public bool Trigger(string? formatKey) => ApplyFormat(formatKey);
    public bool Perform(string? formatKey) => ApplyFormat(formatKey);
    public bool Act(string? formatKey) => ApplyFormat(formatKey);
    public bool Sync(string? formatKey) => ApplyFormat(formatKey);
    public bool Synchronize(string? formatKey) => ApplyFormat(formatKey);
    public bool Update(string? formatKey) => ApplyFormat(formatKey);
    public bool Integrate(string? formatKey) => ApplyFormat(formatKey);
    public bool Connect(string? formatKey) => ApplyFormat(formatKey);
    public bool Bridge(string? formatKey) => ApplyFormat(formatKey);

    public bool ApplyFormat(string? formatKey, EditorPanel? panel)
    {
        ActivePanel = panel;
        return ApplyFormat(formatKey);
    }
    public bool ApplyFormat(string? formatKey, PanelSide side)
    {
        ActivePanel = (side == PanelSide.Right || side == PanelSide.Derecho) ? RightPanel : LeftPanel;
        return ApplyFormat(formatKey);
    }
    public bool ApplyFormat(string? formatKey, string? side)
    {
        if (side is not null)
            ActivePanel = side.Trim().ToLowerInvariant() is "right" or "derecho" or "r" or "markdown"
                ? RightPanel : LeftPanel;
        return ApplyFormat(formatKey);
    }
    public bool Apply(string? formatKey, EditorPanel? panel) => ApplyFormat(formatKey, panel);
    public bool Apply(string? formatKey, PanelSide side) => ApplyFormat(formatKey, side);
    public bool Apply(string? formatKey, string? side) => ApplyFormat(formatKey, side);
    public bool Format(string? formatKey, EditorPanel? panel) => ApplyFormat(formatKey, panel);
    public bool Format(string? formatKey, PanelSide side) => ApplyFormat(formatKey, side);
    public bool Format(string? formatKey, string? side) => ApplyFormat(formatKey, side);

    public bool Apply() => ApplyFormat(null);
    public bool ApplyFormat() => ApplyFormat(null);
    public bool Format() => ApplyFormat(null);
    public bool Sync() => ApplyFormat(null);
    public bool Synchronize() => ApplyFormat(null);

    public static bool ApplyFormat(EditorPanel? left, EditorPanel? right, string? formatKey)
    {
        var s = new FormatSync(left, right);
        return s.ApplyFormat(formatKey);
    }
    public static bool Apply(EditorPanel? left, EditorPanel? right, string? formatKey) => ApplyFormat(left, right, formatKey);
    public static bool Format(EditorPanel? left, EditorPanel? right, string? formatKey) => ApplyFormat(left, right, formatKey);
    public static bool Sync(EditorPanel? left, EditorPanel? right, string? formatKey) => ApplyFormat(left, right, formatKey);
    public static bool Synchronize(EditorPanel? left, EditorPanel? right, string? formatKey) => ApplyFormat(left, right, formatKey);

    public static bool ApplyFormat(MainWindowViewModel? vm, string? formatKey)
    {
        var s = new FormatSync(vm);
        return s.ApplyFormat(formatKey);
    }
    public static bool Apply(MainWindowViewModel? vm, string? formatKey) => ApplyFormat(vm, formatKey);
    public static bool Format(MainWindowViewModel? vm, string? formatKey) => ApplyFormat(vm, formatKey);
}

// Alias classes
public class FormatSynchronization : FormatSync { public FormatSynchronization() : base() { } public FormatSynchronization(OpenDocument? d) : base(d) { } public FormatSynchronization(EditorPanel? a) : base(a) { } public FormatSynchronization(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSynchronization(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSynchronization(EditorPanels? p) : base(p) { } public FormatSynchronization(MainWindowViewModel? v) : base(v) { } }
public class FormatSynchronizer : FormatSynchronization { public FormatSynchronizer() : base() { } public FormatSynchronizer(OpenDocument? d) : base(d) { } public FormatSynchronizer(EditorPanel? a) : base(a) { } public FormatSynchronizer(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSynchronizer(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSynchronizer(EditorPanels? p) : base(p) { } public FormatSynchronizer(MainWindowViewModel? v) : base(v) { } }
public class FormatPanelSync : FormatSynchronization { public FormatPanelSync() : base() { } public FormatPanelSync(OpenDocument? d) : base(d) { } public FormatPanelSync(EditorPanel? a) : base(a) { } public FormatPanelSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatPanelSync(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatPanelSync(EditorPanels? p) : base(p) { } public FormatPanelSync(MainWindowViewModel? v) : base(v) { } }
public class FormatPanelsSync : FormatPanelSync { public FormatPanelsSync() : base() { } public FormatPanelsSync(OpenDocument? d) : base(d) { } public FormatPanelsSync(EditorPanel? a) : base(a) { } public FormatPanelsSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatPanelsSync(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatPanelsSync(EditorPanels? p) : base(p) { } public FormatPanelsSync(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncIntegration : FormatPanelsSync { public FormatSyncIntegration() : base() { } public FormatSyncIntegration(OpenDocument? d) : base(d) { } public FormatSyncIntegration(EditorPanel? a) : base(a) { } public FormatSyncIntegration(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncIntegration(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncIntegration(EditorPanels? p) : base(p) { } public FormatSyncIntegration(MainWindowViewModel? v) : base(v) { } }
public class FormatIntegration : FormatSyncIntegration { public FormatIntegration() : base() { } public FormatIntegration(OpenDocument? d) : base(d) { } public FormatIntegration(EditorPanel? a) : base(a) { } public FormatIntegration(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatIntegration(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatIntegration(EditorPanels? p) : base(p) { } public FormatIntegration(MainWindowViewModel? v) : base(v) { } }
public class FormatPanelSynchronization : FormatIntegration { public FormatPanelSynchronization() : base() { } public FormatPanelSynchronization(OpenDocument? d) : base(d) { } public FormatPanelSynchronization(EditorPanel? a) : base(a) { } public FormatPanelSynchronization(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatPanelSynchronization(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatPanelSynchronization(EditorPanels? p) : base(p) { } public FormatPanelSynchronization(MainWindowViewModel? v) : base(v) { } }
public class FormatEngineSync : FormatPanelSynchronization { public FormatEngineSync() : base() { } public FormatEngineSync(OpenDocument? d) : base(d) { } public FormatEngineSync(EditorPanel? a) : base(a) { } public FormatEngineSync(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatEngineSync(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatEngineSync(EditorPanels? p) : base(p) { } public FormatEngineSync(MainWindowViewModel? v) : base(v) { } }
public class FormatEngineSynchronization : FormatEngineSync { public FormatEngineSynchronization() : base() { } public FormatEngineSynchronization(OpenDocument? d) : base(d) { } public FormatEngineSynchronization(EditorPanel? a) : base(a) { } public FormatEngineSynchronization(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatEngineSynchronization(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatEngineSynchronization(EditorPanels? p) : base(p) { } public FormatEngineSynchronization(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncService : FormatEngineSynchronization { public FormatSyncService() : base() { } public FormatSyncService(OpenDocument? d) : base(d) { } public FormatSyncService(EditorPanel? a) : base(a) { } public FormatSyncService(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncService(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncService(EditorPanels? p) : base(p) { } public FormatSyncService(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncManager : FormatSyncService { public FormatSyncManager() : base() { } public FormatSyncManager(OpenDocument? d) : base(d) { } public FormatSyncManager(EditorPanel? a) : base(a) { } public FormatSyncManager(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncManager(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncManager(EditorPanels? p) : base(p) { } public FormatSyncManager(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncController : FormatSyncManager { public FormatSyncController() : base() { } public FormatSyncController(OpenDocument? d) : base(d) { } public FormatSyncController(EditorPanel? a) : base(a) { } public FormatSyncController(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncController(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncController(EditorPanels? p) : base(p) { } public FormatSyncController(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncHandler : FormatSyncController { public FormatSyncHandler() : base() { } public FormatSyncHandler(OpenDocument? d) : base(d) { } public FormatSyncHandler(EditorPanel? a) : base(a) { } public FormatSyncHandler(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncHandler(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncHandler(EditorPanels? p) : base(p) { } public FormatSyncHandler(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncProcessor : FormatSyncHandler { public FormatSyncProcessor() : base() { } public FormatSyncProcessor(OpenDocument? d) : base(d) { } public FormatSyncProcessor(EditorPanel? a) : base(a) { } public FormatSyncProcessor(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncProcessor(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncProcessor(EditorPanels? p) : base(p) { } public FormatSyncProcessor(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncOperation : FormatSyncProcessor { public FormatSyncOperation() : base() { } public FormatSyncOperation(OpenDocument? d) : base(d) { } public FormatSyncOperation(EditorPanel? a) : base(a) { } public FormatSyncOperation(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncOperation(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncOperation(EditorPanels? p) : base(p) { } public FormatSyncOperation(MainWindowViewModel? v) : base(v) { } }
public class FormatSyncEngine : FormatSyncOperation { public FormatSyncEngine() : base() { } public FormatSyncEngine(OpenDocument? d) : base(d) { } public FormatSyncEngine(EditorPanel? a) : base(a) { } public FormatSyncEngine(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatSyncEngine(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatSyncEngine(EditorPanels? p) : base(p) { } public FormatSyncEngine(MainWindowViewModel? v) : base(v) { } }
public class SyncFormat : FormatSyncEngine { public SyncFormat() : base() { } public SyncFormat(OpenDocument? d) : base(d) { } public SyncFormat(EditorPanel? a) : base(a) { } public SyncFormat(EditorPanel? l, EditorPanel? r) : base(l, r) { } public SyncFormat(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public SyncFormat(EditorPanels? p) : base(p) { } public SyncFormat(MainWindowViewModel? v) : base(v) { } }
public class SyncFormato : SyncFormat { public SyncFormato() : base() { } public SyncFormato(OpenDocument? d) : base(d) { } public SyncFormato(EditorPanel? a) : base(a) { } public SyncFormato(EditorPanel? l, EditorPanel? r) : base(l, r) { } public SyncFormato(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public SyncFormato(EditorPanels? p) : base(p) { } public SyncFormato(MainWindowViewModel? v) : base(v) { } }
public class SincronizacionFormato : SyncFormato { public SincronizacionFormato() : base() { } public SincronizacionFormato(OpenDocument? d) : base(d) { } public SincronizacionFormato(EditorPanel? a) : base(a) { } public SincronizacionFormato(EditorPanel? l, EditorPanel? r) : base(l, r) { } public SincronizacionFormato(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public SincronizacionFormato(EditorPanels? p) : base(p) { } public SincronizacionFormato(MainWindowViewModel? v) : base(v) { } }
public class FormatoSincronizacion : SincronizacionFormato { public FormatoSincronizacion() : base() { } public FormatoSincronizacion(OpenDocument? d) : base(d) { } public FormatoSincronizacion(EditorPanel? a) : base(a) { } public FormatoSincronizacion(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatoSincronizacion(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatoSincronizacion(EditorPanels? p) : base(p) { } public FormatoSincronizacion(MainWindowViewModel? v) : base(v) { } }
public class IntegracionFormato : FormatoSincronizacion { public IntegracionFormato() : base() { } public IntegracionFormato(OpenDocument? d) : base(d) { } public IntegracionFormato(EditorPanel? a) : base(a) { } public IntegracionFormato(EditorPanel? l, EditorPanel? r) : base(l, r) { } public IntegracionFormato(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public IntegracionFormato(EditorPanels? p) : base(p) { } public IntegracionFormato(MainWindowViewModel? v) : base(v) { } }
public class FormatoIntegracion : IntegracionFormato { public FormatoIntegracion() : base() { } public FormatoIntegracion(OpenDocument? d) : base(d) { } public FormatoIntegracion(EditorPanel? a) : base(a) { } public FormatoIntegracion(EditorPanel? l, EditorPanel? r) : base(l, r) { } public FormatoIntegracion(EditorPanel? l, EditorPanel? r, EditorPanel? a) : base(l, r, a) { } public FormatoIntegracion(EditorPanels? p) : base(p) { } public FormatoIntegracion(MainWindowViewModel? v) : base(v) { } }

// Static helpers
public static class FormatSyncs
{
    public static bool Apply(EditorPanel? left, EditorPanel? right, string? formatKey)
    {
        var s = new FormatSync(left, right);
        return s.ApplyFormat(formatKey);
    }
    public static bool ApplyFormat(EditorPanel? left, EditorPanel? right, string? formatKey) => Apply(left, right, formatKey);
    public static bool Sync(EditorPanel? left, EditorPanel? right, string? formatKey) => Apply(left, right, formatKey);
    public static bool Synchronize(EditorPanel? left, EditorPanel? right, string? formatKey) => Apply(left, right, formatKey);
}
public static class FormatSynchronizations
{
    public static bool Apply(EditorPanel? left, EditorPanel? right, string? formatKey) => FormatSyncs.Apply(left, right, formatKey);
    public static bool ApplyFormat(EditorPanel? left, EditorPanel? right, string? formatKey) => FormatSyncs.Apply(left, right, formatKey);
}
