namespace EggMdEditor.App;

public static class SyncExtensions
{
    public static bool SyncEdit(this Synchronizer sync, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool SyncEdit(this Synchronizer sync, string? newContent) => sync.SyncEdit(newContent);
    public static bool Sync(this Synchronizer sync, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool Sync(this Synchronizer sync, string? newContent) => sync.SyncEdit(newContent);
    public static bool Synchronize(this Synchronizer sync, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool Synchronize(this Synchronizer sync, string? newContent) => sync.SyncEdit(newContent);

    public static bool SyncEdit(this EditorPanel panel, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool SyncEdit(this EditorPanel panel, string? newContent) => Synchronizer.SyncEdit(panel, newContent);
    public static bool Sync(this EditorPanel panel, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool Sync(this EditorPanel panel, string? newContent) => Synchronizer.SyncEdit(panel, newContent);
    public static bool Synchronize(this EditorPanel panel, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool Synchronize(this EditorPanel panel, string? newContent) => Synchronizer.SyncEdit(panel, newContent);

    public static bool SyncEdit(this EditorPanels panels, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool SyncEdit(this EditorPanels panels, string? newContent) => Synchronizer.SyncEdit(panels.Left, newContent);
    public static bool Sync(this EditorPanels panels, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool Sync(this EditorPanels panels, string? newContent) => Synchronizer.SyncEdit(panels.Left, newContent);

    public static bool SyncEdit(this OpenDocument doc, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool SyncEdit(this OpenDocument doc, string? newContent) => doc.SyncEdit(newContent);

// Scroll sync extensions (T-046)
public static bool SyncScroll(this EditorPanel panel, EditorPanel? targetPanel) => ScrollSynchronizer.SyncScroll(panel, targetPanel);
public static bool SyncScroll(this EditorPanel panel, EditorPanel? targetPanel, PositionMap? map) => ScrollSynchronizer.SyncScroll(panel, targetPanel, map);
public static bool ScrollToAnchor(this EditorPanel panel, int offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);
public static bool ScrollToAnchor(this EditorPanel panel, double offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);
public static bool ScrollToAnchor(this EditorPanel panel, object? offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);

public static bool SyncScroll(this Synchronizer sync, EditorPanel? sourcePanel, EditorPanel? targetPanel) => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel);
public static bool SyncScroll(this Synchronizer sync, EditorPanel? sourcePanel, EditorPanel? targetPanel, PositionMap? map) => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel, map);
public static bool ScrollToAnchor(this Synchronizer sync, EditorPanel? panel, int offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);
public static bool ScrollToAnchor(this Synchronizer sync, EditorPanel? panel, object? offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);

public static bool SyncScroll(this EditorPanels panels, EditorPanel? sourcePanel, EditorPanel? targetPanel) => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel);
public static bool SyncScroll(this EditorPanels panels) => ScrollSynchronizer.SyncScroll(panels.Left, panels.Right);
public static bool ScrollToAnchor(this EditorPanels panels, EditorPanel? panel, int offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);
public static bool ScrollToAnchor(this EditorPanels panels, EditorPanel? panel, object? offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);

public static bool SyncScroll(this OpenDocument doc, EditorPanel? sourcePanel, EditorPanel? targetPanel) => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel);
public static bool ScrollToAnchor(this OpenDocument doc, EditorPanel? panel, int offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);
public static bool ScrollToAnchor(this OpenDocument doc, EditorPanel? panel, object? offset) => ScrollSynchronizer.ScrollToAnchor(panel, offset);
    public static bool Sync(this OpenDocument doc, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool Sync(this OpenDocument doc, string? newContent) => doc.SyncEdit(newContent);
}