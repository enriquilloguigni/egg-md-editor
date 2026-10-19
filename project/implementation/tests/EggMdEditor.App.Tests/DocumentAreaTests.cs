using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using EggMdEditor.App.Catalogs;
using Xunit;

namespace EggMdEditor.App.Tests;

// SC-001's document area empty in F-001 (T-010, AC-015): the surface only
// shows the menu bar and the format bar; the document area is left without
// views, panels or content (FR-001 → F-003, FR-004 → F-002); the tooltips
// are painted over it.
public class DocumentAreaTests
{
    [Fact]
    public void The_document_area_is_empty_with_no_views_nor_content()
    {
        var window = new MainWindow(new ApplicationSession());

        var area = window.FindControl<Border>("DocumentArea");

        Assert.NotNull(area);
        Assert.Null(area!.Child);
        Assert.Empty(((ILogical)area).LogicalChildren);
        Assert.Empty(area.GetVisualChildren());
    }

    [Fact]
    public void The_window_only_shows_menu_format_bar_and_document_area()
    {
        var window = new MainWindow(new ApplicationSession());

        var root = Assert.IsType<DockPanel>(window.Content);

        // Z2 menu bar, Z3 format bar and Z4 document area: with no extra
        // panels or views (AC-015, BR-F001-008).
        Assert.Equal(3, root.Children.Count);
        Assert.IsType<Menu>(root.Children[0]);
        Assert.IsType<StackPanel>(root.Children[1]);
        Assert.IsType<Border>(root.Children[2]);
    }
}
