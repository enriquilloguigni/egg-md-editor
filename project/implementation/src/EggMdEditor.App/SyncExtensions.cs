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
    public static bool Sync(this OpenDocument doc, EditorPanel? sourcePanel, string? newContent) => Synchronizer.SyncEdit(sourcePanel, newContent);
    public static bool Sync(this OpenDocument doc, string? newContent) => doc.SyncEdit(newContent);
}