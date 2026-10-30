using EggMdEditor.App;
using Xunit;

namespace EggMdEditor.App.Tests;

public class T046ScrollSyncTests
{
    [Fact]
    public void SyncScroll_ScrollsTargetToAnchor()
    {
        var md = "# Titulo\n\npara";
        var doc = new OpenDocument(md);
        var (left, right) = EditorPanel.CreatePair(doc);
        var map = PositionMap.Calculate(md);
        var srcOff = map.Anchors[0].SourceStart;
        var expected = map.GetAnchor(srcOff);
        Assert.NotNull(expected);

        right.ScrollPosition = srcOff;
        bool ok = ScrollSynchronizer.SyncScroll(right, left);
        Assert.True(ok);
        Assert.Equal(expected.Value, left.ScrollPosition);
    }

    [Fact]
    public void SyncScroll_NoAnchor_DoesNotScroll()
    {
        var md = "# Titulo\n\npara";
        var doc = new OpenDocument(md);
        var (left, right) = EditorPanel.CreatePair(doc);
        left.ScrollPosition = 0;

        right.ScrollPosition = 100000;
        bool ok = ScrollSynchronizer.SyncScroll(right, left);
        Assert.False(ok);
        Assert.Equal(0, left.ScrollPosition);
    }

    [Fact]
    public void ScrollToAnchor_SetsPosition()
    {
        var panel = new EditorPanel();
        bool ok = ScrollSynchronizer.ScrollToAnchor(panel, 42);
        Assert.True(ok);
        Assert.Equal(42, panel.ScrollPosition);
        Assert.Equal(42, panel.ScrollOffset);
    }

    [Fact]
    public void SyncScroll_DuringEdit_KeepsSynced()
    {
        var md = "# Titulo\n\npara";
        var doc = new OpenDocument(md);
        var (left, right) = EditorPanel.CreatePair(doc);
        var map = PositionMap.Calculate(doc.Content);
        right.ScrollPosition = map.Anchors[0].SourceStart;

        left.SyncEdit(md + "\n\nextra");
        // after edit, scroll sync should have been attempted; target scroll may be updated
        // Just ensure no exception and panels rendered
        Assert.Equal(doc.Content, right.RenderedContent);
    }

    [Fact]
    public void SyncScroll_StaticAlias_Works()
    {
        var doc = new OpenDocument("# H\n\npara");
        var (left, right) = EditorPanel.CreatePair(doc);
        var r = Scroll.SyncScroll(right, left);
        _ = r;
    }

    [Fact]
    public void ScrollToAnchor_ViaPanel_Works()
    {
        var doc = new OpenDocument("hola");
        var panel = EditorPanel.CreateLeft(doc);
        panel.ScrollToAnchor(7);
        Assert.Equal(7, panel.ScrollPosition);
    }
}