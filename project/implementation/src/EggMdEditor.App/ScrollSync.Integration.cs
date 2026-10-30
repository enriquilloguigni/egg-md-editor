using System;

namespace EggMdEditor.App;

public partial class Synchronizer
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

public partial class EditorPanel
{
    public bool SyncScroll(EditorPanel? targetPanel)
        => ScrollSynchronizer.SyncScroll(this, targetPanel);

    public bool SyncScroll(EditorPanel? targetPanel, PositionMap? map)
        => ScrollSynchronizer.SyncScroll(this, targetPanel, map);

    public bool SyncScroll(EditorPanel? targetPanel, object? map)
        => ScrollSynchronizer.SyncScroll(this, targetPanel, map);

    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel);

    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel, PositionMap? map)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel, map);

    public static bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel, object? map)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel, map);

    public bool ScrollToAnchor(int offset)
        => ScrollSynchronizer.ScrollToAnchor(this, offset);

    public bool ScrollToAnchor(double offset)
        => ScrollSynchronizer.ScrollToAnchor(this, offset);

    public bool ScrollToAnchor(object? offset)
        => ScrollSynchronizer.ScrollToAnchor(this, offset);

    public bool ScrollToAnchor(int offset, PositionMap? map)
        => ScrollSynchronizer.ScrollToAnchor(this, offset, map);

    public bool ScrollToAnchor(int offset, object? map)
        => ScrollSynchronizer.ScrollToAnchor(this, offset, map);

    public bool ScrollToAnchor(object? offset, PositionMap? map)
        => ScrollSynchronizer.ScrollToAnchor(this, offset, map);

    public bool ScrollToAnchor(object? offset, object? map)
        => ScrollSynchronizer.ScrollToAnchor(this, offset, map);

    public static bool ScrollToAnchor(EditorPanel? panel, int offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);

    public static bool ScrollToAnchor(EditorPanel? panel, object? offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);

    public static bool ScrollToAnchor(EditorPanel? panel, int offset, PositionMap? map)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset, map);

    public static bool ScrollToAnchor(EditorPanel? panel, int offset, object? map)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset, map);

    public static bool ScrollToAnchor(EditorPanel? panel, object? offset, PositionMap? map)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset, map);

    public static bool ScrollToAnchor(EditorPanel? panel, object? offset, object? map)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset, map);
}

public partial class EditorPanels
{
    public bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel);

    public bool SyncScroll()
        => ScrollSynchronizer.SyncScroll(Left, Right);

    public bool ScrollToAnchor(EditorPanel? panel, int offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);

    public bool ScrollToAnchor(EditorPanel? panel, object? offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);
}

public partial class MainWindowViewModel
{
    public bool SyncScroll(EditorPanel? sourcePanel, EditorPanel? targetPanel)
        => ScrollSynchronizer.SyncScroll(sourcePanel, targetPanel);

    public bool ScrollToAnchor(EditorPanel? panel, int offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);

    public bool ScrollToAnchor(EditorPanel? panel, object? offset)
        => ScrollSynchronizer.ScrollToAnchor(panel, offset);
}