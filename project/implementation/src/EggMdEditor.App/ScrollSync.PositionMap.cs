using System;

namespace EggMdEditor.App;

public partial class PositionMap
{
    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel);

    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel, PositionMap? map)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel, map);

    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel, object? map)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel, map);

    public static bool ScrollToAnchor(EditorPanel? panel, int offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);

    public static bool ScrollToAnchor(EditorPanel? panel, double offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);

    public static bool ScrollToAnchor(EditorPanel? panel, object? offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);
}